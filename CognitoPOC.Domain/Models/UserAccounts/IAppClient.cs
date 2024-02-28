using CognitoPOC.Domain.Models.UserAccounts.DomainValues;

namespace CognitoPOC.Domain.Models.UserAccounts;

public interface IAppClient
{
    public string? ClientId { get; }
    public string? Secret { get; }
    public UserAgentEnum UserAgent { get; }
}