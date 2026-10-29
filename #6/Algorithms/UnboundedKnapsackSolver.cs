using _6.Models;

namespace _6.Algorithms;

public sealed class UnboundedKnapsackSolver : IUnboundedKnapsackSolver
{
    public KnapsackResult Solve(IReadOnlyList<KnapsackItem> items, int capacity)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (items.Count == 0)
        {
            throw new ArgumentException("Нужно указать хотя бы один товар.", nameof(items));
        }

        if (capacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Грузоподъемность не может быть отрицательной.");
        }

        foreach (var item in items)
        {
            if (item.Mass <= 0)
            {
                throw new ArgumentException($"Масса товара {item.Number} должна быть положительным целым числом.", nameof(items));
            }

            if (item.Cost < 0)
            {
                throw new ArgumentException($"Стоимость товара {item.Number} не может быть отрицательной.", nameof(items));
            }
        }

        var bestValues = new decimal[capacity + 1];
        var selectedItems = Enumerable.Repeat(-1, capacity + 1).ToArray();
        var trace = new List<KnapsackCapacityTrace>(capacity);
        long candidateChecks = 0;

        for (var currentCapacity = 1; currentCapacity <= capacity; currentCapacity++)
        {
            var bestValue = 0m;
            var selectedItemIndex = -1;
            var candidates = new List<KnapsackCandidateTrace>(items.Count);

            for (var itemIndex = 0; itemIndex < items.Count; itemIndex++)
            {
                var item = items[itemIndex];
                candidateChecks++;

                if (item.Mass > currentCapacity)
                {
                    candidates.Add(new KnapsackCandidateTrace(
                        item.Number,
                        item.Mass,
                        item.Cost,
                        null,
                        null));
                    continue;
                }

                var previousCapacity = currentCapacity - item.Mass;
                var candidateValue = bestValues[previousCapacity] + item.Cost;
                candidates.Add(new KnapsackCandidateTrace(
                    item.Number,
                    item.Mass,
                    item.Cost,
                    previousCapacity,
                    candidateValue));

                if (candidateValue > bestValue)
                {
                    bestValue = candidateValue;
                    selectedItemIndex = itemIndex;
                }
            }

            bestValues[currentCapacity] = bestValue;
            selectedItems[currentCapacity] = selectedItemIndex;
            trace.Add(new KnapsackCapacityTrace(
                currentCapacity,
                bestValue,
                selectedItemIndex >= 0 ? items[selectedItemIndex].Number : null,
                candidates));
        }

        var quantities = new Dictionary<int, int>();
        var remainingCapacity = capacity;
        var totalMass = 0;

        while (remainingCapacity > 0 && selectedItems[remainingCapacity] >= 0)
        {
            var item = items[selectedItems[remainingCapacity]];
            quantities[item.Number] = quantities.TryGetValue(item.Number, out var quantity)
                ? quantity + 1
                : 1;
            totalMass += item.Mass;
            remainingCapacity -= item.Mass;
        }

        return new KnapsackResult(
            capacity,
            bestValues,
            trace,
            quantities,
            totalMass,
            bestValues[capacity],
            candidateChecks);
    }
}
