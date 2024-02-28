using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CognitoPOC.Infrastructure.Middleware;

public class SwaggerDeviceAuthOperationFilter : SwaggerDeviceOperationFilter
{
    public override void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();
        if (context.MethodInfo.GetCustomAttribute(typeof(SwaggerDeviceAuthAttribute)) is SwaggerDeviceAuthAttribute
            attribute)
        {
            DeviceIdRequired = true;
            operation.Parameters.Add(new()
            {
                Name = "Device_Password",
                In = ParameterLocation.Header,
                Required = true,
                Schema = new()
                {
                    Type = "string" 
                }
            });
        }
        else
            DeviceIdRequired = false;
        base.Apply(operation, context);
    }
}

public class SwaggerDeviceAuthAttribute : Attribute
{
    
}