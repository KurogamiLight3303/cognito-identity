using Mediator;
using Microsoft.Extensions.DependencyInjection;
using CognitoPOC.Domain.Core.Common;
using System.Reflection;
using FluentValidation;

namespace CognitoPOC.Domain.Core;

public static class CoreConfiguration
{
    public static IServiceCollection ConfigureMediator(this IServiceCollection serviceCollection)
    {
        return serviceCollection
                .AddMediator(p => p.ServiceLifetime = ServiceLifetime.Scoped)
                .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly())
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(FluentValidationPipeline<,>))
            ;
    }
}