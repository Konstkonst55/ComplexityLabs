using Tables.Tables;

namespace _7.Output;

public sealed class ConsoleReportPrinter
{
    private readonly ConsoleTableRenderer tableRenderer = new();

    public void Print(AlgorithmReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        foreach (var block in report.Blocks)
        {
            switch (block)
            {
                case TextBlock text:
                    Console.WriteLine(text.Text);
                    break;
                case TableBlock table:
                    Console.WriteLine(tableRenderer.Render(table.Table));
                    break;
            }

            Console.WriteLine();
        }
    }
}
