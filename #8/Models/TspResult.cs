using _8.Output;

namespace _8.Models;

public sealed class TspResult
{
    public IReadOnlyList<int> BestPath { get; init; } = Array.Empty<int>();

    public long? BestCost { get; init; }

    public int ExpandedNodes { get; init; }

    public long BoundCalls { get; init; }

    public int PrunedBranches { get; init; }

    public int ImpossibleBranches { get; init; }

    public int Returns { get; init; }

    public InitialSearchStatistics InitialSearch { get; init; } = new();

    public AlgorithmReport? Trace { get; init; }
}

public sealed class InitialSearchStatistics
{
    public long TriedEdges { get; init; }

    public long Backtracks { get; init; }

    public long CheckedClosures { get; init; }
}
