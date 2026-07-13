# Persistência de templates biométricos

Recomendação inicial: salvar o template como `VARBINARY(MAX)` junto com metadados do provider.

Campos mínimos:

- `UserId`
- `ProviderKey`
- `DeviceModel`
- `TemplateFormat`
- `Template`
- `Quality`
- `IsActive`
- `CreatedAt`
- `RevokedAt`

O script base está em:

```text
sql/UserBiometricTemplates.sql
```

## Segurança

Template biométrico é dado sensível.

Recomendações:

- Não salvar imagem da digital, salvo necessidade técnica muito justificada.
- Não registrar template em log.
- Não exibir Base64 do template em produção.
- Criptografar o campo `Template` no banco ou na aplicação.
- Auditar cadastro, validação administrativa, revogação e troca de template.
- Permitir revogar templates sem apagar histórico operacional.
