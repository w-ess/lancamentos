# Modelagem de Dominio

Este documento fixa a linguagem ubiqua, os limites entre servicos e os contratos principais que orientam a implementacao das proximas tarefas.

## Contextos delimitados

### Lancamentos

Responsabilidade:
- receber, validar e persistir lancamentos de fluxo de caixa
- expor consulta individual de lancamento
- publicar eventos de integracao sem depender do servico de consolidado

O que pertence a este contexto:
- regras de criacao de `Lancamento`
- validacao de `TipoLancamento`, valor e data
- persistencia transacional do lancamento
- persistencia da tabela de saida para integracao assincrona

O que nao pertence a este contexto:
- calculo de saldo agregado por dia
- consulta consolidada para leitura analitica

### ConsolidadoDiario

Responsabilidade:
- consumir eventos de lancamentos confirmados
- consolidar creditos, debitos e saldo por data
- expor leitura otimizada do saldo diario

O que pertence a este contexto:
- regras de atualizacao de `SaldoDiario`
- idempotencia de processamento de eventos
- exposicao do endpoint de consulta diaria

O que nao pertence a este contexto:
- criacao ou alteracao de lancamentos
- dependencia sincronizada do servico transacional

## Linguagem ubiqua

- `Lancamento`: movimento financeiro persistido pelo servico transacional.
- `TipoLancamento`: classificacao do movimento, limitada a `Credito` ou `Debito`.
- `ValorMonetario`: valor positivo com duas casas decimais.
- `DataLancamento`: data civil a que o movimento pertence.
- `SaldoDiario`: consolidado de uma data com total de creditos, total de debitos e saldo resultante.
- `MensagemSaida`: registro local de evento de integracao pendente de publicacao.
- `EventoProcessado`: registro usado pelo consolidado para evitar processamento duplicado.
- `LancamentoRegistradoV1`: evento publicado apos confirmacao transacional do lancamento.
- `Defasado`: indicador de que a leitura do consolidado pode estar atrasada em relacao ao fluxo de eventos consumidos.

## Modelo principal

### Agregado `Lancamento`

Raiz do agregado:
- `Lancamento`

Atributos principais:
- `Id`
- `Tipo`
- `Valor`
- `DataLancamento`
- `RegistradoEmUtc`

Invariantes:
- `Valor` deve ser maior que zero.
- `Tipo` deve ser apenas `Credito` ou `Debito`.
- `DataLancamento` nao pode ser vazia ou invalida.
- `RegistradoEmUtc` e definido pelo sistema no momento da gravacao.
- apos persistido, o lancamento e imutavel para manter rastreabilidade e simplificar a consolidacao.

Objetos de valor:
- `TipoLancamento`
- `ValorMonetario`
- `DataLancamento`

### Agregado `SaldoDiario`

Raiz do agregado:
- `SaldoDiario`

Atributos principais:
- `Data`
- `TotalCreditos`
- `TotalDebitos`
- `Saldo`
- `AtualizadoEmUtc`

Regras:
- para evento de `Credito`, somar em `TotalCreditos` e recalcular `Saldo`.
- para evento de `Debito`, somar em `TotalDebitos` e recalcular `Saldo`.
- `Saldo` e sempre `TotalCreditos - TotalDebitos`.
- a chave natural do agregado e a propria `Data`.
- o processamento deve ser idempotente por `EventoId`.

## Contratos principais

### Contrato `Lancamento`

Representa o recurso HTTP do servico transacional e a entidade persistida.

Campos:
- `Id: Guid`
- `Tipo: string`
- `Valor: decimal`
- `DataLancamento: date`
- `RegistradoEmUtc: datetime`

### Contrato `SaldoDiario`

Representa o recurso HTTP de leitura do consolidado.

Campos:
- `Data: date`
- `TotalCreditos: decimal`
- `TotalDebitos: decimal`
- `Saldo: decimal`
- `AtualizadoEmUtc: datetime`
- `Defasado: bool`

Observacao:
- quando nao houver movimento para a data consultada, a resposta retorna zeros para os valores monetarios; `AtualizadoEmUtc` representa a ultima confirmacao conhecida do processador e `Defasado` indica se o fluxo esta possivelmente atrasado.

### Evento `LancamentoRegistradoV1`

Representa a mensagem de integracao publicada pelo servico de `Lancamentos`.

Campos:
- `EventoId: Guid`
- `OcorridoEmUtc: datetime`
- `LancamentoId: Guid`
- `Tipo: string`
- `Valor: decimal`
- `DataLancamento: date`
- `CorrelacaoId: string`

## Nomes centrais fixados

### Classes e tipos

- `Lancamento`
- `SaldoDiario`
- `TipoLancamento`
- `ValorMonetario`
- `MensagemSaida`
- `EventoProcessado`
- `LancamentoRegistradoV1`
- `PublicadorMensagensSaida`
- `ProcessadorLancamentoRegistrado`

### Rotas HTTP

- `POST /api/v1/lancamentos`
- `GET /api/v1/lancamentos/{id}`
- `GET /api/v1/saldos-diarios/{data}`

### Mensageria

- exchange: `lancamentos.eventos`
- routing key: `lancamento.registrado.v1`
- fila principal: `consolidado-diario.lancamento-registrado.v1`
- fila de mensagens mortas: `consolidado-diario.lancamento-registrado.v1.dlq`

### Tabelas principais

- `lancamentos`
- `mensagens_saida`
- `saldos_diarios`
- `eventos_processados`

## Fluxo principal

1. O cliente envia `POST /api/v1/lancamentos` com `Tipo`, `Valor` e `DataLancamento`.
2. O servico de `Lancamentos` valida o comando, cria o agregado `Lancamento` e persiste o registro.
3. Na mesma transacao, grava uma `MensagemSaida` contendo o evento `LancamentoRegistradoV1`.
4. A API responde com sucesso sem depender da disponibilidade do `ConsolidadoDiario`.
5. Um `PublicadorMensagensSaida` em background le a tabela de saida e publica o evento em `lancamentos.eventos`.
6. O `ProcessadorLancamentoRegistrado` consome a fila `consolidado-diario.lancamento-registrado.v1`.
7. O servico de `ConsolidadoDiario` verifica idempotencia por `EventoId`, aplica o movimento em `SaldoDiario` e atualiza `AtualizadoEmUtc`.
8. O endpoint `GET /api/v1/saldos-diarios/{data}` retorna a leitura consolidada da data, inclusive quando nao houver movimento.

## Decisoes de modelagem

- O sistema trabalha com consistencia eventual entre o servico transacional e o consolidado.
- O contrato de integracao carrega apenas os dados necessarios para consolidacao; o consolidado nao consulta o banco do servico transacional.
- O agregado `Lancamento` nao depende de conta, categoria ou centro de custo neste escopo inicial.
- O modelo prioriza imutabilidade do lancamento para simplificar auditoria e reprocessamento.
- O indicador `Defasado` sera calculado no servico de leitura a partir do atraso observado no processamento de eventos.
