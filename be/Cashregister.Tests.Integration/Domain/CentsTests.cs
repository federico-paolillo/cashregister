using Cashregister.Domain;

namespace Cashregister.Tests.Integration.Domain;

public sealed class CentsTests
{
    [Theory]
    [InlineData(0L)]
    [InlineData(1L)]
    [InlineData(2L)]
    [InlineData(999L)]
    public void From_ShouldPreserveExactCents(long value)
    {
        var cents = Cents.From(value);

        Assert.Equal(value, cents.Value);
    }

    [Fact]
    public void From_ShouldRejectNegativeValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Cents.From(-1L));
    }
}