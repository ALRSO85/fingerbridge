using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NITGEN.SDK.NBioBSP;
using FingerBridge;

namespace FingerBridge.Providers.NitgenHamsterDx
{
    public sealed class NitgenHamsterDxFingerprintService : IFingerprintService
    {
        private readonly object _sync = new object();
        private readonly FingerprintServiceOptions _options;
        private readonly SynchronizationContext _callbackContext;
        private NBioAPI _api;
        private bool _disposed;
        private bool _operationRunning;
        private bool _deviceOpen;
        private bool _cancelRequested;

        public NitgenHamsterDxFingerprintService(FingerprintServiceOptions options, SynchronizationContext callbackContext)
        {
            _options = options ?? new FingerprintServiceOptions();
            _callbackContext = callbackContext;
        }

        public event EventHandler<FingerprintStatusEventArgs> StatusChanged;
        public event EventHandler<FingerprintImageEventArgs> ImageCaptured;

        public Task<FingerprintEnrollmentResult> EnrollAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            EnsureIdle();

            return Task.Factory.StartNew(delegate
            {
                lock (_sync)
                {
                    _operationRunning = true;
                    _cancelRequested = false;
                }

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    RaiseStatus("Abrindo dispositivo Nitgen Hamster DX.");
                    OpenDevice();

                    cancellationToken.ThrowIfCancellationRequested();
                    RaiseStatus("Siga a janela do SDK Nitgen para cadastrar a digital.");

                    NBioAPI.Type.HFIR newFir;
                    uint ret = _api.Enroll(out newFir, null);
                    if (WasCancelled(cancellationToken))
                        return FingerprintEnrollmentResult.Fail(-2, "Operacao cancelada.");

                    if (ret != NBioAPI.Error.NONE)
                        return FingerprintEnrollmentResult.Fail((int)ret, "Falha no cadastro Nitgen. Codigo: " + ret);

                    NBioAPI.Type.FIR_TEXTENCODE textFir;
                    ret = _api.GetTextFIRFromHandle(newFir, out textFir, true);
                    if (ret != NBioAPI.Error.NONE)
                        return FingerprintEnrollmentResult.Fail((int)ret, "Falha ao converter FIR para TextFIR. Codigo: " + ret);

                    var text = textFir.TextFIR ?? string.Empty;
                    var bytes = Encoding.UTF8.GetBytes(text);
                    if (bytes.Length == 0)
                        return FingerprintEnrollmentResult.Fail(-1, "O SDK Nitgen retornou um template vazio.");

                    RaiseStatus("Cadastro Nitgen concluido.");
                    return FingerprintEnrollmentResult.Ok(bytes, 0, BiometricProviderKeys.NitgenHamsterDx, "Hamster DX", FingerprintTemplateFormats.NitgenNBioBspTextFir);
                }
                catch (OperationCanceledException)
                {
                    return FingerprintEnrollmentResult.Fail(-2, "Operacao cancelada.");
                }
                catch (Exception ex)
                {
                    return FingerprintEnrollmentResult.Fail(-1, ex.Message);
                }
                finally
                {
                    CloseDevice();
                    lock (_sync)
                    {
                        _operationRunning = false;
                    }
                }
            }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public Task<FingerprintVerificationResult> VerifyAsync(byte[] storedTemplate, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            EnsureIdle();

            if (storedTemplate == null || storedTemplate.Length == 0)
                throw new ArgumentException("Template armazenado nao informado.", "storedTemplate");

            var storedText = Encoding.UTF8.GetString(storedTemplate);

            return Task.Factory.StartNew(delegate
            {
                lock (_sync)
                {
                    _operationRunning = true;
                    _cancelRequested = false;
                }

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    RaiseStatus("Abrindo dispositivo Nitgen Hamster DX.");
                    OpenDevice();

                    NBioAPI.Type.FIR_TEXTENCODE firText = new NBioAPI.Type.FIR_TEXTENCODE();
                    firText.TextFIR = storedText;

                    bool matched;
                    NBioAPI.Type.FIR_PAYLOAD payload = new NBioAPI.Type.FIR_PAYLOAD();

                    RaiseStatus("Siga a janela do SDK Nitgen para validar a digital.");
                    uint ret = _api.Verify(firText, out matched, payload);
                    if (WasCancelled(cancellationToken))
                        return FingerprintVerificationResult.Fail(-2, "Operacao cancelada.");

                    if (ret != NBioAPI.Error.NONE)
                        return FingerprintVerificationResult.Fail((int)ret, "Falha na validacao Nitgen. Codigo: " + ret);

                    RaiseStatus(matched ? "Digital conferida pelo SDK Nitgen." : "Digital nao confere no SDK Nitgen.");
                    return FingerprintVerificationResult.Ok(matched, BiometricProviderKeys.NitgenHamsterDx, FingerprintTemplateFormats.NitgenNBioBspTextFir);
                }
                catch (OperationCanceledException)
                {
                    return FingerprintVerificationResult.Fail(-2, "Operacao cancelada.");
                }
                catch (Exception ex)
                {
                    return FingerprintVerificationResult.Fail(-1, ex.Message);
                }
                finally
                {
                    CloseDevice();
                    lock (_sync)
                    {
                        _operationRunning = false;
                    }
                }
            }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void Cancel()
        {
            lock (_sync)
            {
                _cancelRequested = true;
            }

            try
            {
                CloseDevice();
            }
            catch
            {
                // Cancelamento no NBioBSP e best-effort. Em algumas versoes, a janela do SDK precisa ser fechada pelo usuario.
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            Cancel();

            if (_api != null)
            {
                try
                {
                    _api.Dispose();
                }
                catch
                {
                }

                _api = null;
            }
        }

        private void OpenDevice()
        {
            if (_api == null)
                _api = new NBioAPI();

            uint ret = _api.OpenDevice(ResolveDeviceId());
            if (ret != NBioAPI.Error.NONE)
                throw new InvalidOperationException("Nao foi possivel abrir o dispositivo Nitgen. Codigo: " + ret);

            _deviceOpen = true;
        }

        private void CloseDevice()
        {
            if (_api == null || !_deviceOpen)
                return;

            try
            {
                _api.CloseDevice(ResolveDeviceId());
            }
            catch
            {
            }
            finally
            {
                _deviceOpen = false;
            }
        }

        private short ResolveDeviceId()
        {
            if (string.IsNullOrWhiteSpace(_options.DeviceId) || string.Equals(_options.DeviceId, "AUTO", StringComparison.OrdinalIgnoreCase))
                return NBioAPI.Type.DEVICE_ID.AUTO;

            short parsed;
            if (short.TryParse(_options.DeviceId, out parsed))
                return parsed;

            return NBioAPI.Type.DEVICE_ID.AUTO;
        }

        private bool WasCancelled(CancellationToken cancellationToken)
        {
            lock (_sync)
            {
                return _cancelRequested || cancellationToken.IsCancellationRequested;
            }
        }

        private void EnsureIdle()
        {
            lock (_sync)
            {
                if (_operationRunning)
                    throw new InvalidOperationException("Ja existe uma operacao biometrica em andamento.");
            }
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
    }
}
