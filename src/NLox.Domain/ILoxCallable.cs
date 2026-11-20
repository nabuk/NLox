namespace NLox.Domain;

public interface ILoxCallable
{
    int Arity { get; }
    object? Call(Interpreter interpreter, IReadOnlyList<object?> arguments);
}