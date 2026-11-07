using NLox.Domain.Contracts;

namespace NLox.Interpreter;

public sealed class StandardErrorReporter : IErrorReporter
{
    public bool HadError { get; private set; }

    public void ResetHadError()
    {
        HadError = false;
    }

    public void Error(int line, string message)
    {
        Report(line, "",  message);
    }

    private void Report(int line, string where, string message)
    {
        Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
        HadError = true;
    }
}