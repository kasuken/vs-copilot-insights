using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.ToolWindows;
using Microsoft.VisualStudio.RpcContracts.RemoteUI;
using vs_copilot_insights.Services;

namespace vs_copilot_insights;

[VisualStudioContribution]
internal sealed class CopilotInsightsToolWindow : ToolWindow
{
    private readonly VisualStudioExtensibility _extensibility;
    private readonly GitHubCopilotService _service;
    private readonly LocalStorageService _storage;
    private CopilotInsightsControl? _control;

    public CopilotInsightsToolWindow(VisualStudioExtensibility extensibility, GitHubCopilotService service, LocalStorageService storage)
        : base(extensibility)
    {
        Title = "Copilot Insights";
        _extensibility = extensibility;
        _service = service;
        _storage = storage;
    }

    public override ToolWindowConfiguration ToolWindowConfiguration => new()
    {
        Placement = ToolWindowPlacement.DocumentWell,
    };

    public override Task<IRemoteUserControl> GetContentAsync(CancellationToken cancellationToken)
    {
        _control ??= new CopilotInsightsControl(new CopilotInsightsViewModel(_service, _extensibility, _storage));
        return Task.FromResult<IRemoteUserControl>(_control);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _control?.Dispose();
            _control = null;
        }

        base.Dispose(disposing);
    }
}
