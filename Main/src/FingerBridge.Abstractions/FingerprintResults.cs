using System;

namespace FingerBridge
{
    public sealed class FingerprintTemplate
    {
        public FingerprintTemplate(byte[] data, string providerKey, string deviceModel, string templateFormat)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            Data = Copy(data);
            ProviderKey = string.IsNullOrWhiteSpace(providerKey) ? BiometricProviderKeys.Mock : providerKey;
            DeviceModel = deviceModel ?? string.Empty;
            TemplateFormat = string.IsNullOrWhiteSpace(templateFormat) ? FingerprintTemplateFormats.ProviderNative : templateFormat;
            Base64 = Convert.ToBase64String(data);
        }

        public byte[] Data { get; private set; }
        public string ProviderKey { get; private set; }
        public string DeviceModel { get; private set; }
        public string TemplateFormat { get; private set; }
        public string Base64 { get; private set; }

        private static byte[] Copy(byte[] source)
        {
            var copy = new byte[source.Length];
            Buffer.BlockCopy(source, 0, copy, 0, source.Length);
            return copy;
        }
    }

    public sealed class FingerprintEnrollmentResult
    {
        private FingerprintEnrollmentResult()
        {
        }

        public bool Success { get; private set; }
        public byte[] Template { get; private set; }
        public string TemplateBase64 { get; private set; }
        public FingerprintTemplate TemplateInfo { get; private set; }
        public string ProviderKey { get; private set; }
        public string DeviceModel { get; private set; }
        public string TemplateFormat { get; private set; }
        public int Quality { get; private set; }
        public int ErrorCode { get; private set; }
        public string ErrorMessage { get; private set; }

        public static FingerprintEnrollmentResult Ok(byte[] template, int quality)
        {
            return Ok(template, quality, BiometricProviderKeys.Mock, string.Empty, FingerprintTemplateFormats.ProviderNative);
        }

        public static FingerprintEnrollmentResult Ok(byte[] template, int quality, string providerKey, string deviceModel, string templateFormat)
        {
            if (template == null)
                throw new ArgumentNullException("template");

            var info = new FingerprintTemplate(template, providerKey, deviceModel, templateFormat);
            return new FingerprintEnrollmentResult
            {
                Success = true,
                Template = Copy(template),
                TemplateBase64 = info.Base64,
                TemplateInfo = info,
                ProviderKey = info.ProviderKey,
                DeviceModel = info.DeviceModel,
                TemplateFormat = info.TemplateFormat,
                Quality = quality
            };
        }

        public static FingerprintEnrollmentResult Fail(int errorCode, string errorMessage)
        {
            return new FingerprintEnrollmentResult
            {
                Success = false,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage
            };
        }

        private static byte[] Copy(byte[] source)
        {
            var copy = new byte[source.Length];
            Buffer.BlockCopy(source, 0, copy, 0, source.Length);
            return copy;
        }
    }

    public sealed class FingerprintVerificationResult
    {
        private FingerprintVerificationResult()
        {
        }

        public bool Success { get; private set; }
        public bool Matched { get; private set; }
        public string ProviderKey { get; private set; }
        public string TemplateFormat { get; private set; }
        public int ErrorCode { get; private set; }
        public string ErrorMessage { get; private set; }

        public static FingerprintVerificationResult Ok(bool matched)
        {
            return Ok(matched, string.Empty, string.Empty);
        }

        public static FingerprintVerificationResult Ok(bool matched, string providerKey, string templateFormat)
        {
            return new FingerprintVerificationResult
            {
                Success = true,
                Matched = matched,
                ProviderKey = providerKey ?? string.Empty,
                TemplateFormat = templateFormat ?? string.Empty
            };
        }

        public static FingerprintVerificationResult Fail(int errorCode, string errorMessage)
        {
            return new FingerprintVerificationResult
            {
                Success = false,
                Matched = false,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage
            };
        }
    }
}
