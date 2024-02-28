using AutoMapper;
using CognitoPOC.Domain.Common;
using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Core.Common.Commands;
using CognitoPOC.Domain.Core.Features.UserAccounts.Manage.Commands;
using CognitoPOC.Domain.Internationalization;
using CognitoPOC.Domain.Models.UserAccounts;
using CognitoPOC.Domain.Models.UserAccounts.Resume;
using CognitoPOC.Domain.Repositories.UserAccounts;
using CognitoPOC.Domain.Services;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Manage.Handlers;

public class CommandHandler(
    IUserAccountCommandRepository repository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IAuthenticationSignUpService service)
    :
        ICommandHandler<CreateUserCommand, UserAccountBaseResume>,
        ICommandHandler<UpdateUserCommand, UserAccountBaseResume>,
        ICommandHandler<DisableUserCommand>,
        ICommandHandler<DisableMultipleUserCommand>
{
    public async ValueTask<OperationResultValue<UserAccountBaseResume>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new UserAccountObject()
        {
            IsActive = true,
            Username = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = new VerifiableValue<string>()
            {
                Verified = true,
                Value = request.Email
            },
            PhoneNumber = new VerifiableValue<string>()
            {
                Verified = true,
                Value = request.Phone
            }
        };
        await repository.AddAsync(user, cancellationToken);
        var result = await service.Subscribe(user, request.Password!, cancellationToken);
        if (result.Success)
            return await unitOfWork.CommitAsync(cancellationToken)
                ? new(true, mapper.Map<UserAccountBaseResume>(user))
                : new(false, I18n.UnknownError);
        await service.UnSubscribe(user, cancellationToken);
        return new(false, I18n.UnknownError);
    }

    public async ValueTask<OperationResultValue<UserAccountBaseResume>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await repository.FindAsync(p => p.Id == request.Id, cancellationToken);
        if (user == null)
            return new(false, I18n.UserNotFound);
        user.Email = new VerifiableValue<string>()
        {
            Verified = true,
            Value = request.Email
        };
        user.PhoneNumber = new VerifiableValue<string>()
        {
            Verified = true,
            Value = request.Phone
        };
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;

        repository.Update(user);
        return await unitOfWork.CommitAsync(cancellationToken)
            ? new(true, mapper.Map<UserAccountBaseResume>(user))
            : new(false, I18n.UnknownError);
    }

    public async ValueTask<OperationResultValue> Handle(DisableUserCommand request, CancellationToken cancellationToken)
    {
        var user = await repository.FindAsync(p => p.Id == request.Id, cancellationToken);
        if (user == null)
            return new(false, I18n.UserNotFound);
        user.IsActive = false;

        repository.Update(user);
        return await unitOfWork.CommitAsync(cancellationToken)
            ? new(true, I18n.UserIsDisabled)
            : new(false, I18n.UnknownError);
    }

    public async ValueTask<OperationResultValue> Handle(DisableMultipleUserCommand request, CancellationToken cancellationToken)
    {
        if (request.data == null || request.data.Length == 0)
            return new(true, I18n.UserIsDisabled);
        return await unitOfWork.ExecuteInTransactionAsync<OperationResultValue>(async (cc) =>
        {
            foreach (var user in await repository.AllAsync(p
                         => request.data.Contains(p.Id) && p.IsActive, cc))
            {
                user.IsActive = false;
                repository.Update(user);
            }

            return await unitOfWork.CommitAsync(cc)
                ? new(true, I18n.UserIsDisabled)
                : new(false, I18n.UnknownError);
        }, cancellationToken);
    }
}