namespace _4.Algorithms;

public sealed class FastMultiplicationStatistics
{
    public Dictionary<int, long> AdditionsWithoutOverflow { get; } = new();
    public Dictionary<int, long> AdditionsWithOverflow { get; } = new();
    public Dictionary<int, long> MultiplicationsWithoutOverflow { get; } = new();
    public Dictionary<int, long> MultiplicationsWithOverflow { get; } = new();

    public long GetAdditionsWithoutOverflow(int digits) => GetValue(AdditionsWithoutOverflow, digits);
    public long GetAdditionsWithOverflow(int digits) => GetValue(AdditionsWithOverflow, digits);
    public long GetMultiplicationsWithoutOverflow(int digits) => GetValue(MultiplicationsWithoutOverflow, digits);
    public long GetMultiplicationsWithOverflow(int digits) => GetValue(MultiplicationsWithOverflow, digits);

    private static long GetValue(IReadOnlyDictionary<int, long> source, int digits)
    {
        return source.TryGetValue(digits, out var value) ? value : 0;
    }
}
