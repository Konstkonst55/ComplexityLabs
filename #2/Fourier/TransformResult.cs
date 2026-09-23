using System.Numerics;

namespace _2.Fourier;

public sealed class TransformResult
{
    public TransformResult(
        Complex[] coefficients,
        IReadOnlyList<string> steps,
        long summandCount,
        int costPerSummand,
        long operationCount)
    {
        Coefficients = coefficients;
        Steps = steps;
        SummandCount = summandCount;
        CostPerSummand = costPerSummand;
        OperationCount = operationCount;
    }

    public Complex[] Coefficients { get; }

    public IReadOnlyList<string> Steps { get; }

    public long SummandCount { get; }

    public int CostPerSummand { get; }

    public long OperationCount { get; }
}
