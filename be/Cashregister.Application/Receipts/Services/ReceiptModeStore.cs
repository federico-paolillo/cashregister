using Cashregister.Application.Receipts.Models;

namespace Cashregister.Application.Receipts.Services;

/// <summary>
///     Stores the current non-persistent receipt printing mode for this process.
/// </summary>
public sealed class ReceiptModeStore
{
    private readonly object _lock = new();

    private ReceiptMode _current = ReceiptMode.Normal;

    public ReceiptMode Current
    {
        get
        {
            lock (_lock)
            {
                return _current;
            }
        }
    }

    public void Select(ReceiptMode mode)
    {
        lock (_lock)
        {
            _current = mode;
        }
    }
}