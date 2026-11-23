namespace NLox.Domain;

public class Return(object? value) : Exception
{
    public object? Value => value;
}