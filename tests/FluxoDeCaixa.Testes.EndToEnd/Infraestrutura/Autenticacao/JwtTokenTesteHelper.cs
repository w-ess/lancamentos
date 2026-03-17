namespace FluxoDeCaixa.Testes.EndToEnd.Infraestrutura.Autenticacao;

internal static class JwtTokenTesteHelper
{
    public static string GerarToken(
        string issuer,
        string audience,
        string chaveAssinatura,
        IEnumerable<string> escopos,
        DateTimeOffset emitidoEmUtc,
        DateTimeOffset expiraEmUtc)
    {
        var credenciais = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveAssinatura)),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, "usuario-end-to-end"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new("scope", string.Join(' ', escopos))
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: emitidoEmUtc.UtcDateTime,
            expires: expiraEmUtc.UtcDateTime,
            signingCredentials: credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
