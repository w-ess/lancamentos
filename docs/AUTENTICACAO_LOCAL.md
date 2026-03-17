# Autenticacao Local

As APIs de `Lancamentos` e `ConsolidadoDiario` usam JWT bearer com assinatura simetrica HS256.

## Como gerar o token

Use este comando:

```bash
TOKEN="$(./scripts/gerar_token_jwt_local.sh)"
```

Esse e o fluxo padrao do projeto. Ele ja funciona sem configurar `issuer`, `audience` ou chave no ambiente.

## Variaveis de ambiente

Por padrao, as duas APIs usam a chave fixa de exemplo abaixo:

```bash
export Autenticacao__ChaveAssinatura="fluxodecaixa-chave-demo-fixa-2026"
```

Observacoes:

- se voce nao definir nada, o projeto ja sobe usando `fluxodecaixa-chave-demo-fixa-2026`.
- `Autenticacao__ChaveAssinatura`, quando informada, precisa ter ao menos 32 bytes.
- o token precisa apenas estar assinado com a mesma chave e dentro da validade.
- `/` e `/health` permanecem anonimos; os endpoints de negocio exigem token.

## Permissoes exigidas

- `POST /api/v1/lancamentos`: `lancamentos.escrita`
- `GET /api/v1/lancamentos/{id}`: `lancamentos.leitura`
- `GET /api/v1/saldos-diarios/{data}`: `consolidado.leitura`

As permissoes sao emitidas na claim `scope`, separadas por espaco.

## Gerar token local

O script `scripts/gerar_token_jwt_local.sh` gera um JWT compativel com a configuracao acima.

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
curl -X POST "http://localhost:8081/api/v1/lancamentos" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"Tipo":"Credito","Valor":125.50,"DataLancamento":"2026-03-17"}'
```

```bash
curl "http://localhost:8081/api/v1/lancamentos/<id>" \
  -H "Authorization: Bearer $TOKEN"
```

```bash
curl "http://localhost:8082/api/v1/saldos-diarios/2026-03-17" \
  -H "Authorization: Bearer $TOKEN"
```
