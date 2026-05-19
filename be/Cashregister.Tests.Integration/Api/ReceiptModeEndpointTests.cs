using System.Net;

using Cashregister.Api.ReceiptModes.Models;

using Xunit.Abstractions;

namespace Cashregister.Tests.Integration.Api;

public sealed class ReceiptModeEndpointTests(
    ITestOutputHelper testOutputHelper
) : IntegrationTest(testOutputHelper)
{
    [Fact]
    public async Task GetReceiptMode_ReturnsNormalByDefault()
    {
        await PrepareEnvironmentAsync();

        using var httpClient = CreateHttpClient();

        var response = await httpClient.GetAsync("/receipt-mode");
        var receiptMode = await response.Content.ReadFromJsonAsync<ReceiptModeDto>();

        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(receiptMode);
        Assert.Equal("normal", receiptMode.Mode);
    }

    [Theory]
    [InlineData("detail")]
    [InlineData("normal")]
    public async Task SelectReceiptMode_ChangesReceiptMode(string mode)
    {
        await PrepareEnvironmentAsync();

        using var httpClient = CreateHttpClient();

        var selectResponse = await httpClient.PostAsync($"/receipt-mode/{mode}", null);
        var getResponse = await httpClient.GetAsync("/receipt-mode");
        var receiptMode = await getResponse.Content.ReadFromJsonAsync<ReceiptModeDto>();

        Assert.Equal(HttpStatusCode.NoContent, selectResponse.StatusCode);
        Assert.True(getResponse.IsSuccessStatusCode);
        Assert.NotNull(receiptMode);
        Assert.Equal(mode, receiptMode.Mode);
    }

    [Fact]
    public async Task SelectReceiptMode_ReturnsBadRequest_WhenModeIsInvalid()
    {
        await PrepareEnvironmentAsync();

        using var httpClient = CreateHttpClient();

        var response = await httpClient.PostAsync("/receipt-mode/verbose", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}