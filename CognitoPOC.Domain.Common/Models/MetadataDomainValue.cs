using System.Text.Json;

namespace CognitoPOC.Domain.Common.Models;

public sealed class MetadataDomainValue : DomainValue
{
    public MetadataDomainValue()
    {
        _data = new();
    }

    public MetadataDomainValue(IEnumerable<KeyValuePair<string, object>> data)
    {
        _data = new(data);
    }
    public MetadataDomainValue(string data)
    {
        _data = JsonSerializer.Deserialize<Dictionary<string, object>>(data) ?? new ();
    }
    
    private bool _hasChanges;
    private readonly Dictionary<string, object> _data;
    
    public void SetValue<T>(string key, T value)
    {
        if (value != null)
        {
            _data[key] = value;
            _hasChanges = true;
        }
        else
            _data.Remove(key);
    }

    public T? GetValue<T>(string key)
    {
        if (!_data.ContainsKey(key)) return default;
        if (!typeof(T).IsClass && _data[key] is T result)
            return result;
        if (typeof(T).IsClass && typeof(T) == typeof(string) && _data[key] is T sResult)
            return sResult;
        object data;
        if (typeof(T).IsClass
            && typeof(T) != typeof(string)
            && (data = _data[key]) != null)
        {
            try
            {
                if (data.GetType() == typeof(T))
                    return (T)data;
                T? oResult;
                if((oResult = JsonSerializer.Deserialize<T>(data.ToString()!)) != null)
                    return oResult;
            }
            catch
            {
                return default;
            }
        }

        return default;
    }

    public string? GetValue(string key)
    {
        return _data.ContainsKey(key) 
            ? _data[key].ToString() : default;
    }

    public bool HasChanges() => _hasChanges;
    protected override IEnumerable<object> GetCompareFields()
    {
        return _data.Keys;
    }

    public override string ToString() 
        => JsonSerializer.Serialize(_data, typeof(Dictionary<string, object>), new JsonSerializerOptions());
}

public interface IMetadataContainer
{
    public MetadataDomainValue? Metadata { get; }
}