using API.Attributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API.Swagger;

public class IdempotencyKeyOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAttribute = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<IdempotentAttribute>()
            .Any();

        if (!hasAttribute)
        {
            return;
        }

        operation.Parameters ??= new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "Idempotency-Key",
            In = ParameterLocation.Header,
            Required = false,
            Description = "Unique key for safely retrying requests",
            Schema = new OpenApiSchema
            {
                Type = "string"
            }
        });
    }
}
