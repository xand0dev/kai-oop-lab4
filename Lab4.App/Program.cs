using System.Text;

namespace Lab4.App;

internal static class Program
{
    private static int Main(string[] args)
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = new UTF8Encoding(false);

        if (args.Length == 1 && args[0] == "--demo")
        {
            Demonstration.RunAll(Console.Out);
            return 0;
        }
        if (args.Length != 0)
        {
            Console.Error.WriteLine("Запуск: dotnet run або dotnet run -- --demo");
            return 1;
        }

        new ConsoleMenu(Console.In, Console.Out).Run();
        return 0;
    }
}
