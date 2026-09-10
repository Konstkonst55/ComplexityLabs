namespace _1.Algorithms;

public sealed class SortResult
{
    public SortResult(
        int[] values,
        long movements,
        long comparisons,
        IReadOnlyList<string> steps)
    {
        Values = values;
        Movements = movements;
        Comparisons = comparisons;
        Steps = steps;
    }

    public int[] Values { get; }

    public long Movements { get; }

    public long Comparisons { get; }

    public IReadOnlyList<string> Steps { get; }
}
