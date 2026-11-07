using NLox.Domain;
using NLox.Domain.Contracts;

namespace NLox.Interpreter;

public sealed class Interpreter(IErrorReporter errorReporter)
{
    public async Task RunFile(string path)
    {
        var content = await File.ReadAllTextAsync(path);
        Run(content);

        if (errorReporter.HadError)
            throw new LoxException(65);
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
        }
    }

    private void Run(string source)
    {
        var scanner = new Scanner(source, errorReporter);
        var tokens = scanner.ScanTokens();

        foreach (var token in tokens)
        {
            Console.WriteLine(token);
        }
    }
}