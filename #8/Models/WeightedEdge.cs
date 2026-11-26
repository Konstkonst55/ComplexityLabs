namespace _8.Models;

public sealed record WeightedEdge(int From, int To, long Weight)
{
    public override string ToString() => $"{From} -> {To} ({Weight})";
}
