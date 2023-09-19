using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace APIContagem.RateLimiting;

public class ClientIdHeaderOperationFilter : IOperationFilter
{
    private readonly IConfiguration _configuration;

    public ClientIdHeaderOperationFilter(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null)
            operation.Parameters = new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = _configuration["AspNetCoreRateLimit:ClientIdHeader"],
            In = ParameterLocation.Header,
            Required = true,
            Schema = new OpenApiSchema
            {
                Type = "string"
            }
        });
    }
}