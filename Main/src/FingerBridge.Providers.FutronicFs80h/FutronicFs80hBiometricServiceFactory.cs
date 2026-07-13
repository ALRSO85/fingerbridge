using System.Threading;
using FingerBridge;

namespace FingerBridge.Providers.FutronicFs80h
{
    public sealed class FutronicFs80hBiometricServiceFactory : IBiometricServiceFactory
    {
        public string ProviderKey
        {
            get { return BiometricProviderKeys.FutronicFs80h; }
        }

        public string DisplayName
        {
            get { return "Futronic FS80H - SDK"; }
        }

        public FingerprintDeviceType DeviceType
        {
            get { return FingerprintDeviceType.FutronicFs80h; }
        }

        public string DefaultTemplateFormat
        {
            get { return FingerprintTemplateFormats.FutronicSdk42; }
        }

        public bool IsAvailable(out string reason)
        {
            reason = "Provider Futronic carregado. Verifique se o driver do FS80H esta instalado e se as DLLs nativas estao ao lado do executavel.";
            return true;
        }

        public IFingerprintService Create(FingerprintServiceOptions options, SynchronizationContext callbackContext)
        {
            return new FutronicFs80hFingerprintService(options, callbackContext);
        }
    }
}
