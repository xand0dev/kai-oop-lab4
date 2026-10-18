namespace Lab4.Arithmetic;

public enum ArithmeticOperation
{
    Addition,
    Subtraction,
    Multiplication,
    Division,
    Negation,
    Power
}

public static class ArithmeticOperationText
{
    public static string Symbol(this ArithmeticOperation operation) => operation switch
    {
        ArithmeticOperation.Addition => "+",
        ArithmeticOperation.Subtraction => "-",
        ArithmeticOperation.Multiplication => "*",
        ArithmeticOperation.Division => "/",
        ArithmeticOperation.Negation => "-",
        ArithmeticOperation.Power => "^",
        _ => throw new ArgumentOutOfRangeException(nameof(operation))
    };

    public static string Name(this ArithmeticOperation operation) => operation switch
    {
        ArithmeticOperation.Addition => "додавання",
        ArithmeticOperation.Subtraction => "віднімання",
        ArithmeticOperation.Multiplication => "множення",
        ArithmeticOperation.Division => "ділення",
        ArithmeticOperation.Negation => "зміна знака",
        ArithmeticOperation.Power => "піднесення до степеня",
        _ => throw new ArgumentOutOfRangeException(nameof(operation))
    };

    public static string Expression(ArithmeticOperation operation, int left, int? right) =>
        operation == ArithmeticOperation.Negation
            ? "-(" + left + ")"
            : left + " " + operation.Symbol() + " " + (right is < 0 ? "(" + right + ")" : right?.ToString());
}
