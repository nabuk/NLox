using NLox.Domain;

namespace NLox.ConsoleInterpreter;

public sealed class ConsoleRunner(IConsole console, IErrorReporter errorReporter)
{
    private readonly Interpreter interpreter = new(console, errorReporter);

    public async Task RunFile(string path)
    {
        var content = await File.ReadAllTextAsync(path);
        Run(content);

        if (errorReporter.HadError)
            throw new LoxError(65);
        
        if (errorReporter.HadRuntimeError)
            throw new LoxError(70);
    }

    public void RunPrompt()
    {
        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (line == null)
                break;

            Run(line);
            
            errorReporter.ResetHadError();
            errorReporter.ResetHadRuntimeError();
        }
    }

    private void Run(string source)
    {
        var scanner = new Scanner(source, errorReporter);
        var tokens = scanner.ScanTokens();
        var parser = new Parser(tokens, errorReporter);
        var resolver = new Resolver(interpreter, errorReporter);
        
        if (errorReporter.HadError)
            return;
        
        var statements = parser.Parse();
        if (errorReporter.HadError)
            return;
        
        resolver.Resolve(statements);
        if (errorReporter.HadError)
            return;
        
        interpreter.Interpret(statements);
    }
}