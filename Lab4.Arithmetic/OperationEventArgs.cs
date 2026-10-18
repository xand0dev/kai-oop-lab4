namespace Lab4.Arithmetic;

/// <summary>Аргумент події, яка повідомляє про кожну виконану операцію.</summary>
public sealed class OperationEventArgs : EventArgs
{
    public OperationEventArgs(ArithmeticOperation operation, int left, int? right, int result, bool overflowed)
    {
        Operation = operation;
        Left = left;
        Right = right;
        Result = result;
        Overflowed = overflowed;
    }

    public ArithmeticOperation Operation { get; }
    public int Left { get; }
    public int? Right { get; }
    public int Result { get; }
    public bool Overflowed { get; }

    public string Expression => ArithmeticOperationText.Expression(Operation, Left, Right);
}
