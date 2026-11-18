namespace NLox.Domain;

public class EnvironmentVars
{
    private readonly Dictionary<string, object?> values = new Dictionary<string, object?>();

    public void Define(string name, object? value)
    {
        values[name] = value;
    }
    
    public object? Get(Token name)
    {
        return values.TryGetValue(name.Lexeme, out var value)
            ? value
            : throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'");
    }
}