# Arquitetura

Target principal: **.NET Framework 4.8**. Plataforma recomendada para providers reais: **x86**.

Este repositório separa a aplicação de teste WinForms da camada de integração biométrica.

A aplicação principal deve depender apenas do projeto `FingerBridge.Abstractions`.
Cada leitor físico fica isolado em um provider separado.

## Projetos

| Projeto | Responsabilidade |
|---|---|
| `FingerBridge.Abstractions` | Interfaces, modelos, resultados e registry de providers. |
| `FingerBridge.Providers.Mock` | Provider fake para testar fluxo sem leitor físico. |
| `FingerBridge.Providers.FutronicFs80h` | Integração com Futronic FS80H usando SDK Futronic. |
| `FingerBridge.Providers.NitgenHamsterDx` | Integração com FingerTech/Nitgen Hamster DX usando eNBioBSP. |
| `FingerBridge.WinFormsTester` | Aplicação WinForms para cadastrar, salvar/carregar template e validar. |

## Fluxo de cadastro

1. O usuário escolhe o provider.
2. A aplicação cria o serviço pelo `BiometricProviderRegistry`.
3. O serviço executa `EnrollAsync`.
4. O resultado devolve `Template`, `ProviderKey`, `DeviceModel`, `TemplateFormat` e `Quality`.
5. A aplicação grava esses dados no banco.

## Fluxo de validação 1:1

1. A aplicação carrega do banco o template ativo do usuário.
2. A aplicação lê o `ProviderKey` gravado junto com o template.
3. A aplicação cria o provider correspondente.
4. O serviço executa `VerifyAsync(templateDoBanco)`.
5. O resultado indica `Matched = true` ou `false`.

## Regra importante

Não misture templates de SDKs diferentes.

Um template gerado por `futronic.fs80h` deve ser validado inicialmente pelo provider Futronic.
Um template gerado por `nitgen.hamsterdx` deve ser validado inicialmente pelo provider Nitgen.

Caso precise interoperabilidade entre fornecedores, avalie suporte real a ISO/ANSI no SDK de captura e no algoritmo de matching usado.
