public class Database
{
    public List<Table> Tables { get; set; }

    public Database()
    {
        Tables = new List<Table>();
    }

    public void CreateTable(string tableName, List<Column> columns)
    {
        var table = new Table(tableName);
        foreach (var column in columns)
        {
            table.AddColumn(column);
        }
        Tables.Add(table);
    }

    public bool DropTable(string tableName)
    {
        var table = FindTableByName(tableName);

        if (table != null)
        {
            Tables.Remove(table);
            return true;
        }

        return false;
    }

    private Table FindTableByName(string tableName)
    {
        foreach (var table in Tables)
        {
            if (table.Name == tableName)
            {
                return table;
            }
        }
        return null; // Return null if not found
    }
}