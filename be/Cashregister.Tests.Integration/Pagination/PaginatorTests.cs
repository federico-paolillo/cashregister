using System.Collections.Immutable;

using Cashregister.Application.Pagination;
using Cashregister.Domain;

namespace Cashregister.Tests.Integration.Pagination;

public sealed class PaginatorTests
{
    [Fact]
    public async Task FetchPageAsync_WithNoAfter_ShouldReturnFirstPage()
    {
        var query = new FakePaginationQuery(
        [
            MakeItem("01"),
            MakeItem("02"),
            MakeItem("03")
        ]);

        var result = await Paginator.FetchPageAsync(query, new PageRequest
        {
            After = null,
            Size = 2
        });

        Assert.True(result.Ok);
        var page = result.Value;

        Assert.Equal(2, page.Size);
        Assert.True(page.HasNext);
        Assert.NotNull(page.Next);
        Assert.Equal("02", page.Next.Value);
    }

    [Fact]
    public async Task FetchPageAsync_WithAfter_ShouldReturnNextPage()
    {
        var query = new FakePaginationQuery(
        [
            MakeItem("01"),
            MakeItem("02"),
            MakeItem("03"),
            MakeItem("04")
        ]);

        var result = await Paginator.FetchPageAsync(query, new PageRequest
        {
            After = Identifier.From("02"),
            Size = 2
        });

        Assert.True(result.Ok);
        var page = result.Value;

        Assert.Equal(2, page.Size);
        Assert.False(page.HasNext);
        Assert.Null(page.Next);
        Assert.Equal("03", page.Items[0].Id.Value);
        Assert.Equal("04", page.Items[1].Id.Value);
    }

    [Fact]
    public async Task FetchPageAsync_WithSizeLargerThanAvailable_ShouldReturnAll()
    {
        var query = new FakePaginationQuery(
        [
            MakeItem("01"),
            MakeItem("02")
        ]);

        var result = await Paginator.FetchPageAsync(query, new PageRequest
        {
            After = null,
            Size = 10
        });

        Assert.True(result.Ok);
        var page = result.Value;

        Assert.Equal(2, page.Size);
        Assert.False(page.HasNext);
        Assert.Null(page.Next);
    }

    [Fact]
    public async Task FetchPageAsync_WithZeroSize_ShouldReturnEmptyPage()
    {
        var query = new FakePaginationQuery(
        [
            MakeItem("01")
        ]);

        var result = await Paginator.FetchPageAsync(query, new PageRequest
        {
            After = null,
            Size = 0
        });

        Assert.True(result.Ok);
        var page = result.Value;

        Assert.Equal(0, page.Size);
        Assert.True(page.HasNext);
        Assert.Null(page.Next);
    }

    [Fact]
    public async Task FetchPageAsync_WithNoItems_ShouldReturnEmptyPage()
    {
        var query = new FakePaginationQuery([]);

        var result = await Paginator.FetchPageAsync(query, new PageRequest
        {
            After = null,
            Size = 5
        });

        Assert.True(result.Ok);
        var page = result.Value;

        Assert.Equal(0, page.Size);
        Assert.False(page.HasNext);
        Assert.Null(page.Next);
    }

    [Fact]
    public async Task FetchPageAsync_WithNullPageRequest_ShouldThrowArgumentNullException()
    {
        var query = new FakePaginationQuery([]);

        await Assert.ThrowsAsync<ArgumentNullException>(() => Paginator.FetchPageAsync(query, null!)
        );
    }

    [Fact]
    public async Task FetchPageAsync_WithNullQuery_ShouldThrowArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => Paginator.FetchPageAsync<FakePageItem>(null!,
            new PageRequest
            {
                After = null,
                Size = 5
            })
        );
    }

    [Fact]
    public async Task FetchPageAsync_WithUntil_ShouldReturnAccumulatedViewPlusNextPage()
    {
        var query = new FakePaginationQuery(
        [
            MakeItem("01"),
            MakeItem("02"),
            MakeItem("03")
        ]);

        var result = await Paginator.FetchPageAsync(query, new PageRequest
        {
            After = null,
            Until = Identifier.From("01"),
            Size = 1
        });

        Assert.True(result.Ok);
        var page = result.Value;

        Assert.Equal(2, page.Size);
        Assert.Equal("01", page.Items[0].Id.Value);
        Assert.Equal("02", page.Items[1].Id.Value);
    }

    [Fact]
    public async Task FetchPageAsync_WithUntil_ShouldSetNextCursorToLastNewItem()
    {
        var query = new FakePaginationQuery(
        [
            MakeItem("01"),
            MakeItem("02"),
            MakeItem("03"),
            MakeItem("04")
        ]);

        var result = await Paginator.FetchPageAsync(query, new PageRequest
        {
            After = null,
            Until = Identifier.From("01"),
            Size = 1
        });

        Assert.True(result.Ok);
        var page = result.Value;

        Assert.True(page.HasNext);
        Assert.NotNull(page.Next);
        Assert.Equal("02", page.Next.Value);
    }

    [Fact]
    public async Task FetchPageAsync_WithUntil_ShouldReportHasNextFalse_WhenNoMoreItems()
    {
        var query = new FakePaginationQuery(
        [
            MakeItem("01"),
            MakeItem("02")
        ]);

        var result = await Paginator.FetchPageAsync(query, new PageRequest
        {
            After = null,
            Until = Identifier.From("02"),
            Size = 10
        });

        Assert.True(result.Ok);
        var page = result.Value;

        Assert.False(page.HasNext);
        Assert.Null(page.Next);
    }

    private static FakePageItem MakeItem(string id)
    {
        return new FakePageItem
        {
            Id = Identifier.From(id)
        };
    }

    private sealed class FakePageItem : IPageItem
    {
        public required Identifier Id { get; init; }
    }

    private sealed class FakePaginationQuery(
        ImmutableArray<FakePageItem> items
    ) : IPaginationQuery<FakePageItem>
    {
        public Task<ImmutableArray<FakePageItem>> FetchAsync(uint count, Identifier? after = null)
        {
            var integerCount = (int)count;
            var afterValue = after?.Value;

            var result = items
                .Where(i => afterValue is null || string.Compare(i.Id.Value, afterValue, StringComparison.Ordinal) > 0)
                .OrderBy(i => i.Id.Value)
                .Take(integerCount)
                .ToImmutableArray();

            return Task.FromResult(result);
        }

        public Task<ImmutableArray<FakePageItem>> FetchUntilAsync(Identifier until)
        {
            var untilValue = until.Value;

            var result = items
                .Where(i => string.Compare(i.Id.Value, untilValue, StringComparison.Ordinal) <= 0)
                .OrderBy(i => i.Id.Value)
                .ToImmutableArray();

            return Task.FromResult(result);
        }
    }
}