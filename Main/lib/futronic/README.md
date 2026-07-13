# DLLs do SDK Futronic

Copie para esta pasta os arquivos do SDK/driver instalados na maquina de desenvolvimento.

Normalmente voce vai precisar de algo como:

- `ftrSDKHelper10.dll` ou o helper .NET equivalente do SDK Futronic;
- `FTRAPI.dll`;
- `ftrScanAPI.dll`;
- demais dependencias nativas instaladas pelo SDK, se existirem.

Depois disso, compile o projeto `FingerBridge.Providers.FutronicFs80h`.
O post-build tenta copiar o provider e as DLLs para a pasta `bin` da aplicacao WinForms.

Se o helper .NET tiver outro nome, ajuste a referencia no arquivo:

`src/FingerBridge.Providers.FutronicFs80h/FingerBridge.Providers.FutronicFs80h.csproj`
