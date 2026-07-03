namespace ArchiveOrgUploader;

/// <summary>
/// Wraps one CSV data row (as a raw string array) plus the header-name-to-column-index map,
/// so callers can read/write columns by name without disturbing any other columns in the row.
/// </summary>
public class LogRow
{
    private readonly string[] _fields;
    private readonly Dictionary<string, int> _headerIndex;

    public LogRow(string[] fields, Dictionary<string, int> headerIndex)
    {
        _fields = fields;
        _headerIndex = headerIndex;
    }

    public IReadOnlyList<string> RawFields => _fields;

    public string Get(string column)
    {
        if (_headerIndex.TryGetValue(column, out var idx) && idx < _fields.Length)
            return _fields[idx] ?? "";
        return "";
    }

    public void Set(string column, string value)
    {
        if (_headerIndex.TryGetValue(column, out var idx) && idx < _fields.Length)
            _fields[idx] = value;
    }
}
