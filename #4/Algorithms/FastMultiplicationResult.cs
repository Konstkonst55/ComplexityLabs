namespace _4.Algorithms;

public sealed class FastMultiplicationResult
{
    public FastMultiplicationResult(
        string value,
        IReadOnlyDictionary<int, long> multiplicationsWithoutOverflow,
        IReadOnlyDictionary<int, long> multiplicationsWithOverflow,
        IReadOnlyDictionary<int, long> additionsWithoutOverflow,
        IReadOnlyDictionary<int, long> additionsWithOverflow)
    {
        Value = value;
        MultiplicationsWithoutOverflow = multiplicationsWithoutOverflow;
        MultiplicationsWithOverflow = multiplicationsWithOverflow;
        AdditionsWithoutOverflow = additionsWithoutOverflow;
        AdditionsWithOverflow = additionsWithOverflow;
    }

    public string Value { get; }
    public IReadOnlyDictionary<int, long> MultiplicationsWithoutOverflow { get; }
    public IReadOnlyDictionary<int, long> MultiplicationsWithOverflow { get; }
    public IReadOnlyDictionary<int, long> AdditionsWithoutOverflow { get; }
    public IReadOnlyDictionary<int, long> AdditionsWithOverflow { get; }
}
