using System.Text;

namespace ArchiveOrgUploader;

/// <summary>
/// A small hand-rolled CSV parser/writer. No external dependencies (like CsvHelper) so the
/// project builds with nothing but the .NET SDK. Handles quoted fields, embedded commas,
/// embedded quotes ("" escaping), and embedded newlines inside quoted fields.
/// </summary>
public static class Csv
{
    public static List<List<string>> Parse(string text)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        bool inQuotes = false;
        int i = 0;
        int len = text.Length;

        while (i < len)
        {
            char c = text[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < len && text[i + 1] == '"')
                    {
                        field.Append('"');
                        i += 2;
                    }
                    else
                    {
                        inQuotes = false;
                        i++;
                    }
                }
                else
                {
                    field.Append(c);
                    i++;
                }
                continue;
            }

            switch (c)
            {
                case '"':
                    inQuotes = true;
                    i++;
                    break;
                case ',':
                    row.Add(field.ToString());
                    field.Clear();
                    i++;
                    break;
                case '\r':
                    i++;
                    break;
                case '\n':
                    row.Add(field.ToString());
                    field.Clear();
                    rows.Add(row);
                    row = new List<string>();
                    i++;
                    break;
                default:
                    field.Append(c);
                    i++;
                    break;
            }
        }

        // Flush the last field/row if the file doesn't end with a newline.
        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            rows.Add(row);
        }

        return rows;
    }

    public static string WriteRow(IEnumerable<string> fields) =>
        string.Join(",", fields.Select(Escape));

    private static string Escape(string? value)
    {
        value ??= "";
        bool needsQuoting = value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0;
        return needsQuoting ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;
    }
}
