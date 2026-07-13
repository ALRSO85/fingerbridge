# Como adicionar um novo dispositivo

1. Crie um novo projeto Class Library **.NET Framework 4.8**:

```text
src/FingerBridge.Providers.<FornecedorModelo>/
```

2. Referencie:

```text
FingerBridge.Abstractions
```

3. Implemente uma factory:

```csharp
public sealed class MeuLeitorBiometricServiceFactory : IBiometricServiceFactory
{
    public string ProviderKey { get { return "fornecedor.modelo"; } }
    public string DisplayName { get { return "Fornecedor Modelo"; } }
    public FingerprintDeviceType DeviceType { get { return FingerprintDeviceType.Custom; } }
    public string DefaultTemplateFormat { get { return "Fornecedor-Nativo"; } }

    public bool IsAvailable(out string reason)
    {
        reason = "Provider disponivel.";
        return true;
    }

    public IFingerprintService Create(FingerprintServiceOptions options, SynchronizationContext callbackContext)
    {
        return new MeuLeitorFingerprintService(options, callbackContext);
    }
}
```

4. Implemente o serviço:

```csharp
public sealed class MeuLeitorFingerprintService : IFingerprintService
{
    public event EventHandler<FingerprintStatusEventArgs> StatusChanged;
    public event EventHandler<FingerprintImageEventArgs> ImageCaptured;

    public Task<FingerprintEnrollmentResult> EnrollAsync(CancellationToken cancellationToken)
    {
        // Capturar via SDK, extrair template e devolver bytes.
    }

    public Task<FingerprintVerificationResult> VerifyAsync(byte[] storedTemplate, CancellationToken cancellationToken)
    {
        // Capturar via SDK e comparar contra storedTemplate.
    }

    public void Cancel() { }
    public void Dispose() { }
}
```

5. Configure o post-build para copiar o plugin para a pasta do executavel do tester.

6. Rode o tester e clique em `Atualizar`.

## Convenção de chaves

Use `fornecedor.modelo`, em minúsculo, sem espaços.

Exemplos:

- `futronic.fs80h`
- `nitgen.hamsterdx`
- `secugen.hamsterpro20`
- `digitalpersona.uareu4500`

## Persistência recomendada

Sempre grave junto com o template:

- ProviderKey
- DeviceModel
- TemplateFormat
- Template
- Quality
- Data/hora e usuário responsável pelo cadastro

Isso evita tentar validar um template em SDK incompatível.
