ArchiveOrgUploader
A small console app that reads `ArchiveOrgLog.csv`, uploads each referenced file to
Archive.org, and writes attempt/success timestamps back into the CSV as it goes.
What it does
Looks for `ArchiveOrgLog.csv` and `appsettings.json` in the same directory as the exe.
Reads each row's `FileName`, `PublicationDate`, `Title`, `Topics`, `Category`,
`AuthorOrSubject`, `GeneratedDescription`, `AdditionalDescription`,
`AttemptedUploadTime`, `SuccessfulUploadTime`.
For each row (skipping any row whose `SuccessfulUploadTime` is already filled in, unless
you turn that off in config):
Finds the file next to the exe using `FileName`.
Builds an Archive.org item identifier from the filename.
Uploads via Archive.org's S3-like API using `Title`, `Topics`, `AuthorOrSubject`, and
`GeneratedDescription + AdditionalDescription` as metadata.
Writes the current timestamp to `AttemptedUploadTime` and saves the CSV before the
upload starts, so partial progress is never lost if the app is interrupted mid-run.
On a successful upload, writes the current timestamp to `SuccessfulUploadTime` and saves
again.
No third-party NuGet packages are used — just the .NET SDK's built-in `HttpClient` and
`System.Text.Json`, plus a small hand-rolled CSV parser (so quoted fields with embedded commas,
like in your example row, parse correctly).
Setup
Get your Archive.org S3-like API keys at https://archive.org/account/s3.php.
Open `appsettings.json` and fill in `AccessKey` and `SecretKey`.
`ItemIdentifierPrefix` is prepended to every item identifier (e.g. `fsc-archives-`) —
change or clear it as you like.
`DelayBetweenUploadsMs` throttles requests between files (default 2 seconds) — bump this
up if Archive.org starts rate-limiting you.
`SkipIfAlreadySuccessful` (default `true`) skips rows that already have a
`SuccessfulUploadTime`, so you can safely re-run the app over a partially-completed log.
Build it:
```
   dotnet build -c Release
   ```
Copy (or symlink) `ArchiveOrgLog.csv` and all the PDFs it references into the build output
folder (e.g. `bin/Release/net8.0/`), alongside the exe and `appsettings.json`.
Run it:
```
   dotnet run -c Release
   ```
or run the built exe directly.
Notes / things you'll likely want to tweak
Item identifiers: currently derived from the filename (stripped of the extension and any
characters Archive.org disallows). If you'd rather control identifiers explicitly, add an
`Identifier` column to the CSV and read it in `Program.cs` instead of calling
`IdentifierHelper.BuildIdentifier`.
PublicationDate / Category: both are read from the CSV into each row but aren't currently
sent as Archive.org metadata (the spec only called for Title/Topics/AuthorOrSubject/
Description). `ArchiveOrgClient.cs` has a comment showing where to add `date` and
`collection` metadata headers if you want to wire those in.
Topics → subject: `Topics` is split on commas and sent as multiple Archive.org `subject`
values.
Error handling is intentionally minimal — failures are logged to the console and the row
is left with `AttemptedUploadTime` set but no `SuccessfulUploadTime`, so it'll be retried on
the next run.
Duplicate identifiers: if two files would sanitize to the same identifier, the second
upload will fail (Archive.org items are unique by identifier). Worth a quick sanity check
across your ~750 filenames before a big batch run.
This is deliberately "rough" per your ask — solid enough to run against your real log, but you'll
likely want to adjust identifier naming, metadata fields, and error handling to match your
archive's conventions as you go.