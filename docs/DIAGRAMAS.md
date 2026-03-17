# Diagramas

Diagramas simples para leitura rapida da arquitetura e do fluxo principal implementado no repositorio.

## Componentes

```mermaid
flowchart LR
    Cliente[Cliente HTTP]
    LancApi[Lancamentos API]
    LancDb[(PostgreSQL<br/>lancamentos_db)]
    Outbox[(Tabela mensagens_saida)]
    Rabbit[RabbitMQ]
    Proc[Processador Consolidado]
    ConsDb[(PostgreSQL<br/>consolidado_db)]
    ConsApi[ConsolidadoDiario API]

    Cliente -->|POST/GET| LancApi
    LancApi --> LancDb
    LancApi --> Outbox
    Outbox --> LancApi
    LancApi -->|Publica LancamentoRegistradoV1| Rabbit
    Rabbit -->|Consome fila| Proc
    Proc --> ConsDb
    Cliente -->|GET saldo diario| ConsApi
    ConsApi --> ConsDb
```

## Sequencia do fluxo principal

```mermaid
sequenceDiagram
    participant C as Cliente
    participant L as Lancamentos API
    participant LD as lancamentos_db
    participant R as RabbitMQ
    participant P as Processador Consolidado
    participant CD as consolidado_db
    participant A as ConsolidadoDiario API

    C->>L: POST /api/v1/lancamentos
    L->>LD: grava Lancamento
    L->>LD: grava mensagens_saida
    L-->>C: 201 Created
    L->>R: publica LancamentoRegistradoV1
    R->>P: entrega evento
    P->>CD: aplica credito/debito em SaldoDiario
    C->>A: GET /api/v1/saldos-diarios/{data}
    A->>CD: consulta saldo consolidado
    A-->>C: 200 OK
```

## Comportamento esperado em falha

```mermaid
sequenceDiagram
    participant C as Cliente
    participant L as Lancamentos API
    participant LD as lancamentos_db
    participant R as RabbitMQ
    participant P as Consolidado indisponivel

    C->>L: POST /api/v1/lancamentos
    L->>LD: grava Lancamento
    L->>LD: grava mensagens_saida
    L-->>C: 201 Created
    L->>R: tenta publicar/reenfileirar
    Note over P: API do consolidado ou processador pode estar parado
    Note over C,L: O transacional continua operando\ncom consistencia eventual
```
