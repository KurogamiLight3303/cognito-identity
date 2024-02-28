using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CognitoPOC.Infrastructure.Configurations;
using StackExchange.Redis;

namespace CognitoPOC.Infrastructure.Persistence.Repositories;

public class BaseRedisRepository
{
    private readonly IConnectionMultiplexer? _connection;
    private IDatabase? _db;
    private readonly ILogger _logger;
    private IDatabase Database => _db ??= _connection?.GetDatabase() 
                                           ?? throw new Exception("Unable to connect to cache database");
    protected BaseRedisRepository(RedisConfiguration? configuration, ILogger logger)
    {
        _connection = ConnectionFactory.GetConnection(configuration);
        _logger = logger;
    }
    protected BaseRedisRepository(IOptions<RedisConfiguration> configuration, ILogger logger)
        : this(configuration.Value, logger)
    {
    }
    protected async Task<TData?> Get<TData>(string key)
    {
        TData? result = default;
        try
        {
            var value = await Database.StringGetAsync(key);
            if (value.HasValue) 
                result = JsonSerializer.Deserialize<TData>(value!);
        }
        catch (Exception exc)
        {
            _logger.LogError(exc, "Unable to retrieve key {Key}", key);
        }
            
        return result;
    }

    protected Task<bool> SetData<TData>(string key, TData data, DateTimeOffset expirationTime)
    {
        return SetData(key, data, expirationTime.DateTime.Subtract(DateTime.Now));
    }
    protected async Task<bool> SetData<TData>(string key, TData data, TimeSpan expirationTime)
    {
        var result = false;
        try
        {
            result = await Database.StringSetAsync(key, JsonSerializer.Serialize(data), expirationTime);
        }
        catch (Exception exc)
        {
            _logger.LogError(exc, "Unable to persist key {Key}", key);
        }
        return result;
    }
        
    protected async Task<bool> RemoveData(string key) 
    {
        var result = true;
        try
        {
            if (await Database.KeyExistsAsync(key)) 
                result = await Database.KeyDeleteAsync(key);
        }
        catch (Exception exc)
        {
            _logger.LogError(exc, "Unable to remove {Key}", key);
        }
        return result;
    }

    protected async Task<bool> ContainsKey(string key)
    {
        var result = false;
        try
        {
            result = await Database.KeyExistsAsync(key);
        }
        catch (Exception exc)
        {
            _logger.LogError(exc, "Unable to check key {Key}", key);
        }
        return result;
    }
}

internal static class ConnectionFactory
{
    internal static Func<Task<IConnectionMultiplexer>>? GetConnectionAsync(RedisConfiguration? configuration)
    {
        if (configuration == null)
            return null;
        var config = ConfigurationOptions.Parse(configuration.ConnectionString!);
        config.DefaultDatabase = configuration.Database;
        return async () => await ConnectionMultiplexer.ConnectAsync(config);
    }
    internal static IConnectionMultiplexer? GetConnection(RedisConfiguration? configuration)
    {
        if (configuration == null)
            return null;
        var config = ConfigurationOptions.Parse(configuration.ConnectionString!);
        config.DefaultDatabase = configuration.Database;
        return ConnectionMultiplexer.Connect(config);
    }
}