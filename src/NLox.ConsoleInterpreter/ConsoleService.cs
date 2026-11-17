using NLox.Domain;

namespace NLox.ConsoleInterpreter;

public class ConsoleService : IConsole
{
    public void PrintLine(string line)
    {
        Console.WriteLine(line);
    }
}