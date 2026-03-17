using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Lancamentos.Api.Autenticacao;

public sealed class AutenticacaoJwtOpcoes
{
    public const string Secao = "Autenticacao";

    [Required]
    public string Issuer { get; init; } = "fluxodecaixa-local";

    [Required]
    public string Audience { get; init; } = "fluxodecaixa-clientes";

    [Required]
    [MinLength(32)]
    public string ChaveAssinatura { get; init; } = string.Empty;

    [Range(1, 1440)]
    public int ExpiracaoEmMinutos { get; init; } = 60;

    public bool PossuiChaveAssinaturaValida()
    {
        return Encoding.UTF8.GetByteCount(ChaveAssinatura) >= 32;
    }

    public SymmetricSecurityKey CriarChaveSeguranca()
    {
        var chave = Encoding.UTF8.GetBytes(ChaveAssinatura);

        if (!PossuiChaveAssinaturaValida())
        {
            throw new InvalidOperationException(
                "A chave de assinatura JWT deve possuir ao menos 32 bytes em UTF-8.");
        }

        return new SymmetricSecurityKey(chave);
    }
}
