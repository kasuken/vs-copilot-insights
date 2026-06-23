using Microsoft.VisualStudio.Extensibility.UI;

namespace vs_copilot_insights;

internal sealed class CopilotInsightsControl : RemoteUserControl
{
    private readonly CopilotInsightsViewModel _viewModel;

    public CopilotInsightsControl(CopilotInsightsViewModel viewModel)
        : base(dataContext: viewModel)
    {
        _viewModel = viewModel;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _viewModel.Dispose();
        }

        base.Dispose(disposing);
    }
}
