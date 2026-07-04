namespace ArchiveOrgUploader;

/// <summary>
/// Derives the Archive.org "mediatype" value (texts, image, audio, movies, software, data...)
/// from a file's extension. This drives which item-page template/viewer Archive.org uses, so
/// getting it right matters more than most other metadata fields.
/// </summary>
public static class MediaTypeHelper
{
    // Not exhaustive — extend as your archive's file types grow. Anything not listed here
    // falls back to DefaultMediaType.
    private static readonly Dictionary<string, string> ExtensionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // Documents / text
        [".pdf"] = "texts",
        [".txt"] = "texts",
        [".doc"] = "texts",
        [".docx"] = "texts",
        [".rtf"] = "texts",
        [".odt"] = "texts",
        [".epub"] = "texts",
        [".mobi"] = "texts",

        // Images
        [".jpg"] = "image",
        [".jpeg"] = "image",
        [".png"] = "image",
        [".gif"] = "image",
        [".tif"] = "image",
        [".tiff"] = "image",
        [".bmp"] = "image",

        // Audio
        [".mp3"] = "audio",
        [".wav"] = "audio",
        [".flac"] = "audio",
        [".m4a"] = "audio",
        [".ogg"] = "audio",
        [".wma"] = "audio",

        // Video
        [".mp4"] = "movies",
        [".mov"] = "movies",
        [".avi"] = "movies",
        [".mkv"] = "movies",
        [".wmv"] = "movies",
        [".m4v"] = "movies",

        // Software / archives
        [".zip"] = "software",
        [".rar"] = "software",
        [".7z"] = "software",
        [".exe"] = "software",
        [".iso"] = "software",
    };

    private const string DefaultMediaType = "data";

    public static string GetMediaType(string fileName)
    {
        var ext = Path.GetExtension(fileName);
        return !string.IsNullOrEmpty(ext) && ExtensionMap.TryGetValue(ext, out var mediaType)
            ? mediaType
            : DefaultMediaType;
    }
}