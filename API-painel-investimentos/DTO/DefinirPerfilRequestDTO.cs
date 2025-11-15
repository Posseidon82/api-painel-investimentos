using System.ComponentModel.DataAnnotations;

namespace API_painel_investimentos.DTO;

public class DefinirPerfilRequest
{
    [Required(ErrorMessage = "O tipo de perfil é obrigatório")]
    [StringLength(50, ErrorMessage = "Tipo de perfil deve ter no máximo 50 caracteres")]
    public string TipoPerfil { get; set; } = string.Empty;

    [Range(1, 100, ErrorMessage = "Nível de risco deve estar entre 1 e 100")]
    public int NivelRisco { get; set; }

    [Required(ErrorMessage = "Objetivo de investimento é obrigatório")]
    public string Objetivo { get; set; } = string.Empty;

    public string? Observacoes { get; set; }
}
