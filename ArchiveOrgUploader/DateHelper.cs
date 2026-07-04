using System.Globalization;

namespace ArchiveOrgUploader;

/// <summary>
/// Archive.org's metadata "date" field is only reliably parsed by their systems when it's in
/// ISO 8601 (YYYY-MM-DD) form. This converts whatever format the CSV happens to have
/// (3/25/1945, 1945-03-25, March 25 1945, etc.) into that form.
/// </summary>
public static class DateHelper
{
    // Tried in order, as exact formats, before falling back to general parsing. Listed mainly
    // so that ambiguous formats (M/d/yyyy vs d/M/yyyy) resolve the way a US-sourced archive
    // log is likely to mean them, rather than however the local OS culture would guess.
    private static readonly string[] ExactFormats =
    {
        "M/d/yyyy", "MM/dd/yyyy", "M/d/yy", "MM/dd/yy",
        "yyyy-MM-dd", "yyyy/MM/dd",
        "MMMM d, yyyy", "MMM d, yyyy",
        "MMMM d yyyy", "MMM d yyyy",
        "d MMMM yyyy", "d MMM yyyy"
    };

    /// <summary>
    /// Attempts to convert <paramref name="raw"/> to YYYY-MM-DD. If parsing fails, returns the
    /// original string unchanged and sets Parsed to false so the caller can warn/log it.
    /// </summary>
    public static (string Value, bool Parsed) ToIso8601(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return (raw, false);

        var trimmed = raw.Trim();

        if (DateTime.TryParseExact(trimmed, ExactFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact))
            return (exact.ToString("yyyy-MM-dd"), true);

        // Fall back to general parsing (invariant culture, so "3/4/1945" is always read as
        // month/day rather than depending on the machine's regional settings).
        if (DateTime.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.None, out var general))
            return (general.ToString("yyyy-MM-dd"), true);

        return (raw, false);
    }
}