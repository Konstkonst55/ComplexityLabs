using System.Numerics;

namespace _4.Algorithms;

public sealed class FastMultiplication
{
    private readonly Dictionary<int, long> _multiplicationsWithoutOverflow = new();
    private readonly Dictionary<int, long> _multiplicationsWithOverflow = new();
    private readonly Dictionary<int, long> _additionsWithoutOverflow = new();
    private readonly Dictionary<int, long> _additionsWithOverflow = new();

    public FastMultiplicationResult Multiply(string first, string second)
    {
        Validate(first);
        Validate(second);
        Reset();

        var x = BigInteger.Parse(first);
        var y = BigInteger.Parse(second);
        var width = Math.Max(GetBitLength(x), GetBitLength(y));
        width = Math.Max(1, NextPowerOfTwo(width));

        var result = MultiplyRecursive(x, y, width);
        return new FastMultiplicationResult(
            result.ToString(),
            new Dictionary<int, long>(_multiplicationsWithoutOverflow),
            new Dictionary<int, long>(_multiplicationsWithOverflow),
            new Dictionary<int, long>(_additionsWithoutOverflow),
            new Dictionary<int, long>(_additionsWithOverflow));
    }

    private BigInteger MultiplyRecursive(BigInteger x, BigInteger y, int n)
    {
        if (n == 1)
        {
            Add(_multiplicationsWithoutOverflow, 1, 1);
            return x * y;
        }

        var k = n / 2;
        var mask = (BigInteger.One << k) - 1;
        var a = x >> k;
        var b = x & mask;
        var c = y >> k;
        var d = y & mask;

        var v = MultiplyRecursive(a, c, k);
        var w = MultiplyRecursive(b, d, k);

        var sumA = a + b;
        var sumC = c + d;
        var overflow = sumA >= (BigInteger.One << k) || sumC >= (BigInteger.One << k);

        BigInteger u;
        if (!overflow)
        {
            u = MultiplyRecursive(sumA, sumC, k);
        }
        else
        {
            Add(_multiplicationsWithOverflow, k, 1);
            var a2 = sumA & mask;
            var b2 = sumC & mask;
            u = a2 * b2;
            var highA = sumA >> k;
            var highB = sumC >> k;

            if (highA != 0 && highB != 0)
            {
                Add(_additionsWithOverflow, k, 1);
            }
            else if (highA != 0 || highB != 0)
            {
                Add(_additionsWithOverflow, k, 1);
            }

            u += ((highA * b2) + (highB * a2)) << k;
            u += (highA * highB) << (2 * k);
        }

        Add(_additionsWithoutOverflow, n, 4);
        var middle = u - v - w;
        return (v << (2 * k)) + (middle << k) + w;
    }

    private void Reset()
    {
        _multiplicationsWithoutOverflow.Clear();
        _multiplicationsWithOverflow.Clear();
        _additionsWithoutOverflow.Clear();
        _additionsWithOverflow.Clear();
    }

    private static void Add(Dictionary<int, long> target, int width, long value)
    {
        target[width] = target.TryGetValue(width, out var current) ? current + value : value;
    }

    private static int GetBitLength(BigInteger value)
    {
        if (value.IsZero)
        {
            return 1;
        }

        var bytes = value.ToByteArray(isUnsigned: true, isBigEndian: true);
        var first = bytes[0];
        var leading = 0;
        while ((first & 0x80) == 0)
        {
            leading++;
            first <<= 1;
        }

        return bytes.Length * 8 - leading;
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

    private static void Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Число не должно быть пустым.");
        }

        foreach (var character in value)
        {
            if (character is < '0' or > '9')
            {
                throw new ArgumentException("Допускаются только неотрицательные целые числа.");
            }
        }
    }
}
