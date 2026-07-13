namespace FingerBridge
{
    public sealed class FingerprintServiceOptions
    {
        public FingerprintServiceOptions()
        {
            ProviderKey = BiometricProviderKeys.Mock;
            DeviceId = "AUTO";
            DeviceModel = string.Empty;
            TemplateFormat = FingerprintTemplateFormats.ProviderNative;
            FakeDetection = true;
            LfdControl = true;
            Farn = 166;
            MiotControl = true;
            MaxModels = 3;
            AbortWhenFakeSourceDetected = true;
            OperationTimeoutMilliseconds = 0;
        }

        /// <summary>
        /// Provider desejado na chamada. Ex.: mock, futronic.fs80h, nitgen.hamsterdx.
        /// </summary>
        public string ProviderKey { get; set; }

        /// <summary>
        /// Identificador do dispositivo quando o SDK permitir escolher o leitor. Use AUTO para auto-detect.
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// Modelo amigavel para log/persistencia. Ex.: FS80H, Hamster DX.
        /// </summary>
        public string DeviceModel { get; set; }

        /// <summary>
        /// Formato solicitado para o template. Nem todo SDK suporta todos os formatos.
        /// </summary>
        public string TemplateFormat { get; set; }

        public int OperationTimeoutMilliseconds { get; set; }

        // Opcoes especificas/compatíveis com Futronic. Providers que nao suportarem devem ignorar.
        public bool FakeDetection { get; set; }
        public bool LfdControl { get; set; }
        public int Farn { get; set; }
        public bool MiotControl { get; set; }
        public int MaxModels { get; set; }
        public bool AbortWhenFakeSourceDetected { get; set; }
    }

    public static class BiometricProviderKeys
    {
        public const string Mock = "mock";
        public const string FutronicFs80h = "futronic.fs80h";
        public const string NitgenHamsterDx = "nitgen.hamsterdx";
    }

    public static class FingerprintTemplateFormats
    {
        public const string ProviderNative = "ProviderNative";
        public const string FutronicSdk42 = "Futronic-SDK-4.2";
        public const string NitgenNBioBspTextFir = "Nitgen-NBioBSP-TextFIR";
        public const string Iso19794_2_2005 = "ISO/IEC-19794-2:2005";
        public const string AnsiIncits378_2004 = "ANSI/INCITS-378-2004";
    }
}
