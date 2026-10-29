namespace _6.Models;

public sealed record KnapsackCandidateTrace(
    int ItemNumber,
    int Mass,
    decimal Cost,
    int? PreviousCapacity,
    decimal? CandidateValue)
{
    public bool IsDefined => CandidateValue.HasValue;
}
