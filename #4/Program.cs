using System.Numerics;
using _4.Algorithms;
using Tables.Tables;

Console.WriteLine("1 - умножение столбиком O(N^2)");
Console.WriteLine("2 - быстрое умножение O(N^1.585)");
Console.Write("Выберите режим: ");
var mode = Console.ReadLine();

Console.Write("Введите число X: ");
var first = Console.ReadLine();
Console.Write("Введите число Y: ");
var second = Console.ReadLine();

if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(second))
{
    Console.WriteLine("Ошибка: числа не должны быть пустыми.");
    return;
}

try
{
    switch (mode)
    {
        case "1":
            PrintColumnMultiplication(first, second);
            break;
        case "2":
            PrintFastMultiplication(first, second);
            break;
        default: 
            Console.WriteLine("Ошибка: выберите режим 1 или 2.");
            break;
    }
}
catch (ArgumentException exception)
{
    Console.WriteLine($"Ошибка: {exception.Message}");
}

static void PrintColumnMultiplication(string first, string second)
{
    var result = new ColumnMultiplication().Multiply(first, second);

    Console.WriteLine();
    Console.WriteLine($"Умножение столбиком {first} (длина {first.Length}) на {second} (длина {second.Length}):");
    Console.WriteLine();

    for (var i = 0; i < second.Length; i++)
    {
        var digit = second[second.Length - 1 - i];
        Console.WriteLine($"Разряд y[{i}] = {digit}: промежуточное накопление в позиции сдвига 10^{i}");
    }

    Console.WriteLine();
    Console.WriteLine($"Результат умножения столбиком: {result.Value}");
    Console.WriteLine($"Число элементарных умножений цифр: {result.ElementaryMultiplications} (теоретически N*M = {first.Length * second.Length})");
    Console.WriteLine($"Число сложений со сдвигами: {result.AdditionsWithShifts}");
}

static void PrintFastMultiplication(string first, string second)
{
    var algorithm = new FastMultiplication();
    var detailedResult = algorithm.Multiply(first, second);
    var kernelResult = algorithm.Multiply(first, second, captureTrace: false);
    var reference = (BigInteger.Parse(first) * BigInteger.Parse(second)).ToString();
    var columnResult = new ColumnMultiplication().Multiply(first, second);

    Console.WriteLine();
    Console.WriteLine($"Найти произведение {first} и {second} по формуле быстрого умножения (полная рекурсивная декомпозиция):");
    Console.WriteLine("Разобьем числа на разряды:");
    Console.WriteLine("x = a * 10^k + b");
    Console.WriteLine("y = c * 10^k + d");
    Console.WriteLine("Формула быстрого умножения:");
    Console.WriteLine("u = (a + b) * (c + d)");
    Console.WriteLine("v = a * c");
    Console.WriteLine("w = b * d");
    Console.WriteLine("x * y = v * 10^(2k) + (u - v - w) * 10^k + w");
    Console.WriteLine("При переполнении используется разложение:");
    Console.WriteLine("a * b = a1 * b1 * 10^(2k) + (a1 * b2 + a2 * b1) * 10^k + a2 * b2");
    Console.WriteLine($"В данном случае k = {detailedResult.PaddedDigitCount / 2}.");

    foreach (var step in detailedResult.Trace)
    {
        Console.WriteLine(new string(' ', Math.Min(step.IndentLevel, 30) * 2) + step.Text);
    }

    Console.WriteLine();
    Console.WriteLine($"Ответ: {first} * {second} = {detailedResult.Value}.");
    Console.WriteLine();
    Console.WriteLine("Верификация с эталоном BigInteger:");
    PrintVerification("Столбик", columnResult.Value, reference);
    PrintVerification("Карацуба (ядро)", kernelResult.Value, reference);
    PrintVerification("Карацуба (подробный расчет)", detailedResult.Value, reference);
    Console.WriteLine();
    Console.WriteLine("Счетчики операций рекурсивно накапливаются по уровням декомпозиции.");

    var table = BuildStatisticsTable(detailedResult);
    Console.WriteLine(new ConsoleTableRenderer().Render(table));
}

static void PrintVerification(string name, string actual, string expected)
{
    var status = actual == expected ? "ok (совпадает)" : "ошибка (не совпадает)";
    Console.WriteLine($"{name}: {status}");
}

static TableDefinition BuildStatisticsTable(FastMultiplicationResult result)
{
    var sizes = new List<int>();
    for (var size = 1; size <= result.PaddedDigitCount; size *= 2)
    {
        sizes.Add(size);
    }

    var builder = new TableBuilder("Таблица трудоемкости метода быстрого умножения по разрядностям")
        .AddColumn("Показатель");

    foreach (var size in sizes)
    {
        builder.AddColumn($"Разрядность N = {size}");
    }

    AddStatisticsRow(builder, "Количество сложений (без переполнения)", sizes, size => result.Statistics.GetAdditionsWithoutOverflow(size));
    AddStatisticsRow(builder, "Количество сложений (с переполнением)", sizes, size => result.Statistics.GetAdditionsWithOverflow(size));
    AddStatisticsRow(builder, "Количество умножений (без переполнения)", sizes, size => result.Statistics.GetMultiplicationsWithoutOverflow(size));
    AddStatisticsRow(builder, "Количество умножений (с переполнением)", sizes, size => result.Statistics.GetMultiplicationsWithOverflow(size));

    var currentWorkloads = sizes.ToDictionary(
        size => size,
        size => result.Statistics.GetAdditionsWithoutOverflow(size)
            + result.Statistics.GetAdditionsWithOverflow(size)
            + result.Statistics.GetMultiplicationsWithoutOverflow(size)
            + result.Statistics.GetMultiplicationsWithOverflow(size));

    AddStatisticsRow(builder, "Трудоемкость на текущем разряде", sizes, size => currentWorkloads[size]);

    var accumulated = 0L;
    var accumulatedWorkloads = new Dictionary<int, long>();
    foreach (var size in sizes)
    {
        accumulated += currentWorkloads[size];
        accumulatedWorkloads[size] = accumulated;
    }

    AddStatisticsRow(builder, "Общая трудоемкость (накопленная)", sizes, size => accumulatedWorkloads[size]);
    return builder.Build();
}

static void AddStatisticsRow(
    TableBuilder builder,
    string label,
    IReadOnlyList<int> sizes,
    Func<int, long> valueSelector)
{
    var cells = new object?[sizes.Count + 1];
    cells[0] = label;
    for (var i = 0; i < sizes.Count; i++)
    {
        cells[i + 1] = valueSelector(sizes[i]);
    }

    builder.AddRow(cells);
}
