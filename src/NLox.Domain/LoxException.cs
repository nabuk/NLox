namespace NLox.Domain;

public class LoxException : Exception
{
    public LoxException(int exitCode)
    {
        ExitCode = exitCode;
    }
    
    public int ExitCode { get; }
}