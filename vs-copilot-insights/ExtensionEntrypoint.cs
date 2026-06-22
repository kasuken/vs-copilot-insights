using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;
using vs_copilot_insights.Services;

namespace vs_copilot_insights;

/// <summary>
/// Extension entrypoint for the VisualStudio.Extensibility extension.
/// </summary>
[VisualStudioContribution]
internal class ExtensionEntrypoint : Extension
{
    /// <inheritdoc/>
    public override ExtensionConfiguration ExtensionConfiguration => new()
    {
        Metadata = new(
                id: "vs_copilot_insights.032230a6-8fe0-4fa1-8bfa-be9ad67b7f61",
                version: this.ExtensionAssemblyVersion,
                publisherName: "Emanuele Bartolesi",
                displayName: "Copilot Insights",
                description: "See your GitHub Copilot plan, quotas, reset window, and AI credit usage trends directly inside Visual Studio."),
    };

    /// <inheritdoc />
    protected override void InitializeServices(IServiceCollection serviceCollection)
    {
        base.InitializeServices(serviceCollection);

        serviceCollection.AddSingleton<GitHubCopilotService>();
    }
}
