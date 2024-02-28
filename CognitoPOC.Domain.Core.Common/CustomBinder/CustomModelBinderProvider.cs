using System.Reflection;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace CognitoPOC.Domain.Core.Common.CustomBinder;

public class CustomModelBinderProvider(
    IList<IInputFormatter> formatters,
    IHttpRequestStreamReaderFactory readerFactory,
    ILogger logger)
    : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        try
        {
            var customProperties = context.Metadata.ModelType
                .GetProperties()
                .Where(p => p.GetCustomAttribute(typeof(CustomAttributeBinder)) != null)
                .ToArray();
            if (customProperties.Length != 0)
                return new CustomModelBinder(formatters, readerFactory, customProperties);
        }
        catch(Exception exc)
        {
            logger.LogError(exc, "Error binging Model");
        }
        return null!;
    }
}