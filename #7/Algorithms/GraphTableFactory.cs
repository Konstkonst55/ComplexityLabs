using _7.Models;
using Tables.Tables;

namespace _7.Algorithms;

internal static class GraphTableFactory
{
    public static TableDefinition AdjacencyMatrix(WeightedGraph graph)
    {
        var table = new TableBuilder("Матрица смежности").AddColumn("Вершина");
        foreach (var vertex in graph.Vertices)
            table.AddColumn($"v{vertex}");

        foreach (var rowVertex in graph.Vertices)
        {
            var cells = new List<object?> { $"v{rowVertex}" };
            foreach (var columnVertex in graph.Vertices)
                cells.Add(Format(graph.GetWeight(rowVertex, columnVertex)));
            table.AddRow(cells.ToArray());
        }

        return table.Build();
    }

    public static string Format(long? value) => value?.ToString() ?? "inf";

    public static string FormatSet(IEnumerable<int> vertices) =>
        $"({string.Join(",", vertices.OrderBy(vertex => vertex))})";

    public static string FormatEdge(WeightedEdge edge) => $"({edge.From}-{edge.To}, вес={edge.Weight})";

    public static string FormatEdges(IEnumerable<WeightedEdge> edges)
    {
        var result = string.Join(" ", edges.Select(FormatEdge));
        return string.IsNullOrEmpty(result) ? "нет" : result;
    }
}
