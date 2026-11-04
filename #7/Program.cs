using _7.Algorithms;
using _7.Models;
using _7.Output;

internal static class Program
{
    private static readonly WeightedGraph ShortestPathGraph = WeightedGraph.FromAdjacencyMatrix(
        new long?[,]
        {
            { 0,    25,   15,   7,    2 },
            { 25,   0,    6,    null, null },
            { 15,   6,    0,    4,    null },
            { 7,    null, 4,    0,    3 },
            { 2,    null, null, 3,    0 }
        },
        firstVertex: 0,
        isDirected: false);

    private const int DijkstraStartVertex = 3;
    private const int BellmanFordStartVertex = 0;

    private static readonly WeightedGraph KruskalGraph = new(
        vertices: Enumerable.Range(1, 7),
        edges: new WeightedEdge[]
        {
            new(1, 2, 20),
            new(1, 6, 23),
            new(1, 7, 1),
            new(2, 3, 5),
            new(2, 7, 4),
            new(3, 4, 3),
            new(3, 7, 9),
            new(4, 5, 17),
            new(4, 7, 16),
            new(5, 6, 28),
            new(5, 7, 25),
            new(6, 7, 36)
        },
        isDirected: false);

    private static void Main()
    {
        var printer = new ConsoleReportPrinter();

        while (true)
        {
            Console.WriteLine("Графовые алгоритмы");
            Console.WriteLine("1  Дейкстра");
            Console.WriteLine("2  Форд–Беллман");
            Console.WriteLine("3  Краскал");
            Console.WriteLine("0  Выход");
            Console.Write("Метод: ");

            var choice = Console.ReadLine()?.Trim();
            if (choice == "0")
                return;

            try
            {
                var report = choice switch
                {
                    "1" => new DijkstraAlgorithm().Execute(ShortestPathGraph, DijkstraStartVertex),
                    "2" => new BellmanFordAlgorithm().Execute(ShortestPathGraph, BellmanFordStartVertex),
                    "3" => new KruskalAlgorithm().Execute(KruskalGraph),
                    _ => null
                };

                if (report is null)
                {
                    Console.WriteLine("Выберите один из пунктов меню.");
                }
                else
                {
                    Console.WriteLine();
                    printer.Print(report);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Ошибка: {exception.Message}");
            }

            Console.WriteLine();
            Console.Write("Enter — вернуться в меню...");
            Console.ReadLine();
            Console.Clear();
        }
    }
}
