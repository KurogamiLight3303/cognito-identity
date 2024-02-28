namespace CognitoPOC.Domain.Common.Models;

public class VerifiableValue<TValue>
{
    public bool Verified { get; set; }
    public TValue? Value { get; set; }

    public void Verify()
    {
        Verified = true;
    }
}