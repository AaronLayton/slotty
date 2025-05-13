using Slotty.Validation;
using System.Text.RegularExpressions;
using Xunit;

namespace Slotty.Tests.Validation;

public class SlotNameValidatorTests
{
    [Theory]
    [InlineData("mySlot", true)]
    [InlineData("slot_with_underscores", true)]
    [InlineData("slot-with-hyphens", true)]
    [InlineData("SlotWithCamelCase", true)]
    [InlineData("slot123", true)]
    [InlineData("header:before", true)] // :before modifier
    [InlineData("sidebar:after", true)] // :after modifier
    [InlineData("content:before", true)] // :before modifier
    [InlineData("navigation:after", true)] // :after modifier
    [InlineData("123slot", false)] // Can't start with number
    [InlineData("_slot", false)] // Can't start with underscore
    [InlineData("-slot", false)] // Can't start with hyphen
    [InlineData("slot with spaces", false)] // No spaces
    [InlineData("slot@symbol", false)] // No special characters
    [InlineData("", false)] // Empty string
    [InlineData("   ", false)] // Whitespace only
    [InlineData("a", true)] // Single letter is valid
    [InlineData("header:", false)] // Can't end with colon
    [InlineData("header::", false)] // Double colon not allowed
    [InlineData("header:1after", false)] // Modifier can't start with number
    [InlineData("header:middle", false)] // Only :before and :after allowed
    [InlineData("header:custom", false)] // Only :before and :after allowed
    [InlineData("slot:before:after", false)] // Multiple modifiers not allowed
    public void IsValidSlotName_ShouldValidateCorrectly(string slotName, bool expected)
    {
        // Act
        var result = SlotNameValidator.IsValidSlotName(slotName);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsValidSlotName_WithNullInput_ReturnsFalse()
    {
        // Act
        var result = SlotNameValidator.IsValidSlotName(null!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetSlotNameRegex_ShouldReturnCompiledRegex()
    {
        // Act
        var regex = SlotNameValidator.GetSlotNameRegex();

        // Assert
        Assert.NotNull(regex);
        Assert.True(regex.Options.HasFlag(RegexOptions.Compiled));
    }

    [Theory]
    [InlineData("header")]
    [InlineData("header:before")]
    [InlineData("header:after")]
    public void GetSlotNameRegex_ShouldMatchValidNames(string validName)
    {
        // Arrange
        var regex = SlotNameValidator.GetSlotNameRegex();

        // Act
        var isMatch = regex.IsMatch(validName);

        // Assert
        Assert.True(isMatch);
    }

    [Theory]
    [InlineData("123invalid")]
    [InlineData("header:middle")]
    [InlineData("header:")]
    public void GetSlotNameRegex_ShouldNotMatchInvalidNames(string invalidName)
    {
        // Arrange
        var regex = SlotNameValidator.GetSlotNameRegex();

        // Act
        var isMatch = regex.IsMatch(invalidName);

        // Assert
        Assert.False(isMatch);
    }
} 