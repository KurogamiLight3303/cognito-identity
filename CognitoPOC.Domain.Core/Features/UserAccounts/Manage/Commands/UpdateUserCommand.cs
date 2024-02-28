using System.Text.Json.Serialization;
using CognitoPOC.Domain.Core.Common.CustomBinder;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Manage.Commands;

public class UpdateUserCommand : BaseUpdateUserCommand
{
    [JsonIgnore]
    [CustomAttributeBinder]
    public Guid Id { get; set; }
}