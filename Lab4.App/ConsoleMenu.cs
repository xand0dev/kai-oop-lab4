using System.Globalization;
using Lab4.Arithmetic;
using Lab4.Delegates;

namespace Lab4.App;

public sealed class ConsoleMenu
{
    private const int MaxMatrixSize = 10;

    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly OverflowNotifier _notifier;
    private readonly OverflowJournal _journal = new();
    private IntegerArithmetic _calculator;
    private bool _journalEnabled = true;

    public ConsoleMenu(TextReader input, TextWriter output)
    {
        _input = input;
        _output = output;
        _notifier = new OverflowNotifier(output);
        _calculator = CreateCalculator(8);
    }

    public IntegerArithmetic Calculator => _calculator;
    public OverflowJournal Journal => _journal;

    public void Run()
    {
        while (true)
        {
            _output.WriteLine();
            _output.WriteLine("ЛР 3.4 | Варіант 7 | Делегати, події, лямбда-вирази");
            _output.WriteLine($"Калькулятор: {_calculator.BitWidth} біт [{_calculator.MinValue}; {_calculator.MaxValue}], " +
                $"журнал {(_journalEnabled ? "підписаний" : "відписаний")}");
            _output.WriteLine("1 - Сума елементів введеної матриці\n2 - Сума елементів матриці-прикладу\n" +
                "3 - Виконати арифметичну операцію\n4 - Змінити розрядність\n" +
                "5 - Підписати / відписати журнал переповнень\n6 - Показати журнал\n7 - Повна демонстрація\n0 - Вихід");

            string? choice = ReadLine("Оберіть пункт: ");
            if (choice is null || choice == "0")
                return;

            try
            {
                switch (choice)
                {
                    case "1": SumMatrix(ReadMatrix()); break;
                    case "2": SumMatrix(Demonstration.SampleMatrix()); break;
                    case "3": Calculate(); break;
                    case "4": ChangeBitWidth(); break;
                    case "5": ToggleJournal(); break;
                    case "6": ShowJournal(); break;
                    case "7": Demonstration.RunAll(_output); break;
                    default: _output.WriteLine("Невідомий пункт меню."); break;
                }
            }
            catch (InputCancelledException)
            {
                return;
            }
            catch (Exception error) when (error is ArgumentException or ArithmeticException)
            {
                _output.WriteLine("Помилка: " + error.Message);
            }
        }
    }

    private void SumMatrix(int[,] matrix)
    {
        _output.WriteLine(MatrixSummation.Format(matrix));
        _output.WriteLine("Анонімний метод: " + MatrixSummation.Calculate(matrix, MatrixSummation.AnonymousMethod));
        _output.WriteLine("Лямбда-вираз:    " + MatrixSummation.Calculate(matrix, MatrixSummation.Lambda));
    }

    private int[,] ReadMatrix()
    {
        int rows = ReadInt("Кількість рядків (1-10): ", 1, MaxMatrixSize);
        int columns = ReadInt("Кількість стовпців (1-10): ", 1, MaxMatrixSize);
        var matrix = new int[rows, columns];
        for (int row = 0; row < rows; row++)
        {
            while (true)
            {
                string line = ReadRequired($"Рядок {row + 1} ({columns} цілих чисел через пробіл): ");
                string[] parts = line.Split(new[] { ' ', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == columns && parts.All(part => TryParse(part, out _)))
                {
                    for (int column = 0; column < columns; column++)
                    {
                        TryParse(parts[column], out int value);
                        matrix[row, column] = value;
                    }
                    break;
                }
                _output.WriteLine($"Потрібно рівно {columns} цілих чисел у межах int.");
            }
        }
        return matrix;
    }

    private void Calculate()
    {
        _output.WriteLine("Операції: + - * / ^ neg");
        string symbol = ReadRequired("Операція: ").Trim().ToLowerInvariant();
        ArithmeticOperation? operation = symbol switch
        {
            "+" => ArithmeticOperation.Addition,
            "-" => ArithmeticOperation.Subtraction,
            "*" => ArithmeticOperation.Multiplication,
            "/" => ArithmeticOperation.Division,
            "^" => ArithmeticOperation.Power,
            "neg" => ArithmeticOperation.Negation,
            _ => null
        };
        if (operation is null)
        {
            _output.WriteLine("Невідома операція.");
            return;
        }

        int left = ReadInt("Перше число: ", _calculator.MinValue, _calculator.MaxValue);
        int right = operation switch
        {
            ArithmeticOperation.Negation => 0,
            ArithmeticOperation.Power => ReadInt("Показник степеня: ", 0, IntegerArithmetic.MaxExponent),
            _ => ReadInt("Друге число: ", _calculator.MinValue, _calculator.MaxValue)
        };
        int result = _calculator.Calculate(operation.Value, left, right);
        _output.WriteLine("Результат: " + result);
    }

    private void ChangeBitWidth()
    {
        int bits = ReadInt($"Розрядність ({IntegerArithmetic.MinBitWidth}-{IntegerArithmetic.MaxBitWidth}): ",
            IntegerArithmetic.MinBitWidth, IntegerArithmetic.MaxBitWidth);
        // Старий об'єкт відписуємо, щоб обробники не залишались прив'язаними до нього.
        _calculator.Overflow -= _notifier.HandleOverflow;
        _calculator.Overflow -= _journal.HandleOverflow;
        _calculator = CreateCalculator(bits);
        _output.WriteLine($"Новий діапазон: [{_calculator.MinValue}; {_calculator.MaxValue}].");
    }

    private void ToggleJournal()
    {
        if (_journalEnabled)
            _calculator.Overflow -= _journal.HandleOverflow;
        else
            _calculator.Overflow += _journal.HandleOverflow;
        _journalEnabled = !_journalEnabled;
        _output.WriteLine(_journalEnabled ? "Журнал підписано на подію." : "Журнал відписано від події.");
    }

    private void ShowJournal()
    {
        if (_journal.Records.Count == 0)
            _output.WriteLine("Журнал порожній.");
        foreach (string record in _journal.Records)
            _output.WriteLine(record);
    }

    private IntegerArithmetic CreateCalculator(int bits)
    {
        var calculator = new IntegerArithmetic(bits, bits + "-бітний калькулятор");
        calculator.Overflow += _notifier.HandleOverflow;
        if (_journalEnabled)
            calculator.Overflow += _journal.HandleOverflow;
        return calculator;
    }

    private int ReadInt(string prompt, int min, int max)
    {
        while (true)
        {
            string text = ReadRequired(prompt);
            if (TryParse(text.Trim(), out int value) && value >= min && value <= max)
                return value;
            _output.WriteLine($"Введіть ціле число від {min} до {max}.");
        }
    }

    private static bool TryParse(string text, out int value) =>
        int.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);

    private string ReadRequired(string prompt) => ReadLine(prompt) ?? throw new InputCancelledException();

    private string? ReadLine(string prompt)
    {
        _output.Write(prompt);
        return _input.ReadLine();
    }

    private sealed class InputCancelledException : Exception
    {
    }
}
