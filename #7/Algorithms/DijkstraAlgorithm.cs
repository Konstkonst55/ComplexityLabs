using _7.Models;
using _7.Output;
using Tables.Tables;

namespace _7.Algorithms;

public sealed class DijkstraAlgorithm
{
    public AlgorithmReport Execute(WeightedGraph graph, int startVertex)
    {
        ArgumentNullException.ThrowIfNull(graph);
        if (!graph.ContainsVertex(startVertex))
            throw new ArgumentException($"Стартовая вершина {startVertex} отсутствует в графе.", nameof(startVertex));
        if (graph.Edges.Any(edge => edge.Weight < 0))
            throw new ArgumentException("Алгоритм Дейкстры не работает с отрицательными весами рёбер.", nameof(graph));

        var report = new AlgorithmReport();
        report.AddText($"Алгоритм Дейкстры - стартовая вершина v{startVertex}");
        report.AddTable(GraphTableFactory.AdjacencyMatrix(graph));

        var distances = graph.Vertices.ToDictionary(vertex => vertex, _ => (long?)null);
        var predecessors = new Dictionary<int, int>();
        var visited = new HashSet<int> { startVertex };
        distances[startVertex] = 0;

        foreach (var vertex in graph.Vertices)
        {
            if (vertex == startVertex)
                continue;

            distances[vertex] = graph.GetWeight(startVertex, vertex);
            if (distances[vertex].HasValue)
                predecessors[vertex] = startVertex;
        }

        report.AddText($"D^[0] = {FormatVector(graph.Vertices, distances)}");
        report.AddText($"S^[0] = {GraphTableFactory.FormatSet(visited)}");

        var stepRows = new TableBuilder("Таблица шагов")
            .AddColumn("S")
            .AddColumn("w")
            .AddColumn("D(w)");
        foreach (var vertex in graph.Vertices)
            stepRows.AddColumn($"D({vertex})");

        stepRows.AddRow(BuildInitialRow(graph, visited, distances));

        var step = 0;
        while (visited.Count < graph.VertexCount)
        {
            var next = graph.Vertices
                .Where(vertex => !visited.Contains(vertex) && distances[vertex].HasValue)
                .OrderBy(vertex => distances[vertex])
                .FirstOrDefault();

            if (!graph.Vertices.Any(vertex => !visited.Contains(vertex) && distances[vertex].HasValue))
            {
                report.AddText("Оставшиеся вершины недостижимы из стартовой вершины.");
                break;
            }

            step++;
            var currentDistance = distances[next]!.Value;
            var candidates = graph.Vertices.Where(vertex => !visited.Contains(vertex)).ToArray();
            report.AddText($"Шаг {step}");
            report.AddText($"Выбираем w с минимальным D(w): среди {GraphTableFactory.FormatSet(candidates)} -> w = {next}, D(w) = {currentDistance}");
            report.AddText("Пересчёт D[v]:");

            visited.Add(next);
            foreach (var vertex in graph.Vertices.Where(vertex => !visited.Contains(vertex)))
            {
                var oldDistance = distances[vertex];
                var edgeWeight = graph.GetWeight(next, vertex);
                long? candidateDistance = edgeWeight.HasValue
                    ? checked(currentDistance + edgeWeight.Value)
                    : null;

                var candidateText = edgeWeight.HasValue
                    ? $"{currentDistance}+{edgeWeight.Value}={candidateDistance}"
                    : $"{currentDistance}+inf";
                var newDistance = Min(oldDistance, candidateDistance);
                report.AddText($"  D[{vertex}] = min({GraphTableFactory.Format(oldDistance)}, {candidateText}) = {GraphTableFactory.Format(newDistance)}");

                if (candidateDistance.HasValue && (!oldDistance.HasValue || candidateDistance.Value < oldDistance.Value))
                {
                    distances[vertex] = candidateDistance;
                    predecessors[vertex] = next;
                }
            }

            report.AddText($"D^[{step}] = {FormatVector(graph.Vertices, distances)}");
            report.AddText($"S^[{step}] = {GraphTableFactory.FormatSet(visited)}");
            stepRows.AddRow(BuildStepRow(graph, visited, distances, next, currentDistance));
        }

        report.AddTable(stepRows.Build());

        var resultTable = new TableBuilder("Результат")
            .AddColumn("Вершина")
            .AddColumn("Расстояние")
            .AddColumn("Маршрут");
        foreach (var vertex in graph.Vertices)
            resultTable.AddRow($"v{vertex}", GraphTableFactory.Format(distances[vertex]), BuildPath(startVertex, vertex, distances[vertex], predecessors));
        report.AddTable(resultTable.Build());
        return report;
    }

    private static object?[] BuildInitialRow(
        WeightedGraph graph,
        IReadOnlySet<int> visited,
        IReadOnlyDictionary<int, long?> distances)
    {
        var cells = new List<object?> { GraphTableFactory.FormatSet(visited), "—", "—" };
        cells.AddRange(graph.Vertices.Select(vertex => GraphTableFactory.Format(distances[vertex])));
        return cells.ToArray();
    }

    private static object?[] BuildStepRow(
        WeightedGraph graph,
        IReadOnlySet<int> visited,
        IReadOnlyDictionary<int, long?> distances,
        int selectedVertex,
        long selectedDistance)
    {
        var cells = new List<object?>
        {
            GraphTableFactory.FormatSet(visited),
            selectedVertex,
            selectedDistance.ToString()
        };

        foreach (var vertex in graph.Vertices)
        {
            if (vertex != selectedVertex && visited.Contains(vertex))
                cells.Add("—");
            else
                cells.Add(GraphTableFactory.Format(distances[vertex]));
        }

        return cells.ToArray();
    }

    private static long? Min(long? left, long? right)
    {
        if (!left.HasValue) return right;
        if (!right.HasValue) return left;
        return Math.Min(left.Value, right.Value);
    }

    private static string FormatVector(IEnumerable<int> vertices, IReadOnlyDictionary<int, long?> distances) =>
        $"({string.Join(", ", vertices.Select(vertex => GraphTableFactory.Format(distances[vertex])))})";

    private static string BuildPath(
        int start,
        int target,
        long? distance,
        IReadOnlyDictionary<int, int> predecessors)
    {
        if (!distance.HasValue)
            return "нет пути";

        var path = new List<int> { target };
        var current = target;
        var guard = 0;
        while (current != start && predecessors.TryGetValue(current, out var previous) && guard++ <= predecessors.Count)
        {
            current = previous;
            path.Add(current);
        }

        if (current != start)
            return "путь не восстановлен";

        path.Reverse();
        return string.Join(" -> ", path.Select(vertex => $"v{vertex}"));
    }
}
