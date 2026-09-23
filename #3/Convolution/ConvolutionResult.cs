namespace _3.Convolution;

public sealed class ConvolutionResult
{
    public ConvolutionResult(
        double[] values,
        long multiplications,
        long additions,
        long operationCount,
        int? transformLength,
        IReadOnlyList<string> steps)
    {
        Values = values;
        Multiplications = multiplications;
        Additions = additions;
        OperationCount = operationCount;
        TransformLength = transformLength;
        Steps = steps;
    }

    public double[] Values { get; }

    public long Multiplications { get; }

    public long Additions { get; }

    public long OperationCount { get; }

    public int? TransformLength { get; }

    public IReadOnlyList<string> Steps { get; }
}
