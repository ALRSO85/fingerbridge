using System.Threading;
using FingerBridge;

namespace FingerBridge.Providers.Mock
{
    public sealed class MockBiometricServiceFactory : IBiometricServiceFactory
    {
        public string ProviderKey
        {
            get { return "mock"; }
        }

        public string DisplayName
        {
            get { return "Mock - sem leitor fisico"; }
        }

        public FingerprintDeviceType DeviceType
        {
            get { return FingerprintDeviceType.Mock; }
        }

        public string DefaultTemplateFormat
        {
            get { return FingerprintTemplateFormats.ProviderNative; }
        }

        public bool IsAvailable(out string reason)
        {
            reason = "Disponivel para testar a tela sem leitor fisico.";
            return true;
        }

        public IFingerprintService Create(FingerprintServiceOptions options, SynchronizationContext callbackContext)
        {
            return new MockFingerprintService(callbackContext);
        }
    }
}
