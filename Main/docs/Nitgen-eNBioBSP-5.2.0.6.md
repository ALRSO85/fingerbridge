# Provider FingerTech / Nitgen Hamster DX - eNBioBSP 5.2.0.6

Este provider foi preparado para o pacote oficial recebido:

```text
eNBioBSP_v5.2.0.6_Windows.zip
```

Conteúdo observado no pacote:

```text
eNBioBSP_v5.2.0.6.exe
Manual/EN eNBSP SDK Programmer's Guide .NET.pdf
Manual/EN eNBSP SDK Programmer's Guide C++.pdf
Manual/EN eNBSP SDK Programmer's Guide Java.pdf
Manual/ISO 19794-4/*
Manual/NFIQ/*
SERIAL.txt
```

O ZIP não traz as DLLs de runtime soltas. Elas são instaladas pelo executável `eNBioBSP_v5.2.0.6.exe` no Windows.

> Não coloque o conteúdo de `SERIAL.txt` em repositório público, log, print ou chamado de suporte que não seja com o fornecedor.

## Modelo usado pelo provider

O provider usa a biblioteca .NET:

```text
NITGEN.SDK.NBioBSP.dll
```

E depende da biblioteca nativa principal:

```text
NBioBSP.dll
```

A documentação .NET do SDK descreve o fluxo base assim:

1. Inicializar o módulo:

```csharp
m_NBioAPI = new NBioAPI();
```

2. Abrir o dispositivo:

```csharp
ret = m_NBioAPI.OpenDevice(NBioAPI.Type.DEVICE_ID.AUTO);
```

3. Cadastrar digital:

```csharp
NBioAPI.Type.HFIR hNewFIR;
ret = m_NBioAPI.Enroll(out hNewFIR, null);
```

4. Converter o template para TextFIR:

```csharp
NBioAPI.Type.FIR_TEXTENCODE textFIR;
m_NBioAPI.GetTextFIRFromHandle(hNewFIR, out textFIR, true);
```

5. Validar 1:1 contra o template armazenado:

```csharp
bool result;
NBioAPI.Type.FIR_PAYLOAD payload = new NBioAPI.Type.FIR_PAYLOAD();
ret = m_NBioAPI.Verify(storedTextFir, out result, payload);
```

## Formato persistido

A framework persiste o `TextFIR` como `byte[]` em UTF-8.

Banco recomendado:

```text
ProviderKey: nitgen.hamsterdx
DeviceModel: Hamster DX
TemplateFormat: Nitgen-NBioBSP-TextFIR
Template: VARBINARY(MAX)
```

## Instalação no Windows

1. Extraia o pacote oficial.
2. Execute `eNBioBSP_v5.2.0.6.exe` como administrador.
3. Informe o serial/licença quando o instalador solicitar.
4. Instale o driver do Hamster DX, se o instalador oferecer essa opção.
5. Conecte o leitor e confirme se o Windows reconheceu o dispositivo.
6. Localize a pasta `Bin` do SDK contendo as DLLs.
7. Rode o script:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Prepare-NitgenRuntime.ps1 -SdkBinPath "C:\CAMINHO\DO\SDK\Bin"
```

Se você não souber o caminho da pasta `Bin`, rode:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Prepare-NitgenRuntime.ps1
```

O script tenta encontrar a instalação em caminhos comuns do Windows.

## Arquivos esperados

Obrigatórios para o provider compilar/carregar:

```text
NITGEN.SDK.NBioBSP.dll
NBioBSP.dll
```

Comuns/úteis, dependendo do instalador e arquitetura:

```text
NBioAPI.dll
NImgConv.dll
NBioBSPCOM.dll
NBioBSPJNI.dll
```

A orientação prática é copiar todas as DLLs da pasta `Bin` do SDK para:

```text
lib/nitgen/
```

## Arquitetura

O tester e os providers estão configurados como `x86` por padrão.

Use uma combinação consistente:

```text
Aplicação x86  -> DLLs Nitgen 32 bits
Aplicação x64  -> DLLs Nitgen 64 bits
```

Para .NET Framework 4.8 + WinForms legado, comece em `x86`.

## Teste mínimo

1. Compile `FingerBridge.Abstractions`.
2. Compile `FingerBridge.Providers.Mock`.
3. Compile `FingerBridge.WinFormsTester`.
4. Rode o tester com Mock.
5. Rode `Prepare-NitgenRuntime.ps1`.
6. Compile `FingerBridge.Providers.NitgenHamsterDx`.
7. Rode o tester novamente.
8. Clique em `Atualizar`.
9. Selecione `FingerTech / Nitgen Hamster DX - eNBioBSP`.
10. Teste cadastro e validação.

## Diagnóstico rápido

Se o provider não aparecer ou aparecer indisponível:

- confirme se `NITGEN.SDK.NBioBSP.dll` está na pasta do `.exe` do tester;
- confirme se `NBioBSP.dll` está na pasta do `.exe` do tester;
- confirme se a arquitetura é x86/x64 compatível;
- confirme se o driver do Hamster DX está instalado;
- compile o projeto provider separadamente para ver o erro real do Visual Studio.
