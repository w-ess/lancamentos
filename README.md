# Fluxo de Caixa

Solucao de controle de fluxo de caixa em .NET 8 com arquitetura de microsservicos, persistencia em PostgreSQL, integracao assincrona com RabbitMQ, autenticacao JWT e testes automatizados.

## Visao geral

O repositorio contem dois contextos de negocio:

- `Lancamentos`: recebe e persiste creditos e debitos, expoe consulta individual e publica eventos de integracao.
- `ConsolidadoDiario`: consome eventos de lancamentos e mantem o saldo diario consolidado para leitura.

Principais caracteristicas:

- .NET 8 e C#
- PostgreSQL com bancos `lancamentos_db` e `consolidado_db`
- RabbitMQ com fila principal e fila de mensagens mortas
- JWT bearer com escopos por endpoint
- Docker Compose para subir o ambiente completo
- testes unitarios, de integracao e ponta a ponta

## Arquitetura

- `src/Lancamentos`: API, aplicacao, dominio e infraestrutura do servico transacional.
- `src/ConsolidadoDiario`: API, aplicacao, dominio, infraestrutura e processador do consolidado.
- `tests`: suites unitarias, integracao HTTP e ponta a ponta com `Testcontainers`.
- `docs`: modelagem de dominio, autenticacao local, diagramas, ADRs e registro de execucao.

Documentacao complementar:

- [Modelagem de dominio](docs/MODELAGEM_DOMINIO.md)
- [Autenticacao local](docs/AUTENTICACAO_LOCAL.md)
- [Diagramas](docs/DIAGRAMAS.md)
- [ADR 001 - Separacao dos contextos](docs/adr/ADR-001-separacao-contextos.md)
- [ADR 002 - Outbox com RabbitMQ](docs/adr/ADR-002-outbox-rabbitmq.md)
- [ADR 003 - JWT local por escopos](docs/adr/ADR-003-jwt-local-escopos.md)

## Requisitos

- Docker e Docker Compose
- .NET SDK 8.0
- Bash e `openssl` para gerar JWT local pelo script

## Subir o ambiente completo

1. Copie as variaveis padrao:

```bash
cp .env.example .env
```

2. Suba toda a stack:

```bash
docker compose up --build -d
```

3. Verifique a saude dos servicos:

```bash
docker compose ps
```

Endpoints e portas padrao:

- API `Lancamentos`: `http://localhost:8081`
- API `ConsolidadoDiario`: `http://localhost:8082`
- PostgreSQL: `localhost:55432`
- RabbitMQ AMQP: `localhost:55672`
- RabbitMQ Management: `http://localhost:55673`

Health checks anonimos:

- `GET http://localhost:8081/health`
- `GET http://localhost:8082/health`

Para encerrar o ambiente:

```bash
docker compose down -v --remove-orphans
```

## Autenticacao

Os endpoints de negocio exigem JWT bearer. O projeto usa por padrao:

- `Issuer`: `fluxodecaixa-local`
- `Audience`: `fluxodecaixa-clientes`
- algoritmo `HS256`
- claim `scope` com permissoes separadas por espaco

Escopos exigidos:

- `POST /api/v1/lancamentos`: `lancamentos.escrita`
- `GET /api/v1/lancamentos/{id}`: `lancamentos.leitura`
- `GET /api/v1/saldos-diarios/{data}`: `consolidado.leitura`

Para carregar as variaveis do `.env` no shell atual:

```bash
set -a
. ./.env
set +a
```

Para gerar um token com todas as permissoes:

```bash
TOKEN="$(./scripts/gerar_token_jwt_local.sh)"
```

Para gerar um token com escopos especificos:

```bash
TOKEN="$(./scripts/gerar_token_jwt_local.sh lancamentos.escrita lancamentos.leitura)"
```

## Uso rapido

Criar um lancamento:

```bash
curl -X POST "http://localhost:8081/api/v1/lancamentos" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -H "X-Correlation-Id: avaliacao-local-001" \
  -d '{"Tipo":"Credito","Valor":150.75,"DataLancamento":"2026-03-17"}'
```

Consultar um lancamento pelo `Id` retornado no `POST`:

```bash
curl "http://localhost:8081/api/v1/lancamentos/<id>" \
  -H "Authorization: Bearer $TOKEN"
```

Consultar o saldo diario consolidado:

```bash
curl "http://localhost:8082/api/v1/saldos-diarios/2026-03-17" \
  -H "Authorization: Bearer $TOKEN"
```

Observacoes importantes:

- o consolidado e eventual; apos o `POST`, o saldo pode levar alguns segundos para refletir o evento.
- se `ConsolidadoDiario` ou o processador estiverem indisponiveis, a API de `Lancamentos` continua aceitando gravacoes.
- `/` e `/health` sao anonimos; os endpoints de negocio nao sao.

## Executar sem Docker Compose completo

Para desenvolvimento local, voce pode subir apenas as dependencias externas via Docker e rodar os processos .NET no host, em terminais separados:

```bash
docker compose up -d postgres rabbitmq
dotnet run --project src/Lancamentos/Lancamentos.Api/Lancamentos.Api.csproj
dotnet run --project src/ConsolidadoDiario/ConsolidadoDiario.Api/ConsolidadoDiario.Api.csproj
dotnet run --project src/ConsolidadoDiario/ConsolidadoDiario.Processador/ConsolidadoDiario.Processador.csproj
```

Nesse modo, use as `ConnectionStrings` e configuracoes de autenticacao equivalentes ao `.env.example`.

## Testes

Build da solution:

```bash
dotnet build FluxoDeCaixa.sln -c Release
```

Executar toda a suite:

```bash
dotnet test FluxoDeCaixa.sln -c Release --logger "console;verbosity=minimal"
```

Executar apenas ponta a ponta:

```bash
dotnet test tests/FluxoDeCaixa.Testes.EndToEnd/FluxoDeCaixa.Testes.EndToEnd.csproj -c Release --logger "console;verbosity=minimal"
```

## Cenarios validados

- criacao e consulta de lancamentos
- publicacao assincrona via tabela de saida
- consolidacao diaria de creditos e debitos
- autenticacao e autorizacao por escopos
- continuidade do servico transacional sem o consolidado
- execucao local completa com Docker Compose
