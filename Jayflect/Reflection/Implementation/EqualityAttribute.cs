namespace Jay.Reflection.Implementation;

[AttributeUsage(validOn: AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class EqualityAttribute : Attribute
{
    public bool ParticipatesInEquality { get; init; }

    public EqualityAttribute(bool participatesInEquality = true)
    {
        this.ParticipatesInEquality = participatesInEquality;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return ParticipatesInEquality ? "Participates" : "Doesn't";
    }
}