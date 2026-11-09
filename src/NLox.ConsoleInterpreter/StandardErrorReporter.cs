using NLox.Domain;

namespace NLox.ConsoleInterpreter;

public sealed class StandardErrorReporter : IErrorReporter
{
    public bool HadError { get; private set; }
    
    public bool HadRuntimeError { get; private set; }

    public void ResetHadError()
    {
        HadError = false;
    }
    
    public void ResetHadRuntimeError()
    {
        HadRuntimeError = false;
    }

    public void Error(int line, string message)
    {
        Report(line, "",  message);
    }
    
    public void Error(Token token, string message)
    {
        Report(
            token.Line,
            token.Type == TokenType.Eof ? " at end" : $" at '{token.Lexeme}'",
            message);
    }

    public void RuntimeError(RuntimeError error)
    {
        Console.Error.WriteLine("error.Message");
        Console.Error.WriteLine($"[line {error.Token.Line}]");
        HadRuntimeError = true;
    }

    private void Report(int line, string where, string message)
    {
        Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
        HadError = true;
    }
}