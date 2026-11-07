using System;
using NLox.Domain;
using NLox.Domain.Contracts;

namespace NLox.Interpreter;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var errorReporter = new StandardErrorReporter();
        var interpreter = new Interpreter(errorReporter);
        
        try
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: nlox [script]");
                throw new LoxException(64);
            }
            else if (args.Length == 1)
            {
                await interpreter.RunFile(args[0]);
            }
            else
            {
                interpreter.RunPrompt();
            }
        }
        catch (LoxException ex)
        {
            return ex.ExitCode;
        }

        return 0;
    }
}
