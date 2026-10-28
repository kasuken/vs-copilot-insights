using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using vs_copilot_insights.Models;

namespace vs_copilot_insights.Services;

internal sealed class GitHubCopilotService
{
    private static readonly HttpClient s_httpClient = new(new HttpClientHandler { AllowAutoRedirect = false })
    {
        DefaultRequestHeaders =
        {
            { "User-Agent", "VS-Copilot-Insights" },
        },
    };

    public async Task<CopilotUserData> GetCopilotUserDataAsync(string? enterpriseUri, CancellationToken cancellationToken)
    {
        GitHubHostConfiguration host = GitHubHostConfiguration.Parse(enterpriseUri);
        string token = await GetGitHubTokenAsync(host.Hostname, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Could not obtain a GitHub token for {host.Hostname}. Ensure GitHub CLI is installed and run 'gh auth login --hostname {host.Hostname}'.");

        using var request = new HttpRequestMessage(HttpMethod.Get, $"{host.ApiBaseUrl}/copilot_internal/user");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await s_httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"GitHub API at {host.Hostname} returned {(int)response.StatusCode} {response.ReasonPhrase}. " +
                (response.StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden
                    ? $"Check account access and run 'gh auth login --hostname {host.Hostname}'."
                    : "This instance may not expose the internal Copilot endpoint."));
        }

        string json = await response.Content.ReadAsStringAsync(cancellationToken);
        CopilotUserData data = JsonSerializer.Deserialize<CopilotUserData>(json)
            ?? throw new InvalidOperationException("Failed to deserialize Copilot user data.");
        data.RawJson = json;
        return data;
    }

    private static async Task<string?> GetGitHubTokenAsync(string hostname, CancellationToken cancellationToken)
    {
        try
        {
            var psi = new ProcessStartInfo("gh")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            psi.ArgumentList.Add("auth");
            psi.ArgumentList.Add("token");
            psi.ArgumentList.Add("--hostname");
            psi.ArgumentList.Add(hostname);

            using var process = Process.Start(psi);
            if (process is null)
            {
                return null;
            }

            using var registration = cancellationToken.Register(() =>
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill(entireProcessTree: true);
                    }
                }
                catch (InvalidOperationException) { }
                catch (Win32Exception) { }
            });
            Task<string> output = process.StandardOutput.ReadToEndAsync(cancellationToken);
            Task<string> error = process.StandardError.ReadToEndAsync(cancellationToken);
            await Task.WhenAll(output, error, process.WaitForExitAsync(cancellationToken));
            string token = await output;

            return process.ExitCode == 0 && !string.IsNullOrWhiteSpace(token) ? token.Trim() : null;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Win32Exception)
        {
            return null;
        }
        catch (FileNotFoundException)
        {
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
