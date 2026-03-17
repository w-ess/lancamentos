# Autenticacao Local

As APIs de `Lancamentos` e `ConsolidadoDiario` usam JWT bearer com assinatura simetrica HS256.

## Variaveis de ambiente

Defina as mesmas variaveis para as duas APIs:

```bash
export Autenticacao__Issuer="fluxodecaixa-local"
export Autenticacao__Audience="fluxodecaixa-clientes"
export Autenticacao__ChaveAssinatura="uma-chave-local-com-pelo-menos-32-bytes"
export Autenticacao__ExpiracaoEmMinutos="60"
```

Observacoes:

- `Autenticacao__ChaveAssinatura` e obrigatoria e precisa ter ao menos 32 bytes.
- `Autenticacao__Issuer` e `Autenticacao__Audience` precisam bater com os valores usados na geracao do token.
- `/` e `/health` permanecem anonimos; os endpoints de negocio exigem token.

## Permissoes exigidas

- `POST /api/v1/lancamentos`: `lancamentos.escrita`
- `GET /api/v1/lancamentos/{id}`: `lancamentos.leitura`
- `GET /api/v1/saldos-diarios/{data}`: `consolidado.leitura`

As permissoes sao emitidas na claim `scope`, separadas por espaco.

## Gerar token local

O script `scripts/gerar_token_jwt_local.sh` gera um JWT compativel com a configuracao acima.

Exemplo com todas as permissoes:

```bash
TOKEN="$(./scripts/gerar_token_jwt_local.sh)"
```

Exemplo com permissoes especificas:

```bash
TOKEN="$(./scripts/gerar_token_jwt_local.sh lancamentos.escrita lancamentos.leitura)"
```

Se quiser alterar o `sub` do token:

```bash
JWT_SUBJECT="avaliador-local" TOKEN="$(./scripts/gerar_token_jwt_local.sh consolidado.leitura)"
```

## Exemplos de uso

```bash
curl -X POST "http://localhost:5001/api/v1/lancamentos" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"Tipo":"Credito","Valor":125.50,"DataLancamento":"2026-03-17"}'
```

```bash
curl "http://localhost:5001/api/v1/lancamentos/<id>" \
  -H "Authorization: Bearer $TOKEN"
```

```bash
curl "http://localhost:5002/api/v1/saldos-diarios/2026-03-17" \
  -H "Authorization: Bearer $TOKEN"
```
