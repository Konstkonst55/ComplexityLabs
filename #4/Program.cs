using _4.Algorithms;

Console.WriteLine("1 - Умножение столбиком O(N^2)");
Console.WriteLine("2 - Быстрое умножение O(N^1.58)");
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
    if (mode == "1")
    {
        PrintColumn(first, second);
    }
    else if (mode == "2")
    {
        PrintFast(first, second);
    }
    else
    {
        Console.WriteLine("Ошибка: выберите режим 1 или 2.");
    }
}
catch (ArgumentException exception)
{
    Console.WriteLine($"Ошибка: {exception.Message}");
}

static void PrintColumn(string first, string second)
{
    var algorithm = new ColumnMultiplication();
    var result = algorithm.Multiply(first, second);

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

static void PrintFast(string first, string second)
{
    var algorithm = new FastMultiplication();
    var result = algorithm.Multiply(first, second);
    var width = GetTableWidth(result);

    Console.WriteLine();
    Console.WriteLine($"Быстрое умножение {first} на {second}");
    Console.WriteLine($"Результат быстрого умножения: {result.Value}");
    Console.WriteLine();
    Console.WriteLine("Таблица трудоемкости по разрядам:");
    PrintTable(result, width);
    Console.WriteLine();
    Console.WriteLine("Трудоемкость быстрого умножения оценивается как O(N^1.58).");
}

static void PrintTable(FastMultiplicationResult result, int width)
{
    var widths = Enumerable.Range(0, width <= 64 ? 7 : 7).Select(i => 1 << i).ToArray();
    Console.Write("Показатель".PadRight(38));
    foreach (var n in widths)
    {
        Console.Write($"{n,12}");
    }
    Console.WriteLine();

    PrintRow("Умножения без переполнения", result.MultiplicationsWithoutOverflow, widths);
    PrintRow("Умножения с переполнением", result.MultiplicationsWithOverflow, widths);
    PrintRow("Сложения без переполнения", result.AdditionsWithoutOverflow, widths);
    PrintRow("Сложения с переполнением", result.AdditionsWithOverflow, widths);
    PrintTotalRow("Общая трудоемкость без переполнения", result.MultiplicationsWithoutOverflow, result.AdditionsWithoutOverflow, widths);
    PrintTotalRow("Общая трудоемкость с переполнением", result.MultiplicationsWithOverflow, result.AdditionsWithOverflow, widths);
}

static void PrintRow(string name, IReadOnlyDictionary<int, long> values, int[] widths)
{
    Console.Write(name.PadRight(38));
    foreach (var width in widths)
    {
        Console.Write($"{GetValue(values, width),12}");
    }
    Console.WriteLine();
}

static void PrintTotalRow(string name, IReadOnlyDictionary<int, long> multiplications, IReadOnlyDictionary<int, long> additions, int[] widths)
{
    Console.Write(name.PadRight(38));
    foreach (var width in widths)
    {
        Console.Write($"{GetValue(multiplications, width) + GetValue(additions, width),12}");
    }
    Console.WriteLine();
}

static long GetValue(IReadOnlyDictionary<int, long> values, int width)
{
    return values.TryGetValue(width, out var value) ? value : 0;
}

static int GetTableWidth(FastMultiplicationResult result)
{
    var maxWidth = result.MultiplicationsWithoutOverflow.Keys
        .Concat(result.MultiplicationsWithOverflow.Keys)
        .Concat(result.AdditionsWithoutOverflow.Keys)
        .Concat(result.AdditionsWithOverflow.Keys)
        .DefaultIfEmpty(1)
        .Max();

    return Math.Min(64, Math.Max(1, maxWidth));
}
