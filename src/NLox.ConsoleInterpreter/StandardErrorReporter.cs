using NLox.Domain;

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
    
    public void Error(Token token, string message)
    {
        Report(
            token.Line,
            token.Type == TokenType.Eof ? " at end" : $" at '{token.Lexeme}'",
            message);
    }

    private void Report(int line, string where, string message)
    {
        Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
        HadError = true;
    }
}