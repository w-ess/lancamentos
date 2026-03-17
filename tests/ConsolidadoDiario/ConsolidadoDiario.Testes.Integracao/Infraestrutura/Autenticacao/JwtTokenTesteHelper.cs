using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ConsolidadoDiario.Testes.Integracao.Infraestrutura;

internal static class JwtTokenTesteHelper
{
    public static string GerarToken(
        string issuer,
        string audience,
        string chaveAssinatura,
        IReadOnlyCollection<string> escopos,
        DateTimeOffset emitidoEmUtc,
        DateTimeOffset expiraEmUtc,
        string subject = "usuario-teste")
    {
        var header = new Dictionary<string, object?>
        {
            ["alg"] = "HS256",
            ["typ"] = "JWT"
        };

        var payload = new Dictionary<string, object?>
        {
            ["iss"] = issuer,
            ["aud"] = audience,
            ["sub"] = subject,
            ["iat"] = emitidoEmUtc.ToUnixTimeSeconds(),
            ["nbf"] = emitidoEmUtc.ToUnixTimeSeconds(),
            ["exp"] = expiraEmUtc.ToUnixTimeSeconds(),
            ["jti"] = Guid.NewGuid().ToString("N"),
            ["scope"] = string.Join(' ', escopos)
        };

        var headerCodificado = CodificarBase64Url(JsonSerializer.SerializeToUtf8Bytes(header));
        var payloadCodificado = CodificarBase64Url(JsonSerializer.SerializeToUtf8Bytes(payload));
        var conteudoAssinado = Encoding.UTF8.GetBytes($"{headerCodificado}.{payloadCodificado}");

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(chaveAssinatura));
        var assinatura = CodificarBase64Url(hmac.ComputeHash(conteudoAssinado));

        return $"{headerCodificado}.{payloadCodificado}.{assinatura}";
    }

    private static string CodificarBase64Url(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
