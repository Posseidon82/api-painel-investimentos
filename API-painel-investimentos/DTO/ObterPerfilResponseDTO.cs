namespace API_painel_investimentos.DTO;

public class ObterPerfilResponse
{
    public string TipoPerfil { get; set; } = string.Empty;
    public int NivelRisco { get; set; }
    public string Objetivo { get; set; } = string.Empty;
    public string? Observacoes { get; set; }
    public DateTime DataDefinicao { get; set; }
    public string Descricao { get; set; } = string.Empty;
}
