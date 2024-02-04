public class Table
{
    public string Name { get; set; }
    public List<Column> Columns { get; private set; }
    public List<Dictionary<string, object>> Rows { get; private set; }

    public Table()
    {
        Columns = new List<Column>();
        Rows = new List<Dictionary<string, object>>();
    }

    public Table(string name) : this()
    {
        Name = name;
    }

    public void SetColumns(List<Column> columns)
    {
        Columns = columns;
    }

    public void SetRows(List<Dictionary<string, object>> rows)
    {
        Rows = rows;
    }

    public void AddColumn(Column column)
    {
        Columns.Add(column);
    }
    public void AddRow(Dictionary<string, object> rowData)
    {
        var convertedRow = new Dictionary<string, object>();
        foreach (var column in Columns)
        {
            if (rowData.TryGetValue(column.Name, out var value))
            {
                // Convert value to the correct data type based on column.Type
                convertedRow[column.Name] = ConvertToType(value, column.Type);
            }
            else if (!string.IsNullOrEmpty(column.DefaultValue))
            {
                // Use default value if provided
                convertedRow[column.Name] = ConvertToType(column.DefaultValue, column.Type);
            }
        }
        Rows.Add(convertedRow);
    }

    public void ReplaceRows(List<Dictionary<string, object>> newRows)
    {
        Rows = newRows;
    }
    public void ClearRows()
    {
        Rows.Clear();
    }

    #region Methods
    private object ConvertToType(object value, string type)
    {
        switch (type)
        {
            case "int":
                return Convert.ToInt32(value);
            case "double":
                return Convert.ToDouble(value);
            case "decimal":
                return Convert.ToDecimal(value);
            case "float":
                return Convert.ToSingle(value);
            case "bool":
                return Convert.ToBoolean(value);
            case "date":
                return Convert.ToDateTime(value);
            case "string":
                return value.ToString();
            case "byte":
                return Convert.ToByte(value);
            case "sbyte":
                return Convert.ToSByte(value);
            case "char":
                return Convert.ToChar(value);
            case "uint":
                return Convert.ToUInt32(value);
            case "nint":
                // Assuming nint is an alias for IntPtr
                return (IntPtr)Convert.ToInt64(value);
            case "nuint":
                // Assuming nuint is an alias for UIntPtr
                return (UIntPtr)Convert.ToUInt64(value);
            case "long":
                return Convert.ToInt64(value);
            case "ulong":
                return Convert.ToUInt64(value);
            case "short":
                return Convert.ToInt16(value);
            case "ushort":
                return Convert.ToUInt16(value);
            default:
                throw new InvalidOperationException($"Unsupported type: {type}");
        }
    }
    #endregion
}