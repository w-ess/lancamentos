# Plano de Trabalho

Este arquivo funciona como a fonte principal de continuidade do projeto.

A tecnica aqui e, na pratica, uma continuidade por arquivos com memoria externa: o plano fica estavel neste arquivo e a execucao corrente fica registrada em `docs/REGISTRO_DE_EXECUCAO.md`.

## Como usar em um chat novo

1. Pedir para o agente ler `docs/PLANO_DE_TRABALHO.md` e `docs/REGISTRO_DE_EXECUCAO.md`.
2. Mandar o agente executar apenas a próxima tarefa pendente.
3. Ao final da tarefa, exigir que o agente atualize `docs/REGISTRO_DE_EXECUCAO.md`.
4. Se o escopo mudar, atualizar este plano antes de continuar.

## Prompt sugerido para retomada

```text
Leia docs/PLANO_DE_TRABALHO.md e docs/REGISTRO_DE_EXECUCAO.md. Continue a partir da proxima tarefa pendente. Ao finalizar a tarefa, atualize docs/REGISTRO_DE_EXECUCAO.md com: o que foi feito, arquivos alterados, testes executados, pendencias e qual e a proxima tarefa.
```

## Objetivo do projeto

Implementar uma solucao de controle de fluxo de caixa com arquitetura de microsservicos em C#/.NET 8, com:

- servico transacional de lancamentos
- servico de consolidado diario
- comunicacao assincrona entre servicos
- seguranca basica com JWT
- testes automatizados
- ambiente local completo com Docker Compose
- documentacao suficiente para avaliacao tecnica e execucao local

## Decisoes fixadas

- Linguagem e plataforma: C# com .NET 8
- Arquitetura: microsservicos com separacao entre `Lancamentos` e `ConsolidadoDiario`
- Persistencia: PostgreSQL com bancos `lancamentos_db` e `consolidado_db`
- Mensageria: RabbitMQ
- Execucao local: Docker Compose
- Seguranca: autenticacao JWT local, preparada para evolucao futura
- Consistencia: eventual entre transacional e consolidado
- Nomenclatura: tudo em portugues no codigo, na infraestrutura e na documentacao
- Fora de escopo: teste de carga automatizado e integracao real com provedor OIDC externo

## Regras de execucao

- Trabalhar uma tarefa principal por vez.
- Cada tarefa deve deixar o projeto compilando no escopo afetado.
- Toda tarefa concluida deve atualizar `docs/REGISTRO_DE_EXECUCAO.md`.
- Se uma tarefa exigir quebrar em subtarefas durante a implementacao, registrar isso no arquivo de execucao.
- Nao avancar para a proxima tarefa sem registrar o estado atual.

## Tarefas

### T01 - Modelar o dominio e os contratos principais

Objetivo:
- Definir a linguagem ubiqua e a modelagem principal antes da implementacao da solution e da infraestrutura.

Entregas:
- delimitacao dos contextos `Lancamentos` e `ConsolidadoDiario`
- definicao do agregado raiz, entidades e objetos de valor principais
- definicao dos nomes centrais de classes, eventos, filas, rotas e tabelas principais
- definicao dos contratos `Lancamento`, `SaldoDiario` e `LancamentoRegistradoV1`
- definicao do fluxo principal entre transacional, publicacao de evento e consolidacao
- documento curto de modelagem para servir de referencia das proximas tarefas

Definicao de pronto:
- os nomes principais de dominio e integracao estao definidos em portugues
- os limites entre os dois servicos estao claros
- as proximas tarefas conseguem implementar sem rediscutir nomes e responsabilidades principais

Dependencias:
- nenhuma

### T02 - Criar a base da solution

Objetivo:
- Criar a solution .NET e a estrutura inicial de projetos por contexto de negocio.

Entregas:
- solution principal
- projetos separados para `Lancamentos` e `ConsolidadoDiario`
- camadas `Api`, `Aplicacao`, `Dominio` e `Infraestrutura`
- projeto do `Processador` do consolidado
- projetos de teste unitario e integracao
- referencias entre projetos configuradas

Definicao de pronto:
- a solution restaura com sucesso
- os projetos compilam vazios ou com estruturas minimas
- a estrutura de pastas e nomes reflete o plano em portugues

Dependencias:
- T01

### T03 - Implementar o dominio de Lancamentos

Objetivo:
- Modelar as regras centrais do servico transacional com DDD e testes unitarios.

Entregas:
- entidade `Lancamento`
- tipos e validacoes de dominio para credito e debito
- invariantes de valor, tipo e data
- casos de uso basicos de criacao e consulta
- testes unitarios do dominio e da aplicacao

Definicao de pronto:
- o dominio impede estados invalidos
- os testes cobrem as regras de negocio principais
- nao ha dependencia de infraestrutura dentro do dominio

Dependencias:
- T02

### T04 - Implementar persistencia e API de Lancamentos

Objetivo:
- Expor o servico transacional por HTTP e persistir os dados no PostgreSQL.

Entregas:
- mapeamentos do EF Core
- contexto de dados do servico `Lancamentos`
- repositorios e implementacoes de infraestrutura
- endpoint `POST /api/v1/lancamentos`
- endpoint `GET /api/v1/lancamentos/{id}`
- validacao de entrada e tratamento padrao de erro
- migrations iniciais do banco `lancamentos_db`

Definicao de pronto:
- e possivel criar e consultar lancamentos localmente
- a API responde com codigos HTTP coerentes
- migrations aplicam sem ajuste manual de codigo

Dependencias:
- T03

### T05 - Implementar tabela de saida e publicacao de eventos

Objetivo:
- Garantir integracao assincrona sem acoplamento direto entre os servicos.

Entregas:
- tabela de saida no banco de `Lancamentos`
- mensagem de integracao `LancamentoRegistradoV1`
- publicador em background
- integracao com RabbitMQ
- exchange, fila principal e fila de mensagens mortas
- logs e correlacao minima do fluxo

Definicao de pronto:
- ao registrar lancamento, uma mensagem e persistida e publicada
- falha no consolidado nao impede gravacao do lancamento
- a publicacao fica rastreavel por logs

Dependencias:
- T04

### T06 - Implementar o dominio e o processador de ConsolidadoDiario

Objetivo:
- Consumir eventos e manter o saldo diario consolidado.

Entregas:
- modelo `SaldoDiario`
- regra de consolidacao por data
- contexto de dados do servico `ConsolidadoDiario`
- processador consumidor do RabbitMQ
- persistencia no banco `consolidado_db`
- migrations iniciais do consolidado
- testes unitarios da consolidacao

Definicao de pronto:
- o processador consome `LancamentoRegistradoV1`
- creditos e debitos atualizam corretamente o saldo do dia
- o consolidado fica salvo em estrutura propria de leitura

Dependencias:
- T05

### T07 - Implementar API de consulta do consolidado

Objetivo:
- Expor o saldo diario consolidado por HTTP.

Entregas:
- endpoint `GET /api/v1/saldos-diarios/{data}`
- resposta com `Data`, `TotalCreditos`, `TotalDebitos`, `Saldo`, `AtualizadoEm` e `Defasado`
- tratamento para data sem movimento
- testes de aplicacao e API para consulta

Definicao de pronto:
- a API retorna saldo correto para data com e sem movimento
- o contrato de resposta esta documentado e consistente

Dependencias:
- T06

### T08 - Implementar autenticacao e autorizacao

Objetivo:
- Proteger os endpoints com seguranca basica e reproduzivel localmente.

Entregas:
- autenticacao JWT bearer nas APIs
- politicas `lancamentos.escrita`, `lancamentos.leitura` e `consolidado.leitura`
- configuracao por variavel de ambiente
- utilitario ou instrucao clara para gerar token local
- testes cobrindo `401` e `403`

Definicao de pronto:
- endpoints protegidos exigem token valido
- permissoes diferenciam escrita e leitura
- a documentacao explica claramente como se autenticar

Dependencias:
- T04
- T07

### T09 - Subir toda a infraestrutura com Docker Compose

Objetivo:
- Permitir execucao local completa com um unico comando.

Entregas:
- `docker-compose.yml`
- `Dockerfile` dos processos .NET
- servicos `postgres`, `rabbitmq`, `api-lancamentos`, `api-consolidado` e `processador-consolidado`
- verificacoes de saude, volumes, rede interna e configuracoes por ambiente
- `.env.example`

Definicao de pronto:
- `docker compose up --build` sobe toda a aplicacao
- APIs e dependencias ficam acessiveis nas portas documentadas
- o fluxo completo funciona apenas com o compose

Dependencias:
- T05
- T07
- T08

### T10 - Implementar testes de integracao ponta a ponta

Objetivo:
- Validar os fluxos principais do sistema de forma automatizada.

Entregas:
- testes de integracao com `Testcontainers`
- cobertura do fluxo de criacao de lancamento ate atualizacao do consolidado
- cobertura basica de autenticacao e autorizacao
- validacao de indisponibilidade do consolidado sem bloquear o transacional

Definicao de pronto:
- os cenarios criticos passam de forma repetivel
- falhas relevantes de integracao ficam protegidas por teste

Dependencias:
- T08
- T09

### T11 - Documentar e fechar a entrega

Objetivo:
- Deixar o repositorio autoexplicativo para avaliacao tecnica e execucao local.

Entregas:
- `README.md` detalhado
- diagramas simples de componentes e sequencia
- ADRs curtos das decisoes arquiteturais principais
- instrucoes completas de execucao, autenticacao e uso

Definicao de pronto:
- um avaliador consegue subir, autenticar e usar o sistema apenas pelo README
- os diagramas e textos estao consistentes com a implementacao real

Dependencias:
- T09
- T10

## Criterios gerais de aceite

- O servico de `Lancamentos` continua operando mesmo se o consolidado estiver parado.
- O saldo diario reflete corretamente creditos e debitos do dia.
- O projeto roda localmente a partir das instrucoes do repositorio.
- A implementacao usa nomes em portugues e organizacao coerente com DDD, SOLID e Clean Code.
- Ha testes automatizados cobrindo regras de negocio e integracao principal.
- Toda decisao relevante de implementacao fica registrada em documentacao.
