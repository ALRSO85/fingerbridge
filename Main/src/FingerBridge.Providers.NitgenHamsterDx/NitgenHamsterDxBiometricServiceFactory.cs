using System.IO;
using System.Threading;
using FingerBridge;

namespace FingerBridge.Providers.NitgenHamsterDx
{
    public sealed class NitgenHamsterDxBiometricServiceFactory : IBiometricServiceFactory
    {
        public string ProviderKey
        {
            get { return BiometricProviderKeys.NitgenHamsterDx; }
        }

        public string DisplayName
        {
            get { return "FingerTech / Nitgen Hamster DX - eNBioBSP"; }
        }

        public FingerprintDeviceType DeviceType
        {
            get { return FingerprintDeviceType.NitgenHamsterDx; }
        }

        public string DefaultTemplateFormat
        {
            get { return FingerprintTemplateFormats.NitgenNBioBspTextFir; }
        }

        public bool IsAvailable(out string reason)
        {
            var baseDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var sdkDotNet = Path.Combine(baseDirectory, "NITGEN.SDK.NBioBSP.dll");
            var sdkNative = Path.Combine(baseDirectory, "NBioBSP.dll");

            if (!File.Exists(sdkDotNet))
            {
                reason = "Provider Nitgen carregado, mas NITGEN.SDK.NBioBSP.dll nao foi encontrada na pasta do executavel.";
                return false;
            }

            if (!File.Exists(sdkNative))
            {
                reason = "Provider Nitgen carregado, mas NBioBSP.dll nao foi encontrada na pasta do executavel.";
                return false;
            }

            reason = "Provider Nitgen disponivel. Verifique se o driver do Hamster DX e o SDK eNBioBSP estao instalados.";
            return true;
        }

        public IFingerprintService Create(FingerprintServiceOptions options, SynchronizationContext callbackContext)
        {
            if (options == null)
                options = new FingerprintServiceOptions();

            options.ProviderKey = ProviderKey;
            if (string.IsNullOrWhiteSpace(options.DeviceModel))
                options.DeviceModel = "Hamster DX";
            if (string.IsNullOrWhiteSpace(options.TemplateFormat) || options.TemplateFormat == FingerprintTemplateFormats.ProviderNative)
                options.TemplateFormat = DefaultTemplateFormat;

            return new NitgenHamsterDxFingerprintService(options, callbackContext);
        }
    }
}
