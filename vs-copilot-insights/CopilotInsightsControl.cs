using Microsoft.VisualStudio.Extensibility.UI;

namespace vs_copilot_insights;

internal sealed class CopilotInsightsControl : RemoteUserControl
{
    public CopilotInsightsControl(CopilotInsightsViewModel viewModel)
        : base(dataContext: viewModel)
    {
    }
}
