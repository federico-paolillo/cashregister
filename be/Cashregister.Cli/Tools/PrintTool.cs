namespace Cashregister.Printmon.Tools;

public sealed class PrintTool
{
    public Task<int> ExecuteAsync(
        string orderId,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        // TODO

        return Task.FromResult(0);
    }
}
