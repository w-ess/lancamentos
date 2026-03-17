# Registro de Execucao

Este arquivo e a memoria operacional do projeto.

Toda vez que uma tarefa for concluida, este arquivo deve ser atualizado para que um novo chat consiga continuar sem contexto anterior.

## Resumo atual

- Estado geral: T01, T02 e T03 concluidas; aguardando inicio da T04
- Ultima tarefa concluida: T03 - Implementar o dominio de Lancamentos
- Proxima tarefa: T04 - Implementar persistencia e API de Lancamentos
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
- Decisoes tomadas: manter nomes em portugues; adotar consistencia eventual com tabela de saida; tratar `Lancamento` como agregado imutavel apos persistencia; usar `EventoId` para idempotencia no consolidado; definir `Defasado` como indicador calculado pelo servico de leitura a partir do atraso no processamento.
- Pendencias: nenhuma dentro do escopo da T01.
- Proxima tarefa recomendada: T02 - Criar a base da solution.

## T02 - Criar a base da solution

- Status: concluida
- Data: 2026-03-17
- Objetivo da tarefa: criar a solution .NET inicial com a separacao por contexto de negocio, camadas internas, processador do consolidado e projetos de teste.
- O que foi feito: criada a solution `Processo.sln`; gerados os projetos `Api`, `Aplicacao`, `Dominio` e `Infraestrutura` para `Lancamentos` e `ConsolidadoDiario`; criado o projeto `Processo.ConsolidadoDiario.Processador`; criados quatro projetos de teste separados em unitarios e integracao por contexto; configuradas as referencias entre projetos conforme a dependencia das camadas; substituidos os placeholders dos templates por classes marcadoras e testes minimos de composicao; adicionados endpoints base `/` e `/health` nas APIs e um background service inicial com o nome `ProcessadorLancamentoRegistrado`; adicionada a `.gitignore`; validada a solution com restore, build e teste.
- Arquivos criados ou alterados: `Processo.sln`; `.gitignore`; `src/Lancamentos/Processo.Lancamentos.Api/Processo.Lancamentos.Api.csproj`; `src/Lancamentos/Processo.Lancamentos.Api/Program.cs`; `src/Lancamentos/Processo.Lancamentos.Api/MarcadorApi.cs`; `src/Lancamentos/Processo.Lancamentos.Api/appsettings.json`; `src/Lancamentos/Processo.Lancamentos.Api/appsettings.Development.json`; `src/Lancamentos/Processo.Lancamentos.Api/Properties/launchSettings.json`; `src/Lancamentos/Processo.Lancamentos.Aplicacao/Processo.Lancamentos.Aplicacao.csproj`; `src/Lancamentos/Processo.Lancamentos.Aplicacao/MarcadorAplicacao.cs`; `src/Lancamentos/Processo.Lancamentos.Dominio/Processo.Lancamentos.Dominio.csproj`; `src/Lancamentos/Processo.Lancamentos.Dominio/MarcadorDominio.cs`; `src/Lancamentos/Processo.Lancamentos.Infraestrutura/Processo.Lancamentos.Infraestrutura.csproj`; `src/Lancamentos/Processo.Lancamentos.Infraestrutura/MarcadorInfraestrutura.cs`; `src/ConsolidadoDiario/Processo.ConsolidadoDiario.Api/Processo.ConsolidadoDiario.Api.csproj`; `src/ConsolidadoDiario/Processo.ConsolidadoDiario.Api/Program.cs`; `src/ConsolidadoDiario/Processo.ConsolidadoDiario.Api/MarcadorApi.cs`; `src/ConsolidadoDiario/Processo.ConsolidadoDiario.Api/appsettings.json`; `src/ConsolidadoDiario/Processo.ConsolidadoDiario.Api/appsettings.Development.json`; `src/ConsolidadoDiario/Processo.ConsolidadoDiario.Api/Properties/launchSettings.json`; `src/ConsolidadoDiario/Processo.ConsolidadoDiario.Aplicacao/Processo.ConsolidadoDiario.Aplicacao.csproj`; `src/ConsolidadoDiario/Processo.ConsolidadoDiario.Aplicacao/MarcadorAplicacao.cs`; `src/ConsolidadoDiario/Processo.ConsolidadoDiario.Dominio/Processo.ConsolidadoDiario.Dominio.csproj`; `src/ConsolidadoDiario/Processo.ConsolidadoDiario.Dominio/MarcadorDominio.cs`; `src/ConsolidadoDiario/Processo.ConsolidadoDiario.Infraestrutura/Processo.ConsolidadoDiario.Infraestrutura.csproj`; `src/ConsolidadoDiario/Processo.ConsolidadoDiario.Infraestrutura/MarcadorInfraestrutura.cs`; `src/ConsolidadoDiario/Processo.ConsolidadoDiario.Processador/Processo.ConsolidadoDiario.Processador.csproj`; `src/ConsolidadoDiario/Processo.ConsolidadoDiario.Processador/Program.cs`; `src/ConsolidadoDiario/Processo.ConsolidadoDiario.Processador/ProcessadorLancamentoRegistrado.cs`; `src/ConsolidadoDiario/Processo.ConsolidadoDiario.Processador/MarcadorProcessador.cs`; `src/ConsolidadoDiario/Processo.ConsolidadoDiario.Processador/appsettings.json`; `src/ConsolidadoDiario/Processo.ConsolidadoDiario.Processador/appsettings.Development.json`; `src/ConsolidadoDiario/Processo.ConsolidadoDiario.Processador/Properties/launchSettings.json`; `tests/Lancamentos/Processo.Lancamentos.Testes.Unitarios/Processo.Lancamentos.Testes.Unitarios.csproj`; `tests/Lancamentos/Processo.Lancamentos.Testes.Unitarios/EstruturaInicialTests.cs`; `tests/Lancamentos/Processo.Lancamentos.Testes.Unitarios/GlobalUsings.cs`; `tests/Lancamentos/Processo.Lancamentos.Testes.Integracao/Processo.Lancamentos.Testes.Integracao.csproj`; `tests/Lancamentos/Processo.Lancamentos.Testes.Integracao/EstruturaInicialTests.cs`; `tests/Lancamentos/Processo.Lancamentos.Testes.Integracao/GlobalUsings.cs`; `tests/ConsolidadoDiario/Processo.ConsolidadoDiario.Testes.Unitarios/Processo.ConsolidadoDiario.Testes.Unitarios.csproj`; `tests/ConsolidadoDiario/Processo.ConsolidadoDiario.Testes.Unitarios/EstruturaInicialTests.cs`; `tests/ConsolidadoDiario/Processo.ConsolidadoDiario.Testes.Unitarios/GlobalUsings.cs`; `tests/ConsolidadoDiario/Processo.ConsolidadoDiario.Testes.Integracao/Processo.ConsolidadoDiario.Testes.Integracao.csproj`; `tests/ConsolidadoDiario/Processo.ConsolidadoDiario.Testes.Integracao/EstruturaInicialTests.cs`; `tests/ConsolidadoDiario/Processo.ConsolidadoDiario.Testes.Integracao/GlobalUsings.cs`; `docs/REGISTRO_DE_EXECUCAO.md`.
- Testes executados: `dotnet test Processo.sln -c Release --no-build --logger 'console;verbosity=minimal'` com 4 projetos de teste aprovados; validacao complementar com `dotnet restore Processo.sln` e `dotnet build Processo.sln -c Release --no-restore`.
- Decisoes tomadas: usar o prefixo de namespace `Processo`; separar testes unitarios e de integracao por contexto; manter APIs e processador com bootstrap minimo e classes marcadoras para permitir evolucao incremental sem acoplamento prematuro; expor `/health` desde a base inicial; remover placeholders `Class1` e `UnitTest1` dos templates.
- Pendencias: nenhuma dentro do escopo da T02; a solution foi deixada propositalmente com estrutura minima, sem implementacao de regras de dominio ou infraestrutura real.
- Proxima tarefa recomendada: T03 - Implementar o dominio de Lancamentos.

## T03 - Implementar o dominio de Lancamentos

- Status: concluida
- Data: 2026-03-17
- Objetivo da tarefa: modelar as regras centrais do servico transacional de `Lancamentos` com DDD e cobrir o dominio e a aplicacao com testes unitarios.
- O que foi feito: implementados os objetos de valor `TipoLancamento`, `ValorMonetario` e `DataLancamento`; criada a entidade `Lancamento` com criacao imutavel e validacao de `RegistradoEmUtc` em UTC; adicionada a excecao de dominio `ExcecaoDominio`; criadas as abstracoes `ILancamentosRepositorio` e `IRelogioUtc`; implementados os casos de uso `RegistrarLancamentoCasoDeUso` e `ConsultarLancamentoPorIdCasoDeUso`, alem do contrato de saida `LancamentoDto` e do comando `RegistrarLancamentoComando`; substituido o teste inicial de estrutura por testes unitarios reais de dominio e aplicacao, com doubles em memoria para repositorio e relogio.
- Arquivos criados ou alterados: `src/Lancamentos/Processo.Lancamentos.Dominio/Excecoes/ExcecaoDominio.cs`; `src/Lancamentos/Processo.Lancamentos.Dominio/ObjetosDeValor/TipoLancamento.cs`; `src/Lancamentos/Processo.Lancamentos.Dominio/ObjetosDeValor/ValorMonetario.cs`; `src/Lancamentos/Processo.Lancamentos.Dominio/ObjetosDeValor/DataLancamento.cs`; `src/Lancamentos/Processo.Lancamentos.Dominio/Entidades/Lancamento.cs`; `src/Lancamentos/Processo.Lancamentos.Aplicacao/Abstracoes/ILancamentosRepositorio.cs`; `src/Lancamentos/Processo.Lancamentos.Aplicacao/Abstracoes/IRelogioUtc.cs`; `src/Lancamentos/Processo.Lancamentos.Aplicacao/CasosDeUso/LancamentoDto.cs`; `src/Lancamentos/Processo.Lancamentos.Aplicacao/CasosDeUso/RegistrarLancamento/RegistrarLancamentoComando.cs`; `src/Lancamentos/Processo.Lancamentos.Aplicacao/CasosDeUso/RegistrarLancamento/RegistrarLancamentoCasoDeUso.cs`; `src/Lancamentos/Processo.Lancamentos.Aplicacao/CasosDeUso/ConsultarLancamento/ConsultarLancamentoPorIdCasoDeUso.cs`; `tests/Lancamentos/Processo.Lancamentos.Testes.Unitarios/Dominio/TipoLancamentoTests.cs`; `tests/Lancamentos/Processo.Lancamentos.Testes.Unitarios/Dominio/ValorMonetarioTests.cs`; `tests/Lancamentos/Processo.Lancamentos.Testes.Unitarios/Dominio/DataLancamentoTests.cs`; `tests/Lancamentos/Processo.Lancamentos.Testes.Unitarios/Dominio/LancamentoTests.cs`; `tests/Lancamentos/Processo.Lancamentos.Testes.Unitarios/Aplicacao/RegistrarLancamentoCasoDeUsoTests.cs`; `tests/Lancamentos/Processo.Lancamentos.Testes.Unitarios/Aplicacao/ConsultarLancamentoPorIdCasoDeUsoTests.cs`; `tests/Lancamentos/Processo.Lancamentos.Testes.Unitarios/Doubles/LancamentosRepositorioEmMemoria.cs`; `tests/Lancamentos/Processo.Lancamentos.Testes.Unitarios/Doubles/RelogioUtcFixo.cs`; `tests/Lancamentos/Processo.Lancamentos.Testes.Unitarios/EstruturaInicialTests.cs` removido; `docs/REGISTRO_DE_EXECUCAO.md`.
- Testes executados: `dotnet build Processo.sln -c Release`; `dotnet test Processo.sln -c Release --no-build --logger 'console;verbosity=minimal'` com 24 testes aprovados no total, sendo 21 no projeto `Processo.Lancamentos.Testes.Unitarios`.
- Decisoes tomadas: manter `TipoLancamento` como objeto de valor com valores canonicos `Credito` e `Debito`; rejeitar `RegistradoEmUtc` fora de UTC para preservar rastreabilidade consistente; aceitar valores monetarios positivos com ate duas casas decimais; fazer a consulta por id retornar `null` quando o lancamento nao existir; manter repositorio e relogio como abstracoes da camada de aplicacao para a infraestrutura implementar na T04.
- Pendencias: nenhuma dentro do escopo da T03; ainda falta conectar persistencia real e expor os casos de uso via HTTP, o que fica para a T04.
- Proxima tarefa recomendada: T04 - Implementar persistencia e API de Lancamentos.
