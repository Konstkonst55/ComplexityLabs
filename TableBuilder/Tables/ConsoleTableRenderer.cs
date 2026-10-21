using System.Text;

namespace Tables.Tables;

public sealed class ConsoleTableRenderer
{
    public string Render(TableDefinition table)
    {
        ArgumentNullException.ThrowIfNull(table);

        var widths = new int[table.Headers.Count];
        for (var column = 0; column < widths.Length; column++)
        {
            widths[column] = table.Headers[column].Length;
        }

        foreach (var row in table.Rows)
        {
            for (var column = 0; column < widths.Length; column++)
            {
                widths[column] = Math.Max(widths[column], row[column].Length);
            }
        }

        var border = BuildBorder(widths);
        var output = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(table.Title))
        {
            output.AppendLine(table.Title);
        }

        output.AppendLine(border);
        output.AppendLine(BuildRow(table.Headers, widths));
        output.AppendLine(border);

        foreach (var row in table.Rows)
        {
            output.AppendLine(BuildRow(row, widths));
        }

        output.Append(border);
        return output.ToString();
    }

    private static string BuildBorder(IReadOnlyList<int> widths)
    {
        return "+" + string.Join("+", widths.Select(width => new string('-', width + 2))) + "+";
    }

    private static string BuildRow(IReadOnlyList<string> cells, IReadOnlyList<int> widths)
    {
        var formatted = cells.Select((cell, index) => " " + cell.PadRight(widths[index]) + " ");
        return "|" + string.Join("|", formatted) + "|";
    }
}
