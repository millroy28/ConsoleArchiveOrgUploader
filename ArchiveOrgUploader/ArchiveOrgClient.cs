namespace ArchiveOrgUploader;

/// <summary>
/// Talks to Archive.org's S3-like upload API (IAS3): https://archive.org/developers/ias3.html
/// A PUT to https://s3.us.archive.org/{identifier}/{filename} both creates the item (if it
/// doesn't exist) and uploads the file, driven by x-archive-meta-* headers for metadata.
/// </summary>
public class ArchiveOrgClient
{
    private readonly HttpClient _http;
    private readonly string _accessKey;
    private readonly string _secretKey;

    public ArchiveOrgClient(string accessKey, string secretKey)
    {
        _accessKey = accessKey;
        _secretKey = secretKey;
        _http = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(30) // large PDFs / scans can take a while
        };
    }

    public async Task<(bool Success, string Message)> UploadAsync(
        string filePath,
        string identifier,
        string remoteFileName,
        string title,
        string topics,
        string author,
        string description,
        string publicationDate)
    {
        var url = $"https://s3.us.archive.org/{identifier}/{Uri.EscapeDataString(remoteFileName)}";

        await using var fileStream = File.OpenRead(filePath);
        using var content = new StreamContent(fileStream);
        content.Headers.ContentLength = fileStream.Length;

        using var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = content
        };

        // "LOW" auth scheme, per IAS3 docs. Keys come from https://archive.org/account/s3.php
        request.Headers.TryAddWithoutValidation("Authorization", $"LOW {_accessKey}:{_secretKey}");

        // Auto-create the item if it doesn't already exist.
        request.Headers.TryAddWithoutValidation("x-amz-auto-make-bucket", "1");

        AddMetaField(request, "title", new[] { title });
        AddMetaField(request, "creator", new[] { author });
        AddMetaField(request, "description", new[] { description });

        // Topics can be a comma-separated list; Archive.org supports multiple "subject"
        // values via numbered headers (x-archive-meta01-subject, x-archive-meta02-subject, ...).
        var topicList = topics.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        AddMetaField(request, "subject", topicList);

        // Extension point: PublicationDate and Category are read from the CSV (see Program.cs)
        // but aren't sent as metadata yet. To wire them in, add e.g.:
        AddMetaField(request, "date", new[] { publicationDate });
        //   AddMetaField(request, "collection", new[] { someCollectionForCategory });

        var response = await _http.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        return (response.IsSuccessStatusCode, $"{(int)response.StatusCode} {response.ReasonPhrase} {Truncate(body, 300)}");
    }

    private static void AddMetaField(HttpRequestMessage request, string field, IEnumerable<string> values)
    {
        int i = 1;
        foreach (var v in values)
        {
            if (string.IsNullOrWhiteSpace(v)) continue;

            // Single-valued fields can use the plain header name; anything that might repeat
            // (like subject) needs the two-digit index prefix so each value survives.
            var headerName = (i == 1 && field != "subject")
                ? $"x-archive-meta-{field}"
                : $"x-archive-meta{i:D2}-{field}";

            request.Headers.TryAddWithoutValidation(headerName, EncodeHeaderValue(v.Trim()));
            i++;
        }
    }

    private static string EncodeHeaderValue(string value) =>
        // Archive.org expects percent-encoded UTF-8 for header values containing non-ASCII
        // or otherwise header-unsafe characters.
        Uri.EscapeDataString(value);

    private static string Truncate(string s, int max) =>
        string.IsNullOrEmpty(s) || s.Length <= max ? s : s[..max] + "...";
}
