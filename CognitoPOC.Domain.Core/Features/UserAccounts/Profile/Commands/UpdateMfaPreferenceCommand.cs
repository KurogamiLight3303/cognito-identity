using CognitoPOC.Domain.Core.Common.Commands;
using CognitoPOC.Domain.Models.UserAccounts.DomainValues;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Profile.Commands;

public class UpdateMfaPreferenceCommand : CommandBase
{
    public MfaTypesEnum DefaultMfa { get; init; }
}