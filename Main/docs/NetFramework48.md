# .NET Framework 4.8

O FingerBridge usa **.NET Framework 4.8** como alvo principal para atender projetos Windows legados, especialmente WinForms, serviços Windows e aplicações corporativas que ainda dependem do .NET Framework clássico.

## Decisão

- Target framework: `.NET Framework 4.8`
- Plataforma recomendada para providers reais: `x86`
- Plataforma aceitável para Mock/Abstractions: `Any CPU`

## Motivo

SDKs biométricos costumam depender de DLLs nativas, drivers e bibliotecas COM/interop. Por isso, o uso de .NET Framework 4.8 reduz risco em aplicações legadas Windows sem migrar para .NET moderno.

## Compatibilidade

Uma aplicação em .NET Framework 4.5.2 não referencia diretamente bibliotecas compiladas para .NET Framework 4.8. Para consumir o FingerBridge, atualize a aplicação legada para .NET Framework 4.8 ou mantenha uma branch separada para net452.

## Build recomendado

1. Instale o Developer Pack do .NET Framework 4.8.
2. Abra `FingerBridge.sln` no Visual Studio.
3. Compile primeiro `FingerBridge.Abstractions`, `FingerBridge.Providers.Mock` e `FingerBridge.WinFormsTester`.
4. Para providers reais, copie as DLLs oficiais para `lib/futronic` ou `lib/nitgen` e compile o provider correspondente em `x86`.
