namespace _5.MatrixChain;

public sealed class MatrixChainResult
{
    public MatrixChainResult(
        int[] dimensions,
        long[,] costs,
        int[,] splits,
        string optimalParenthesization,
        long minimumCost,
        long candidateEvaluations,
        IReadOnlyList<MatrixChainTraceStep> trace)
    {
        Dimensions = dimensions;
        Costs = costs;
        Splits = splits;
        OptimalParenthesization = optimalParenthesization;
        MinimumCost = minimumCost;
        CandidateEvaluations = candidateEvaluations;
        Trace = trace;
    }

    public int[] Dimensions { get; }
    public int MatrixCount => Dimensions.Length - 1;
    public long[,] Costs { get; }
    public int[,] Splits { get; }
    public string OptimalParenthesization { get; }
    public long MinimumCost { get; }
    public long CandidateEvaluations { get; }
    public IReadOnlyList<MatrixChainTraceStep> Trace { get; }
}
