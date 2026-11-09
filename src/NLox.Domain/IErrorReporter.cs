namespace NLox.Domain;

public interface IErrorReporter
{
    bool HadError { get; }

    void ResetHadError();
    
    void Error(int line, string message);
    
    void Error(Token token, string message);
}