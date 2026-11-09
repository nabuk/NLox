namespace NLox.Domain;

public class LoxException(int exitCode) : Exception
{
    public int ExitCode { get; } = exitCode;
}