using System.Numerics;

namespace Lab4.Arithmetic;

/// <summary>
/// Арифметика з цілими числами заданої розрядності (від 2 до 32 біт, знакові числа в доповняльному коді).
/// Якщо точний результат не вміщується в розрядну сітку, генерується подія Overflow,
/// а операція повертає значення з відкинутими старшими розрядами — так само, як unchecked-арифметика C#.
/// </summary>
public class IntegerArithmetic
{
    public const int MinBitWidth = 2;
    public const int MaxBitWidth = 32;
    public const int MaxExponent = 1000;

    public IntegerArithmetic(int bitWidth = MaxBitWidth, string name = "Калькулятор")
    {
        if (bitWidth < MinBitWidth || bitWidth > MaxBitWidth)
            throw new ArgumentOutOfRangeException(nameof(bitWidth), $"Розрядність має бути від {MinBitWidth} до {MaxBitWidth} біт.");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Вкажіть назву калькулятора.", nameof(name));

        BitWidth = bitWidth;
        Name = name.Trim();
        MinValue = (int)-(1L << (bitWidth - 1));
        MaxValue = (int)((1L << (bitWidth - 1)) - 1);
    }

    public string Name { get; }
    public int BitWidth { get; }
    public int MinValue { get; }
    public int MaxValue { get; }
    public int OperationCount { get; private set; }
    public int OverflowCount { get; private set; }

    /// <summary>Переповнення розрядної сітки.</summary>
    public event EventHandler<OverflowEventArgs>? Overflow;

    /// <summary>Будь-яка завершена операція, зокрема і з переповненням.</summary>
    public event EventHandler<OperationEventArgs>? OperationCompleted;

    public int Add(int left, int right) =>
        Execute(ArithmeticOperation.Addition, left, right, () => (BigInteger)left + right);

    public int Subtract(int left, int right) =>
        Execute(ArithmeticOperation.Subtraction, left, right, () => (BigInteger)left - right);

    public int Multiply(int left, int right) =>
        Execute(ArithmeticOperation.Multiplication, left, right, () => (BigInteger)left * right);

    /// <summary>Цілочисельне ділення з відкиданням дробової частини; MinValue / -1 переповнює сітку.</summary>
    public int Divide(int left, int right)
    {
        if (right == 0)
            throw new DivideByZeroException("Ділення на нуль неможливе.");
        return Execute(ArithmeticOperation.Division, left, right, () => BigInteger.Divide(left, right));
    }

    public int Negate(int value) =>
        Execute(ArithmeticOperation.Negation, value, null, () => -(BigInteger)value);

    public int Power(int value, int exponent)
    {
        if (exponent < 0 || exponent > MaxExponent)
            throw new ArgumentOutOfRangeException(nameof(exponent), $"Показник степеня має бути від 0 до {MaxExponent}.");
        return Execute(ArithmeticOperation.Power, value, exponent, () => BigInteger.Pow(value, exponent));
    }

    public int Calculate(ArithmeticOperation operation, int left, int right) => operation switch
    {
        ArithmeticOperation.Addition => Add(left, right),
        ArithmeticOperation.Subtraction => Subtract(left, right),
        ArithmeticOperation.Multiplication => Multiply(left, right),
        ArithmeticOperation.Division => Divide(left, right),
        ArithmeticOperation.Negation => Negate(left),
        ArithmeticOperation.Power => Power(left, right),
        _ => throw new ArgumentOutOfRangeException(nameof(operation))
    };

    public bool Fits(BigInteger value) => value >= MinValue && value <= MaxValue;

    /// <summary>Залишає лише молодші BitWidth розрядів і повертає знакове значення.</summary>
    public int Wrap(BigInteger value)
    {
        BigInteger modulus = BigInteger.One << BitWidth;
        BigInteger shifted = BigInteger.Remainder(value - MinValue, modulus);
        if (shifted.Sign < 0)
            shifted += modulus;
        return (int)(shifted + MinValue);
    }

    protected virtual void OnOverflow(OverflowEventArgs e) => Overflow?.Invoke(this, e);

    protected virtual void OnOperationCompleted(OperationEventArgs e) => OperationCompleted?.Invoke(this, e);

    private int Execute(ArithmeticOperation operation, int left, int? right, Func<BigInteger> exact)
    {
        EnsureInRange(left, nameof(left));
        if (right.HasValue && operation != ArithmeticOperation.Power)
            EnsureInRange(right.Value, nameof(right));

        BigInteger value = exact();
        bool overflowed = !Fits(value);
        int result = overflowed ? Wrap(value) : (int)value;

        OperationCount++;
        if (overflowed)
        {
            OverflowCount++;
            OnOverflow(new OverflowEventArgs(operation, left, right, value, result, BitWidth, MinValue, MaxValue));
        }
        OnOperationCompleted(new OperationEventArgs(operation, left, right, result, overflowed));
        return result;
    }

    private void EnsureInRange(int value, string parameterName)
    {
        if (value < MinValue || value > MaxValue)
            throw new ArgumentOutOfRangeException(parameterName,
                $"Число {value} не вміщується в {BitWidth}-бітну сітку [{MinValue}; {MaxValue}].");
    }
}
