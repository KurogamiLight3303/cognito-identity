using Mediator;
using CognitoPOC.Domain.Common.Models;

namespace CognitoPOC.Domain.Core.Common.Commands;

public interface ICommandBase<TResult> : ICustomCommandBase<OperationResultValue<TResult>>
{
}

public interface ICustomCommandBase<out TCustom> : IRequest<TCustom>
{
}