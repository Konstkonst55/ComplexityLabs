namespace _4.Algorithms;

public interface ILongMultiplicationAlgorithm
{
    string Name { get; }
    MultiplicationResult Multiply(string first, string second);
}
