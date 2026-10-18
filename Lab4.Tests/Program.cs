using System.Numerics;
using Lab4.App;
using Lab4.Arithmetic;
using Lab4.Delegates;

namespace Lab4.Tests;

internal static class Program
{
    private static int _passed;
    private static int _failed;

    private static int Main()
    {
        Run("Анонімний метод, лямбда, Func та іменований метод дають однакову суму", () =>
        {
            int[,] matrix = Demonstration.SampleMatrix();
            Require(MatrixSummation.AnonymousMethod(matrix) == 27);
            Require(MatrixSummation.Lambda(matrix) == 27);
            Require(MatrixSummation.ExpressionLambda(matrix) == 27);
            Require(MatrixSummation.SumByRows(matrix) == 27);
            Require(MatrixSummation.Calculate(matrix, MatrixSummation.Lambda) == 27);
        });
        Run("Порожня, одноелементна та велика матриці", () =>
        {
            Require(MatrixSummation.Lambda(new int[0, 0]) == 0);
            Require(MatrixSummation.AnonymousMethod(new[,] { { -7 } }) == -7);
            int[,] large = { { int.MaxValue, int.MaxValue }, { int.MinValue, int.MaxValue } };
            long expected = 2L * int.MaxValue + int.MaxValue + (long)int.MinValue;
            Require(MatrixSummation.AnonymousMethod(large) == expected);
            Require(MatrixSummation.ExpressionLambda(large) == expected);
        });
        Run("null-матриця та null-делегат відхиляються", () =>
        {
            Throws<ArgumentNullException>(() => MatrixSummation.AnonymousMethod(null!));
            Throws<ArgumentNullException>(() => MatrixSummation.Lambda(null!));
            Throws<ArgumentNullException>(() => MatrixSummation.ExpressionLambda(null!));
            Throws<ArgumentNullException>(() => MatrixSummation.Calculate(new int[1, 1], null!));
        });
        Run("Мультиделегат повертає результат останнього методу", () =>
        {
            var order = new List<string>();
            MatrixSum first = m => { order.Add("first"); return 1; };
            MatrixSum second = m => { order.Add("second"); return 2; };
            MatrixSum both = first + second;
            Require(both(new int[1, 1]) == 2);
            Require(order.SequenceEqual(new[] { "first", "second" }));
            Require((both - second)!.GetInvocationList().Length == 1);
        });
        Run("Межі розрядної сітки", () =>
        {
            var eight = new IntegerArithmetic(8);
            Require(eight.MinValue == -128 && eight.MaxValue == 127);
            var two = new IntegerArithmetic(2);
            Require(two.MinValue == -2 && two.MaxValue == 1);
            var full = new IntegerArithmetic();
            Require(full.MinValue == int.MinValue && full.MaxValue == int.MaxValue);
            Throws<ArgumentOutOfRangeException>(() => new IntegerArithmetic(1));
            Throws<ArgumentOutOfRangeException>(() => new IntegerArithmetic(33));
            Throws<ArgumentException>(() => new IntegerArithmetic(8, " "));
        });
        Run("Операції без переповнення не генерують подію", () =>
        {
            var calculator = new IntegerArithmetic(8);
            int raised = 0;
            calculator.Overflow += (sender, e) => raised++;
            Require(calculator.Add(100, 27) == 127);
            Require(calculator.Subtract(-100, 28) == -128);
            Require(calculator.Multiply(-16, 8) == -128);
            Require(calculator.Divide(-7, 2) == -3);
            Require(calculator.Negate(-127) == 127);
            Require(calculator.Power(-2, 7) == -128);
            Require(calculator.Power(5, 0) == 1);
            Require(raised == 0 && calculator.OperationCount == 7 && calculator.OverflowCount == 0);
        });
        Run("Переповнення: подія, ініціатор та аргумент", () =>
        {
            var calculator = new IntegerArithmetic(8, "Тест");
            object? source = null;
            OverflowEventArgs? args = null;
            calculator.Overflow += (sender, e) => { source = sender; args = e; };
            int result = calculator.Add(100, 28);
            Require(result == -128);
            Require(ReferenceEquals(source, calculator));
            Require(args != null && args.Operation == ArithmeticOperation.Addition);
            Require(args!.Left == 100 && args.Right == 28 && args.ExactResult == 128 && args.WrappedResult == -128);
            Require(args.IsAboveMaximum && args.BitWidth == 8 && args.Expression == "100 + 28");
        });
        Run("Усі види переповнення в 8-бітній сітці", () =>
        {
            var calculator = new IntegerArithmetic(8);
            var exact = new List<BigInteger>();
            calculator.Overflow += (sender, e) => exact.Add(e.ExactResult);
            Require(calculator.Subtract(-100, 29) == 127);
            Require(calculator.Multiply(16, 8) == -128);
            Require(calculator.Divide(-128, -1) == -128);
            Require(calculator.Negate(-128) == -128);
            Require(calculator.Power(2, 7) == -128);
            Require(calculator.Multiply(127, 127) == 1);
            Require(exact.SequenceEqual(new BigInteger[] { -129, 128, 128, 128, 128, 16129 }));
            Require(calculator.OverflowCount == 6);
        });
        Run("32 біти збігаються з unchecked-арифметикою C#", () =>
        {
            var calculator = new IntegerArithmetic();
            int events = 0;
            calculator.Overflow += (sender, e) => events++;
            Require(calculator.Add(int.MaxValue, 1) == unchecked(int.MaxValue + 1));
            Require(calculator.Subtract(int.MinValue, 1) == unchecked(int.MinValue - 1));
            Require(calculator.Multiply(int.MaxValue, int.MaxValue) == unchecked(int.MaxValue * int.MaxValue));
            Require(calculator.Negate(int.MinValue) == int.MinValue);
            Require(calculator.Power(3, 1000) == Pow32(3, 1000));
            Require(events == 5);
        });
        Run("Ділення на нуль, неприпустимі операнди та степінь", () =>
        {
            var calculator = new IntegerArithmetic(8);
            int completed = 0;
            calculator.OperationCompleted += (sender, e) => completed++;
            Throws<DivideByZeroException>(() => calculator.Divide(1, 0));
            Throws<ArgumentOutOfRangeException>(() => calculator.Add(128, 0));
            Throws<ArgumentOutOfRangeException>(() => calculator.Add(0, -129));
            Throws<ArgumentOutOfRangeException>(() => calculator.Power(2, -1));
            Throws<ArgumentOutOfRangeException>(() => calculator.Power(2, IntegerArithmetic.MaxExponent + 1));
            Throws<ArgumentOutOfRangeException>(() => calculator.Calculate((ArithmeticOperation)99, 1, 1));
            Require(completed == 0 && calculator.OperationCount == 0);
            Require(calculator.Power(1, 1000) == 1);
        });
        Run("Calculate викликає відповідну операцію", () =>
        {
            var calculator = new IntegerArithmetic(16);
            Require(calculator.Calculate(ArithmeticOperation.Addition, 2, 3) == 5);
            Require(calculator.Calculate(ArithmeticOperation.Subtraction, 2, 3) == -1);
            Require(calculator.Calculate(ArithmeticOperation.Multiplication, 2, 3) == 6);
            Require(calculator.Calculate(ArithmeticOperation.Division, 7, 3) == 2);
            Require(calculator.Calculate(ArithmeticOperation.Negation, 2, 999) == -2);
            Require(calculator.Calculate(ArithmeticOperation.Power, 2, 3) == 8);
        });
        Run("Мультиадресна подія, порядок обробників та відписка", () =>
        {
            var calculator = new IntegerArithmetic(4, "Чотири біти");
            var output = new StringWriter();
            var notifier = new OverflowNotifier(output);
            var journal = new OverflowJournal();
            var order = new List<string>();
            calculator.Overflow += notifier.HandleOverflow;
            calculator.Overflow += journal.HandleOverflow;
            calculator.Overflow += (sender, e) => order.Add("lambda");
            calculator.OperationCompleted += (sender, e) => order.Add(e.Overflowed ? "completed-overflow" : "completed");

            calculator.Add(7, 1);
            Require(output.ToString().Contains("ПЕРЕПОВНЕННЯ") && output.ToString().Contains("Чотири біти, 4 біт"));
            Require(journal.Records.Count == 1 && journal.Records[0].Contains("7 + 1 = 8 -> -8"));
            Require(order.SequenceEqual(new[] { "lambda", "completed-overflow" }));

            calculator.Overflow -= notifier.HandleOverflow;
            string before = output.ToString();
            calculator.Subtract(-8, 1);
            Require(output.ToString() == before);
            Require(journal.Records.Count == 2);
            journal.Clear();
            Require(journal.Records.Count == 0);
        });
        Run("OperationEventArgs та форматування від'ємних операндів", () =>
        {
            var calculator = new IntegerArithmetic(8);
            OperationEventArgs? last = null;
            calculator.OperationCompleted += (sender, e) => last = e;
            calculator.Subtract(5, -3);
            Require(last != null && last.Result == 8 && !last.Overflowed && last.Expression == "5 - (-3)");
            calculator.Negate(-128);
            Require(last!.Overflowed && last.Expression == "-(-128)" && last.Right is null);
        });
        Run("Обробник отримує невідоме джерело без помилки", () =>
        {
            var output = new StringWriter();
            var args = new OverflowEventArgs(ArithmeticOperation.Addition, 1, 1, -300, 0, 8, -128, 127);
            new OverflowNotifier(output).HandleOverflow(null, args);
            new OverflowJournal().HandleOverflow("рядок", args);
            Require(output.ToString().Contains("невідоме джерело") && output.ToString().Contains("менший за мінімум -128"));
        });
        Run("Меню: матриця, операція з переповненням, зміна розрядності, журнал", () =>
        {
            string script = string.Join("\n",
                "1", "2", "3", "1 2 x", "1 2 3", "-4 5 6",
                "3", "+", "100", "28",
                "5", "3", "*", "100", "2", "5",
                "3", "^", "2", "8",
                "6",
                "4", "16", "3", "+", "200", "300",
                "3", "/", "1", "0",
                "3", "?",
                "9", "0");
            var output = new StringWriter();
            var menu = new ConsoleMenu(new StringReader(script), output);
            menu.Run();
            string text = output.ToString();
            Require(text.Contains("Анонімний метод: 13") && text.Contains("Лямбда-вираз:    13"));
            Require(text.Contains("Потрібно рівно 3 цілих чисел"));
            Require(text.Contains("Результат: -128") && text.Contains("ПЕРЕПОВНЕННЯ"));
            Require(text.Contains("Журнал відписано") && text.Contains("Журнал підписано"));
            Require(menu.Journal.Records.Count == 2);
            Require(menu.Calculator.BitWidth == 16 && text.Contains("Результат: 500"));
            Require(text.Contains("Помилка: Ділення на нуль") && text.Contains("Невідома операція") && text.Contains("Невідомий пункт"));
        });
        Run("Меню: повторне введення числа та завершення потоку", () =>
        {
            var output = new StringWriter();
            new ConsoleMenu(new StringReader("3\n+\n500\nабв\n1\n"), output).Run();
            Require(output.ToString().Contains("Введіть ціле число від -128 до 127"));
            var empty = new StringWriter();
            new ConsoleMenu(new StringReader(""), empty).Run();
            Require(empty.ToString().Contains("Оберіть пункт"));
        });
        Run("Повна демонстрація", () =>
        {
            var output = new StringWriter();
            Demonstration.RunAll(output);
            string text = output.ToString();
            Require(text.Contains("Анонімний метод через делегат:        27"));
            Require(text.Contains("calls = 2") && text.Contains("Мультиделегат містить 2"));
            Require(text.Contains("Операцій: 10, переповнень: 7"));
            Require(text.Contains("компонент повернув -2147483648"));
        });

        Console.WriteLine();
        Console.WriteLine($"Пройдено: {_passed}; помилок: {_failed}");
        return _failed == 0 ? 0 : 1;
    }

    private static int Pow32(int value, int exponent)
    {
        int result = 1;
        for (int i = 0; i < exponent; i++)
            result = unchecked(result * value);
        return result;
    }

    private static void Run(string name, Action test)
    {
        try
        {
            test();
            _passed++;
            Console.WriteLine("PASS: " + name);
        }
        catch (Exception error)
        {
            _failed++;
            Console.WriteLine("FAIL: " + name + " — " + error.Message);
        }
    }

    private static void Require(bool condition)
    {
        if (!condition)
            throw new InvalidOperationException("Умова не виконана.");
    }

    private static void Throws<TException>(Action action) where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }
        throw new InvalidOperationException("Очікувався виняток " + typeof(TException).Name + ".");
    }
}
