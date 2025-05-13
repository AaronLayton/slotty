using Slotty.Exceptions;
using Xunit;

namespace Slotty.Tests.Exceptions;

public class SlotNotFoundExceptionTests
{
    [Fact]
    public void Constructor_WithSlotName_SetsPropertiesCorrectly()
    {
        // Arrange
        var slotName = "testSlot";

        // Act
        var exception = new SlotNotFoundException(slotName);

        // Assert
        Assert.Equal(slotName, exception.SlotName);
        Assert.Contains(slotName, exception.Message);
        Assert.Contains("was not found", exception.Message);
    }

    [Fact]
    public void Constructor_WithSlotNameAndMessage_SetsPropertiesCorrectly()
    {
        // Arrange
        var slotName = "testSlot";
        var message = "Custom error message";

        // Act
        var exception = new SlotNotFoundException(slotName, message);

        // Assert
        Assert.Equal(slotName, exception.SlotName);
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void Constructor_WithSlotNameMessageAndInnerException_SetsPropertiesCorrectly()
    {
        // Arrange
        var slotName = "testSlot";
        var message = "Custom error message";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new SlotNotFoundException(slotName, message, innerException);

        // Assert
        Assert.Equal(slotName, exception.SlotName);
        Assert.Equal(message, exception.Message);
        Assert.Equal(innerException, exception.InnerException);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("header")]
    [InlineData("sidebar:before")]
    public void Constructor_WithVariousSlotNames_CreatesExpectedException(string slotName)
    {
        // Act
        var exception = new SlotNotFoundException(slotName);

        // Assert
        Assert.Equal(slotName, exception.SlotName);
        Assert.IsType<SlotNotFoundException>(exception);
    }
} 