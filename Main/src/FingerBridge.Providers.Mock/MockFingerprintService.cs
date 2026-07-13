using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using FingerBridge;

namespace FingerBridge.Providers.Mock
{
    public sealed class MockFingerprintService : IFingerprintService
    {
        private readonly SynchronizationContext _callbackContext;
        private readonly Random _random = new Random();
        private bool _disposed;
        private CancellationTokenSource _currentOperation;

        public MockFingerprintService(SynchronizationContext callbackContext)
        {
            _callbackContext = callbackContext;
        }

        public event EventHandler<FingerprintStatusEventArgs> StatusChanged;
        public event EventHandler<FingerprintImageEventArgs> ImageCaptured;

        public async Task<FingerprintEnrollmentResult> EnrollAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _currentOperation = linked;

            try
            {
                RaiseStatus("MOCK: coloque o dedo no leitor.");
                await Task.Delay(600, linked.Token);
                RaiseImage(CreateMockImage());

                RaiseStatus("MOCK: remova o dedo do leitor.");
                await Task.Delay(600, linked.Token);

                RaiseStatus("MOCK: repetindo captura para cadastro.");
                await Task.Delay(600, linked.Token);
                RaiseImage(CreateMockImage());

                var template = CreateMockTemplate();
                RaiseStatus("MOCK: cadastro finalizado.");
                return FingerprintEnrollmentResult.Ok(template, 85, BiometricProviderKeys.Mock, "Mock", FingerprintTemplateFormats.ProviderNative);
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
                if (_currentOperation == linked)
                    _currentOperation = null;

                linked.Dispose();
            }
        }

        public async Task<FingerprintVerificationResult> VerifyAsync(byte[] storedTemplate, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            if (storedTemplate == null || storedTemplate.Length == 0)
                throw new ArgumentException("Template armazenado nao informado.", "storedTemplate");

            var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _currentOperation = linked;

            try
            {
                RaiseStatus("MOCK: coloque o dedo para validacao.");
                await Task.Delay(700, linked.Token);
                RaiseImage(CreateMockImage());
                await Task.Delay(700, linked.Token);

                // No modo mock, considera OK qualquer template que tenha sido carregado/cadastrado.
                RaiseStatus("MOCK: validacao finalizada.");
                return FingerprintVerificationResult.Ok(true, BiometricProviderKeys.Mock, FingerprintTemplateFormats.ProviderNative);
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
                if (_currentOperation == linked)
                    _currentOperation = null;

                linked.Dispose();
            }
        }

        public void Cancel()
        {
            var current = _currentOperation;
            if (current != null)
                current.Cancel();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            Cancel();
        }

        private byte[] CreateMockTemplate()
        {
            var template = new byte[512];
            _random.NextBytes(template);

            // Assinatura simples para facilitar reconhecer arquivo gerado no modo mock.
            template[0] = (byte)'M';
            template[1] = (byte)'O';
            template[2] = (byte)'C';
            template[3] = (byte)'K';
            return template;
        }

        private Bitmap CreateMockImage()
        {
            var bitmap = new Bitmap(260, 320);

            using (var graphics = Graphics.FromImage(bitmap))
            using (var background = new SolidBrush(Color.WhiteSmoke))
            using (var pen = new Pen(Color.DimGray, 2f))
            using (var penLight = new Pen(Color.LightGray, 1f))
            {
                graphics.FillRectangle(background, 0, 0, bitmap.Width, bitmap.Height);

                for (int i = 0; i < 28; i++)
                {
                    int offset = 10 + i * 10;
                    graphics.DrawEllipse(penLight, 30 - i, offset / 3, 200 + i * 2, 260 - offset / 2);
                }

                for (int i = 0; i < 18; i++)
                {
                    int x = 35 + _random.Next(170);
                    int y = 25 + _random.Next(250);
                    graphics.DrawArc(pen, x, y, 50 + _random.Next(45), 30 + _random.Next(40), _random.Next(180), 160);
                }
            }

            return bitmap;
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
            if (handler == null)
                return;

            if (_callbackContext != null)
            {
                _callbackContext.Post(delegate
                {
                    handler(this, new FingerprintImageEventArgs(image));
                }, null);
            }
            else
            {
                handler(this, new FingerprintImageEventArgs(image));
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }
    }
}
