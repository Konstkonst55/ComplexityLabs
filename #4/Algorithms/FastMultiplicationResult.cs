namespace _4.Algorithms;

public sealed class FastMultiplicationResult
{
    public FastMultiplicationResult(
        string value,
        int paddedDigitCount,
        IReadOnlyList<MultiplicationTraceStep> trace,
        FastMultiplicationStatistics statistics)
    {
        Value = value;
        PaddedDigitCount = paddedDigitCount;
        Trace = trace;
        Statistics = statistics;
    }

    public string Value { get; }
    public int PaddedDigitCount { get; }
    public IReadOnlyList<MultiplicationTraceStep> Trace { get; }
    public FastMultiplicationStatistics Statistics { get; }
}
