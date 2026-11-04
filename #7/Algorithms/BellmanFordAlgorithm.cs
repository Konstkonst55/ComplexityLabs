using _7.Models;
using _7.Output;
using Tables.Tables;

namespace _7.Algorithms;

public sealed class BellmanFordAlgorithm
{
    public AlgorithmReport Execute(WeightedGraph graph, int startVertex)
    {
        ArgumentNullException.ThrowIfNull(graph);
        if (!graph.ContainsVertex(startVertex))
            throw new ArgumentException($"Стартовая вершина {startVertex} отсутствует в графе.", nameof(startVertex));

        var report = new AlgorithmReport();
        report.AddText($"Алгоритм Форда–Беллмана - стартовая вершина v{startVertex}");
        report.AddTable(GraphTableFactory.AdjacencyMatrix(graph));

        var distances = graph.Vertices.ToDictionary(vertex => vertex, _ => (long?)null);
        var paths = graph.Vertices.ToDictionary(vertex => vertex, _ => (List<int>?)null);
        distances[startVertex] = 0;
        paths[startVertex] = new List<int> { startVertex };

        report.AddText($"D^[0] = {FormatVector(graph.Vertices, distances)}");

        var table = new TableBuilder("Таблица динамики")
            .AddColumn("k");
        foreach (var vertex in graph.Vertices)
            table.AddColumn($"D({vertex})");
        table.AddRow(new object?[] { 0 }.Concat(graph.Vertices.Select(vertex => GraphTableFactory.Format(distances[vertex]))).ToArray());

        var stabilized = false;
        var negativeCycleDetected = false;

        for (var step = 1; step <= graph.VertexCount; step++)
        {
            report.AddText($"Шаг {step}");
            var nextDistances = new Dictionary<int, long?>();
            var nextPaths = new Dictionary<int, List<int>?>();
            var changed = false;

            foreach (var target in graph.Vertices)
            {
                var candidates = new List<(int From, long Value, string Expression)>();
                foreach (var from in graph.Vertices)
                {
                    if (!distances[from].HasValue)
                        continue;

                    var weight = graph.GetWeight(from, target);
                    if (!weight.HasValue)
                        continue;

                    var value = checked(distances[from]!.Value + weight.Value);
                    candidates.Add((from, value, $"{distances[from]}+{weight.Value}={value}"));
                }

                var best = candidates.OrderBy(candidate => candidate.Value).FirstOrDefault();
                long? bestValue = candidates.Count == 0 ? null : best.Value;
                var oldValue = distances[target];
                var chosenPath = candidates.Count == 0 ? null : AppendPath(paths[best.From], target);
                var valueToStore = Min(oldValue, bestValue);

                if (candidates.Count > 0)
                {
                    var expression = string.Join(", ", candidates.Select(candidate => candidate.Expression));
                    report.AddText($"D^[{step}]({target}) = min({expression}) = {GraphTableFactory.Format(valueToStore)}");
                }
                else
                {
                    report.AddText($"D^[{step}]({target}) = min() = inf");
                }

                if (bestValue.HasValue && (!oldValue.HasValue || bestValue.Value < oldValue.Value))
                {
                    changed = true;
                    nextPaths[target] = chosenPath;
                }
                else
                {
                    nextPaths[target] = paths[target];
                }

                nextDistances[target] = valueToStore;
            }

            distances = nextDistances;
            paths = nextPaths;
            report.AddText($"D^[{step}] = {FormatVector(graph.Vertices, distances)}");
            table.AddRow(new object?[] { step }.Concat(graph.Vertices.Select(vertex => GraphTableFactory.Format(distances[vertex]))).ToArray());

            if (!changed)
            {
                report.AddText("Массив не изменился — стабилизация. Стоп.");
                stabilized = true;
                break;
            }

            if (step == graph.VertexCount)
            {
                report.AddText("После V-го прохода значения продолжили уменьшаться. Обнаружен достижимый отрицательный цикл.");
                negativeCycleDetected = true;
            }
        }

        report.AddTable(table.Build());
        if (negativeCycleDetected)
        {
            report.AddText("Кратчайшие расстояния не определены для вершин, на которые влияет отрицательный цикл.");
        }
        else if (!stabilized && !negativeCycleDetected)
        {
            report.AddText("Расчёт завершён без подтверждения стабилизации.");
        }

        var resultTable = new TableBuilder("Результат")
            .AddColumn("Вершина")
            .AddColumn("Расстояние")
            .AddColumn("Маршрут");
        foreach (var vertex in graph.Vertices)
        {
            var path = paths[vertex];
            resultTable.AddRow(
                $"v{vertex}",
                GraphTableFactory.Format(distances[vertex]),
                path is null ? "нет пути" : string.Join(" -> ", path.Select(item => $"v{item}")));
        }
        report.AddTable(resultTable.Build());
        return report;
    }

    private static List<int>? AppendPath(List<int>? path, int target)
    {
        if (path is null)
            return null;
        var result = new List<int>(path) { target };
        return result;
    }

    private static long? Min(long? left, long? right)
    {
        if (!left.HasValue) return right;
        if (!right.HasValue) return left;
        return Math.Min(left.Value, right.Value);
    }

    private static string FormatVector(IEnumerable<int> vertices, IReadOnlyDictionary<int, long?> distances) =>
        $"({string.Join(", ", vertices.Select(vertex => GraphTableFactory.Format(distances[vertex])))})";
}
