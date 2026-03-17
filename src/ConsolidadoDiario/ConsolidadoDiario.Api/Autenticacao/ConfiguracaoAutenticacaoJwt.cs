using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ConsolidadoDiario.Api.Autenticacao;

public static class ConfiguracaoAutenticacaoJwt
{
    public static IServiceCollection AdicionarAutenticacaoJwtConsolidadoDiario(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<AutenticacaoJwtOpcoes>()
            .Bind(configuration.GetSection(AutenticacaoJwtOpcoes.Secao))
            .ValidateDataAnnotations()
            .Validate(
                opcoes => opcoes.PossuiChaveAssinaturaValida(),
                "A chave de assinatura JWT deve possuir ao menos 32 bytes.")
            .ValidateOnStart();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<AutenticacaoJwtOpcoes>>((configuracaoJwt, autenticacaoJwt) =>
            {
                var opcoes = autenticacaoJwt.Value;

                configuracaoJwt.RequireHttpsMetadata = false;
                configuracaoJwt.MapInboundClaims = false;
                configuracaoJwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = opcoes.Issuer,
                    ValidateAudience = true,
                    ValidAudience = opcoes.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = opcoes.CriarChaveSeguranca(),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(
                PoliticasAutorizacao.ConsolidadoLeitura,
                builder => builder.ExigirEscopo(PoliticasAutorizacao.ConsolidadoLeitura));

        return services;
    }

    private static AuthorizationPolicyBuilder ExigirEscopo(
        this AuthorizationPolicyBuilder builder,
        string escopoRequerido)
    {
        builder.RequireAuthenticatedUser();
        builder.RequireAssertion(context => PossuiEscopo(context.User, escopoRequerido));
        return builder;
    }

    private static bool PossuiEscopo(ClaimsPrincipal usuario, string escopoRequerido)
    {
        return usuario.Claims
            .Where(claim => claim.Type is "scope" or "scp")
            .SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Any(valor => string.Equals(valor, escopoRequerido, StringComparison.Ordinal));
    }
}
