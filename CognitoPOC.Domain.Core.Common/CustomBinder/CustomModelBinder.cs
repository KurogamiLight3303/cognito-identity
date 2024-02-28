using System.Reflection;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace CognitoPOC.Domain.Core.Common.CustomBinder;

public class CustomModelBinder : IModelBinder
{
    private readonly BodyModelBinder _defaultBinder;
    private readonly PropertyInfo[] _customProperties;

    public CustomModelBinder(
        IList<IInputFormatter> formatters, 
        IHttpRequestStreamReaderFactory readerFactory, 
        PropertyInfo[] customProp)
    {
        _customProperties = customProp;
        _defaultBinder = new(formatters, readerFactory);
    }

    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        await _defaultBinder.BindModelAsync(bindingContext);

        if (bindingContext.Result.IsModelSet)
        {
            var data = bindingContext.Result.Model;
            if (data != null)
            {
                foreach (var property in _customProperties)
                {
                    var value = bindingContext.ValueProvider.GetValue(property.Name).FirstValue;
                    if (value != null)
                    {
                        if (property.PropertyType == typeof(Guid))
                            property.SetValue(data, Guid.Parse(value));
                        else
                            property.SetValue(data, value);
                    }
                }
                bindingContext.Result = ModelBindingResult.Success(data);
            }
        }
    }
}