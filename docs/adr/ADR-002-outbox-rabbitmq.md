# ADR-002 - Tabela de saida com RabbitMQ para integracao assincrona

- Status: aceito
- Data: 2026-03-17

## Contexto

O consolidado depende dos dados de `Lancamentos`, mas a gravacao do lancamento nao pode falhar por indisponibilidade temporaria do consumidor ou da leitura consolidada.

## Decisao

Persistir uma `MensagemSaida` na mesma transacao do `Lancamento` e publicar o evento `LancamentoRegistradoV1` em RabbitMQ por um publicador em background.

## Consequencias

- aumenta a confiabilidade entre persistencia transacional e publicacao do evento
- permite retentativa e rastreabilidade do fluxo de integracao
- exige idempotencia no consolidado e adiciona componentes operacionais extras
