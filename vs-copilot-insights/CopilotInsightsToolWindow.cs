using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.ToolWindows;
using Microsoft.VisualStudio.RpcContracts.RemoteUI;
using vs_copilot_insights.Services;

namespace vs_copilot_insights;

[VisualStudioContribution]
internal sealed class CopilotInsightsToolWindow : ToolWindow
{
    private readonly GitHubCopilotService _service;

    public CopilotInsightsToolWindow(VisualStudioExtensibility extensibility, GitHubCopilotService service)
        : base(extensibility)
    {
        Title = "Copilot Insights";
        _service = service;
    }

    public override ToolWindowConfiguration ToolWindowConfiguration => new()
    {
        Placement = ToolWindowPlacement.DocumentWell,
    };

    public override Task<IRemoteUserControl> GetContentAsync(CancellationToken cancellationToken)
    {
        var viewModel = new CopilotInsightsViewModel(_service);
        return Task.FromResult<IRemoteUserControl>(new CopilotInsightsControl(viewModel));
    }
}
