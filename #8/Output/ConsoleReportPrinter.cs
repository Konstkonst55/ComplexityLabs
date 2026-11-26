using Tables.Tables;

namespace _8.Output;

public sealed class ConsoleReportPrinter
{
    private readonly ConsoleTableRenderer _tableRenderer;

    public ConsoleReportPrinter(ConsoleTableRenderer? tableRenderer = null)
    {
        _tableRenderer = tableRenderer ?? new ConsoleTableRenderer();
    }

    public void Print(AlgorithmReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        foreach (var block in report.Blocks)
        {
            switch (block)
            {
                case TextBlock textBlock:
                    Console.WriteLine(textBlock.Text);
                    break;
                case TableBlock tableBlock:
                    Console.WriteLine(_tableRenderer.Render(tableBlock.Table));
                    break;
                default:
                    throw new InvalidOperationException($"Неизвестный тип блока отчёта: {block.GetType().Name}.");
            }
        }
    }
}
