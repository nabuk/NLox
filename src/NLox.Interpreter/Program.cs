using System;
using NLox.Domain;

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

    private static void DemoPrintExpr()
    {
        Expr expression = new Expr.Binary(
            new Expr.Binary(
                new Expr.Literal(123),
                new Token(TokenType.Minus, "-", null, 1),
                new Expr.Literal(23)),
            new Token(TokenType.Star, "*", null, 1),
            new Expr.Grouping(
                new Expr.Literal(45.67)));

        Console.WriteLine("AST: " + new AstPrinter().Print(expression));
        Console.WriteLine("RPN: " + new RpnPrinter().Print(expression));
    }
}
