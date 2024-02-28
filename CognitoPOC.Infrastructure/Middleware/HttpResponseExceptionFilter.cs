using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Internationalization;
using CognitoPOC.Infrastructure.Extensions;

namespace CognitoPOC.Infrastructure.Middleware;

public class HttpResponseExceptionFilter : IActionFilter, IOrderedFilter
{
    private ILogger? _logger;

    public int Order => int.MaxValue - 10;

    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        _logger ??= (ILogger?)context.HttpContext.RequestServices.GetService(typeof(ILogger));
        if (context.Exception is ValidationException exception)
        {
            try
            {
                var message = exception.Errors.FirstOrDefault()?.ErrorMessage ?? I18n.UnknownError;
                _logger?.LogWarning("Validation Error {[Exception]}",
                    exception);
                context.Result = new ObjectResult(new OperationResultValue(false, message));
                context.ExceptionHandled = true;
            }
            catch (Exception exc)
            {
                using (_logger?.BeginScope(GetData(context.HttpContext)))
                {
                    _logger?.LogError(exc, "Unhandled exception from handling validation error {[Exception]}",
                        context.Exception);
                }
            }
        }
        else if (context.Exception != null)
        {
            using (_logger?.BeginScope(GetData(context.HttpContext)))
            {
                _logger?.LogError(context.Exception,"Unhandled exception");
            }
            context.Result = new ObjectResult(new OperationResultValue(false, I18n.UnknownError));
            context.ExceptionHandled = true;
        }
    }
    
    private Dictionary<string, object> GetData(HttpContext context)
    {
        return new Dictionary<string, object>()
        {
            ["IpAddress"] = context.Request.GetIpAddress(),
            ["UserAgent"] = context.Request.GetUserAgent() ?? string.Empty,
            ["Username"] = context.GetUsername() ?? string.Empty,
        };
    }
}