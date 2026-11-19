namespace NLox.Domain;

public class EnvironmentScope(EnvironmentScope? enclosing = null)
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
            : enclosing != null
                ? enclosing.Get(name)
                : throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'");
    }
    
    public void Assign(Token name, object? value)
    {
        if (values.ContainsKey(name.Lexeme))
            values[name.Lexeme] = value;
        else if (enclosing != null)
            enclosing.Assign(name, value);
        else
            throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'");
    }
}