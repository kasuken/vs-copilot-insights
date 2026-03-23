using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.UI;

namespace vs_copilot_insights;

internal sealed class AsyncCommand(Func<object?, CancellationToken, Task> executeAsync) : IAsyncCommand
{
    public bool CanExecute => true;

    public Task ExecuteAsync(object? parameter, IClientContext clientContext, CancellationToken cancellationToken)
    {
        return executeAsync(parameter, cancellationToken);
    }
}
