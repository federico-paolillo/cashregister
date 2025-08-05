using Xunit.Abstractions;

namespace Cashregister.Tests.Integration.Articles;

public sealed class ListArticlesByPageQueryTests(ITestOutputHelper testOutputHelper)
    : IntegrationTest(testOutputHelper)
{

    [Fact]
    public async Task ListArticlesByPage_ReturnsProperPage()
    {
        // TODO: Implement test
        await Task.CompletedTask;
    }
}