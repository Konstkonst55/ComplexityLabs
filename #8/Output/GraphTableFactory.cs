using _8.Models;
using Tables.Tables;

namespace _8.Output;

public static class GraphTableFactory
{
    public static TableDefinition AdjacencyMatrix(WeightedGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph);

        var builder = new TableBuilder("Матрица весов (inf - ребро отсутствует)")
            .AddColumn("из/в");

        foreach (var vertex in graph.Vertices)
        {
            builder.AddColumn(vertex.ToString());
        }

        foreach (var from in graph.Vertices)
        {
            var row = new object?[graph.VertexCount + 1];
            row[0] = from;

            for (var index = 0; index < graph.VertexCount; index++)
            {
                var to = graph.Vertices[index];
                row[index + 1] = graph.GetWeight(from, to)?.ToString() ?? "inf";
            }

            builder.AddRow(row);
        }

        return builder.Build();
    }
}
