using NLox.Domain;

namespace NLox.Interpreter;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var errorReporter = new StandardErrorReporter();
        var consoleRunner = new ConsoleRunner(errorReporter);
        
        try
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: nlox [script]");
                throw new LoxException(64);
            }
            else if (args.Length == 1)
            {
                await consoleRunner.RunFile(args[0]);
            }
            else
            {
                consoleRunner.RunPrompt();
            }
        }
        catch (LoxException ex)
        {
            return ex.ExitCode;
        }

        return 0;
    }
}
