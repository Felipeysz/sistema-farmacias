using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SistemaFarmacias.Api.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
{
    private const string HeaderName = "X-Api-Key";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var expectedKey = config["N8n:BackendApiKey"];

        if (string.IsNullOrEmpty(expectedKey))
        {
            context.Result = new ObjectResult("API Key não configurada no servidor.")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var providedKey)
            || providedKey != expectedKey)
        {
            context.Result = new UnauthorizedObjectResult("API Key inválida ou ausente.");
            return;
        }

        await next();
    }
}