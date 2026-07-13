using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FingerBridge
{
    public sealed class BiometricProviderRegistry
    {
        private readonly List<IBiometricServiceFactory> _factories = new List<IBiometricServiceFactory>();

        public void Register(IBiometricServiceFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");

            var existing = _factories.FirstOrDefault(x => string.Equals(x.ProviderKey, factory.ProviderKey, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
                _factories.Remove(existing);

            _factories.Add(factory);
        }

        public IList<IBiometricServiceFactory> GetAll()
        {
            return _factories.ToList();
        }

        public IBiometricServiceFactory GetRequired(string providerKey)
        {
            var factory = _factories.FirstOrDefault(x => string.Equals(x.ProviderKey, providerKey, StringComparison.OrdinalIgnoreCase));
            if (factory == null)
                throw new InvalidOperationException("Provider biometrico nao registrado: " + providerKey);

            return factory;
        }

        public IFingerprintService Create(string providerKey, FingerprintServiceOptions options, SynchronizationContext callbackContext)
        {
            var factory = GetRequired(providerKey);
            string reason;
            if (!factory.IsAvailable(out reason))
                throw new InvalidOperationException(reason);

            options = options ?? new FingerprintServiceOptions();
            options.ProviderKey = factory.ProviderKey;
            if (string.IsNullOrWhiteSpace(options.TemplateFormat))
                options.TemplateFormat = factory.DefaultTemplateFormat;

            return factory.Create(options, callbackContext);
        }
    }
}
