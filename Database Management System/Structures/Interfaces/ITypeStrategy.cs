public interface ITypeStrategy
{
    object Convert(string value);
    bool Compare(object value1, object value2, string operation);
}

public static class TypeComparisonUtil
{
    public static bool CompareValues<T>(T value1, T value2, string operation) where T : IComparable
    {
        if (operation != "=" && operation != "<>" && operation != "<" && operation != ">")
        {
            throw new InvalidOperationException($"Invalid comparison operation: {operation}");
        }

        switch (operation)
        {
            case "=": return value1.CompareTo(value2) == 0;
            case "<>": return value1.CompareTo(value2) != 0;
            case "<": return value1.CompareTo(value2) < 0;
            case ">": return value1.CompareTo(value2) > 0;
            default: throw new InvalidOperationException("Invalid comparison operation");
        }
    }
}

public class IntTypeStrategy : ITypeStrategy
{
    public object Convert(string value) => int.TryParse(value, out int result) ? result : 0;
    public bool Compare(object value1, object value2, string operation)
        => TypeComparisonUtil.CompareValues((int)value1, (int)value2, operation);
}
public class DoubleTypeStrategy : ITypeStrategy
{
    public object Convert(string value)
    {
        if (double.TryParse(value, out double result))
            return result;
        throw new InvalidCastException("Invalid double value: " + value);
    }

    public bool Compare(object value1, object value2, string operation)
    {
        return TypeComparisonUtil.CompareValues((double)value1, (double)value2, operation);
    }
}
public class DecimalTypeStrategy : ITypeStrategy
{
    public object Convert(string value) => decimal.TryParse(value, out decimal result) ? result : 0m;
    public bool Compare(object value1, object value2, string operation)
        => TypeComparisonUtil.CompareValues((decimal)value1, (decimal)value2, operation);
}
public class FloatTypeStrategy : ITypeStrategy
{
    public object Convert(string value) => float.TryParse(value, out float result) ? result : 0f;
    public bool Compare(object value1, object value2, string operation)
        => TypeComparisonUtil.CompareValues((float)value1, (float)value2, operation);
}
public class BoolTypeStrategy : ITypeStrategy
{
    public object Convert(string value) => bool.TryParse(value, out bool result) && result;
    public bool Compare(object value1, object value2, string operation)
        => TypeComparisonUtil.CompareValues((bool)value1, (bool)value2, operation);
}
public class DateTypeStrategy : ITypeStrategy
{
    public object Convert(string value) => DateTime.TryParse(value, out DateTime result) ? result : default(DateTime);
    public bool Compare(object value1, object value2, string operation)
        => TypeComparisonUtil.CompareValues((DateTime)value1, (DateTime)value2, operation);
}
public class StringTypeStrategy : ITypeStrategy
{
    public object Convert(string value) => value.Trim('"'); // Trim quotes when converting
    public bool Compare(object value1, object value2, string operation)
    {
        // Ensure the operation is valid
        if (operation != "=" && operation != "<>" && operation != "<" && operation != ">")
        {
            throw new InvalidOperationException($"Invalid comparison operation: {operation}");
        }
        return TypeComparisonUtil.CompareValues(value1.ToString(), value2.ToString(), operation);
    }
}
public class ByteTypeStrategy : ITypeStrategy
{
    public object Convert(string value) => byte.TryParse(value, out byte result) ? result : (byte)0;
    public bool Compare(object value1, object value2, string operation)
        => TypeComparisonUtil.CompareValues((byte)value1, (byte)value2, operation);
}

public class SByteTypeStrategy : ITypeStrategy
{
    public object Convert(string value) => sbyte.TryParse(value, out sbyte result) ? result : (sbyte)0;
    public bool Compare(object value1, object value2, string operation)
        => TypeComparisonUtil.CompareValues((sbyte)value1, (sbyte)value2, operation);
}

public class CharTypeStrategy : ITypeStrategy
{
    public object Convert(string value) => value.Length > 0 ? value[0] : '\0';
    public bool Compare(object value1, object value2, string operation)
        => TypeComparisonUtil.CompareValues((char)value1, (char)value2, operation);
}

public class UIntTypeStrategy : ITypeStrategy
{
    public object Convert(string value) => uint.TryParse(value, out uint result) ? result : 0u;
    public bool Compare(object value1, object value2, string operation)
        => TypeComparisonUtil.CompareValues((uint)value1, (uint)value2, operation);
}

public class NIntTypeStrategy : ITypeStrategy
{
    public object Convert(string value) => int.TryParse(value, out int result) ? (nint)result : (nint)0;
    public bool Compare(object value1, object value2, string operation)
        => TypeComparisonUtil.CompareValues((nint)value1, (nint)value2, operation);
}

public class NUIntTypeStrategy : ITypeStrategy
{
    public object Convert(string value) => uint.TryParse(value, out uint result) ? (nuint)result : (nuint)0;
    public bool Compare(object value1, object value2, string operation)
        => TypeComparisonUtil.CompareValues((nuint)value1, (nuint)value2, operation);
}

public class LongTypeStrategy : ITypeStrategy
{
    public object Convert(string value) => long.TryParse(value, out long result) ? result : 0L;
    public bool Compare(object value1, object value2, string operation)
        => TypeComparisonUtil.CompareValues((long)value1, (long)value2, operation);
}

public class ULongTypeStrategy : ITypeStrategy
{
    public object Convert(string value) => ulong.TryParse(value, out ulong result) ? result : 0UL;
    public bool Compare(object value1, object value2, string operation)
        => TypeComparisonUtil.CompareValues((ulong)value1, (ulong)value2, operation);
}

public class ShortTypeStrategy : ITypeStrategy
{
    public object Convert(string value) => short.TryParse(value, out short result) ? result : (short)0;
    public bool Compare(object value1, object value2, string operation)
        => TypeComparisonUtil.CompareValues((short)value1, (short)value2, operation);
}

public class UShortTypeStrategy : ITypeStrategy
{
    public object Convert(string value) => ushort.TryParse(value, out ushort result) ? result : (ushort)0;
    public bool Compare(object value1, object value2, string operation)
        => TypeComparisonUtil.CompareValues((ushort)value1, (ushort)value2, operation);
}
