namespace NLox.Domain;

public class LoxError(int exitCode) : Exception
{
    public int ExitCode { get; } = exitCode;
}