namespace NLox.Domain;

public interface IErrorReporter
{
    bool HadError { get; }
    
    bool HadRuntimeError { get; }

    void ResetHadError();
    
    void Error(int line, string message);
    
    void Error(Token token, string message);
    
    void RuntimeError(RuntimeError error);
    void ResetHadRuntimeError();
}