using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CognitoPOC.Domain.Common.Models;

namespace CognitoPOC.Domain.Core.Common;

public static class DomainCoreConfigurationExtensions
{
    public static IActionResult InvalidModelHandling(ActionContext context)
    {
        IEnumerable<string>? errors = null;
        try
        {
            errors = context
                .ModelState
                .Values
                .SelectMany(p => p.Errors)
                .Select(p => p.ErrorMessage);
        }
        catch (Exception exc)
        {
            var logger = context.HttpContext.RequestServices.GetService<ILogger>();
            logger?.LogError(exc, "Invalid Model");
        }
        return new OkObjectResult(
            new OperationResultValue(false, errors?.LastOrDefault() ?? "Invalid Model"));
    }
}