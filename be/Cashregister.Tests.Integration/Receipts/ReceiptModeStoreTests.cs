using Cashregister.Application.Receipts.Models;
using Cashregister.Application.Receipts.Services;

namespace Cashregister.Tests.Integration.Receipts;

public sealed class ReceiptModeStoreTests
{
    [Fact]
    public void Current_DefaultsToNormal()
    {
        var store = new ReceiptModeStore();

        Assert.Equal(ReceiptMode.Normal, store.Current);
    }

    [Fact]
    public void Select_ChangesCurrentMode()
    {
        var store = new ReceiptModeStore();

        store.Select(ReceiptMode.Detail);

        Assert.Equal(ReceiptMode.Detail, store.Current);
    }
}