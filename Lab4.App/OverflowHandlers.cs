using Lab4.Arithmetic;

namespace Lab4.App;

/// <summary>Реакція застосунку на переповнення: повідомлення користувачеві.</summary>
public sealed class OverflowNotifier
{
    private readonly TextWriter _output;

    public OverflowNotifier(TextWriter output) => _output = output;

    // Підпис стандартного обробника .NET: ініціатор події та аргумент події.
    public void HandleOverflow(object? sender, OverflowEventArgs e)
    {
        string source = sender is IntegerArithmetic calculator
            ? calculator.Name + ", " + calculator.BitWidth + " біт"
            : "невідоме джерело";
        string direction = e.IsAboveMaximum ? "більший за максимум " + e.MaxValue : "менший за мінімум " + e.MinValue;

        _output.WriteLine($"  !!! ПЕРЕПОВНЕННЯ РОЗРЯДНОЇ СІТКИ ({source})");
        _output.WriteLine($"      {e.Operation.Name()}: {e.Expression}");
        _output.WriteLine($"      точний результат {e.ExactResult} {direction}");
        _output.WriteLine($"      після відкидання старших розрядів: {e.WrappedResult}");
    }
}

/// <summary>Другий незалежний підписник: зберігає переповнення в журналі.</summary>
public sealed class OverflowJournal
{
    private readonly List<string> _records = new();

    public IReadOnlyList<string> Records => _records;

    public void HandleOverflow(object? sender, OverflowEventArgs e)
    {
        string source = (sender as IntegerArithmetic)?.Name ?? "?";
        _records.Add($"{_records.Count + 1}. [{source}, {e.BitWidth} біт] {e.Expression} = {e.ExactResult} -> {e.WrappedResult}");
    }

    public void Clear() => _records.Clear();
}

/// <summary>Підписник на подію OperationCompleted: рахує операції та виводить їх результат.</summary>
public sealed class OperationPrinter
{
    private readonly TextWriter _output;

    public OperationPrinter(TextWriter output) => _output = output;

    public int Printed { get; private set; }

    public void HandleOperation(object? sender, OperationEventArgs e)
    {
        Printed++;
        _output.WriteLine($"  {e.Expression} = {e.Result}" + (e.Overflowed ? "  (переповнення)" : ""));
    }
}
