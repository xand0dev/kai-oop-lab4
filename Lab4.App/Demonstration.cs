using Lab4.Arithmetic;
using Lab4.Delegates;

namespace Lab4.App;

public static class Demonstration
{
    public static int[,] SampleMatrix() => new[,]
    {
        { 5, -2, 7, 1 },
        { 3, 8, -4, 6 },
        { 10, 0, 2, -9 }
    };

    public static void RunAll(TextWriter output)
    {
        RunDelegates(output);
        output.WriteLine();
        RunEvents(output);
    }

    public static void RunDelegates(TextWriter output)
    {
        output.WriteLine("=== Частина 1. Делегат MatrixSum: сума елементів двовимірного масиву ===");
        int[,] matrix = SampleMatrix();
        output.WriteLine($"Матриця {matrix.GetLength(0)}x{matrix.GetLength(1)}:");
        output.WriteLine(MatrixSummation.Format(matrix));

        MatrixSum anonymous = MatrixSummation.AnonymousMethod;
        MatrixSum lambda = MatrixSummation.Lambda;
        MatrixSum named = MatrixSummation.SumByRows;
        output.WriteLine($"Анонімний метод через делегат:        {anonymous(matrix)}");
        output.WriteLine($"Лямбда-вираз через делегат:           {lambda.Invoke(matrix)}");
        output.WriteLine($"Лямбда-вираз через Func<int[,], long>: {MatrixSummation.ExpressionLambda(matrix)}");
        output.WriteLine($"Іменований метод SumByRows:           {named(matrix)}");
        output.WriteLine($"Лямбда, передана як аргумент:         {MatrixSummation.Calculate(matrix, m => m.Cast<int>().Where(v => v > 0).Sum())} (лише додатні)");

        int[,] large = { { int.MaxValue, int.MaxValue }, { int.MaxValue, 1 } };
        output.WriteLine($"Великі значення: сума {anonymous(large)} перевищує int.MaxValue, тому результат має тип long.");

        // Замикання: лямбда захоплює зовнішню змінну і змінює її — побічний ефект.
        int calls = 0;
        MatrixSum counting = m => { calls++; return MatrixSummation.Lambda(m); };
        counting(matrix);
        counting(matrix);
        output.WriteLine($"Замикання: лямбда змінила зовнішню змінну calls = {calls}.");

        MatrixSum multicast = anonymous + lambda;
        output.WriteLine($"Мультиделегат містить {multicast.GetInvocationList().Length} методи; повертається результат останнього: {multicast(matrix)}.");
    }

    public static void RunEvents(TextWriter output)
    {
        output.WriteLine("=== Частина 2. Подія Overflow: переповнення розрядної сітки ===");
        var register = new IntegerArithmetic(8, "8-бітний регістр");
        var notifier = new OverflowNotifier(output);
        var journal = new OverflowJournal();
        var printer = new OperationPrinter(output);

        register.Overflow += notifier.HandleOverflow;
        register.Overflow += journal.HandleOverflow;
        register.OperationCompleted += printer.HandleOperation;
        output.WriteLine($"{register.Name}: діапазон [{register.MinValue}; {register.MaxValue}]. Підписано 2 обробники Overflow.");

        register.Add(100, 27);
        register.Add(100, 28);
        register.Subtract(-100, 29);
        register.Multiply(16, 8);
        register.Divide(50, 7);
        register.Divide(-128, -1);
        register.Negate(-128);
        register.Power(2, 6);
        register.Power(2, 7);

        register.Overflow -= notifier.HandleOverflow;
        output.WriteLine("Обробник повідомлень відписано (-=). Журнал продовжує отримувати подію:");
        register.Add(120, 10);

        try
        {
            register.Divide(1, 0);
        }
        catch (DivideByZeroException error)
        {
            output.WriteLine("  Ділення на нуль не є переповненням: " + error.Message);
        }

        output.WriteLine($"Операцій: {register.OperationCount}, переповнень: {register.OverflowCount}. Журнал:");
        foreach (string record in journal.Records)
            output.WriteLine("  " + record);

        output.WriteLine();
        var int32 = new IntegerArithmetic(32, "32-бітний int");
        int32.Overflow += notifier.HandleOverflow;
        int result = int32.Add(int.MaxValue, 1);
        output.WriteLine($"Порівняння з C#: unchecked(int.MaxValue + 1) = {unchecked(int.MaxValue + 1)}, компонент повернув {result}.");
    }
}
