namespace FluxoDeCaixa.Testes.EndToEnd.Infraestrutura.Autenticacao;

internal static class JwtTokenTesteHelper
{
    public static string GerarToken(
        string chaveAssinatura,
        IEnumerable<string> escopos,
        DateTimeOffset emitido,
        DateTimeOffset expira)
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
            claims: claims,
            notBefore: emitido.UtcDateTime,
            expires: expira.UtcDateTime,
            signingCredentials: credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
