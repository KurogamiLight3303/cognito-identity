using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CognitoPOC.Infrastructure.Middleware;

public abstract class SwaggerDeviceOperationFilter : IOperationFilter
{
    protected bool DeviceIdRequired { get; set; }
    public virtual void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        operation.Parameters.Add(new()
        {
            Name = "Device_Id",
            In = ParameterLocation.Header,
            Required = DeviceIdRequired,
            Schema = new()
            {
                Type = "string" 
            }
        });
    }
}