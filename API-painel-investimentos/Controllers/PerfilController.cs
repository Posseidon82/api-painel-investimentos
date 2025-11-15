using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API_painel_investimentos.DTO;

namespace API_painel_investimentos.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Authorize]
public class PerfilController : ControllerBase
{
    /// <summary>
    /// Define o perfil de investimento do usuário
    /// </summary>
    /// <param name="request">Dados do perfil de investimento</param>
    /// <returns>Confirmação da operação</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<DefinirPerfilResponse> DefinirPerfil([FromBody] DefinirPerfilRequest request)
    {
        try
        {
            // Validação do modelo
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // TODO: Implementar lógica de negócio aqui
            // - Salvar no banco de dados
            // - Validar regras de negócio
            // - Integrar com serviços externos

            var response = new DefinirPerfilResponse
            {
                Sucesso = true,
                Mensagem = "Perfil definido com sucesso",
                PerfilId = Guid.NewGuid(), // Exemplo
                DataAtualizacao = DateTime.UtcNow
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            // Log da exceção (implementar logging adequado)
            Console.WriteLine($"Erro ao definir perfil: {ex.Message}");

            return StatusCode(500, new DefinirPerfilResponse
            {
                Sucesso = false,
                Mensagem = "Erro interno ao processar a solicitação"
            });
        }
    }

    /// <summary>
    /// Obtém o perfil de investimento do usuário atual
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ObterPerfilResponse> ObterPerfil()
    {
        try
        {
            // TODO: Buscar perfil do usuário atual no banco
            // var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var perfil = new ObterPerfilResponse
            {
                TipoPerfil = "Moderado",
                DataDefinicao = DateTime.UtcNow.AddDays(-30),
                Descricao = "Perfil de investimento moderado"
            };

            return Ok(perfil);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao obter perfil: {ex.Message}");
            return StatusCode(500, "Erro interno ao buscar perfil");
        }
    }
}
