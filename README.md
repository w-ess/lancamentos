# Fluxo de Caixa

Solucao de controle de fluxo de caixa em .NET 8 com arquitetura de microsservicos, persistencia em PostgreSQL, integracao assincrona com RabbitMQ, autenticacao JWT e testes automatizados.

## Comece por aqui

Se esta e a primeira vez que voce vai usar o projeto, siga estes passos exatamente nesta ordem.

### 1. Instale os pre-requisitos

Voce precisa ter instalado:

- Docker
- Docker Compose
- Bash
- `openssl`

O `.NET SDK 8.0` so e necessario se voce quiser rodar testes ou executar os projetos sem o ambiente completo do Docker Compose.

### 2. Entre na pasta do projeto

```bash
cd /caminho/para/o/repositorio
```

### 3. Suba a aplicacao completa

```bash
docker compose up --build -d
```

Esse comando sobe:

- PostgreSQL
- RabbitMQ
- API `Lancamentos`
- API `ConsolidadoDiario`
- processador do consolidado

### 4. Verifique se tudo esta rodando

```bash
docker compose ps
```

O esperado e ver os servicos com status `running` ou `healthy`.

### 5. Gere um token

```bash
TOKEN="$(./scripts/gerar_token_jwt_local.sh)"
```

Esse comando ja funciona do jeito que o projeto esta. Voce nao precisa configurar `issuer`, `audience` ou chave de assinatura para o modo de exemplo.

### 6. Teste criando um lancamento

```bash
curl -X POST "http://localhost:8081/api/v1/lancamentos" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -H "X-Correlation-Id: avaliacao-local-001" \
  -d '{"Tipo":"Credito","Valor":150.75,"DataLancamento":"2026-03-17"}'
```

Se deu certo, a resposta tera um `Id`.

### 7. Teste consultando o saldo consolidado

```bash
curl "http://localhost:8082/api/v1/saldos-diarios/2026-03-17" \
  -H "Authorization: Bearer $TOKEN"
```

Observacao: o consolidado e eventual. Depois de criar um lancamento, pode levar alguns segundos para ele aparecer no saldo consolidado.

### 8. Quando terminar, derrube o ambiente

```bash
docker compose down -v --remove-orphans
```

## Fluxo rapido

Se voce quer apenas copiar e colar os comandos principais:

```bash
docker compose up --build -d
docker compose ps
TOKEN="$(./scripts/gerar_token_jwt_local.sh)"
curl -X POST "http://localhost:8081/api/v1/lancamentos" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -H "X-Correlation-Id: avaliacao-local-001" \
  -d '{"Tipo":"Credito","Valor":150.75,"DataLancamento":"2026-03-17"}'
curl "http://localhost:8082/api/v1/saldos-diarios/2026-03-17" \
  -H "Authorization: Bearer $TOKEN"
```

## Enderecos importantes

- API `Lancamentos`: `http://localhost:8081`
- API `ConsolidadoDiario`: `http://localhost:8082`
- Swagger UI `Lancamentos`: `http://localhost:8081/swagger`
- Swagger UI `ConsolidadoDiario`: `http://localhost:8082/swagger`
- OpenAPI JSON `Lancamentos`: `http://localhost:8081/swagger/v1/swagger.json`
- OpenAPI JSON `ConsolidadoDiario`: `http://localhost:8082/swagger/v1/swagger.json`
- Health `Lancamentos`: `http://localhost:8081/health`
- Health `ConsolidadoDiario`: `http://localhost:8082/health`
- PostgreSQL: `localhost:55432`
- RabbitMQ AMQP: `localhost:55672`
- RabbitMQ Management: `http://localhost:55673`

## Acessos locais

Se voce quiser conectar manualmente nos bancos locais com os valores padrao do Docker Compose, use:

Banco `lancamentos_db`:

```text
Host=localhost;Port=55432;Database=lancamentos_db;Username=postgres;Password=postgres
```

Banco `consolidado_db`:

```text
Host=localhost;Port=55432;Database=consolidado_db;Username=postgres;Password=postgres
```

RabbitMQ AMQP:

```text
Host=localhost
Port=55672
Username=fluxodecaixa
Password=fluxodecaixa
VirtualHost=/
```

URI AMQP equivalente:

```text
amqp://fluxodecaixa:fluxodecaixa@localhost:55672/
```

RabbitMQ Management:

```text
URL: http://localhost:55673
Username: fluxodecaixa
Password: fluxodecaixa
```

JWT local de exemplo:

```text
Autenticacao__ChaveAssinatura=fluxodecaixa-chave-demo-fixa-2026
```

Esses acessos valem para uso pela sua maquina host. Entre os containers, o hostname do PostgreSQL e `postgres` na porta `5432`, e o hostname do RabbitMQ e `rabbitmq` na porta `5672`.

## Como gerar o token

Use este comando:

```bash
TOKEN="$(./scripts/gerar_token_jwt_local.sh)"
```

Para gerar um token com escopos especificos:

```bash
TOKEN="$(./scripts/gerar_token_jwt_local.sh lancamentos.escrita lancamentos.leitura)"
```

Por padrao, o projeto usa a chave fixa de exemplo `fluxodecaixa-chave-demo-fixa-2026`.
Se quiser, voce ainda pode sobrescrever com `Autenticacao__ChaveAssinatura`.

Voce nao precisa criar `.env` para o fluxo padrao da entrevista. O `docker-compose.yml` ja tem valores default para tudo que e necessario.

## Postman

O repositorio inclui a collection [`postman/FluxoDeCaixa.postman_collection.json`](/home/wes/Documentos/Programacao/Pessoal/processo/postman/FluxoDeCaixa.postman_collection.json).

Como usar:

1. Importe a collection no Postman.
2. Rode a request `Auth / Generate JWT`.
3. Ela gera e salva automaticamente a variavel `jwt_token` na collection.
4. Rode `Lancamentos / Create Lancamento`.
5. Essa request salva automaticamente o `Id` retornado em `lancamento_id`.
6. Rode `Lancamentos / Get Lancamento By Id`.
7. Rode `ConsolidadoDiario / Get Saldo Diario`.

Se preferir explorar a API manualmente pelo navegador, use tambem o Swagger:

- `http://localhost:8081/swagger`
- `http://localhost:8082/swagger`

Variaveis importantes da collection:

- `lancamentos_base_url`: default `http://localhost:8081`
- `consolidado_base_url`: default `http://localhost:8082`
- `jwt_secret`: default `fluxodecaixa-chave-demo-fixa-2026`
- `jwt_scope`: default `lancamentos.escrita lancamentos.leitura consolidado.leitura`
- `saldo_data`: default `2026-03-17`

Observacao: a request `Auth / Generate JWT` nao chama um endpoint de login porque o projeto nao tem login. Ela gera localmente um JWT HS256 compativel com a configuracao do sistema.

## Escopos exigidos

- `POST /api/v1/lancamentos`: `lancamentos.escrita`
- `GET /api/v1/lancamentos/{id}`: `lancamentos.leitura`
- `GET /api/v1/saldos-diarios/{data}`: `consolidado.leitura`

## Uso dos endpoints

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

Nesse modo, use as `ConnectionStrings` e configuracoes equivalentes ao `.env.example`.

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

## Cenarios validados

- criacao e consulta de lancamentos
- publicacao assincrona via tabela de saida
- consolidacao diaria de creditos e debitos
- autenticacao e autorizacao por escopos
- continuidade do servico transacional sem o consolidado
- execucao local completa com Docker Compose
