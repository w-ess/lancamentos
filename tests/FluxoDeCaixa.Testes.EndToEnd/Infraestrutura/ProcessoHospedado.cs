namespace FluxoDeCaixa.Testes.EndToEnd.Infraestrutura;

internal sealed class ProcessoHospedado : IAsyncDisposable
{
    private readonly Process _processo;
    private readonly StringBuilder _saida = new();
    private readonly StringBuilder _erros = new();

    public ProcessoHospedado(Process processo)
    {
        _processo = processo;
        _processo.OutputDataReceived += (_, args) =>
        {
            if (args.Data is not null)
            {
                lock (_saida)
                {
                    _saida.AppendLine(args.Data);
                }
            }
        };
        _processo.ErrorDataReceived += (_, args) =>
        {
            if (args.Data is not null)
            {
                lock (_erros)
                {
                    _erros.AppendLine(args.Data);
                }
            }
        };
    }

    public int? ExitCode => _processo.HasExited ? _processo.ExitCode : null;

    public string ObterDiagnostico()
    {
        lock (_saida)
        lock (_erros)
        {
            return $"""
                    Saida:
                    {_saida}

                    Erros:
                    {_erros}
                    """;
        }
    }

    public async Task IniciarAsync(CancellationToken cancellationToken)
    {
        if (!_processo.Start())
        {
            throw new InvalidOperationException("Falha ao iniciar processo hospedado.");
        }

        _processo.BeginOutputReadLine();
        _processo.BeginErrorReadLine();

        await Task.Yield();

        if (_processo.HasExited)
        {
            throw new InvalidOperationException(
                $"O processo terminou imediatamente com codigo {_processo.ExitCode}.{Environment.NewLine}{ObterDiagnostico()}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (!_processo.HasExited)
            {
                _processo.Kill(entireProcessTree: true);
                await _processo.WaitForExitAsync();
            }
        }
        catch
        {
        }
        finally
        {
            _processo.Dispose();
        }
    }
}
