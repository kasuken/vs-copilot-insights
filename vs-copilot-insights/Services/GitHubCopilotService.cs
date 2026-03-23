using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using vs_copilot_insights.Models;

namespace vs_copilot_insights.Services;

internal sealed class GitHubCopilotService
{
    private static readonly HttpClient s_httpClient = new()
    {
        DefaultRequestHeaders =
        {
            { "User-Agent", "VS-Copilot-Insights" },
        },
    };

    public async Task<CopilotUserData> GetCopilotUserDataAsync(CancellationToken cancellationToken)
    {
        string token = await GetGitHubTokenAsync(cancellationToken)
            ?? throw new InvalidOperationException(
                "Could not obtain a GitHub token. Please ensure the GitHub CLI is installed and you are signed in (run 'gh auth login').");

        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/copilot_internal/user");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await s_httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"GitHub API returned {(int)response.StatusCode} {response.ReasonPhrase}. {body}");
        }

        string json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<CopilotUserData>(json)
            ?? throw new InvalidOperationException("Failed to deserialize Copilot user data.");
    }

    private static async Task<string?> GetGitHubTokenAsync(CancellationToken cancellationToken)
    {
        try
        {
            var psi = new ProcessStartInfo("gh", "auth token")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(psi);
            if (process is null)
            {
                return null;
            }

            string token = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            return process.ExitCode == 0 ? token.Trim() : null;
        }
        catch
        {
            return null;
        }
    }
}
