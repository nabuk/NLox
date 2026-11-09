using NLox.Domain;

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
        var parser = new Parser(tokens, errorReporter);
        var expression = parser.Parse();
        
        if (errorReporter.HadError)
            return;

        var printResult = new AstPrinter().Print(expression!);
        Console.WriteLine(printResult);
    }
}