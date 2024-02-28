namespace CognitoPOC.Domain.Common.Models;

public class OperationResultValue
{
    public bool Success { get; }
    public string Message { get; }

    public OperationResultValue(bool success, string? message = null)
    {
        Success = success;
        Message = message ?? string.Empty;
    }
}
public class OperationResultValue<TData> : OperationResultValue
{
    public OperationResultValue(bool success, TData? data, string? message = null) : base(success, message)
    {
        Data = data;
    }
    public OperationResultValue(bool success, string? message = null) : base(success, message)
    {;
    }
    public TData? Data { get; }
}