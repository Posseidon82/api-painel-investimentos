using API_painel_investimentos.DTO;
using Microsoft.AspNetCore.Mvc;

namespace API_painel_investimentos.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvestorProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvestorProfileController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("calculate")]
    [ProducesResponseType(typeof(ProfileResultDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CalculateProfile([FromBody] CalculateProfileRequest request)
    {
        try
        {
            var command = new CalculateProfileCommand(request.UserId, request.Answers);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(ProfileResultDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProfile(Guid userId)
    {
        try
        {
            var query = new GetProfileQuery(userId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}
