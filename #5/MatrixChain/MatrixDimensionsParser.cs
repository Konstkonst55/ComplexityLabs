using System.Globalization;

namespace _5.MatrixChain;

public static class MatrixDimensionsParser
{
    public static int[] Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Необходимо ввести размеры хотя бы одной матрицы.", nameof(input));
        }

        input = input.Trim().Replace('х', 'x').Replace('Х', 'x').Replace('X', 'x');

        var repeated = System.Text.RegularExpressions.Regex.Match(
            input,
            @"^(?:([0-9]+)\s*\*\s*([0-9]+)\s*x\s*([0-9]+)|([0-9]+)\s+матриц(?:ы)?\s+([0-9]+)\s*x\s*([0-9]+))$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (repeated.Success)
        {
            var countText = repeated.Groups[1].Success ? repeated.Groups[1].Value : repeated.Groups[4].Value;
            var rowsText = repeated.Groups[2].Success ? repeated.Groups[2].Value : repeated.Groups[5].Value;
            var columnsText = repeated.Groups[3].Success ? repeated.Groups[3].Value : repeated.Groups[6].Value;

            if (!int.TryParse(countText, NumberStyles.None, CultureInfo.InvariantCulture, out var count) || count <= 0 || count > 500)
            {
                throw new ArgumentException("Количество матриц должно быть от 1 до 500.", nameof(input));
            }

            if (!int.TryParse(rowsText, NumberStyles.None, CultureInfo.InvariantCulture, out var rows) || rows <= 0 ||
                !int.TryParse(columnsText, NumberStyles.None, CultureInfo.InvariantCulture, out var columns) || columns <= 0)
            {
                throw new ArgumentException("Размеры матриц должны быть положительными целыми числами.", nameof(input));
            }

            if (count > 1 && rows != columns)
            {
                throw new ArgumentException(
                    "Одинаковые прямоугольные матрицы нельзя перемножить в цепочку. Для k матриц одинакового размера m x n требуется m = n; для прямоугольных матриц укажите размеры каждой матрицы отдельно.",
                    nameof(input));
            }

            var repeatedDimensions = new int[count + 1];
            repeatedDimensions[0] = rows;

            for (var index = 1; index <= count; index++)
            {
                repeatedDimensions[index] = columns;
            }

            return repeatedDimensions;
        }

        var tokens = input.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length == 0)
        {
            throw new ArgumentException("Не удалось распознать размеры матриц.", nameof(input));
        }

        var dimensions = new List<int>();
        for (var index = 0; index < tokens.Length; index++)
        {
            var token = tokens[index].Replace('х', 'x').Replace('Х', 'x').Replace('X', 'x');
            var pair = token.Split('x', StringSplitOptions.TrimEntries);

            if (pair.Length != 2 ||
                !int.TryParse(pair[0], NumberStyles.None, CultureInfo.InvariantCulture, out var rows) ||
                !int.TryParse(pair[1], NumberStyles.None, CultureInfo.InvariantCulture, out var columns) ||
                rows <= 0 || columns <= 0)
            {
                throw new ArgumentException(
                    $"Размер матрицы #{index + 1} задан неверно: '{tokens[index]}'. Используйте формат 3x7.",
                    nameof(input));
            }

            if (index == 0)
            {
                dimensions.Add(rows);
                dimensions.Add(columns);
            }
            else
            {
                if (dimensions[^1] != rows)
                {
                    throw new ArgumentException(
                        $"Матрицы нельзя перемножить: количество столбцов M{index} ({dimensions[^1]}) не совпадает с количеством строк M{index + 1} ({rows}).",
                        nameof(input));
                }

                dimensions.Add(columns);
            }
        }

        return dimensions.ToArray();
    }
}
