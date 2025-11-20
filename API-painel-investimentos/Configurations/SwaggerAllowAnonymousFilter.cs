using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API_painel_investimentos.Configurations;

public class SwaggerAllowAnonymousFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Verifica se o método ou controller tem [AllowAnonymous]
        var hasAllowAnonymous = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<AllowAnonymousAttribute>()
            .Any() ||
            context.MethodInfo.DeclaringType?
            .GetCustomAttributes(true)
            .OfType<AllowAnonymousAttribute>()
            .Any() == true;

        // Se tem [AllowAnonymous], remove o requisito de segurança
        if (hasAllowAnonymous && operation.Security != null)
        {
            operation.Security.Clear();
        }
    }
}
