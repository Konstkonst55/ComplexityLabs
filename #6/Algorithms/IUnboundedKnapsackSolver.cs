using _6.Models;

namespace _6.Algorithms;

public interface IUnboundedKnapsackSolver
{
    KnapsackResult Solve(IReadOnlyList<KnapsackItem> items, int capacity);
}
