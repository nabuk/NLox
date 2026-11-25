namespace NLox.Domain;

public class EnvironmentScope(EnvironmentScope? enclosing = null)
{
    private readonly EnvironmentScope? enclosing = enclosing;
    private readonly Dictionary<string, object?> values = new ();

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

    public object? GetAt(int distance, string name) => 
        Ancestor(distance).values[name];
    
    public void Assign(Token name, object? value)
    {
        if (values.ContainsKey(name.Lexeme))
            values[name.Lexeme] = value;
        else if (enclosing != null)
            enclosing.Assign(name, value);
        else
            throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'");
    }

    public void AssignAt(int distance, Token name, object? value) =>
        Ancestor(distance).values[name.Lexeme] = value;

    private EnvironmentScope Ancestor(int distance)
    {
        var environment = this;
        for (var i = 0; i < distance; i++)
            environment = environment!.enclosing;

        return environment!;
    }
}