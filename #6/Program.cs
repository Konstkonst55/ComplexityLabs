using System.Globalization;
using _6.Algorithms;
using _6.Models;
using _6.Presentation;

Console.WriteLine("Задача о неограниченном рюкзаке");
Console.WriteLine("Введите массы товаров через запятую, например: 3, 5, 8");
Console.Write("Массы товаров m: ");
var massesInput = Console.ReadLine();
Console.WriteLine("Введите стоимости товаров через запятую, например: 8, 14, 23");
Console.Write("Стоимости товаров c: ");
var costsInput = Console.ReadLine();
Console.Write("Грузоподъемность M: ");
var capacityInput = Console.ReadLine();

try
{
    var masses = ParseIntegerList(massesInput, "массы");
    var costs = ParseDecimalList(costsInput, "стоимости");

    if (masses.Count != costs.Count)
    {
        throw new ArgumentException("Количество масс должно совпадать с количеством стоимостей.");
    }

    if (!int.TryParse(capacityInput, NumberStyles.Integer, CultureInfo.InvariantCulture, out var capacity))
    {
        throw new ArgumentException("Грузоподъемность должна быть целым числом.");
    }

    var items = masses.Select((mass, index) => new KnapsackItem(index + 1, mass, costs[index])).ToArray();
    var solver = new UnboundedKnapsackSolver();
    var result = solver.Solve(items, capacity);
    new KnapsackConsolePrinter().PrintResult(items, result);
}
catch (ArgumentException exception)
{
    Console.WriteLine($"Ошибка: {exception.Message}");
}

static List<int> ParseIntegerList(string? input, string fieldName)
{
    if (string.IsNullOrWhiteSpace(input))
    {
        throw new ArgumentException($"Необходимо ввести {fieldName} товаров.");
    }

    var parts = input.Split(',', StringSplitOptions.TrimEntries);
    var values = new List<int>(parts.Length);

    foreach (var part in parts)
    {
        if (!int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            throw new ArgumentException($"Каждое значение поля «{fieldName}» должно быть целым числом.");
        }

        values.Add(value);
    }

    return values;
}

static List<decimal> ParseDecimalList(string? input, string fieldName)
{
    if (string.IsNullOrWhiteSpace(input))
    {
        throw new ArgumentException($"Необходимо ввести {fieldName} товаров.");
    }

    var parts = input.Split(',', StringSplitOptions.TrimEntries);
    var values = new List<decimal>(parts.Length);

    foreach (var part in parts)
    {
        if (!decimal.TryParse(part, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
        {
            throw new ArgumentException($"Каждое значение поля «{fieldName}» должно быть числом. Для дробной части используйте точку.");
        }

        values.Add(value);
    }

    return values;
}
