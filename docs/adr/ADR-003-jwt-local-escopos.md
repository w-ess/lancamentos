# ADR-003 - JWT bearer local com escopos

- Status: aceito
- Data: 2026-03-17

## Contexto

O desafio pede seguranca basica, reproduzivel localmente e sem dependencia de um provedor OIDC externo real.

## Decisao

Proteger as APIs com JWT bearer assinado localmente por chave simetrica HS256 e autorizar endpoints por escopos na claim `scope`:

- `lancamentos.escrita`
- `lancamentos.leitura`
- `consolidado.leitura`

## Consequencias

- facilita execucao local, testes de `401` e `403` e avaliacao tecnica rapida
- mantem o desenho preparado para futura troca por um emissor externo
- nao substitui requisitos de seguranca mais fortes de um ambiente produtivo real
