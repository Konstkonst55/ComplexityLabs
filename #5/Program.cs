using System.Numerics;
using _5.MatrixChain;
using Tables.Tables;

Console.WriteLine("Задача об оптимальной расстановке скобок");
Console.WriteLine("Введите размеры матриц через запятую, например: 10x20, 20x50, 50x1, 1x100");
Console.Write("Размеры матриц: ");
var input = Console.ReadLine();

if (string.IsNullOrWhiteSpace(input))
{
    input = "10x20, 20x50, 50x1, 1x100";
    Console.WriteLine($"Используется пример: {input}");
}

try
{
    var dimensions = MatrixDimensionsParser.Parse(input);
    var solver = new MatrixChainSolver();
    var result = solver.Solve(dimensions);

    Console.WriteLine();
    Console.WriteLine("Входные матрицы:");
    PrintMatrices(dimensions);
    Console.WriteLine();
    Console.WriteLine("Пошаговое заполнение таблицы динамического программирования:");
    foreach (var step in result.Trace)
    {
        Console.WriteLine($"{step.Number}) {step.Text}");
    }

    Console.WriteLine();
    Console.WriteLine(new ConsoleTableRenderer().Render(BuildCostTable(result)));
    Console.WriteLine();
    Console.WriteLine(new ConsoleTableRenderer().Render(BuildSplitTable(result)));
    Console.WriteLine();
    Console.WriteLine($"Оптимальная расстановка скобок: {result.OptimalParenthesization}");
    Console.WriteLine($"Минимальная трудоемкость: {result.MinimumCost} операций");
    Console.WriteLine($"Трудоемкость правой расстановки M1*(M2*(...*Mn)): {solver.CalculateRightAssociativeCost(dimensions):N0} операций");
    Console.WriteLine($"Количество проверенных вариантов разбиения в ДП: {result.CandidateEvaluations}");

    if (result.MatrixCount <= 10)
    {
        var bruteForceCost = CalculateBruteForceMinimum(dimensions, 1, result.MatrixCount);
        Console.WriteLine(bruteForceCost == result.MinimumCost
            ? "Проверка прямым перебором: совпадает"
            : $"Проверка прямым перебором: ошибка (получено {bruteForceCost})");
    }
    else
    {
        Console.WriteLine("Проверка прямым перебором пропущена для цепочек длиннее 10 матриц.");
    }
}
catch (ArgumentException exception)
{
    Console.WriteLine($"Ошибка: {exception.Message}");
}
catch (OverflowException)
{
    Console.WriteLine("Ошибка: вычисленная трудоемкость превышает диапазон типа long.");
}

static void PrintMatrices(IReadOnlyList<int> dimensions)
{
    var count = dimensions.Count - 1;
    var matrices = Enumerable.Range(1, count)
        .Select(index => $"M{index}[{dimensions[index - 1]}x{dimensions[index]}]");
    Console.WriteLine("  " + string.Join(", ", matrices));
}

static TableDefinition BuildCostTable(MatrixChainResult result)
{
    var n = result.MatrixCount;
    var builder = new TableBuilder("Таблица минимальных трудоемкостей f(k, p)")
        .AddColumn("k\\p");

    for (var column = 1; column <= n; column++)
    {
        builder.AddColumn(column.ToString());
    }

    for (var row = 1; row <= n; row++)
    {
        var cells = new object?[n + 1];
        cells[0] = $"{row}:";
        for (var column = 1; column <= n; column++)
        {
            cells[column] = column < row ? "-" : result.Costs[row, column].ToString();
        }
        builder.AddRow(cells);
    }

    return builder.Build();
}

static TableDefinition BuildSplitTable(MatrixChainResult result)
{
    var n = result.MatrixCount;
    var builder = new TableBuilder("Таблица точек оптимального разбиения s(k, p)")
        .AddColumn("k\\p");

    for (var column = 1; column <= n; column++)
    {
        builder.AddColumn(column.ToString());
    }

    for (var row = 1; row <= n; row++)
    {
        var cells = new object?[n + 1];
        cells[0] = $"{row}:";
        for (var column = 1; column <= n; column++)
        {
            cells[column] = column <= row ? "-" : result.Splits[row, column].ToString();
        }
        builder.AddRow(cells);
    }

    return builder.Build();
}

static long CalculateBruteForceMinimum(IReadOnlyList<int> dimensions, int start, int end)
{
    if (start == end)
    {
        return 0;
    }

    var minimum = long.MaxValue;
    for (var split = start; split < end; split++)
    {
        var left = CalculateBruteForceMinimum(dimensions, start, split);
        var right = CalculateBruteForceMinimum(dimensions, split + 1, end);
        var cost = checked(left + right + (long)dimensions[start - 1] * dimensions[split] * dimensions[end]);
        minimum = Math.Min(minimum, cost);
    }
    return minimum;
}
