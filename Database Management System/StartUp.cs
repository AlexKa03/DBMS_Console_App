using System.Text;
using System.Text.Json;

class StartUp
{
    static Database db = new Database();
    static Dictionary<string, ITypeStrategy> typeStrategies = new Dictionary<string, ITypeStrategy>();

    static void Main()
    {
        LoadDatabase();

        #region TypeStrategy
        typeStrategies.Add("int", new IntTypeStrategy());
        typeStrategies.Add("double", new DoubleTypeStrategy());
        typeStrategies.Add("decimal", new DecimalTypeStrategy());
        typeStrategies.Add("float", new FloatTypeStrategy());
        typeStrategies.Add("bool", new BoolTypeStrategy());
        typeStrategies.Add("date", new DateTypeStrategy());
        typeStrategies.Add("string", new StringTypeStrategy());
        typeStrategies.Add("byte", new ByteTypeStrategy());
        typeStrategies.Add("sbyte", new SByteTypeStrategy());
        typeStrategies.Add("char", new CharTypeStrategy());
        typeStrategies.Add("uint", new UIntTypeStrategy());
        typeStrategies.Add("nint", new NIntTypeStrategy());
        typeStrategies.Add("nuint", new NUIntTypeStrategy());
        typeStrategies.Add("long", new LongTypeStrategy());
        typeStrategies.Add("ulong", new ULongTypeStrategy());
        typeStrategies.Add("short", new ShortTypeStrategy());
        typeStrategies.Add("ushort", new UShortTypeStrategy());
        #endregion

        Console.WriteLine("Simple DBMS Command Line Interface\n");

        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();

            if (input.ToLower() == "quit")
            {
                Environment.Exit(0);
            }

            ProcessCommand(input);
        }
    }

    static void ProcessCommand(string input)
    {
        // Basic parsing to determine the command type

        int firstSpaceIndex = FindFirstSpaceIndex(input);

        string command;
        string parameters;

        if (firstSpaceIndex == -1)
        {
            command = input;
            parameters = "";
        }
        else
        {
            command = CustomSubstring(input, 0, firstSpaceIndex);
            parameters = CustomSubstring(input, firstSpaceIndex + 1, input.Length - firstSpaceIndex - 1);
        }

        switch (command.ToLower())
        {
            case "createtable":
                CreateTable(parameters);
                break;

            case "droptable":
                DropTable(parameters);
                break;

            case "listtables":
                ListTables();
                break;

            case "tableinfo":
                TableInfo(parameters);
                break;

            case "select":
                Select(parameters);
                break;

            case "insert":
                Insert(parameters);
                break;

            case "delete":
                Delete(parameters);
                break;

            default:
                Console.WriteLine("Unknown command.");
                break;
        }
    }

    #region Commands
    static void CreateTable(string input)
    {
        // (INPUT) CreateTable Sample(Id:int, Name:string, BirthDate:date)

        int firstParenthesisIndex = FindFirstChar(input, '(');
        if (firstParenthesisIndex == -1)
        {
            Console.WriteLine("Invalid input format for CreateTable.");
            return;
        }
        string tableName = CustomTrim(CustomSubstring(input, 0, firstParenthesisIndex));

        bool tableExists = false;
        foreach (var table in db.Tables)
        {
            if (table.Name == tableName)
            {
                tableExists = true;
                break;
            }
        }

        if (tableExists)
        {
            Console.WriteLine($"Table '{tableName}' already exists.");
            return;
        }

        int lastParenthesisIndex = FindLastChar(input, ')');
        if (lastParenthesisIndex == -1 || lastParenthesisIndex <= firstParenthesisIndex)
        {
            Console.WriteLine("Invalid input format for CreateTable.");
            return;
        }
        string columnDefinitions = CustomSubstring(input, firstParenthesisIndex + 1, lastParenthesisIndex - firstParenthesisIndex - 1);

        List<Column> columns = ParseColumnDefinitions(columnDefinitions);

        if (columns != null)
        {
            db.CreateTable(tableName, columns);
            SaveDatabase();

            Console.WriteLine($"Table \"{tableName}\" was created with {columns.Count} columns.");
        }
    }

    static void DropTable(string tableName)
    {
        // Assuming the input format is "DropTable TableName"

        bool isDropped = db.DropTable(tableName);

        if (isDropped)
        {
            SaveDatabase();
            Console.WriteLine($"Table '{tableName}' has been successfully dropped.");
        }
        else
        {
            Console.WriteLine($"Table '{tableName}' does not exist or could not be dropped.");
        }
    }

    static void ListTables()
    {
        foreach (var table in db.Tables)
        {
            Console.WriteLine("\t" + table.Name);
        }

        if (db.Tables.Count == 0)
        {
            Console.WriteLine("No tables in the DataBase.");
        }
    }

    static void TableInfo(string tableName)
    {
        Table foundTable = null;
        foreach (var table in db.Tables)
        {
            if (table.Name == tableName)
            {
                foundTable = table;
                break;
            }
        }

        if (foundTable != null)
        {
            Console.WriteLine($"Table: {foundTable.Name}");
            Console.WriteLine("Columns:");
            foreach (var column in foundTable.Columns)
            {
                Console.WriteLine($"  {column.Name} ({column.Type})");
            }
        }
        else
        {
            Console.WriteLine("Table not found.");
        }
    }

    static void Select(string input)
    {
        // Existing parsing logic for extracting columns and table name
        var selectParts = CustomSplit(input, " FROM ");
        if (selectParts.Count < 2)
        {
            Console.WriteLine("Invalid SELECT command format.");
            return;
        }

        var splitColumns = CustomSplit(selectParts[0], ",");
        var columns = new List<string>();

        foreach (var column in splitColumns)
        {
            columns.Add(CustomTrim(column));
        }

        var fromAndWhereParts = CustomSplit(selectParts[1], " WHERE ");
        string tableName = CustomTrim(fromAndWhereParts[0]);
        var table = CustomFindFirst(db.Tables, t => t.Name == tableName);
        if (table == null)
        {
            Console.WriteLine("Table not found.");
            return;
        }

        string whereClause = fromAndWhereParts.Count > 1 ? fromAndWhereParts[1] : null;
        var filteredRows = table.Rows;

        if (!string.IsNullOrEmpty(whereClause))
        {
            filteredRows = new List<Dictionary<string, object>>(ProcessWhereClause(filteredRows, whereClause, table.Columns, typeStrategies));
        }

        // Existing DISTINCT and ORDER BY logic
        bool distinct = input.Contains("DISTINCT");
        string orderByColumn = null;
        int orderByIndex = CustomIndexOf(input, "ORDER BY");
        if (orderByIndex != -1)
        {
            var orderByParts = CustomSplit(CustomSubstring(input, orderByIndex + 9, input.Length - orderByIndex - 9), " ");
            orderByColumn = orderByParts[0];
        }

        if (distinct)
        {
            filteredRows = CustomDistinctAndOrderBy(filteredRows, columns.ToArray(), orderByColumn);
        }
        else if (!string.IsNullOrEmpty(orderByColumn))
        {
            filteredRows = CustomOrderBy(filteredRows, orderByColumn);
        }

        PrintSelectedRows(filteredRows, columns.ToArray());
    }

    static void Insert(string input)
    {
        input = CustomReplace(input, "INTO ", "");
        var parts = CustomSplit(input, " VALUES ");
        if (parts.Count != 2)
        {
            Console.WriteLine("Invalid INSERT command format.");
            return;
        }

        // Extracting table name
        var tableNameParts = CustomSplit(parts[0], "(");
        string tableName = CustomTrim(tableNameParts[0]);
        var table = CustomFindFirst(db.Tables, t => t.Name == tableName);
        if (table == null)
        {
            Console.WriteLine($"Table '{tableName}' not found.");
            return;
        }

        // Extracting column names and values
        int startIndex = CustomIndexOf(parts[0], "(") + 1;
        int endIndex = CustomIndexOf(parts[0], ")");
        var columnParts = CustomSplit(CustomSubstring(parts[0], startIndex, endIndex - startIndex), ",");

        var columns = new List<string>();
        foreach (var part in columnParts)
        {
            columns.Add(CustomTrim(part));
        }

        string valuePart = CustomTrim(CustomSubstringAndTrim(parts[1], 1, parts[1].Length - 2));
        var values = SplitValues(valuePart);

        if (columns.Count != values.Length)
        {
            Console.WriteLine("Columns and values count do not match.");
            return;
        }

        var rowData = new Dictionary<string, object>();
        for (int i = 0; i < columns.Count; i++)
        {
            string columnName = columns[i];
            bool columnExists = false;
            foreach (var col in table.Columns)
            {
                if (col.Name == columnName)
                {
                    columnExists = true;
                    break;
                }
            }

            if (!columnExists)
            {
                Console.WriteLine($"Column '{columnName}' does not exist in the table '{tableName}'.");
                return;
            }

            var columnDefinition = CustomFindFirst(table.Columns, col => col.Name == columnName);
            object value = ConvertToType(CustomTrim(values[i].Trim(new char[] { '"', ' ' })), columnDefinition.Type);
            rowData.Add(columnName, value);
        }

        bool isDuplicate = false;
        foreach (var existingRow in table.Rows)
        {
            if (IsDuplicateRow(rowData, existingRow))
            {
                isDuplicate = true;
                break;
            }
        }

        if (isDuplicate)
        {
            Console.WriteLine("This row already exists. Try another one.");
            return;
        }

        table.AddRow(rowData);
        SaveDatabase();
        Console.WriteLine("Row inserted.");
    }


    static void Delete(string input)
    {
        // Remove 'FROM ' prefix to simplify parsing
        if (!input.StartsWith("FROM "))
        {
            Console.WriteLine("Invalid DELETE command format.");
            return;
        }

        string modifiedInput = CustomTrim(CustomSubstring(input, 5, input.Length - 5)); // 'FROM ' is 5 characters long

        // Find the start of the WHERE clause if it exists
        int whereIndex = CustomIndexOf(modifiedInput, " WHERE ");
        string tableName;
        string whereClause = "";

        if (whereIndex != -1)
        {
            // Extract table name and WHERE clause
            tableName = CustomTrim(CustomSubstring(modifiedInput, 0, whereIndex));
            whereClause = CustomTrim(CustomSubstring(modifiedInput, whereIndex + 7, modifiedInput.Length - whereIndex - 7));
        }
        else
        {
            // If there's no WHERE clause, inform the user and exit
            Console.WriteLine("No WHERE clause was provided.");
            return;
        }

        var table = CustomFindFirst(db.Tables, t => t.Name == tableName);
        if (table == null)
        {
            Console.WriteLine("Table not found.");
            return;
        }

        if (!string.IsNullOrEmpty(whereClause))
        {
            var rowsToKeep = CustomFilter(table.Rows, row => !EvaluateConditionForDelete(row, whereClause, table.Columns, typeStrategies));
            int rowsDeleted = table.Rows.Count - rowsToKeep.Count;
            table.ReplaceRows(rowsToKeep);

            if (rowsDeleted > 0)
            {
                Console.WriteLine(rowsDeleted == 1 ? "Row deleted." : $"{rowsDeleted} rows deleted.");
                SaveDatabase();
            }
            else
            {
                Console.WriteLine("No rows deleted.");
            }
        }
    }
    #endregion

    #region SaveFile
    static void SaveDatabase()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            options.Converters.Add(new CustomTableConverter());

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = Path.Combine(desktopPath, "database.json");
            string json = JsonSerializer.Serialize(db, options);

            File.WriteAllText(filePath, json);
            Console.WriteLine("Database saved successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error saving database: " + ex.Message);
        }
    }

    static void LoadDatabase()
    {
        try
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new CustomTableConverter());

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = Path.Combine(desktopPath, "database.json");

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                db = JsonSerializer.Deserialize<Database>(json, options);
                Console.WriteLine("Database loaded successfully.");
            }
            else
            {
                db = new Database();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error loading database: " + ex.Message);
        }
    }
    #endregion

    #region Methods
    static int FindFirstSpaceIndex(string input)
    {
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == ' ')
            {
                return i;
            }
        }
        return -1;
    }
    static string CustomReplace(string input, string oldValue, string newValue)
    {
        var result = new StringBuilder();
        int start = 0;
        int index;

        while ((index = CustomIndexOf(CustomSubstring(input, start, input.Length - start), oldValue)) != -1)
        {
            result.Append(CustomSubstring(input, start, index));
            result.Append(newValue);
            start += index + oldValue.Length;
        }

        result.Append(CustomSubstring(input, start, input.Length - start));
        return result.ToString();
    }

    static List<string> CustomSplit(string input, string delimiter)
    {
        var results = new List<string>();
        int start = 0;

        while (start < input.Length)
        {
            int index = CustomIndexOf(CustomSubstring(input, start, input.Length - start), delimiter);
            if (index == -1)
            {
                results.Add(CustomSubstring(input, start, input.Length - start));
                break;
            }
            else
            {
                index += start;
            }

            results.Add(CustomSubstring(input, start, index - start));
            start = index + delimiter.Length;
        }

        return results;
    }

    static string CustomTrim(string input)
    {
        int start = 0, end = input.Length - 1;
        while (start <= end && char.IsWhiteSpace(input[start])) start++;
        while (end >= start && char.IsWhiteSpace(input[end])) end--;
        return CustomSubstring(input, start, end - start + 1);
    }

    static int CustomIndexOf(string input, string substring)
    {
        for (int i = 0; i <= input.Length - substring.Length; i++)
        {
            int j;
            for (j = 0; j < substring.Length; j++)
            {
                if (input[i + j] != substring[j])
                    break;
            }
            if (j == substring.Length)
                return i;
        }
        return -1; // Substring not found
    }
    static string CustomSubstring(string input, int startIndex, int length)
    {
        char[] result = new char[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = input[startIndex + i];
        }
        return new string(result);
    }
    static string ExtractOperation(string condition, int startIndex)
    {
        var operation = new StringBuilder();
        for (int i = startIndex; i < condition.Length; i++)
        {
            if (" <>=".Contains(condition[i]))
            {
                operation.Append(condition[i]);
            }
            else
            {
                break;
            }
        }
        return CustomTrim(operation.ToString());
    }
    static string CustomSubstringAndTrim(string input, int startIndex, int length)
    {
        var result = new StringBuilder();
        for (int i = startIndex; i < startIndex + length && i < input.Length; i++)
        {
            if (!char.IsWhiteSpace(input[i]))
            {
                result.Append(input[i]);
            }
        }
        return result.ToString();
    }
    static T CustomFindFirst<T>(IEnumerable<T> items, Func<T, bool> predicate)
    {
        foreach (var item in items)
        {
            if (predicate(item))
            {
                return item;
            }
        }
        return default;
    }
    static IEnumerable<Dictionary<string, object>> CustomWhere(IEnumerable<Dictionary<string, object>> rows, Func<Dictionary<string, object>, bool> predicate)
    {
        foreach (var row in rows)
        {
            if (predicate(row))
            {
                yield return row;
            }
        }
    }
    static List<Dictionary<string, object>> CustomFilter(IEnumerable<Dictionary<string, object>> rows, Func<Dictionary<string, object>, bool> predicate)
    {
        var filteredRows = new List<Dictionary<string, object>>();
        foreach (var row in rows)
        {
            if (predicate(row))
            {
                filteredRows.Add(row);
            }
        }
        return filteredRows;
    }
    static bool CustomContains(string input, string substring)
    {
        return CustomIndexOf(input, substring) != -1;
    }

    #region CreateTable_Methods
    static List<Column> ParseColumnDefinitions(string input)
    {
        List<Column> columns = new List<Column>();

        int counter = 0;
        int start = 0;

        while (counter < Counter(input))
        {
            int commaIndex = FindNextComma(input, start);
            string columnDef = commaIndex == -1 ? input.Substring(start) : input.Substring(start, commaIndex - start);

            int colonIndex = FindFirstChar(columnDef, ':');
            if (colonIndex == -1)
            {
                Console.WriteLine($"Invalid column definition: {columnDef}");
                return null;
            }

            string columnName = columnDef.Substring(0, colonIndex).Trim();

            string remainingDef = columnDef.Substring(colonIndex + 1).Trim();
            string columnType, defaultValue = null;
            int defaultIndex = remainingDef.IndexOf("default");
            if (defaultIndex != -1)
            {
                columnType = remainingDef.Substring(0, defaultIndex).Trim();
                defaultValue = remainingDef.Substring(defaultIndex + 8).Trim(new char[] { '“', '”', ' ' });
            }
            else
            {
                columnType = remainingDef;
            }

            columns.Add(new Column(columnName, columnType, defaultValue));
            start = commaIndex + 1;
            counter++;
        }

        return columns;
    }
    static int FindFirstChar(string input, char toFind)
    {
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == toFind)
            {
                return i;
            }
        }
        return -1;
    }
    static int FindLastChar(string input, char toFind)
    {
        for (int i = input.Length - 1; i >= 0; i--)
        {
            if (input[i] == toFind)
            {
                return i;
            }
        }
        return -1;
    }
    static int FindNextComma(string input, int start)
    {
        for (int i = start; i < input.Length; i++)
        {
            if (input[i] == ',')
            {
                return i;
            }
        }
        return -1;
    }
    static int Counter(string input)
    {
        int counter = 0;

        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == ':')
            {
                counter++;
            }
        }

        return counter;
    }
    #endregion
    #region Select_Methods
    static void PrintSelectedRows(IEnumerable<Dictionary<string, object>> rows, string[] columns)
    {
        // Calculate the maximum length of each column
        Dictionary<string, int> columnLengths = new Dictionary<string, int>();
        foreach (var column in columns)
        {
            columnLengths[column] = column.Length; // Start with the column name length
        }

        // Update the dictionary with the maximum length of data in each column
        foreach (var row in rows)
        {
            foreach (var col in columns)
            {
                int length = row[col].ToString().Length;
                if (length > columnLengths[col])
                {
                    columnLengths[col] = length;
                }
            }
        }

        // Create a format string for each column
        string[] formatStrings = columns.Select(col => $"{{0,-{columnLengths[col]}}}").ToArray();

        // Print the header
        for (int i = 0; i < columns.Length; i++)
        {
            Console.Write(formatStrings[i], columns[i]);
            Console.Write(" ");
        }
        Console.WriteLine();

        // Print each row
        foreach (var row in rows)
        {
            for (int i = 0; i < columns.Length; i++)
            {
                string col = columns[i];
                string data = row.ContainsKey(col) ? row[col].ToString() : "NULL";
                Console.Write(formatStrings[i], data);
                Console.Write(" ");
            }
            Console.WriteLine();
        }
    }
    static bool EvaluateCondition(Dictionary<string, object> row, string fieldName, string operation, string value, List<Column> columns, Dictionary<string, ITypeStrategy> strategies)
    {
        if (!row.ContainsKey(fieldName)) return false;

        Column column = null;
        foreach (var col in columns)
        {
            if (col.Name == fieldName)
            {
                column = col;
                break;
            }
        }

        if (column == null || !strategies.ContainsKey(column.Type.ToLower())) return false;

        var strategy = strategies[column.Type.ToLower()];
        var rowValue = row[fieldName];

        // Convert both the rowValue and the comparison value using the strategy
        var convertedRowValue = strategy.Convert(rowValue.ToString());
        var convertedComparisonValue = strategy.Convert(value);

        return strategy.Compare(convertedRowValue, convertedComparisonValue, operation);
    }
    static IEnumerable<Dictionary<string, object>> ProcessWhereClause(IEnumerable<Dictionary<string, object>> rows, string whereClause, List<Column> columns, Dictionary<string, ITypeStrategy> strategies)
    {
        var conditions = CustomSplit(whereClause, " AND ");
        foreach (var condition in conditions)
        {
            string fieldName = "", operation = "", value = "";

            // Handle different operators: =, <>, <, >
            string[] parts;
            if (CustomContains(condition, "<>"))
            {
                parts = CustomSplit(condition, "<>").ToArray();
                operation = "<>";
            }
            else if (CustomContains(condition, "<"))
            {
                parts = CustomSplit(condition, "<").ToArray();
                operation = "<";
            }
            else if (CustomContains(condition, ">"))
            {
                parts = CustomSplit(condition, ">").ToArray();
                operation = ">";
            }
            else if (CustomContains(condition, "="))
            {
                parts = CustomSplit(condition, "=").ToArray();
                operation = "=";
            }
            else
            {
                continue; // Skip if no valid operator is found
            }

            if (parts.Length == 2)
            {
                fieldName = CustomTrim(parts[0]);
                value = CustomTrimQuotes(CustomTrim(parts[1]));
            }

            rows = CustomWhere(rows, row => EvaluateCondition(row, fieldName, operation, value, columns, strategies));
        }
        return rows;
    }
    static string CustomTrimQuotes(string input)
    {
        int start = 0, end = input.Length - 1;
        while (start <= end && (char.IsWhiteSpace(input[start]) || input[start] == '\'' || input[start] == '"')) start++;
        while (end >= start && (char.IsWhiteSpace(input[end]) || input[end] == '\'' || input[end] == '"')) end--;
        return CustomSubstring(input, start, end - start + 1);
    }
    static List<Dictionary<string, object>> CustomDistinctAndOrderBy(IEnumerable<Dictionary<string, object>> rows, string[] columns, string orderByColumn)
    {
        var distinctRows = new List<Dictionary<string, object>>();
        var seenCombinations = new HashSet<string>();

        foreach (var row in rows)
        {
            var combinationParts = new List<string>();
            foreach (var col in columns)
            {
                combinationParts.Add(row.ContainsKey(col) ? row[col].ToString() : "");
            }
            var combination = string.Join(",", combinationParts);
            if (seenCombinations.Add(combination))
            {
                distinctRows.Add(row);
            }
        }

        return CustomOrderBy(distinctRows, orderByColumn);
    }
    static List<Dictionary<string, object>> CustomOrderBy(IEnumerable<Dictionary<string, object>> rows, string orderByColumn)
    {
        var sortedRows = new List<Dictionary<string, object>>(rows);

        sortedRows.Sort((row1, row2) =>
        {
            var value1 = row1.ContainsKey(orderByColumn) ? row1[orderByColumn] : null;
            var value2 = row2.ContainsKey(orderByColumn) ? row2[orderByColumn] : null;

            // Assuming the values are comparable and of the same type
            return Comparer<object>.Default.Compare(value1, value2);
        });

        return sortedRows;
    }
    #endregion
    #region Insert_Method
    static bool IsDuplicateRow(Dictionary<string, object> newRow, Dictionary<string, object> existingRow)
    {
        foreach (var pair in newRow)
        {
            if (!existingRow.ContainsKey(pair.Key) || !existingRow[pair.Key].Equals(pair.Value))
                return false;
        }
        return true;
    }
    static string[] SplitValues(string valuePart)
    {
        // Splitting values considering spaces inside quotes
        List<string> values = new List<string>();
        bool inQuotes = false;
        int startIdx = 0;

        for (int i = 0; i < valuePart.Length; i++)
        {
            if (valuePart[i] == '"')
                inQuotes = !inQuotes;  // Toggle the state of being inside quotes

            bool atLastChar = (i == valuePart.Length - 1);
            if ((valuePart[i] == ',' && !inQuotes) || atLastChar)
            {
                int length = atLastChar ? i - startIdx + 1 : i - startIdx;
                values.Add(valuePart.Substring(startIdx, length).Trim());
                startIdx = i + 1;
            }
        }

        return values.ToArray();
    }
    static object ConvertToType(string value, string type)
    {
        // Handle conversion based on the type
        switch (type.ToLower())
        {
            case "int":
                return int.TryParse(value, out int intValue) ? intValue : 0;
            case "float":
                return float.TryParse(value, out float floatValue) ? floatValue : 0.0f;
            case "bool":
                return bool.TryParse(value, out bool boolValue) && boolValue;
            case "date":
                return DateTime.TryParse(value, out DateTime dateValue) ? dateValue : default(DateTime);
            default: // Assuming string for default
                return value;
        }
    }
    #endregion
    #region Delete_Methods
    static bool EvaluateConditionForDelete(Dictionary<string, object> row, string condition, List<Column> columns, Dictionary<string, ITypeStrategy> strategies)
    {
        // Find the index of the operator
        int indexOfOperator = FindOperatorIndex(condition);

        if (indexOfOperator < 0) return false; // Operator not found

        var fieldName = CustomSubstringAndTrim(condition, 0, indexOfOperator);
        var operation = ExtractOperation(condition, indexOfOperator);
        var value = CustomTrimQuotes(CustomSubstringAndTrim(condition, indexOfOperator + operation.Length, condition.Length - (indexOfOperator + operation.Length)));

        // Check if fieldName is valid
        foreach (var column in columns)
        {
            if (column.Name == fieldName)
            {
                return EvaluateCondition(row, fieldName, operation, value, columns, strategies);
            }
        }

        Console.WriteLine($"Field name '{fieldName}' not found.");
        return false;
    }
    static int FindOperatorIndex(string condition)
    {
        var operators = new[] { "<>", "=", "<", ">" };
        foreach (var op in operators)
        {
            var index = CustomIndexOf(condition, op);
            if (index >= 0)
                return index;
        }
        return -1; // No operator found
    }
    #endregion

    #endregion
}