namespace CognitoPOC.Infrastructure.Configurations;

public class ElasticConfiguration
{
    public bool Debug { get; set; }
    public string? Url { get; set; }
    public string? DefaultIndex { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}