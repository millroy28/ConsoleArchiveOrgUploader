using System.Text;
using System.Text.Json;
using ArchiveOrgUploader;


// STAGE 00 - INIT

ConsoleWriter console = new();

console.PrintHeader("ARCHIVE.ORG WEB UPLOADER - V 0.1");

var baseDir = AppContext.BaseDirectory;
var configPath = Path.Combine(baseDir, "appsettings.json");
bool upload = false;

// STAGE 01 - GET CONFIG
console.PrintBreaker();
console.PrintHeader("Loading configuration...");
var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath))
             ?? throw new Exception("Could not parse appsettings.json");
int batchSize = config.DefaultBatchSize;

if (string.IsNullOrWhiteSpace(config.AccessKey) || config.AccessKey.StartsWith("YOUR_"))
{
    console.PrintError(["ERROR: Please set AccessKey/SecretKey in appsettings.json before running.", "...exiting program..."]);
    return;
}

if (!File.Exists(configPath))
{
    console.PrintError([$"ERROR: Could not find {configPath}."
                       , "Copy appsettings.json next to the exe and fill in your Archive.org S3-like keys"
                       , "(get them at https://archive.org/account/s3.php)."
                       , "...exiting program..."]);
    return;
}

console.PrintDefault(["CONFIGURATION LOADED"
                    ,$"Identifier Prefix: {config.ItemIdentifierPrefix}"
                    ,$"Log File Name: {config.LogFileName}"
                    ,$"Batch Size: {config.DefaultBatchSize.ToString()}"]);


// STAGE 02 - GET LOG CSV
console.PrintBreaker();
console.PrintHeader("Reading Logfile...");

var basePath = ConsoleHelpers.GetLocalArchivePath(config);
var csvPath = Path.Combine(basePath, config.LogFileName);
 
string[] requiredColumns =
{
    "FileName", "PublicationDate", "Title", "Topics", "Category",
    "AuthorOrSubject", "GeneratedDescription", "AdditionalDescription",
    "AttemptedUploadTime", "SuccessfulUploadTime"
};

string rawText="";
try
{
    rawText = File.ReadAllText(csvPath);
}
catch
{
    console.PrintError([$"Unable to open {config.LogFileName}", "...exiting program..."]);
    return;
}
var allRows = Csv.Parse(rawText);

if (allRows.Count == 0)
{
    console.PrintError(["CSV file is empty!", "...exiting program..."]);
    return;
}

var headers = allRows[0];
var headerIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
for (int i = 0; i < headers.Count; i++)
    headerIndex[headers[i].Trim()] = i;

var missing = requiredColumns.Where(c => !headerIndex.ContainsKey(c)).ToList();
if (missing.Count > 0)
{
    console.PrintError(["CSV is missing required column(s): " + string.Join(", ", missing), "...exiting program..."]);
    return;
}

// Wrap each data row, padding short rows out to the full header width first.
var dataRows = new List<LogRow>();
for (int r = 1; r < allRows.Count; r++)
{
    var row = allRows[r];
    if (row.Count == 1 && string.IsNullOrWhiteSpace(row[0]))
        continue; // skip a trailing blank line

    while (row.Count < headers.Count)
        row.Add("");

    dataRows.Add(new LogRow(row.ToArray(), headerIndex));
}

console.PrintDefault($"Found {dataRows.Count} row(s) in {config.LogFileName}.");

string mode = console.GetStringInput("Do you wish to proceed? (Y/YES to continue, N/NO to exit, T/TEST to continue without uploading",
                                     ["y"
                                     ,"yes"
                                     ,"n"
                                     ,"no"
                                     ,"t"
                                     ,"test"]);
mode = mode.ToLower().Trim();
if (mode == "n" || mode == "no")
{
    console.PrintError("...exiting program...");
    return;
}
if (mode == "y" ||  mode == "yes")
{
    upload = true;
    string changeBatchSize = console.GetStringInput("Do you wish to change the batch size (number of files to upload)? (Y/N)", ["y", "yes", "n", "no"]);
    if(changeBatchSize.ToLower().Trim() == "yes" || changeBatchSize.ToLower().Trim() == "y")
    {
        batchSize = console.GetIntInput("Enter desired batch size: ", 200, 0);
    }
}

// STAGE 03 - SEND TO ARCHIVE.ORG
console.PrintBreaker();
console.PrintHeader("Uploading to Archive.org...");

var client = new ArchiveOrgClient(config.AccessKey, config.SecretKey);

int successful = 0;
int skipped = 0;
int failed = 0;
int total = 0;

foreach (var row in dataRows)
{
    total++;
    var fileName = row.Get("FileName").Trim();

    if(skipped + failed > batchSize)
    {
        console.PrintWarn("Batch limit reached, skipping '{fileName}'");
        skipped++;
        continue;
    }

    if (string.IsNullOrWhiteSpace(fileName))
    {
        console.PrintWarn("Skipping row with no FileName...");
        skipped++;
        continue;
    }

    var existingSuccessTime = row.Get("SuccessfulUploadTime");
    if (config.SkipIfAlreadySuccessful && !string.IsNullOrWhiteSpace(existingSuccessTime))
    {
        console.PrintWarn($"Skipping '{fileName}' — already uploaded successfully on {existingSuccessTime}.");
        skipped++;
        continue;
    }

    var filePath = Path.Combine(basePath, fileName);
    if (!File.Exists(filePath))
    {
        console.PrintWarn($"File not found for row: {filePath}. Skipping.");
        skipped++;
        continue;
    }

    var identifier = IdentifierHelper.BuildIdentifier(config.ItemIdentifierPrefix, fileName);

    var title = row.Get("Title");
    var topics = row.Get("Topics");
    var author = row.Get("AuthorOrSubject");
    var publishedDate = row.Get("PublicationDate");
    var description = string.Join(
        " ",
        new[] { row.Get("GeneratedDescription"), row.Get("AdditionalDescription") }
            .Where(s => !string.IsNullOrWhiteSpace(s)));

    console.PrintDefault($"Uploading '{fileName}' as item '{identifier}'...");

    if (upload)
    {
        row.Set("AttemptedUploadTime", DateTime.Now.ToString("o"));
        SaveCsv();

        try
        {
            var (success, message) = await client.UploadAsync(
                filePath,
                identifier,
                fileName,
                title,
                topics,
                author,
                description,
                publishedDate);

            if (success)
            {
                successful++;
                console.PrintSuccess($"  SUCCESS: {message}");
                row.Set("SuccessfulUploadTime", DateTime.Now.ToString("o"));
                SaveCsv();
            }
            else
            {
                failed++;
                console.PrintFail($"  FAILED: {message}");
            }
        }
        catch (Exception ex)
        {
            failed++;
            console.PrintError($"  ERROR: {ex.Message}");
        }

        if (config.DelayBetweenUploadsMs > 0)
            await Task.Delay(config.DelayBetweenUploadsMs);
    }
    else
    {
        skipped++;
        console.PrintWarn("...upload skipped in test mode");
    }
}

// STAGE 04 REPORT
console.PrintBreaker();
console.PrintHeader("Job Complete!");
console.PrintDefault([$"Total Files In Log......{total.ToString()}"
                     ,$"Successfully Uploaded...{successful.ToString()}"
                     ,$"Skipped.................{skipped.ToString()}"
                     ,$"Failed..................{failed.ToString()}"]);
                                                                                




void SaveCsv()
{
    var sb = new StringBuilder();
    sb.AppendLine(Csv.WriteRow(headers));
    foreach (var row in dataRows)
        sb.AppendLine(Csv.WriteRow(row.RawFields));
    File.WriteAllText(csvPath, sb.ToString());
}
public class Config
{
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public string ItemIdentifierPrefix { get; set; } = "";
    public int DelayBetweenUploadsMs { get; set; } = 2000;
    public bool SkipIfAlreadySuccessful { get; set; } = true;
    public string LogFileName { get; set; } = "";
    public int DefaultBatchSize {  get; set; } 
}

public class ConsoleHelpers
{
    public static string GetLocalArchivePath(Config config)
    {
        ConsoleWriter console = new();
        var input = console.GetStringInput("Please specify directory for archive files and log: ");

        var path = Path.Combine(input, config.LogFileName);
        if (!File.Exists(path))
        {
            console.PrintWarn($"Invalid path: could not find {config.LogFileName} in specified directory...");
            return GetLocalArchivePath(config);
        }
        return input;
    }
}