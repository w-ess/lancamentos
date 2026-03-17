# Registro de Execucao

Este arquivo e a memoria operacional do projeto.

Toda vez que uma tarefa for concluida, este arquivo deve ser atualizado para que um novo chat consiga continuar sem contexto anterior.

## Resumo atual

- Estado geral: T01, T02, T03 e T04 concluidas; aguardando inicio da T05
- Ultima tarefa concluida: T04 - Implementar persistencia e API de Lancamentos
- Proxima tarefa: T05 - Implementar tabela de saida e publicacao de eventos
- Ultima atualizacao: 2026-03-17
- Bloqueios conhecidos: nenhum

## Regras de atualizacao

- Atualizar o bloco `Resumo atual` ao final de cada tarefa.
- Adicionar uma nova secao no historico para cada tarefa concluida.
- Registrar apenas fatos concretos do que foi implementado.
- Informar arquivos alterados e testes executados.
- Se algo ficou incompleto, registrar claramente em `Pendencias`.

## Template obrigatorio para novas entradas

Copiar o modelo abaixo ao concluir cada tarefa:

```md
## TXX - Nome da tarefa

- Status: concluida
- Data:
- Objetivo da tarefa:
- O que foi feito:
- Arquivos criados ou alterados:
- Testes executados:
- Decisoes tomadas:
- Pendencias:
- Proxima tarefa recomendada:
```

## Historico

## T01 - Modelar o dominio e os contratos principais

- Status: concluida
- Data: 2026-03-17
- Objetivo da tarefa: definir a linguagem ubiqua, os limites entre `Lancamentos` e `ConsolidadoDiario` e os contratos centrais antes da criacao da solution.
- O que foi feito: criado o documento `docs/MODELAGEM_DOMINIO.md` com delimitacao dos contextos, agregados `Lancamento` e `SaldoDiario`, invariantes de dominio, contratos `Lancamento`, `SaldoDiario` e `LancamentoRegistradoV1`, nomes fixados para rotas, filas, exchange, routing key e tabelas, alem do fluxo principal entre gravacao transacional, tabela de saida, publicacao e consolidacao.
- Arquivos criados ou alterados: `docs/MODELAGEM_DOMINIO.md`; `docs/REGISTRO_DE_EXECUCAO.md`.
- Testes executados: nenhum teste automatizado; tarefa documental revisada por leitura local.
- Decisoes tomadas: manter nomes em portugues; adotar consistencia eventual com tabela de saida; tratar `Lancamento` como agregado imutavel apos persistencia; usar `LancamentoId` como chave de idempotencia de negocio no consolidado e manter `EventoId` apenas como identidade tecnica da mensagem; definir `Defasado` como indicador calculado pelo servico de leitura a partir do atraso no processamento.
- Pendencias: nenhuma dentro do escopo da T01.
- Proxima tarefa recomendada: T02 - Criar a base da solution.

## T02 - Criar a base da solution

- Status: concluida
- Data: 2026-03-17
- Objetivo da tarefa: criar a solution .NET inicial com a separacao por contexto de negocio, camadas internas, processador do consolidado e projetos de teste.
- O que foi feito: criada a solution `""sln`; gerados os projetos `Api`, `Aplicacao`, `Dominio` e `Infraestrutura` para `Lancamentos` e `ConsolidadoDiario`; criado o projeto `ConsolidadoDiario.Processador`; criados quatro projetos de teste separados em unitarios e integracao por contexto; configuradas as referencias entre projetos conforme a dependencia das camadas; substituidos os placeholders dos templates por classes marcadoras e testes minimos de composicao; adicionados endpoints base `/` e `/health` nas APIs e um background service inicial com o nome `ProcessadorLancamentoRegistrado`; adicionada a `.gitignore`; validada a solution com restore, build e teste.
- Arquivos criados ou alterados: `""sln`; `.gitignore`; `src/Lancamentos/Lancamentos.Api/Lancamentos.Api.csproj`; `src/Lancamentos/Lancamentos.Api/Program.cs`; `src/Lancamentos/Lancamentos.Api/MarcadorApi.cs`; `src/Lancamentos/Lancamentos.Api/appsettings.json`; `src/Lancamentos/Lancamentos.Api/appsettings.Development.json`; `src/Lancamentos/Lancamentos.Api/Properties/launchSettings.json`; `src/Lancamentos/Lancamentos.Aplicacao/Lancamentos.Aplicacao.csproj`; `src/Lancamentos/Lancamentos.Aplicacao/MarcadorAplicacao.cs`; `src/Lancamentos/Lancamentos.Dominio/Lancamentos.Dominio.csproj`; `src/Lancamentos/Lancamentos.Dominio/MarcadorDominio.cs`; `src/Lancamentos/Lancamentos.Infraestrutura/Lancamentos.Infraestrutura.csproj`; `src/Lancamentos/Lancamentos.Infraestrutura/MarcadorInfraestrutura.cs`; `src/ConsolidadoDiario/ConsolidadoDiario.Api/ConsolidadoDiario.Api.csproj`; `src/ConsolidadoDiario/ConsolidadoDiario.Api/Program.cs`; `src/ConsolidadoDiario/ConsolidadoDiario.Api/MarcadorApi.cs`; `src/ConsolidadoDiario/ConsolidadoDiario.Api/appsettings.json`; `src/ConsolidadoDiario/ConsolidadoDiario.Api/appsettings.Development.json`; `src/ConsolidadoDiario/ConsolidadoDiario.Api/Properties/launchSettings.json`; `src/ConsolidadoDiario/ConsolidadoDiario.Aplicacao/ConsolidadoDiario.Aplicacao.csproj`; `src/ConsolidadoDiario/ConsolidadoDiario.Aplicacao/MarcadorAplicacao.cs`; `src/ConsolidadoDiario/ConsolidadoDiario.Dominio/ConsolidadoDiario.Dominio.csproj`; `src/ConsolidadoDiario/ConsolidadoDiario.Dominio/MarcadorDominio.cs`; `src/ConsolidadoDiario/ConsolidadoDiario.Infraestrutura/ConsolidadoDiario.Infraestrutura.csproj`; `src/ConsolidadoDiario/ConsolidadoDiario.Infraestrutura/MarcadorInfraestrutura.cs`; `src/ConsolidadoDiario/ConsolidadoDiario.Processador/ConsolidadoDiario.Processador.csproj`; `src/ConsolidadoDiario/ConsolidadoDiario.Processador/Program.cs`; `src/ConsolidadoDiario/ConsolidadoDiario.Processador/ProcessadorLancamentoRegistrado.cs`; `src/ConsolidadoDiario/ConsolidadoDiario.Processador/MarcadorProcessador.cs`; `src/ConsolidadoDiario/ConsolidadoDiario.Processador/appsettings.json`; `src/ConsolidadoDiario/ConsolidadoDiario.Processador/appsettings.Development.json`; `src/ConsolidadoDiario/ConsolidadoDiario.Processador/Properties/launchSettings.json`; `tests/Lancamentos/Lancamentos.Testes.Unitarios/Lancamentos.Testes.Unitarios.csproj`; `tests/Lancamentos/Lancamentos.Testes.Unitarios/EstruturaInicialTests.cs`; `tests/Lancamentos/Lancamentos.Testes.Unitarios/GlobalUsings.cs`; `tests/Lancamentos/Lancamentos.Testes.Integracao/Lancamentos.Testes.Integracao.csproj`; `tests/Lancamentos/Lancamentos.Testes.Integracao/EstruturaInicialTests.cs`; `tests/Lancamentos/Lancamentos.Testes.Integracao/GlobalUsings.cs`; `tests/ConsolidadoDiario/ConsolidadoDiario.Testes.Unitarios/ConsolidadoDiario.Testes.Unitarios.csproj`; `tests/ConsolidadoDiario/ConsolidadoDiario.Testes.Unitarios/EstruturaInicialTests.cs`; `tests/ConsolidadoDiario/ConsolidadoDiario.Testes.Unitarios/GlobalUsings.cs`; `tests/ConsolidadoDiario/ConsolidadoDiario.Testes.Integracao/ConsolidadoDiario.Testes.Integracao.csproj`; `tests/ConsolidadoDiario/ConsolidadoDiario.Testes.Integracao/EstruturaInicialTests.cs`; `tests/ConsolidadoDiario/ConsolidadoDiario.Testes.Integracao/GlobalUsings.cs`; `docs/REGISTRO_DE_EXECUCAO.md`.
- Testes executados: `dotnet test ""sln -c Release --no-build --logger 'console;verbosity=minimal'` com 4 projetos de teste aprovados; validacao complementar com `dotnet restore ""sln` e `dotnet build ""sln -c Release --no-restore`.
- Decisoes tomadas: não usar prefixo de namespace `Processo`; separar testes unitarios e de integracao por contexto; manter APIs e processador com bootstrap minimo e classes marcadoras para permitir evolucao incremental sem acoplamento prematuro; expor `/health` desde a base inicial; remover placeholders `Class1` e `UnitTest1` dos templates.
- Pendencias: nenhuma dentro do escopo da T02; a solution foi deixada propositalmente com estrutura minima, sem implementacao de regras de dominio ou infraestrutura real.
- Proxima tarefa recomendada: T03 - Implementar o dominio de Lancamentos.

## T03 - Implementar o dominio de Lancamentos

- Status: concluida
- Data: 2026-03-17
- Objetivo da tarefa: modelar as regras centrais do servico transacional de `Lancamentos` com DDD e cobrir o dominio e a aplicacao com testes unitarios.
- O que foi feito: implementados os objetos de valor `TipoLancamento`, `ValorMonetario` e `DataLancamento`; criada a entidade `Lancamento` com criacao imutavel e validacao de `RegistradoEmUtc` em UTC; adicionada a excecao de dominio `ExcecaoDominio`; criadas as abstracoes `ILancamentosRepositorio` e `IRelogioUtc`; implementados os casos de uso `RegistrarLancamentoCasoDeUso` e `ConsultarLancamentoPorIdCasoDeUso`, alem do contrato de saida `LancamentoDto` e do comando `RegistrarLancamentoComando`; substituido o teste inicial de estrutura por testes unitarios reais de dominio e aplicacao, com doubles em memoria para repositorio e relogio.
- Arquivos criados ou alterados: `src/Lancamentos/Lancamentos.Dominio/Excecoes/ExcecaoDominio.cs`; `src/Lancamentos/Lancamentos.Dominio/ObjetosDeValor/TipoLancamento.cs`; `src/Lancamentos/Lancamentos.Dominio/ObjetosDeValor/ValorMonetario.cs`; `src/Lancamentos/Lancamentos.Dominio/ObjetosDeValor/DataLancamento.cs`; `src/Lancamentos/Lancamentos.Dominio/Entidades/Lancamento.cs`; `src/Lancamentos/Lancamentos.Aplicacao/Abstracoes/ILancamentosRepositorio.cs`; `src/Lancamentos/Lancamentos.Aplicacao/Abstracoes/IRelogioUtc.cs`; `src/Lancamentos/Lancamentos.Aplicacao/CasosDeUso/LancamentoDto.cs`; `src/Lancamentos/Lancamentos.Aplicacao/CasosDeUso/RegistrarLancamento/RegistrarLancamentoComando.cs`; `src/Lancamentos/Lancamentos.Aplicacao/CasosDeUso/RegistrarLancamento/RegistrarLancamentoCasoDeUso.cs`; `src/Lancamentos/Lancamentos.Aplicacao/CasosDeUso/ConsultarLancamento/ConsultarLancamentoPorIdCasoDeUso.cs`; `tests/Lancamentos/Lancamentos.Testes.Unitarios/Dominio/TipoLancamentoTests.cs`; `tests/Lancamentos/Lancamentos.Testes.Unitarios/Dominio/ValorMonetarioTests.cs`; `tests/Lancamentos/Lancamentos.Testes.Unitarios/Dominio/DataLancamentoTests.cs`; `tests/Lancamentos/Lancamentos.Testes.Unitarios/Dominio/LancamentoTests.cs`; `tests/Lancamentos/Lancamentos.Testes.Unitarios/Aplicacao/RegistrarLancamentoCasoDeUsoTests.cs`; `tests/Lancamentos/Lancamentos.Testes.Unitarios/Aplicacao/ConsultarLancamentoPorIdCasoDeUsoTests.cs`; `tests/Lancamentos/Lancamentos.Testes.Unitarios/Doubles/LancamentosRepositorioEmMemoria.cs`; `tests/Lancamentos/Lancamentos.Testes.Unitarios/Doubles/RelogioUtcFixo.cs`; `tests/Lancamentos/Lancamentos.Testes.Unitarios/EstruturaInicialTests.cs` removido; `docs/REGISTRO_DE_EXECUCAO.md`.
- Testes executados: `dotnet build ""sln -c Release`; `dotnet test ""sln -c Release --no-build --logger 'console;verbosity=minimal'` com 24 testes aprovados no total, sendo 21 no projeto `Lancamentos.Testes.Unitarios`.
- Decisoes tomadas: manter `TipoLancamento` como objeto de valor com valores canonicos `Credito` e `Debito`; rejeitar `RegistradoEmUtc` fora de UTC para preservar rastreabilidade consistente; aceitar valores monetarios positivos com ate duas casas decimais; fazer a consulta por id retornar `null` quando o lancamento nao existir; manter repositorio e relogio como abstracoes da camada de aplicacao para a infraestrutura implementar na T04.
- Pendencias: nenhuma dentro do escopo da T03; ainda falta conectar persistencia real e expor os casos de uso via HTTP, o que fica para a T04.
- Proxima tarefa recomendada: T04 - Implementar persistencia e API de Lancamentos.

## T04 - Implementar persistencia e API de Lancamentos

- Status: concluida
- Data: 2026-03-17
- Objetivo da tarefa: expor o servico transacional por HTTP, persistir os dados no PostgreSQL com EF Core e deixar migrations iniciais prontas para aplicacao.
- O que foi feito: implementado o `LancamentosDbContext` com mapeamento da tabela `lancamentos`; criado o repositorio EF Core e a injecao de dependencias da infraestrutura; adicionados o relogio de sistema UTC e a fabrica de contexto em tempo de design para suportar migrations; conectada a API aos casos de uso com os endpoints `POST /api/v1/lancamentos` e `GET /api/v1/lancamentos/{id}`; adicionados validacao de entrada e tratamento padrao de erro com `ProblemDetails`, inclusive para falhas de bind de JSON; configurada a connection string padrao local em `appsettings.json`; gerada a migration inicial `InicialLancamentos`; substituido o teste de integracao placeholder por testes HTTP reais cobrindo criacao, consulta, `400` e `404`, usando SQLite em memoria para exercitar a persistencia sem depender de PostgreSQL no pipeline local.
- Arquivos criados ou alterados: `src/Lancamentos/Lancamentos.Api/Lancamentos.Api.csproj`; `src/Lancamentos/Lancamentos.Api/Program.cs`; `src/Lancamentos/Lancamentos.Api/appsettings.json`; `src/Lancamentos/Lancamentos.Api/Contratos/RegistrarLancamentoRequest.cs`; `src/Lancamentos/Lancamentos.Api/Endpoints/LancamentosEndpoints.cs`; `src/Lancamentos/Lancamentos.Api/Erros/ManipuladorExcecoesHttp.cs`; `src/Lancamentos/Lancamentos.Infraestrutura/Lancamentos.Infraestrutura.csproj`; `src/Lancamentos/Lancamentos.Infraestrutura/Configuracao/ConfiguracaoInfraestrutura.cs`; `src/Lancamentos/Lancamentos.Infraestrutura/Persistencia/LancamentosDbContext.cs`; `src/Lancamentos/Lancamentos.Infraestrutura/Persistencia/FabricaLancamentosDbContextTempoDeDesign.cs`; `src/Lancamentos/Lancamentos.Infraestrutura/Persistencia/Mapeamentos/LancamentoMapeamento.cs`; `src/Lancamentos/Lancamentos.Infraestrutura/Persistencia/Migrations/20260317143853_InicialLancamentos.cs`; `src/Lancamentos/Lancamentos.Infraestrutura/Persistencia/Migrations/20260317143853_InicialLancamentos.Designer.cs`; `src/Lancamentos/Lancamentos.Infraestrutura/Persistencia/Migrations/LancamentosDbContextModelSnapshot.cs`; `src/Lancamentos/Lancamentos.Infraestrutura/Repositorios/LancamentosRepositorio.cs`; `src/Lancamentos/Lancamentos.Infraestrutura/Servicos/RelogioSistemaUtc.cs`; `tests/Lancamentos/Lancamentos.Testes.Integracao/Lancamentos.Testes.Integracao.csproj`; `tests/Lancamentos/Lancamentos.Testes.Integracao/GlobalUsings.cs`; `tests/Lancamentos/Lancamentos.Testes.Integracao/Api/LancamentosEndpointsTests.cs`; `tests/Lancamentos/Lancamentos.Testes.Integracao/Infraestrutura/LancamentosApiFactory.cs`; `tests/Lancamentos/Lancamentos.Testes.Integracao/EstruturaInicialTests.cs` removido; `docs/REGISTRO_DE_EXECUCAO.md`.
- Testes executados: `dotnet build FluxoDeCaixa.sln -c Release`; `/tmp/dotnet-tools/dotnet-ef migrations add InicialLancamentos --project src/Lancamentos/Lancamentos.Infraestrutura/Lancamentos.Infraestrutura.csproj --startup-project src/Lancamentos/Lancamentos.Api/Lancamentos.Api.csproj --output-dir Persistencia/Migrations`; `dotnet test FluxoDeCaixa.sln -c Release --no-build --logger 'console;verbosity=minimal'` com 27 testes aprovados no total, sendo 21 unitarios de `Lancamentos`, 4 integrados de `Lancamentos` e 2 testes ja existentes de `ConsolidadoDiario`.
- Decisoes tomadas: manter as migrations do servico em `Lancamentos.Infraestrutura`; usar PostgreSQL como provider de runtime e SQLite em memoria apenas nos testes de integracao para exercitar endpoints e persistencia com isolamento local; preservar os nomes de contrato definidos no plano e padronizar erros HTTP em `ProblemDetails` para dominio, validacao e JSON invalido; deixar a connection string pronta para override por variavel de ambiente nas proximas tarefas.
- Pendencias: nenhuma dentro do escopo da T04; a tabela de saida, a mensagem `LancamentoRegistradoV1` e a publicacao em RabbitMQ continuam para a T05.
- Proxima tarefa recomendada: T05 - Implementar tabela de saida e publicacao de eventos.
