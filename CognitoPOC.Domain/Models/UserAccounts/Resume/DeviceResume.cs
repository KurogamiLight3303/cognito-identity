using CognitoPOC.Domain.Models.UserAccounts.DomainValues;
using CognitoPOC.Domain.Common.Models;
using UAParser;

namespace CognitoPOC.Domain.Models.UserAccounts.Resume;

public class DeviceResume : DomainResume<UserDevicesObject>
{
    public Guid Id { get; set; }
    public string? Name { get; set; }

    public string? ShortName
    {
        get
        {
            var parser = Parser.GetDefault();
            if (string.IsNullOrEmpty(Name))
                return string.Empty;
            var c = parser.Parse(Name);

            return c.Device.Family == "Other" 
                ? $"{c.OS.Family} ({c.UA.Family})"
                : $"{c.Device.Family} {c.Device.Model} ({c.UA.Family})";
        }
    }

    public string? IpAddress { get; set; }
    public DateTime LastAuth { get; set; }
    public UserAgentEnum UserAgent { get; set; }
}