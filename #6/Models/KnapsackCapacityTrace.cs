namespace _6.Models;

public sealed record KnapsackCapacityTrace(
    int Capacity,
    decimal BestValue,
    int? SelectedItemNumber,
    IReadOnlyList<KnapsackCandidateTrace> Candidates);
