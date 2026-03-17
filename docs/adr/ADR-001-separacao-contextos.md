# ADR-001 - Separacao entre Lancamentos e ConsolidadoDiario

- Status: aceito
- Data: 2026-03-17

## Contexto

O projeto precisa registrar lancamentos com baixa latencia e, ao mesmo tempo, manter uma leitura diaria consolidada sem acoplamento sincronizado entre escrita e consulta analitica.

## Decisao

Separar a solucao em dois contextos de negocio:

- `Lancamentos` como servico transacional de escrita e consulta individual
- `ConsolidadoDiario` como servico de leitura derivada e consolidacao por data

Cada contexto possui suas proprias camadas, banco e contratos internos.

## Consequencias

- reduz acoplamento entre escrita transacional e leitura agregada
- permite que o transacional continue funcionando mesmo com o consolidado indisponivel
- introduz consistencia eventual e maior custo operacional do que um monolito simples
