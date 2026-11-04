namespace _7.Models;

public sealed class WeightedGraph
{
    private readonly Dictionary<int, int> indexes;
    private readonly long?[,] weights;

    public WeightedGraph(IEnumerable<int> vertices, IEnumerable<WeightedEdge> edges, bool isDirected)
    {
        ArgumentNullException.ThrowIfNull(vertices);
        ArgumentNullException.ThrowIfNull(edges);

        var vertexArray = vertices.ToArray();
        if (vertexArray.Length == 0)
            throw new ArgumentException("Граф должен содержать хотя бы одну вершину.", nameof(vertices));
        if (vertexArray.Distinct().Count() != vertexArray.Length)
            throw new ArgumentException("Идентификаторы вершин должны быть уникальными.", nameof(vertices));

        Vertices = vertexArray;
        IsDirected = isDirected;
        indexes = vertexArray.Select((vertex, index) => (vertex, index))
            .ToDictionary(item => item.vertex, item => item.index);
        weights = new long?[vertexArray.Length, vertexArray.Length];
        for (var i = 0; i < vertexArray.Length; i++)
            weights[i, i] = 0;

        var edgeArray = edges.ToArray();
        foreach (var edge in edgeArray)
        {
            if (!indexes.ContainsKey(edge.From) || !indexes.ContainsKey(edge.To))
                throw new ArgumentException($"Ребро {edge.From}--{edge.To} содержит неизвестную вершину.", nameof(edges));

            SetMinimumWeight(edge.From, edge.To, edge.Weight);
            if (!isDirected && edge.From != edge.To)
                SetMinimumWeight(edge.To, edge.From, edge.Weight);
        }

        Edges = edgeArray;
    }

    public IReadOnlyList<int> Vertices { get; }
    public IReadOnlyList<WeightedEdge> Edges { get; }
    public bool IsDirected { get; }
    public int VertexCount => Vertices.Count;

    public long? GetWeight(int from, int to)
    {
        if (!indexes.TryGetValue(from, out var fromIndex))
            throw new ArgumentOutOfRangeException(nameof(from), $"Вершина {from} отсутствует в графе.");
        if (!indexes.TryGetValue(to, out var toIndex))
            throw new ArgumentOutOfRangeException(nameof(to), $"Вершина {to} отсутствует в графе.");

        return weights[fromIndex, toIndex];
    }

    public bool ContainsVertex(int vertex) => indexes.ContainsKey(vertex);

    public long?[,] ToAdjacencyMatrix()
    {
        var result = new long?[VertexCount, VertexCount];
        Array.Copy(weights, result, weights.Length);
        return result;
    }

    public static WeightedGraph FromAdjacencyMatrix(long?[,] matrix, int firstVertex = 0, bool isDirected = false)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        var rowCount = matrix.GetLength(0);
        var columnCount = matrix.GetLength(1);
        if (rowCount == 0 || rowCount != columnCount)
            throw new ArgumentException("Матрица смежности должна быть непустой и квадратной.", nameof(matrix));

        if (!isDirected)
        {
            for (var i = 0; i < rowCount; i++)
            for (var j = i + 1; j < columnCount; j++)
            {
                if (matrix[i, j] != matrix[j, i])
                    throw new ArgumentException("Матрица неориентированного графа должна быть симметричной.", nameof(matrix));
            }
        }

        var vertices = Enumerable.Range(firstVertex, rowCount).ToArray();
        var edges = new List<WeightedEdge>();

        for (var i = 0; i < rowCount; i++)
        {
            if (matrix[i, i] is long diagonal && diagonal < 0)
                edges.Add(new WeightedEdge(vertices[i], vertices[i], diagonal));

            var startColumn = isDirected ? 0 : i + 1;
            for (var j = startColumn; j < columnCount; j++)
            {
                if (i == j || matrix[i, j] is not long weight)
                    continue;

                edges.Add(new WeightedEdge(vertices[i], vertices[j], weight));
            }
        }

        return new WeightedGraph(vertices, edges, isDirected);
    }

    private void SetMinimumWeight(int from, int to, long weight)
    {
        var fromIndex = indexes[from];
        var toIndex = indexes[to];
        if (weights[fromIndex, toIndex] is null || weight < weights[fromIndex, toIndex])
            weights[fromIndex, toIndex] = weight;
    }
}
