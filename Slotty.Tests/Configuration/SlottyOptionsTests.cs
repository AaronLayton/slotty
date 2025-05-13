using Slotty.Configuration;
using Xunit;

namespace Slotty.Tests.Configuration;

public class SlottyOptionsTests
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var options = new SlottyOptions();

        // Assert
        Assert.Equal(SlottyValidationMode.Silent, options.ValidationMode);
    }

    [Theory]
    [InlineData(SlottyValidationMode.Silent)]
    [InlineData(SlottyValidationMode.Log)]
    [InlineData(SlottyValidationMode.Throw)]
    public void ValidationMode_ShouldAcceptAllValidValues(SlottyValidationMode mode)
    {
        // Arrange
        var options = new SlottyOptions();

        // Act
        options.ValidationMode = mode;

        // Assert
        Assert.Equal(mode, options.ValidationMode);
    }
} 