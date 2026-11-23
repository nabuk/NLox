namespace NLox.Domain;

public sealed class LoxFunction(Stmt.Function declaration) : ILoxCallable
{
    public int Arity => declaration.Parameters.Count;
    
    public object? Call(Interpreter interpreter, IReadOnlyList<object?> arguments)
    {
        var environment = new EnvironmentScope(interpreter.Globals);

        foreach (var pair in declaration.Parameters.Zip(arguments))
            environment.Define(pair.First.Lexeme, pair.Second);

        try
        {
            interpreter.ExecuteBlock(declaration.Body, environment);
        }
        catch (Return e)
        {
            return e.Value;
        }
        
        return null;
    }

    public override string ToString() => $"<fn {declaration.Name.Lexeme}>";
}