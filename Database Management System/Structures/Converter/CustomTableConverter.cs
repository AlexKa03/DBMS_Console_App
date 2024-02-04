using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

public class CustomTableConverter : JsonConverter<Table>
{
    public override Table Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var table = new Table();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return table;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            string propertyName = reader.GetString();
            reader.Read();

            switch (propertyName)
            {
                case "Name":
                    table.Name = reader.GetString();
                    break;
                case "Columns":
                    List<Column> columns = JsonSerializer.Deserialize<List<Column>>(ref reader, options);
                    table.SetColumns(columns);
                    break;
                case "Rows":
                    List<Dictionary<string, object>> rows = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(ref reader, options);
                    table.SetRows(rows);
                    break;
                default:
                    throw new JsonException("Unknown property: " + propertyName);
            }
        }

        throw new JsonException("JSON format error.");
    }

    public override void Write(Utf8JsonWriter writer, Table value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("Name", value.Name);

        writer.WritePropertyName("Columns");
        JsonSerializer.Serialize(writer, value.Columns, options);

        writer.WritePropertyName("Rows");
        JsonSerializer.Serialize(writer, value.Rows, options);

        writer.WriteEndObject();
    }
}