using System.Globalization;
using _8.Algorithms;
using _8.Models;
using _8.Output;
using Tables.Tables;

namespace _8;

internal static class Program
{
    // Демонстрационный неориентированный граф из 5 вершин.
    // Оптимальный цикл, найденный алгоритмом: 0 -> 4 -> 3 -> 2 -> 1 -> 0, стоимость 24.
    private static readonly WeightedGraph DemonstrationGraph = WeightedGraph.FromAdjacencyMatrix(
        new long?[,]
        {
            { null, 8, 5, 9, 7 },
            { 8, null, 3, 7, 6 },
            { 5, 3, null, 2, 8 },
            { 9, 7, 2, null, 4 },
            { 7, 6, 8, 4, null }
        },
        firstVertex: 0,
        isDirected: false);

    private static readonly TspOptions AlgorithmOptions = new() { EnableTrace = false };

    private static readonly BranchAndBoundTspAlgorithm Algorithm = new();
    private static readonly ConsoleTableRenderer TableRenderer = new();
    private static readonly ConsoleReportPrinter ReportPrinter = new(TableRenderer);

    private static void Main()
    {
        while (true)
        {
            ClearConsole();
            Console.WriteLine("Задача коммивояжера");
            Console.WriteLine();
            Console.WriteLine("1. Запустить пример");
            Console.WriteLine("2. Ввести свой граф");
            Console.WriteLine("0. Выход");
            Console.Write("Выбор: ");

            var input = Console.ReadLine();
            if (input is null)
            {
                return;
            }

            var choice = input.Trim();
            switch (choice)
            {
                case "1":
                    RunGraph(DemonstrationGraph, 0, "Демонстрационный граф");
                    break;
                case "2":
                {
                    var customGraph = ReadCustomGraph();
                    if (customGraph is not null)
                    {
                        RunGraph(customGraph.Value.Graph, customGraph.Value.StartVertex, "Пользовательский граф");
                    }
                    else
                    {
                        WaitForMenu();
                    }

                    break;
                }
                case "0":
                    return;
                default:
                    Console.WriteLine("Неизвестный пункт меню.");
                    WaitForMenu();
                    break;
            }
        }
    }

    private static void RunGraph(WeightedGraph graph, int startVertex, string title)
    {
        ClearConsole();
        Console.WriteLine(title);
        Console.WriteLine($"Тип графа: {(graph.IsDirected ? "ориентированный" : "неориентированный")}");
        Console.WriteLine($"Количество вершин: {graph.VertexCount}");
        Console.WriteLine($"Начальная вершина: {startVertex}");
        Console.WriteLine();
        Console.WriteLine(TableRenderer.Render(GraphTableFactory.AdjacencyMatrix(graph)));
        Console.WriteLine();

        try
        {
            var result = Algorithm.Execute(graph, startVertex, AlgorithmOptions);

            if (AlgorithmOptions.EnableTrace && result.Trace is not null)
            {
                ReportPrinter.Print(result.Trace);
                Console.WriteLine();
            }

            PrintResult(result);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or OverflowException)
        {
            Console.WriteLine($"Не удалось выполнить алгоритм: {exception.Message}");
        }

        WaitForMenu();
    }

    private static void PrintResult(TspResult result)
    {
        Console.WriteLine("Результат");

        if (result.BestCost is null)
        {
            Console.WriteLine("Гамильтонова цикла нет.");
        }
        else
        {
            Console.WriteLine($"Маршрут: {string.Join(" -> ", result.BestPath)}");
            Console.WriteLine($"Минимальная стоимость: {result.BestCost.Value}");
        }

        Console.WriteLine($"Подъёмов (переключений к отложенным ветвям): {result.Returns}");
        Console.WriteLine($"Раскрытых ветвей: {result.ExpandedNodes}");
        Console.WriteLine($"Вызовов оценки H(D): {result.BoundCalls}");
        Console.WriteLine($"Отсечений по рекорду: {result.PrunedBranches}");
        Console.WriteLine($"Запрещённых / невозможных продолжений: {result.ImpossibleBranches}");
    }

    private static (WeightedGraph Graph, int StartVertex)? ReadCustomGraph()
    {
        ClearConsole();
        Console.WriteLine("Ввод собственного графа");
        Console.WriteLine();

        var vertexCount = ReadInt("Количество вершин (от 2 до 15): ", 2, 15);
        if (vertexCount is null)
        {
            return null;
        }

        Console.WriteLine("Тип графа:");
        Console.WriteLine("1. Неориентированный");
        Console.WriteLine("2. Ориентированный");
        var graphType = ReadInt("Выбор: ", 1, 2);
        if (graphType is null)
        {
            return null;
        }

        var isDirected = graphType == 2;
        var matrix = new long?[vertexCount.Value, vertexCount.Value];

        Console.WriteLine();
        Console.WriteLine(
            $"Введите {vertexCount.Value} строк матрицы по {vertexCount.Value} чисел через пробел.");
        Console.WriteLine("-1 - ребра нет, 0 - ребро с нулевым весом, положительное число - вес ребра.");
        Console.WriteLine("Значения на главной диагонали игнорируются.");

        for (var row = 0; row < vertexCount.Value; row++)
        {
            while (true)
            {
                Console.Write($"Строка {row}: ");
                var line = Console.ReadLine();
                if (line is null)
                {
                    return null;
                }

                var tokens = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length != vertexCount.Value)
                {
                    Console.WriteLine($"Нужно ввести ровно {vertexCount.Value} значений.");
                    continue;
                }

                var parsed = new long[vertexCount.Value];
                var isValid = true;
                for (var column = 0; column < vertexCount.Value; column++)
                {
                    if (!long.TryParse(tokens[column], NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ||
                        value < -1 || value > long.MaxValue / 800)
                    {
                        isValid = false;
                        break;
                    }

                    parsed[column] = value;
                }

                if (!isValid)
                {
                    Console.WriteLine("Допустимы целые числа от -1 до long.MaxValue / 800.");
                    continue;
                }

                for (var column = 0; column < vertexCount.Value; column++)
                {
                    matrix[row, column] = parsed[column] == -1 ? null : parsed[column];
                }

                break;
            }
        }

        if (!isDirected)
        {
            for (var row = 0; row < vertexCount.Value; row++)
            {
                for (var column = row + 1; column < vertexCount.Value; column++)
                {
                    if (matrix[row, column] != matrix[column, row])
                    {
                        Console.WriteLine();
                        Console.WriteLine(
                            $"Ошибка: для неориентированного графа матрица должна быть симметричной " +
                            $"(ячейки [{row},{column}] и [{column},{row}] различаются).");
                        return null;
                    }
                }
            }
        }

        var startVertex = ReadInt($"Начальная вершина (0..{vertexCount.Value - 1}) [0]: ", 0, vertexCount.Value - 1, defaultValue: 0);
        if (startVertex is null)
        {
            return null;
        }

        try
        {
            var graph = WeightedGraph.FromAdjacencyMatrix(
                matrix,
                firstVertex: 0,
                isDirected: isDirected);
            return (graph, startVertex.Value);
        }
        catch (ArgumentException exception)
        {
            Console.WriteLine($"Некорректный граф: {exception.Message}");
            return null;
        }
    }

    private static int? ReadInt(string prompt, int minimum, int maximum, int? defaultValue = null)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (input is null)
            {
                return null;
            }

            input = input.Trim();
            if (input.Length == 0 && defaultValue is not null)
            {
                return defaultValue;
            }

            if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) &&
                value >= minimum && value <= maximum)
            {
                return value;
            }

            Console.WriteLine($"Введите целое число от {minimum} до {maximum}.");
        }
    }

    private static void ClearConsole()
    {
        if (Console.IsOutputRedirected)
        {
            return;
        }

        try
        {
            Console.Clear();
        }
        catch (IOException)
        {
            // Очистка может быть недоступна в некоторых терминалах и средах запуска.
        }
    }

    private static void WaitForMenu()
    {
        Console.WriteLine();
        Console.WriteLine("Нажмите любую клавишу, чтобы вернуться в меню...");

        if (Console.IsInputRedirected)
        {
            Console.ReadLine();
            return;
        }

        Console.ReadKey(intercept: true);
    }
}
