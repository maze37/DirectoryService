using System.Diagnostics;
using System.Net;
using Xunit;

namespace DirectoryService.ApiTests;

public sealed class DockerComposeFixture : IAsyncLifetime
{
    public Uri BaseAddress { get; } = new("http://localhost:5001");

    private static string RepositoryRoot =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));

    public async Task InitializeAsync()
    {
        await RunDockerComposeAsync("up -d --build");
        await WaitUntilApiIsReadyAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task WaitUntilApiIsReadyAsync()
    {
        using var httpClient = new HttpClient
        {
            BaseAddress = BaseAddress,
            Timeout = TimeSpan.FromSeconds(5),
        };

        var startedAt = DateTimeOffset.UtcNow;
        var timeout = TimeSpan.FromMinutes(2);

        while (DateTimeOffset.UtcNow - startedAt < timeout)
        {
            try
            {
                using var response = await httpClient.GetAsync("/swagger/index.html");
                if (response.StatusCode == HttpStatusCode.OK)
                    return;
            }
            catch
            {
                // Service is still starting up.
            }

            await Task.Delay(TimeSpan.FromSeconds(2));
        }

        throw new TimeoutException("DirectoryService API did not become ready within the expected time.");
    }

    private static async Task RunDockerComposeAsync(string arguments)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"compose {arguments}",
                WorkingDirectory = RepositoryRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            }
        };

        process.Start();

        var stdOutTask = process.StandardOutput.ReadToEndAsync();
        var stdErrTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        var stdOut = await stdOutTask;
        var stdErr = await stdErrTask;

        if (process.ExitCode == 0)
            return;

        throw new InvalidOperationException(
            $"docker compose {arguments} failed with exit code {process.ExitCode}.{Environment.NewLine}" +
            $"stdout:{Environment.NewLine}{stdOut}{Environment.NewLine}" +
            $"stderr:{Environment.NewLine}{stdErr}");
    }
}
