namespace _4.Algorithms;

public sealed class MultiplicationResult
{
    public MultiplicationResult(
        string value,
        long elementaryMultiplications,
        long additionsWithShifts)
    {
        Value = value;
        ElementaryMultiplications = elementaryMultiplications;
        AdditionsWithShifts = additionsWithShifts;
    }

    public string Value { get; }
    public long ElementaryMultiplications { get; }
    public long AdditionsWithShifts { get; }
}
