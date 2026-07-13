Copie aqui as DLLs do SDK NITGEN/eNBioBSP usadas pelo Hamster DX.

Versão alvo validada pela documentação do pacote recebido:

- eNBioBSP v5.2.0.6 Windows

O ZIP oficial contém o instalador `eNBioBSP_v5.2.0.6.exe`, manuais e serial/licença.
As DLLs de runtime são instaladas pelo executável no Windows; elas não ficam soltas no ZIP.

Use o script abaixo após instalar o SDK:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Prepare-NitgenRuntime.ps1 -SdkBinPath "C:\CAMINHO\DO\SDK\Bin"
```

Arquivos obrigatórios:

- NITGEN.SDK.NBioBSP.dll
- NBioBSP.dll

Arquivos comuns adicionais:

- NBioAPI.dll
- NImgConv.dll
- NBioBSPCOM.dll
- demais DLLs nativas entregues pelo instalador/SDK

Atenção à arquitetura: use x86 com SDK 32 bits e x64 com SDK 64 bits.
O projeto de exemplo está configurado como x86 por padrão.
