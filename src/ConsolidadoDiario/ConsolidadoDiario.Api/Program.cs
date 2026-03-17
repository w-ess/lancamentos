var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    Servico = "ConsolidadoDiario",
    Status = "Inicializado"
}));

app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy"
}));

app.Run();

public partial class Program;
