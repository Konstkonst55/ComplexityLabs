namespace _6.Models;

public sealed class KnapsackResult
{
    public KnapsackResult(
        int capacity,
        IReadOnlyList<decimal> bestValues,
        IReadOnlyList<KnapsackCapacityTrace> trace,
        IReadOnlyDictionary<int, int> itemQuantities,
        int totalMass,
        decimal totalCost,
        long candidateChecks)
    {
        Capacity = capacity;
        BestValues = bestValues;
        Trace = trace;
        ItemQuantities = itemQuantities;
        TotalMass = totalMass;
        TotalCost = totalCost;
        CandidateChecks = candidateChecks;
    }

    public int Capacity { get; }
    public IReadOnlyList<decimal> BestValues { get; }
    public IReadOnlyList<KnapsackCapacityTrace> Trace { get; }
    public IReadOnlyDictionary<int, int> ItemQuantities { get; }
    public int TotalMass { get; }
    public decimal TotalCost { get; }
    public long CandidateChecks { get; }
}
