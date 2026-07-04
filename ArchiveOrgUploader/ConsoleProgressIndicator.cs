namespace ArchiveOrgUploader;

/// <summary>
/// Wraps an async operation with a live "still working" indicator on the console — a spinner
/// plus elapsed seconds — so a multi-second upload doesn't look like the program has hung.
/// The status line is overwritten in place (via \r) and cleared once the operation completes.
/// </summary>
public static class ProgressIndicator
{
    private static readonly char[] Spinner = { '|', '/', '-', '\\' };

    public static async Task<T> RunAsync<T>(string label, Func<Task<T>> action)
    {
        using var cts = new CancellationTokenSource();
        var animateTask = AnimateAsync(label, cts.Token);

        try
        {
            return await action();
        }
        finally
        {
            cts.Cancel();
            try { await animateTask; } catch (OperationCanceledException) { /* expected */ }
            ClearLine();
        }
    }

    private static async Task AnimateAsync(string label, CancellationToken token)
    {
        var start = DateTime.UtcNow;
        int frame = 0;

        while (!token.IsCancellationRequested)
        {
            var elapsed = (DateTime.UtcNow - start).TotalSeconds;
            var spinnerChar = Spinner[frame % Spinner.Length];
            Console.Write($"\r{label} {spinnerChar} {elapsed,5:0.0}s ");
            frame++;

            try
            {
                await Task.Delay(200, token);
            }
            catch (TaskCanceledException)
            {
                // Expected the moment the operation finishes and we cancel the token.
            }
        }
    }

    private static void ClearLine()
    {
        // A fixed-width blank-out avoids Console.WindowWidth, which throws when output is
        // redirected (e.g. piped to a file or run under CI) rather than an interactive terminal.
        Console.Write("\r" + new string(' ', 80) + "\r");
    }
}