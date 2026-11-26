using _8.Models;
using _8.Output;
using Tables.Tables;

namespace _8.Algorithms;

internal sealed class LowerBoundCalculator
{
    internal const long Infinity = long.MaxValue / 8;

    public BoundCalculation Calculate(
        WeightedGraph graph,
        IReadOnlyList<int> path,
        long prefixCost,
        long callNumber,
        AlgorithmReport? trace)
    {
        var current = path[^1];
        var visited = path.ToHashSet();
        var remaining = graph.Vertices.Where(vertex => !visited.Contains(vertex)).ToArray();

        trace?.AddText(
            $"Оценка H(D), вызов #{callNumber}. Путь: [{FormatPath(path)}]. " +
            $"Стоимость зафиксированного участка: {prefixCost}.");

        if (remaining.Length == 0)
        {
            var closingWeight = graph.GetWeight(current, path[0]);
            if (closingWeight is null)
            {
                trace?.AddText($"Все вершины посещены, но ребра {current} -> {path[0]} нет. Полный цикл невозможен.");
                return BoundCalculation.Impossible;
            }

            var exactCost = SafeAdd(prefixCost, closingWeight.Value);
            trace?.AddText(
                $"Все вершины посещены. Замыкающее ребро {current} -> {path[0]} имеет вес " +
                $"{closingWeight.Value}. Точная стоимость цикла: {prefixCost} + {closingWeight.Value} = {FormatValue(exactCost)}.");
            return new BoundCalculation(exactCost, 0, 0, exactCost < Infinity);
        }

        var rows = new[] { current }.Concat(remaining).ToArray();
        var columns = remaining.Concat(new[] { path[0] }).ToArray();
        var matrix = new long[rows.Length, columns.Length];

        for (var i = 0; i < rows.Length; i++)
        {
            for (var j = 0; j < columns.Length; j++)
            {
                var from = rows[i];
                var to = columns[j];
                var forbiddenClosingEdge = from == current && to == path[0];
                var weight = from == to || forbiddenClosingEdge
                    ? null
                    : graph.GetWeight(from, to);

                matrix[i, j] = weight ?? Infinity;
            }
        }

        trace?.AddText(
            "Строим остаточную матрицу: строки - текущая вершина и непосещённые вершины; " +
            "столбцы - непосещённые вершины и стартовая вершина. " +
            "Преждевременное замыкание цикла запрещено.");
        if (trace is not null)
        {
            trace.AddTable(BuildMatrixTable("Остаточная матрица до редукции", rows, columns, matrix));
        }

        var alpha = new long[rows.Length];
        long alphaSum = 0;

        for (var i = 0; i < rows.Length; i++)
        {
            var minimum = Infinity;
            for (var j = 0; j < columns.Length; j++)
            {
                minimum = Math.Min(minimum, matrix[i, j]);
            }

            if (minimum >= Infinity)
            {
                trace?.AddText($"В строке вершины {rows[i]} нет допустимых выездов. H(D) = inf.");
                return BoundCalculation.Impossible;
            }

            alpha[i] = minimum;
            alphaSum = SafeAdd(alphaSum, minimum);
        }

        if (trace is not null)
        {
            var alphaTable = new TableBuilder("Минимумы строк a")
                .AddColumn("Вершина")
                .AddColumn("a");
            for (var i = 0; i < rows.Length; i++)
            {
                alphaTable.AddRow(rows[i], alpha[i]);
            }

            trace.AddText("Находим минимальную стоимость выезда из каждой строки.");
            trace.AddTable(alphaTable.Build());
        }

        var reduced = new long[rows.Length, columns.Length];
        for (var i = 0; i < rows.Length; i++)
        {
            for (var j = 0; j < columns.Length; j++)
            {
                reduced[i, j] = matrix[i, j] >= Infinity
                    ? Infinity
                    : matrix[i, j] - alpha[i];
            }
        }

        if (trace is not null)
        {
            trace.AddText($"Вычитаем a из соответствующих строк. Сумма минимумов строк: {alphaSum}.");
            trace.AddTable(BuildMatrixTable("Матрица после вычитания a", rows, columns, reduced));
        }

        var beta = new long[columns.Length];
        long betaSum = 0;

        for (var j = 0; j < columns.Length; j++)
        {
            var minimum = Infinity;
            for (var i = 0; i < rows.Length; i++)
            {
                minimum = Math.Min(minimum, reduced[i, j]);
            }

            if (minimum >= Infinity)
            {
                trace?.AddText($"В столбец вершины {columns[j]} невозможно въехать. H(D) = inf.");
                return BoundCalculation.Impossible;
            }

            beta[j] = minimum;
            betaSum = SafeAdd(betaSum, minimum);
        }

        if (trace is not null)
        {
            var betaTable = new TableBuilder("Минимумы столбцов b")
                .AddColumn("Вершина")
                .AddColumn("b");
            for (var j = 0; j < columns.Length; j++)
            {
                betaTable.AddRow(columns[j], beta[j]);
            }

            trace.AddText("Находим минимальную стоимость въезда в каждый столбец после вычитания a.");
            trace.AddTable(betaTable.Build());
        }

        var lowerBound = SafeAdd(SafeAdd(prefixCost, alphaSum), betaSum);
        trace?.AddText(
            $"Итоговая нижняя оценка: H(D) = {prefixCost} + {alphaSum} + {betaSum} = " +
            $"{FormatValue(lowerBound)}.");

        return new BoundCalculation(lowerBound, alphaSum, betaSum, lowerBound < Infinity);
    }

    private static TableDefinition BuildMatrixTable(
        string title,
        IReadOnlyList<int> rows,
        IReadOnlyList<int> columns,
        long[,] matrix)
    {
        var builder = new TableBuilder(title).AddColumn("из/в");
        foreach (var column in columns)
        {
            builder.AddColumn(column.ToString());
        }

        for (var i = 0; i < rows.Count; i++)
        {
            var cells = new object?[columns.Count + 1];
            cells[0] = rows[i];
            for (var j = 0; j < columns.Count; j++)
            {
                cells[j + 1] = FormatValue(matrix[i, j]);
            }

            builder.AddRow(cells);
        }

        return builder.Build();
    }

    internal static long SafeAdd(long left, long right)
    {
        if (left >= Infinity || right >= Infinity || left > Infinity - right)
        {
            return Infinity;
        }

        return left + right;
    }

    internal static string FormatValue(long value) => value >= Infinity ? "inf" : value.ToString();

    internal static string FormatPath(IEnumerable<int> path) => string.Join(" -> ", path);
}

internal sealed record BoundCalculation(long LowerBound, long AlphaSum, long BetaSum, bool IsPossible)
{
    public static BoundCalculation Impossible { get; } = new(LowerBoundCalculator.Infinity, 0, 0, false);
}
