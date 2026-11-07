namespace NLox.Domain.Contracts;

public interface IErrorReporter
{
    bool HadError { get; }

    void ResetHadError();
    
    void Error(int line, string message);
}