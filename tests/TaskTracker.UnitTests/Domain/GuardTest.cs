using TaskTracker.Domain.Common;

namespace TaskTracker.UnitTests.Domain;

public class GuardTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RequiredText_WithBlankValue_ThrowsFieldRequired(string? value)
    {
        var ex = Assert.Throws<DomainException>(() => Guard.RequiredText(value, 10, "Field"));

        Assert.Equal(DomainErrors.FieldRequired, ex.Code);
    }

    [Fact]
    public void RequiredText_TrimsValue()
    {
        var result = Guard.RequiredText("  abc  ", 10, "Field");

        Assert.Equal("abc", result);
    }

    [Fact]
    public void RequiredText_ExactlyAtMaxLength_ReturnsValue()
    {
        var result = Guard.RequiredText("abcde", 5, "Field");

        Assert.Equal("abcde", result);
    }

    [Fact]
    public void RequiredText_OverMaxLength_ThrowsFieldTooLong()
    {
        var ex = Assert.Throws<DomainException>(() => Guard.RequiredText("abcdef", 5, "Field"));

        Assert.Equal(DomainErrors.FieldTooLong, ex.Code);
    }

    [Fact]
    public void RequiredText_MeasuresLengthAfterTrimming()
    {
        var result = Guard.RequiredText("  abcde  ", 5, "Field");

        Assert.Equal("abcde", result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void OptionalText_WithBlankValue_ReturnsNull(string? value)
    {
        var result = Guard.OptionalText(value, 10, "Field");

        Assert.Null(result);
    }

    [Fact]
    public void OptionalText_TrimsValue()
    {
        var result = Guard.OptionalText("  abc  ", 10, "Field");

        Assert.Equal("abc", result);
    }

    [Fact]
    public void OptionalText_OverMaxLength_ThrowsFieldTooLong()
    {
        var ex = Assert.Throws<DomainException>(() => Guard.OptionalText("abcdef", 5, "Field"));

        Assert.Equal(DomainErrors.FieldTooLong, ex.Code);
    }
}