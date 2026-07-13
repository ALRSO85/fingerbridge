using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace FingerBridge
{
    public interface IFingerprintService : IDisposable
    {
        event EventHandler<FingerprintStatusEventArgs> StatusChanged;
        event EventHandler<FingerprintImageEventArgs> ImageCaptured;

        Task<FingerprintEnrollmentResult> EnrollAsync(CancellationToken cancellationToken);
        Task<FingerprintVerificationResult> VerifyAsync(byte[] storedTemplate, CancellationToken cancellationToken);
        void Cancel();
    }

    public sealed class FingerprintStatusEventArgs : EventArgs
    {
        public FingerprintStatusEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; private set; }
    }

    public sealed class FingerprintImageEventArgs : EventArgs
    {
        public FingerprintImageEventArgs(Bitmap image)
        {
            Image = image;
        }

        public Bitmap Image { get; private set; }
    }
}
