using System.Threading;

namespace FingerBridge
{
    public interface IBiometricServiceFactory
    {
        string ProviderKey { get; }
        string DisplayName { get; }
        FingerprintDeviceType DeviceType { get; }
        string DefaultTemplateFormat { get; }
        bool IsAvailable(out string reason);
        IFingerprintService Create(FingerprintServiceOptions options, SynchronizationContext callbackContext);
    }
}
