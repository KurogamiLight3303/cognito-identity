using FluentValidation;
using FluentValidation.Results;
using Mediator;

namespace CognitoPOC.Domain.Core.Common;

public class FluentValidationPipeline<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async ValueTask<TResponse> Handle(TRequest message, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        var failures = new List<ValidationFailure>();
        foreach (var validator in validators)
        {
            var r = await validator.ValidateAsync(message, cancellationToken);
            if(r?.Errors != null)
                failures.AddRange(r.Errors);
        }

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next(message, cancellationToken);
    }
}