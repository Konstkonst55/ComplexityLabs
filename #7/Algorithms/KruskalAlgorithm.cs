using _7.Models;
using _7.Output;
using Tables.Tables;

namespace _7.Algorithms;

public sealed class KruskalAlgorithm
{
    public AlgorithmReport Execute(WeightedGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph);
        if (graph.IsDirected)
            throw new ArgumentException("Алгоритм Краскала применяется к неориентированному графу.", nameof(graph));

        var report = new AlgorithmReport();
        report.AddText("Алгоритм Краскала");
        report.AddText($"Вершин: {graph.VertexCount}, рёбер: {graph.Edges.Count}");

        var sortedEdges = graph.Edges
            .OrderBy(edge => edge.Weight)
            .ThenBy(edge => edge.From)
            .ThenBy(edge => edge.To)
            .ToArray();
        report.AddText($"Отсортированные рёбра: {GraphTableFactory.FormatEdges(sortedEdges)}");

        var colors = graph.Vertices.ToDictionary(vertex => vertex, vertex => vertex);
        var selectedEdges = new List<WeightedEdge>();
        var rejectedEdges = new List<WeightedEdge>();
        long totalWeight = 0;

        report.AddText("Начальное разбиение на компоненты");
        report.AddTable(BuildColorTable(graph, colors));

        var stepsTable = new TableBuilder("Таблица шагов")
            .AddColumn("Шаг")
            .AddColumn("Ребро")
            .AddColumn("Решение")
            .AddColumn("Компоненты")
            .AddColumn("Вес остова");

        for (var index = 0; index < sortedEdges.Length; index++)
        {
            var edge = sortedEdges[index];
            var fromColor = colors[edge.From];
            var toColor = colors[edge.To];
            var addEdge = fromColor != toColor;
            var step = index + 1;

            report.AddText($"Шаг {step}");
            report.AddText($"Рассматриваем ребро {edge.From} -- {edge.To}, вес = {edge.Weight}");
            report.AddText($"До шага: цвет({edge.From}) = {fromColor}, цвет({edge.To}) = {toColor}");

            if (addEdge)
            {
                selectedEdges.Add(edge);
                totalWeight = checked(totalWeight + edge.Weight);
                var recoloredVertices = graph.Vertices.Where(vertex => colors[vertex] == toColor).ToArray();
                foreach (var vertex in recoloredVertices)
                    colors[vertex] = fromColor;

                report.AddText("ADD: цвета разные, добавляем ребро в остов.");
                report.AddText($"Объединяем компоненты: цвет {toColor} заменяется на {fromColor} у вершин {string.Join(" ", recoloredVertices)}.");
            }
            else
            {
                rejectedEdges.Add(edge);
                report.AddText("SKIP: цвета одинаковые, ребро создает цикл.");
                report.AddText("Цвета не меняются.");
            }

            report.AddTable(BuildColorTable(graph, colors));
            report.AddText($"Выбранные рёбра: {GraphTableFactory.FormatEdges(selectedEdges)}");
            report.AddText($"Оставшиеся для проверки: {GraphTableFactory.FormatEdges(sortedEdges.Skip(index + 1))}");
            report.AddText($"Вес уже выбранных рёбер: {totalWeight}");

            stepsTable.AddRow(
                step,
                $"{edge.From}-{edge.To} ({edge.Weight})",
                addEdge ? "добавлено" : "отклонено",
                FormatComponents(graph, colors),
                totalWeight);
        }

        report.AddTable(stepsTable.Build());
        report.AddText("Результат");
        report.AddText($"Рёбра остова: {GraphTableFactory.FormatEdges(selectedEdges)}");
        report.AddText($"Отклонённые рёбра: {GraphTableFactory.FormatEdges(rejectedEdges)}");
        report.AddText($"Суммарный вес: {totalWeight}");
        report.AddText($"Окончательные компоненты: {FormatComponents(graph, colors)}");

        var isSpanningTree = selectedEdges.Count == graph.VertexCount - 1 && colors.Values.Distinct().Count() == 1;
        report.AddText(isSpanningTree
            ? $"Получено минимальное остовное дерево ({selectedEdges.Count} рёбер)."
            : $"Граф несвязный: получен минимальный остовный лес ({selectedEdges.Count} рёбер, {colors.Values.Distinct().Count()} компонент).");
        return report;
    }

    private static TableDefinition BuildColorTable(WeightedGraph graph, IReadOnlyDictionary<int, int> colors)
    {
        var table = new TableBuilder("Таблица цветов")
            .AddColumn("Показатель")
            .AddColumn("Значения");

        table.AddRow("Вершины", string.Join("  ", graph.Vertices));
        table.AddRow("Цвета", string.Join("  ", graph.Vertices.Select(vertex => colors[vertex])));
        table.AddRow("Компоненты", FormatComponents(graph, colors));
        return table.Build();
    }

    private static string FormatComponents(WeightedGraph graph, IReadOnlyDictionary<int, int> colors)
    {
        var components = graph.Vertices
            .GroupBy(vertex => colors[vertex])
            .Select(group => $"({string.Join(",", group)})");
        return string.Join(" ", components);
    }
}
