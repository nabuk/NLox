namespace NLox.Domain;

public sealed class NativeLoxCallable(Func<Interpreter, IReadOnlyList<object?>, object?> callback, int arity) : ILoxCallable
{
    public int Arity => arity;
    
    public object? Call(Interpreter interpreter, IReadOnlyList<object?> arguments) =>
        callback(interpreter, arguments);

    public override string ToString() => "<native fn>";
}