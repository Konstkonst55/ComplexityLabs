namespace _5.MatrixChain;

public sealed class MatrixChainSolver
{
    public MatrixChainResult Solve(IReadOnlyList<int> dimensions)
    {
        ArgumentNullException.ThrowIfNull(dimensions);
        if (dimensions.Count < 2)
        {
            throw new ArgumentException("Для цепочки требуется хотя бы одна матрица.", nameof(dimensions));
        }

        if (dimensions.Any(dimension => dimension <= 0))
        {
            throw new ArgumentException("Размеры матриц должны быть положительными.", nameof(dimensions));
        }

        var copiedDimensions = dimensions.ToArray();
        var n = copiedDimensions.Length - 1;
        var costs = new long[n + 1, n + 1];
        var splits = new int[n + 1, n + 1];
        var trace = new List<MatrixChainTraceStep>();
        var traceNumber = 1;
        long candidateEvaluations = 0;

        for (var i = 1; i <= n; i++)
        {
            costs[i, i] = 0;
        }

        trace.Add(new MatrixChainTraceStep(
            traceNumber++,
            $"Главная диагональ: f(i, i) = 0 для всех i = 1..{n}"));

        for (var t = 1; t < n; t++)
        {
            trace.Add(new MatrixChainTraceStep(
                traceNumber++,
                $"Вычисление для длины подцепочки t = {t}"));

            for (var start = 1; start <= n - t; start++)
            {
                var end = start + t;
                var minimum = long.MaxValue;
                var bestSplit = -1;
                var expressions = new List<string>();

                for (var split = start; split < end; split++)
                {
                    var leftCost = costs[start, split];
                    var rightCost = costs[split + 1, end];
                    var multiplicationCost = checked(
                        (long)copiedDimensions[start - 1] * copiedDimensions[split] * copiedDimensions[end]);
                    var candidate = checked(leftCost + rightCost + multiplicationCost);
                    candidateEvaluations++;
                    expressions.Add(
                        $"f({start},{split}) + f({split + 1},{end}) + " +
                        $"{copiedDimensions[start - 1]}*{copiedDimensions[split]}*{copiedDimensions[end]} = {candidate}");

                    if (candidate < minimum)
                    {
                        minimum = candidate;
                        bestSplit = split;
                    }
                }

                costs[start, end] = minimum;
                splits[start, end] = bestSplit;
                trace.Add(new MatrixChainTraceStep(
                    traceNumber++,
                    $"f({start},{end}) = min({string.Join("; ", expressions)}) = {minimum} (минимум при j = {bestSplit})"));
            }
        }

        var parenthesization = BuildParenthesization(splits, 1, n);
        return new MatrixChainResult(
            copiedDimensions,
            costs,
            splits,
            parenthesization,
            costs[1, n],
            candidateEvaluations,
            trace);
    }

    public long CalculateRightAssociativeCost(IReadOnlyList<int> dimensions)
    {
        ArgumentNullException.ThrowIfNull(dimensions);
        var n = dimensions.Count - 1;
        if (n <= 1)
        {
            return 0;
        }

        long cost = 0;
        for (var firstMatrix = n - 1; firstMatrix >= 1; firstMatrix--)
        {
            cost = checked(cost + (long)dimensions[firstMatrix - 1] * dimensions[firstMatrix] * dimensions[n]);
        }

        return cost;
    }

    private static string BuildParenthesization(int[,] splits, int start, int end)
    {
        if (start == end)
        {
            return $"M{start}";
        }

        var split = splits[start, end];
        return $"({BuildParenthesization(splits, start, split)} {BuildParenthesization(splits, split + 1, end)})";
    }
}
