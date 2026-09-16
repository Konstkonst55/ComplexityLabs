using System.Numerics;

namespace _2.Fourier;

public sealed class InverseTransformResult
{
    public InverseTransformResult(
        Complex[] values,
        IReadOnlyList<string> steps,
        long summandCount,
        int costPerSummand)
    {
        Values = values;
        Steps = steps;
        SummandCount = summandCount;
        CostPerSummand = costPerSummand;
        OperationCount = summandCount * costPerSummand;
    }

    public Complex[] Values { get; }

    public IReadOnlyList<string> Steps { get; }

    public long SummandCount { get; }

    public int CostPerSummand { get; }

    public long OperationCount { get; }
}
