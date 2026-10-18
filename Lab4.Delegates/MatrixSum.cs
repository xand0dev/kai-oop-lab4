namespace Lab4.Delegates;

/// <summary>Підпис методу, який обчислює суму елементів двовимірного цілого масиву.</summary>
public delegate long MatrixSum(int[,] matrix);

public static class MatrixSummation
{
    // Анонімний метод: тіло описане через ключове слово delegate без імені методу.
    public static readonly MatrixSum AnonymousMethod = delegate (int[,] matrix)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        long sum = 0;
        for (int row = 0; row < matrix.GetLength(0); row++)
        {
            for (int column = 0; column < matrix.GetLength(1); column++)
                sum += matrix[row, column];
        }
        return sum;
    };

    // Лямбда-вираз з блоком операторів; foreach обходить int[,] по рядках.
    public static readonly MatrixSum Lambda = matrix =>
    {
        ArgumentNullException.ThrowIfNull(matrix);
        long sum = 0;
        foreach (int value in matrix)
            sum += value;
        return sum;
    };

    // Лямбда-вираз з одним виразом через готовий делегат бібліотеки Func<T, TResult>.
    public static readonly Func<int[,], long> ExpressionLambda =
        matrix => (matrix ?? throw new ArgumentNullException(nameof(matrix))).Cast<int>().Sum(value => (long)value);

    // Звичайний іменований метод для порівняння: його також можна присвоїти делегату.
    public static long SumByRows(int[,] matrix)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        long sum = 0;
        for (int row = 0; row < matrix.GetLength(0); row++)
            sum += SumRow(matrix, row);
        return sum;
    }

    private static long SumRow(int[,] matrix, int row)
    {
        long sum = 0;
        for (int column = 0; column < matrix.GetLength(1); column++)
            sum += matrix[row, column];
        return sum;
    }

    /// <summary>Викликає будь-який метод з потрібним підписом через переданий делегат.</summary>
    public static long Calculate(int[,] matrix, MatrixSum method)
    {
        ArgumentNullException.ThrowIfNull(method);
        return method(matrix);
    }

    public static string Format(int[,] matrix)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        int width = 1;
        foreach (int value in matrix)
            width = Math.Max(width, value.ToString(System.Globalization.CultureInfo.InvariantCulture).Length);

        var lines = new List<string>();
        for (int row = 0; row < matrix.GetLength(0); row++)
        {
            var cells = new string[matrix.GetLength(1)];
            for (int column = 0; column < cells.Length; column++)
                cells[column] = matrix[row, column].ToString(System.Globalization.CultureInfo.InvariantCulture).PadLeft(width);
            lines.Add("  " + string.Join("  ", cells));
        }
        return string.Join(Environment.NewLine, lines);
    }
}
