using System.Globalization;
using _6.Models;

namespace _6.Presentation;

public sealed class KnapsackConsolePrinter
{
    public void PrintResult(IReadOnlyList<KnapsackItem> items, KnapsackResult result)
    {
        Console.WriteLine();
        Console.WriteLine("Пошаговый расчет массива максимальных стоимостей f(M):");
        Console.WriteLine("Базовое состояние: f(0) = 0");

        foreach (var step in result.Trace)
        {
            var candidateTexts = step.Candidates.Select(candidate => FormatCandidate(step.Capacity, candidate));
            var expression = string.Join("; ", candidateTexts);
            var selectedText = step.SelectedItemNumber.HasValue
                ? FormatSelectedItem(items.First(item => item.Number == step.SelectedItemNumber.Value))
                : "пусто";

            Console.WriteLine($"  f({step.Capacity}) = {FormatNumber(step.BestValue)} = max({expression}) ({selectedText})");
        }

        Console.WriteLine();
        Console.WriteLine($"Максимальная стоимость f({result.Capacity}): {FormatNumber(result.TotalCost)}");
        Console.WriteLine("Оптимальный набор товаров:");

        if (result.ItemQuantities.Count == 0)
        {
            Console.WriteLine("  Рюкзак остается пустым.");
        }
        else
        {
            foreach (var item in items.OrderBy(item => item.Number))
            {
                if (result.ItemQuantities.TryGetValue(item.Number, out var quantity))
                {
                    Console.WriteLine($"  Товар {item.Number} (m={item.Mass}, c={FormatNumber(item.Cost)}): {quantity} шт.");
                }
            }
        }

        Console.WriteLine($"Суммарная масса: {result.TotalMass} из {result.Capacity}");
        Console.WriteLine($"Оценка трудоемкости: T = M * n = {result.Capacity} * {items.Count} = {result.CandidateChecks} проверок кандидатов.");
    }

    private static string FormatCandidate(int capacity, KnapsackCandidateTrace candidate)
    {
        if (!candidate.IsDefined)
        {
            return $"f({capacity}-{candidate.Mass}) не опр.";
        }

        return $"f({capacity}-{candidate.Mass})+{FormatNumber(candidate.Cost)}={FormatNumber(candidate.CandidateValue!.Value)}";
    }

    private static string FormatSelectedItem(KnapsackItem item)
    {
        return $"выбран товар {item.Number}: m={item.Mass}, c={FormatNumber(item.Cost)}";
    }

    private static string FormatNumber(decimal value)
    {
        return value.ToString("0.############################", CultureInfo.InvariantCulture);
    }
}
