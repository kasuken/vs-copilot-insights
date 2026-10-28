namespace vs_copilot_insights.Services;

internal sealed record GitHubHostConfiguration(string Hostname, string ApiBaseUrl, string EnterpriseUri)
{
    public static GitHubHostConfiguration Parse(string? enterpriseUri)
    {
        if (string.IsNullOrWhiteSpace(enterpriseUri))
        {
            return new("github.com", "https://api.github.com", string.Empty);
        }

        if (!Uri.TryCreate(enterpriseUri.Trim(), UriKind.Absolute, out Uri? uri)
            || uri.Scheme != Uri.UriSchemeHttps
            || string.IsNullOrEmpty(uri.Host)
            || !string.IsNullOrEmpty(uri.UserInfo)
            || uri.AbsolutePath != "/"
            || !string.IsNullOrEmpty(uri.Query)
            || !string.IsNullOrEmpty(uri.Fragment))
        {
            throw new ArgumentException(
                "GitHub Enterprise URI must be an HTTPS instance URL without credentials, a path, query, or fragment (for example https://tenant.ghe.com).");
        }

        string host = uri.IdnHost.ToLowerInvariant();
        if (uri.IsDefaultPort && (host is "github.com" or "www.github.com" or "api.github.com"))
        {
            return new("github.com", "https://api.github.com", string.Empty);
        }

        if (host.StartsWith("api.", StringComparison.Ordinal) && host.EndsWith(".ghe.com", StringComparison.Ordinal))
        {
            throw new ArgumentException("Enter the GitHub Enterprise instance URL, not its api. URL.");
        }

        string authority = uri.Authority.ToLowerInvariant();
        string apiBaseUrl = uri.IsDefaultPort && host.EndsWith(".ghe.com", StringComparison.Ordinal)
            ? $"https://api.{host}"
            : $"https://{authority}/api/v3";
        return new(authority, apiBaseUrl, $"https://{authority}");
    }
}
