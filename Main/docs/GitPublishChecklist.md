# Checklist para publicar no Git

Antes de publicar o repositório, valide:

- [ ] Não há arquivos `.dll`, `.exe`, `.zip`, `.rar`, `.7z` ou instaladores proprietários versionados.
- [ ] Não há `SERIAL.txt`, chave de licença, token ou arquivo `.lic` versionado.
- [ ] As pastas `lib/futronic` e `lib/nitgen` têm apenas `README.md` e `.gitkeep`.
- [ ] O projeto `Mock` e o `WinFormsTester` compilam sem os SDKs reais.
- [ ] Os providers reais só são compilados depois de copiar as DLLs oficiais para `lib/`.
- [ ] O README informa que templates biométricos são dados sensíveis.
- [ ] A licença do repositório foi definida antes de torná-lo público.

Comandos úteis:

```powershell
git status --short
git add .
git status --short
git commit -m "Initial FingerBridge"
```

Verificação automatizada de arquivos que não devem entrar no Git:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Check-GitSafety.ps1
```

Verificação rápida dos arquivos staged/untracked:

```powershell
git status --short | findstr /i "dll exe zip rar 7z serial lic"
```
