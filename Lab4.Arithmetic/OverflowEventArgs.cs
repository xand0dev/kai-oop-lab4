using System.Numerics;

namespace Lab4.Arithmetic;

/// <summary>Аргумент події переповнення розрядної сітки.</summary>
public sealed class OverflowEventArgs : EventArgs
{
    public OverflowEventArgs(ArithmeticOperation operation, int left, int? right,
        BigInteger exactResult, int wrappedResult, int bitWidth, int minValue, int maxValue)
    {
        Operation = operation;
        Left = left;
        Right = right;
        ExactResult = exactResult;
        WrappedResult = wrappedResult;
        BitWidth = bitWidth;
        MinValue = minValue;
        MaxValue = maxValue;
    }

    public ArithmeticOperation Operation { get; }
    public int Left { get; }
    public int? Right { get; }

    /// <summary>Математично точний результат, який не вмістився в розрядну сітку.</summary>
    public BigInteger ExactResult { get; }

    /// <summary>Результат після відкидання старших розрядів (доповняльний код).</summary>
    public int WrappedResult { get; }

    public int BitWidth { get; }
    public int MinValue { get; }
    public int MaxValue { get; }

    public bool IsAboveMaximum => ExactResult > MaxValue;

    public string Expression => ArithmeticOperationText.Expression(Operation, Left, Right);
}
