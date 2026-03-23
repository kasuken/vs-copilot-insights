using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;

namespace vs_copilot_insights;

/// <summary>
/// Command to open the Copilot Insights tool window.
/// </summary>
[VisualStudioContribution]
internal sealed class ShowCopilotInsightsCommand : Command
{
    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new("%vs_copilot_insights.ShowCopilotInsightsCommand.DisplayName%")
    {
        Icon = new(ImageMoniker.KnownValues.StatusInformation, IconSettings.IconAndText),
        Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
    };

    /// <inheritdoc />
    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        await this.Extensibility.Shell().ShowToolWindowAsync<CopilotInsightsToolWindow>(activate: true, cancellationToken);
    }
}
