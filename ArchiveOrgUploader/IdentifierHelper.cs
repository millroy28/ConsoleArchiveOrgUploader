using System.Text;

namespace ArchiveOrgUploader;

/// <summary>
/// Archive.org item identifiers may only contain letters, digits, underscore, hyphen, and
/// period, and must be 3-80 characters long. This derives one from the source filename.
/// </summary>
public static class IdentifierHelper
{
    public static string BuildIdentifier(string prefix, string fileName)
    {
        var nameNoExt = Path.GetFileNameWithoutExtension(fileName);
        var sb = new StringBuilder();

        foreach (var c in nameNoExt)
        {
            if (char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.')
                sb.Append(c);
            else if (char.IsWhiteSpace(c))
                sb.Append('-');
            // anything else (commas, apostrophes, etc.) is dropped
        }

        var slug = sb.ToString().Trim('-', '.', '_');
        while (slug.Contains("--"))
            slug = slug.Replace("--", "-");

        var identifier = (prefix ?? "") + slug;

        if (identifier.Length > 80)
            identifier = identifier[..80];
        if (identifier.Length < 3)
            identifier = identifier.PadRight(3, '0');

        return identifier.ToLowerInvariant();
    }
}
