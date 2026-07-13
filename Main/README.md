# FingerBridge

Framework e aplicação WinForms de teste para integração com leitores biométricos em **.NET Framework 4.8**.

Objetivo principal: oferecer uma interface única para cadastro e validação biométrica, permitindo adicionar novos dispositivos ao longo do tempo sem acoplar a aplicação real aos SDKs dos fabricantes.

Providers iniciais:

- `mock` — simulação para testar a tela sem leitor físico.
- `futronic.fs80h` — Futronic FS80H usando SDK Futronic.
- `nitgen.hamsterdx` — FingerTech/Nitgen Hamster DX usando eNBioBSP.

> SDKs, drivers, DLLs, instaladores e serial/licenças de fornecedores **não são incluídos** neste repositório.

## Requisitos

- Windows com .NET Framework 4.8 Runtime/Developer Pack.
- Visual Studio com targeting pack do .NET Framework 4.8.
- Plataforma recomendada: `x86`, principalmente ao usar SDKs biométricos nativos antigos.
- SDKs oficiais dos fabricantes instalados localmente quando for usar providers reais.


## Estrutura

```text
FingerBridge/
  FingerBridge.sln
  src/
    FingerBridge.Abstractions/
    FingerBridge.Providers.Mock/
    FingerBridge.Providers.FutronicFs80h/
    FingerBridge.Providers.NitgenHamsterDx/
    FingerBridge.WinFormsTester/
  lib/
    futronic/
    nitgen/
  docs/
  tools/
  sql/
```

## Interface padrão

A aplicação real deve consumir apenas `IFingerprintService`:

```csharp
public interface IFingerprintService : IDisposable
{
    event EventHandler<FingerprintStatusEventArgs> StatusChanged;
    event EventHandler<FingerprintImageEventArgs> ImageCaptured;

    Task<FingerprintEnrollmentResult> EnrollAsync(CancellationToken cancellationToken);

    Task<FingerprintVerificationResult> VerifyAsync(
        byte[] storedTemplate,
        CancellationToken cancellationToken);

    void Cancel();
}
```

## Cadastro de biometria

```csharp
using FingerBridge;
using FingerBridge.Providers.Mock;

var registry = new BiometricProviderRegistry();
registry.Register(new MockBiometricServiceFactory());

var options = new FingerprintServiceOptions
{
    ProviderKey = BiometricProviderKeys.Mock,
    DeviceId = "AUTO"
};

using (var service = registry.Create(options.ProviderKey, options, SynchronizationContext.Current))
{
    var result = await service.EnrollAsync(CancellationToken.None);

    if (result.Success)
    {
        byte[] template = result.Template;
        string providerKey = result.ProviderKey;
        string deviceModel = result.DeviceModel;
        string templateFormat = result.TemplateFormat;
        int quality = result.Quality;

        // Gravar no banco associado ao usuário.
    }
}
```

## Validação 1:1

```csharp
var templateDoBanco = registro.Template;
var providerKeyDoBanco = registro.ProviderKey;
var templateFormatDoBanco = registro.TemplateFormat;

var options = new FingerprintServiceOptions
{
    ProviderKey = providerKeyDoBanco,
    TemplateFormat = templateFormatDoBanco,
    DeviceId = "AUTO"
};

using (var service = registry.Create(options.ProviderKey, options, SynchronizationContext.Current))
{
    var result = await service.VerifyAsync(templateDoBanco, CancellationToken.None);

    if (result.Success && result.Matched)
    {
        // Digital confere.
    }
}
```

## Rodando sem leitor físico

1. Abra `FingerBridge.sln` no Visual Studio.
2. Defina `FingerBridge.WinFormsTester` como projeto inicial.
3. Compile a solução em `.NET Framework 4.8`. Para testes com SDK real, prefira plataforma `x86`. Por padrão, somente `Abstractions`, `Mock` e `WinFormsTester` entram no build da solução.
4. Execute o WinForms.
5. Selecione o provider `Mock`.
6. Teste cadastrar, salvar template, carregar template e validar.

## Futronic FS80H

Copie as DLLs oficiais do SDK Futronic para:

```text
lib/futronic/
```

Arquivos comuns:

```text
ftrSDKHelper10.dll
FTRAPI.dll
ftrScanAPI.dll
```

Depois compile manualmente:

```text
src/FingerBridge.Providers.FutronicFs80h/FingerBridge.Providers.FutronicFs80h.csproj
```

O post-build tenta copiar o provider e as DLLs para a pasta `bin` do WinForms Tester.

## FingerTech / Nitgen Hamster DX

Use o SDK oficial:

```text
eNBioBSP v5.2.0.6 Windows
```

Fluxo recomendado:

1. Instale o SDK executando `eNBioBSP_v5.2.0.6.exe` como administrador.
2. Instale/valide o driver do Hamster DX.
3. Localize a pasta `Bin` do SDK instalado.
4. Rode:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Prepare-NitgenRuntime.ps1 -SdkBinPath "C:\CAMINHO\DO\SDK\Bin"
```

Se não souber o caminho da pasta `Bin`, tente:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Prepare-NitgenRuntime.ps1
```

Arquivos obrigatórios esperados em `lib/nitgen`:

```text
NITGEN.SDK.NBioBSP.dll
NBioBSP.dll
```

Arquivos comuns adicionais:

```text
NBioAPI.dll
NImgConv.dll
NBioBSPCOM.dll
```

Depois compile manualmente:

```text
src/FingerBridge.Providers.NitgenHamsterDx/FingerBridge.Providers.NitgenHamsterDx.csproj
```

## Banco de dados

O script sugerido está em:

```text
sql/UserBiometricTemplates.sql
```

Grave sempre junto com o template:

- `ProviderKey`
- `DeviceModel`
- `TemplateFormat`
- `Template`
- `Quality`

Não assuma compatibilidade entre templates de fornecedores diferentes.

## Adicionando novos dispositivos

Veja:

```text
docs/AddingNewDeviceProvider.md
```

Resumo:

1. Criar um novo projeto `FingerBridge.Providers.NomeDoDispositivo`.
2. Referenciar `FingerBridge.Abstractions`.
3. Implementar `IFingerprintService`.
4. Implementar `IBiometricServiceFactory`.
5. Adicionar o provider ao loader ou copiar o plugin para a pasta do executável.
6. Persistir `ProviderKey` e `TemplateFormat` no banco.

## Segurança

Biometria é dado sensível.

Recomendações mínimas:

- Não salvar imagem da digital.
- Não logar template nem Base64.
- Criptografar o campo `Template` no banco ou na aplicação.
- Auditar cadastro, revogação e validações administrativas.
- Controlar permissão para cadastrar e revogar biometria.
- Não versionar SDKs, DLLs, instaladores, seriais ou licenças de fornecedores.

## Publicação no Git

Veja:

```text
docs/GitPublishChecklist.md
```

Antes de publicar, defina a licença do repositório.
