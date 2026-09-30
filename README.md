# FingerBridge

Camada de integração biométrica para aplicações Windows em **.NET Framework 4.8**. O projeto separa a aplicação consumidora dos SDKs dos fabricantes por meio de uma abstração comum e de providers independentes.

## O que o projeto oferece

- Cadastro e validação biométrica 1:1 por uma interface comum (`IFingerprintService`).
- Registro de providers e seleção do leitor sem acoplar a aplicação ao SDK específico.
- Provider `Mock` para exercitar o fluxo sem um leitor físico.
- Providers para **Futronic FS80H** e **FingerTech/Nitgen Hamster DX**.
- Aplicação WinForms de teste para experimentar os fluxos de captura e validação.

## Como começar

A solução e a documentação detalhada ficam na pasta [`Main/`](Main/).

- [Guia de instalação e execução](Main/README.md)
- [Visão da arquitetura](Main/docs/Architecture.md)
- [Como adicionar um provider](Main/docs/AddingNewDeviceProvider.md)
- [Notas do SDK Nitgen](Main/docs/Nitgen-eNBioBSP-5.2.0.6.md)

Para experimentar sem hardware, siga o guia e use o provider `Mock`.

## Requisitos dos leitores físicos

O projeto tem como alvo o **.NET Framework 4.8**. Para os SDKs biométricos nativos, a plataforma `x86` é a recomendada. Drivers e SDKs oficiais dos fabricantes precisam ser instalados à parte; eles não são distribuídos neste repositório.

## Dados biométricos

Os templates devem permanecer associados ao provider e ao formato que os geraram. Não se deve presumir compatibilidade entre fabricantes. Consulte as [orientações de segurança](Main/README.md#seguranca) antes de armazenar ou operar dados biométricos.

Para relatar uma vulnerabilidade, use o canal privado descrito em [SECURITY.md](SECURITY.md).

## Licença

Este projeto está licenciado sob a [Licença MIT](LICENSE).
