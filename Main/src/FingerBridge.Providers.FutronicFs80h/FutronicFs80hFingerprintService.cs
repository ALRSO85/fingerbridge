using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

// Se a sua versao do ftrSDKHelper10.dll expuser as classes no namespace global,
// remova ou comente a linha abaixo.
using Futronic.SDKHelper;
using FingerBridge;

namespace FingerBridge.Providers.FutronicFs80h
{
    public sealed class FutronicFs80hFingerprintService : IFingerprintService
    {
        private readonly object _sync = new object();
        private readonly FingerprintServiceOptions _options;
        private readonly SynchronizationContext _callbackContext;

        private FutronicSdkBase _operation;
        private TaskCompletionSource<FingerprintEnrollmentResult> _enrollmentCompletion;
        private TaskCompletionSource<FingerprintVerificationResult> _verificationCompletion;
        private CancellationTokenRegistration _cancellationRegistration;
        private bool _disposed;

        public FutronicFs80hFingerprintService(FingerprintServiceOptions options, SynchronizationContext callbackContext)
        {
            _options = options ?? new FingerprintServiceOptions();
            _callbackContext = callbackContext;
        }

        public event EventHandler<FingerprintStatusEventArgs> StatusChanged;
        public event EventHandler<FingerprintImageEventArgs> ImageCaptured;

        public Task<FingerprintEnrollmentResult> EnrollAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            var completion = new TaskCompletionSource<FingerprintEnrollmentResult>();
            FutronicEnrollment operation = null;

            try
            {
                lock (_sync)
                {
                    EnsureIdle();

                    operation = new FutronicEnrollment();
                    ConfigureCommon(operation);
                    operation.MIOTControl = _options.MiotControl;
                    operation.MaxModels = _options.MaxModels;

                    AttachCommonEvents(operation);
                    operation.OnEnrollmentComplete += OnEnrollmentComplete;

                    _operation = operation;
                    _enrollmentCompletion = completion;
                    RegisterCancellation(cancellationToken);
                }

                RaiseStatus("Coloque o dedo no leitor para iniciar o cadastro.");
                operation.Enrollment();
            }
            catch (Exception ex)
            {
                CleanupOperation();
                completion.TrySetResult(FingerprintEnrollmentResult.Fail(-1, ex.Message));
            }

            return completion.Task;
        }

        public Task<FingerprintVerificationResult> VerifyAsync(byte[] storedTemplate, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            if (storedTemplate == null || storedTemplate.Length == 0)
                throw new ArgumentException("Template armazenado nao informado.", "storedTemplate");

            var completion = new TaskCompletionSource<FingerprintVerificationResult>();
            FutronicVerification operation = null;

            try
            {
                lock (_sync)
                {
                    EnsureIdle();

                    operation = new FutronicVerification(Copy(storedTemplate));
                    ConfigureCommon(operation);

                    AttachCommonEvents(operation);
                    operation.OnVerificationComplete += OnVerificationComplete;

                    _operation = operation;
                    _verificationCompletion = completion;
                    RegisterCancellation(cancellationToken);
                }

                RaiseStatus("Coloque o dedo no leitor para validar.");
                operation.Verification();
            }
            catch (Exception ex)
            {
                CleanupOperation();
                completion.TrySetResult(FingerprintVerificationResult.Fail(-1, ex.Message));
            }

            return completion.Task;
        }

        public void Cancel()
        {
            FutronicSdkBase operation;

            lock (_sync)
            {
                operation = _operation;
            }

            if (operation != null)
            {
                try
                {
                    // Nome mantido conforme SDK Futronic.
                    operation.OnCalcel();
                }
                catch
                {
                    // Cancelamento deve ser best-effort.
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            Cancel();
            CleanupOperation();
        }

        private void ConfigureCommon(FutronicSdkBase operation)
        {
            operation.FakeDetection = _options.FakeDetection;
            operation.FFDControl = _options.LfdControl;
            operation.FARN = _options.Farn;
        }

        private void AttachCommonEvents(FutronicSdkBase operation)
        {
            operation.OnPutOn += OnPutOn;
            operation.OnTakeOff += OnTakeOff;
            operation.UpdateScreenImage += OnUpdateScreenImage;
            operation.OnFakeSource += OnFakeSource;
        }

        private void DetachCommonEvents(FutronicSdkBase operation)
        {
            if (operation == null)
                return;

            operation.OnPutOn -= OnPutOn;
            operation.OnTakeOff -= OnTakeOff;
            operation.UpdateScreenImage -= OnUpdateScreenImage;
            operation.OnFakeSource -= OnFakeSource;
        }

        private void OnPutOn(FTR_PROGRESS progress)
        {
            RaiseStatus("Posicione o dedo no leitor.");
        }

        private void OnTakeOff(FTR_PROGRESS progress)
        {
            RaiseStatus("Remova o dedo do leitor.");
        }

        private void OnUpdateScreenImage(Bitmap image)
        {
            RaiseImage(image);
        }

        private bool OnFakeSource(FTR_PROGRESS progress)
        {
            RaiseStatus("Possivel dedo falso detectado.");
            return _options.AbortWhenFakeSourceDetected;
        }

        private void OnEnrollmentComplete(bool success, int retCode)
        {
            var completion = _enrollmentCompletion;
            var operation = _operation as FutronicEnrollment;

            try
            {
                if (completion == null)
                    return;

                if (success && operation != null)
                {
                    completion.TrySetResult(FingerprintEnrollmentResult.Ok(Copy(operation.Template), operation.Quality, BiometricProviderKeys.FutronicFs80h, "FS80H", FingerprintTemplateFormats.FutronicSdk42));
                }
                else
                {
                    completion.TrySetResult(FingerprintEnrollmentResult.Fail(retCode, FutronicSdkBase.SdkRetCode2Message(retCode)));
                }
            }
            finally
            {
                CleanupOperation();
            }
        }

        private void OnVerificationComplete(bool success, int retCode, bool verificationSuccess)
        {
            var completion = _verificationCompletion;

            try
            {
                if (completion == null)
                    return;

                if (success)
                {
                    completion.TrySetResult(FingerprintVerificationResult.Ok(verificationSuccess, BiometricProviderKeys.FutronicFs80h, FingerprintTemplateFormats.FutronicSdk42));
                }
                else
                {
                    completion.TrySetResult(FingerprintVerificationResult.Fail(retCode, FutronicSdkBase.SdkRetCode2Message(retCode)));
                }
            }
            finally
            {
                CleanupOperation();
            }
        }

        private void CleanupOperation()
        {
            FutronicSdkBase operation;

            lock (_sync)
            {
                operation = _operation;
                _operation = null;
                _enrollmentCompletion = null;
                _verificationCompletion = null;
                _cancellationRegistration.Dispose();
            }

            if (operation != null)
            {
                try
                {
                    DetachCommonEvents(operation);

                    var enrollment = operation as FutronicEnrollment;
                    if (enrollment != null)
                        enrollment.OnEnrollmentComplete -= OnEnrollmentComplete;

                    var verification = operation as FutronicVerification;
                    if (verification != null)
                        verification.OnVerificationComplete -= OnVerificationComplete;

                    operation.Dispose();
                }
                catch
                {
                    // Nao propagar excecao durante limpeza do SDK nativo.
                }
            }
        }

        private void RegisterCancellation(CancellationToken cancellationToken)
        {
            _cancellationRegistration.Dispose();

            if (cancellationToken.CanBeCanceled)
            {
                _cancellationRegistration = cancellationToken.Register(delegate
                {
                    var enrollmentCompletion = _enrollmentCompletion;
                    var verificationCompletion = _verificationCompletion;

                    Cancel();

                    if (enrollmentCompletion != null)
                        enrollmentCompletion.TrySetResult(FingerprintEnrollmentResult.Fail(-2, "Operacao cancelada."));

                    if (verificationCompletion != null)
                        verificationCompletion.TrySetResult(FingerprintVerificationResult.Fail(-2, "Operacao cancelada."));

                    CleanupOperation();
                });
            }
        }

        private void EnsureIdle()
        {
            if (_operation != null)
                throw new InvalidOperationException("Ja existe uma operacao biometrica em andamento.");
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        private void RaiseStatus(string message)
        {
            var handler = StatusChanged;
            if (handler == null)
                return;

            if (_callbackContext != null)
            {
                _callbackContext.Post(delegate
                {
                    handler(this, new FingerprintStatusEventArgs(message));
                }, null);
            }
            else
            {
                handler(this, new FingerprintStatusEventArgs(message));
            }
        }

        private void RaiseImage(Bitmap image)
        {
            var handler = ImageCaptured;
            if (handler == null || image == null)
                return;

            Bitmap imageCopy;
            try
            {
                imageCopy = new Bitmap(image);
            }
            catch
            {
                return;
            }

            if (_callbackContext != null)
            {
                _callbackContext.Post(delegate
                {
                    handler(this, new FingerprintImageEventArgs(imageCopy));
                }, null);
            }
            else
            {
                handler(this, new FingerprintImageEventArgs(imageCopy));
            }
        }

        private static byte[] Copy(byte[] source)
        {
            if (source == null)
                return null;

            var copy = new byte[source.Length];
            Buffer.BlockCopy(source, 0, copy, 0, source.Length);
            return copy;
        }
    }
}
