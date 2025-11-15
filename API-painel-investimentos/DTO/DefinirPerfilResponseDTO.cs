using System.ComponentModel.DataAnnotations;

namespace API_painel_investimentos.DTO;

public class DefinirPerfilResponse
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public Guid? PerfilId { get; set; }
    public DateTime DataAtualizacao { get; set; }
}
