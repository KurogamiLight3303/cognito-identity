using System.Text.Json.Serialization;
using CognitoPOC.Domain.Core.Common.Commands;
using CognitoPOC.Domain.Core.Common.CustomBinder;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Manage.Commands;

public class UpdateUserRolesCommand : CommandBase
{
    [JsonIgnore]
    [CustomAttributeBinder]
    public Guid Id { get; set; }
    public string[]? Roles { get; init; }
}