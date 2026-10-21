using System.Numerics;

namespace _4.Algorithms;

public sealed class FastMultiplication
{
    public FastMultiplicationResult Multiply(string first, string second, bool captureTrace = true)
    {
        Validate(first, nameof(first));
        Validate(second, nameof(second));

        var x = BigInteger.Parse(first);
        var y = BigInteger.Parse(second);
        var digits = NextPowerOfTwo(Math.Max(first.Length, second.Length));
        var context = new CalculationContext(captureTrace);
        var result = MultiplyRecursive(x, y, digits, string.Empty, 0, context);

        return new FastMultiplicationResult(
            result.ToString(),
            digits,
            context.Trace,
            context.Statistics);
    }

    private static BigInteger MultiplyRecursive(
        BigInteger x,
        BigInteger y,
        int digits,
        string label,
        int indent,
        CalculationContext context)
    {
        if (digits <= 1)
        {
            var product = x * y;
            Add(context.Statistics.MultiplicationsWithoutOverflow, 1, 1);
            AddTrace(context, indent, label.Length == 0
                ? $"{x} * {y} = {product} (одноразрядное умножение без переполнения)"
                : $"{label} = {x} * {y} = {product} (одноразрядное умножение без переполнения)");
            return product;
        }

        var halfDigits = digits / 2;
        var power = BigInteger.Pow(10, halfDigits);
        var powerSquared = power * power;
        var a = x / power;
        var b = x % power;
        var c = y / power;
        var d = y % power;
        var sumA = a + b;
        var sumC = c + d;
        var overflowA = sumA >= power;
        var overflowC = sumC >= power;
        var overflowCount = (overflowA ? 1 : 0) + (overflowC ? 1 : 0);

        Add(context.Statistics.AdditionsWithoutOverflow, digits, 6);

        if (digits >= 4)
        {
            Add(context.Statistics.AdditionsWithOverflow, digits * 2, overflowCount);
        }
        else if (digits == 2)
        {
            Add(context.Statistics.AdditionsWithOverflow, 4, overflowCount);
            Add(context.Statistics.AdditionsWithOverflow, 2, overflowCount * 2L);
        }

        if (label.Length > 0)
        {
            AddTrace(context, indent, $"Декомпозиция {digits}-разрядных чисел: {x} * {y} (k = {halfDigits})");
        }

        AddTrace(context, indent, $"x = {a} * 10^{halfDigits} + {b}, y = {c} * 10^{halfDigits} + {d}");

        var groupNumber = label.Length == 0 ? 0 : context.NextGroupNumber++;
        var uLabel = groupNumber == 0 ? "U" : $"U{groupNumber}";
        var vLabel = groupNumber == 0 ? "V" : $"V{groupNumber}";
        var wLabel = groupNumber == 0 ? "W" : $"W{groupNumber}";

        BigInteger u;
        if (overflowCount > 0)
        {
            Add(context.Statistics.MultiplicationsWithOverflow, halfDigits, 1);
            var countText = overflowCount == 1 ? "одним переполнением" : "двумя переполнениями";
            AddTrace(context, indent, $"{uLabel} = ({a} + {b}) * ({c} + {d}) = {sumA} * {sumC} ({halfDigits}-разрядное умножение с {countText})");
            u = MultiplyOverflowProduct(sumA, sumC, halfDigits, uLabel, indent + 1, context);
        }
        else
        {
            AddTrace(context, indent, $"{uLabel} = ({a} + {b}) * ({c} + {d}) = {sumA} * {sumC} ({halfDigits}-разрядное умножение без переполнения)");
            u = MultiplyRecursive(sumA, sumC, halfDigits, uLabel, indent + 1, context);
        }

        AddTrace(context, indent, $"{vLabel} = {a} * {c} ({halfDigits}-разрядное умножение без переполнения)");
        var v = MultiplyRecursive(a, c, halfDigits, vLabel, indent + 1, context);

        AddTrace(context, indent, $"{wLabel} = {b} * {d} ({halfDigits}-разрядное умножение без переполнения)");
        var w = MultiplyRecursive(b, d, halfDigits, wLabel, indent + 1, context);

        var middle = u - v - w;
        var result = v * powerSquared + middle * power + w;
        var displayLabel = label.Length == 0 ? $"{x} * {y}" : label;
        AddTrace(context, indent, $"{displayLabel} = {v} * 10^{2 * halfDigits} + ({u} - {v} - {w}) * 10^{halfDigits} + {w} = {result}");
        return result;
    }

    private static BigInteger MultiplyOverflowProduct(
        BigInteger first,
        BigInteger second,
        int digits,
        string label,
        int indent,
        CalculationContext context)
    {
        var power = BigInteger.Pow(10, digits);
        var powerSquared = power * power;
        var highA = first / power;
        var lowA = first % power;
        var highB = second / power;
        var lowB = second % power;

        AddTrace(context, indent, $"Получены числа с дополнительным разрядом для {label}.");
        AddTrace(context, indent, $"a1 = {highA}, a2 = {lowA}; b1 = {highB}, b2 = {lowB}.");

        BigInteger lowProduct;
        if (digits > 1)
        {
            AddTrace(context, indent, $"Вычислим младшее произведение {lowA} * {lowB}:");
            lowProduct = MultiplyRecursive(lowA, lowB, digits, $"{lowA} * {lowB}", indent + 1, context);
        }
        else
        {
            lowProduct = lowA * lowB;
        }

        var cross = highA * lowB + lowA * highB;
        var result = highA * highB * powerSquared + cross * power + lowProduct;
        AddTrace(context, indent, $"{label} = {highA} * {highB} * 10^{2 * digits} + ({highA} * {lowB} + {lowA} * {highB}) * 10^{digits} + {lowA} * {lowB} = {result}");
        return result;
    }

    private static void AddTrace(CalculationContext context, int indent, string text)
    {
        if (context.CaptureTrace)
        {
            context.Trace.Add(new MultiplicationTraceStep(indent, text));
        }
    }

    private static void Add(Dictionary<int, long> counters, int digits, long value)
    {
        if (value == 0)
        {
            return;
        }

        counters[digits] = counters.TryGetValue(digits, out var current) ? current + value : value;
    }

    private static int NextPowerOfTwo(int value)
    {
        var result = 1;
        while (result < value)
        {
            result *= 2;
        }

        return result;
    }

    private static void Validate(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Число не должно быть пустым.", parameterName);
        }

        foreach (var character in value)
        {
            if (character is < '0' or > '9')
            {
                throw new ArgumentException("Допускаются только неотрицательные целые числа.", parameterName);
            }
        }
    }

    private sealed class CalculationContext
    {
        public CalculationContext(bool captureTrace)
        {
            CaptureTrace = captureTrace;
        }

        public bool CaptureTrace { get; }
        public List<MultiplicationTraceStep> Trace { get; } = new();
        public FastMultiplicationStatistics Statistics { get; } = new();
        public int NextGroupNumber { get; set; } = 1;
    }
}
