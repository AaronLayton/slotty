using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Slotty.Configuration;
using Slotty.Exceptions;
using Slotty.Services;
using Xunit;

namespace Slotty.Tests.Services;

public class SlotContentStoreTests
{
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ILogger<SlotContentStore>> _mockLogger;
    private readonly Mock<IOptions<SlottyOptions>> _mockOptions;
    private readonly SlottyOptions _defaultOptions;
    private readonly DefaultHttpContext _httpContext;

    public SlotContentStoreTests()
    {
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockLogger = new Mock<ILogger<SlotContentStore>>();
        _mockOptions = new Mock<IOptions<SlottyOptions>>();
        _defaultOptions = new SlottyOptions();
        _httpContext = new DefaultHttpContext();

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_httpContext);
        _mockOptions.Setup(x => x.Value).Returns(_defaultOptions);
    }

    private SlotContentStore CreateStore(SlottyOptions? options = null)
    {
        if (options != null)
        {
            _mockOptions.Setup(x => x.Value).Returns(options);
        }
        return new SlotContentStore(_mockHttpContextAccessor.Object, _mockLogger.Object, _mockOptions.Object);
    }

    [Fact]
    public void Constructor_WithNullHttpContextAccessor_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new SlotContentStore(null!, _mockLogger.Object, _mockOptions.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new SlotContentStore(_mockHttpContextAccessor.Object, null!, _mockOptions.Object));
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new SlotContentStore(_mockHttpContextAccessor.Object, _mockLogger.Object, null!));
    }

    [Fact]
    public void GetSlotContent_WithNoHttpContext_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);
        var store = CreateStore();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => store.GetSlotContent());
        Assert.Contains("HttpContext is not available", exception.Message);
    }

    [Fact]
    public void GetSlotContent_WhenCalledFirstTime_ReturnsEmptyDictionary()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var result = store.GetSlotContent();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetSlotContent_WhenCalledMultipleTimes_ReturnsSameInstance()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var result1 = store.GetSlotContent();
        var result2 = store.GetSlotContent();

        // Assert
        Assert.Same(result1, result2);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddContent_WithInvalidSlotName_ThrowsArgumentException(string? slotName)
    {
        // Arrange
        var store = CreateStore();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => store.AddContent(slotName!, "content"));
    }

    [Fact]
    public void AddContent_WithNullContent_ThrowsArgumentNullException()
    {
        // Arrange
        var store = CreateStore();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => store.AddContent("testSlot", null!));
    }

    [Fact]
    public void AddContent_WithValidInput_AddsContentToSlot()
    {
        // Arrange
        var store = CreateStore();
        var slotName = "testSlot";
        var content = "test content";

        // Act
        store.AddContent(slotName, content);

        // Assert
        var slotContent = store.GetSlotContent();
        Assert.True(slotContent.ContainsKey(slotName));
        Assert.Single(slotContent[slotName]);
        Assert.Contains(content, slotContent[slotName]);
    }

    [Fact]
    public void AddContent_WithMultipleCallsToSameSlot_AppendsContent()
    {
        // Arrange
        var store = CreateStore();
        var slotName = "testSlot";
        var content1 = "first content";
        var content2 = "second content";

        // Act
        store.AddContent(slotName, content1);
        store.AddContent(slotName, content2);

        // Assert
        var slotContent = store.GetSlotContent();
        Assert.Equal(2, slotContent[slotName].Count);
        Assert.Contains(content1, slotContent[slotName]);
        Assert.Contains(content2, slotContent[slotName]);
    }

    [Fact]
    public void GetContent_WithValidSlotName_ReturnsCorrectContent()
    {
        // Arrange
        var store = CreateStore();
        var slotName = "testSlot";
        var content = "test content";
        store.AddContent(slotName, content);

        // Act
        var result = store.GetContent(slotName);

        // Assert
        Assert.Single(result);
        Assert.Contains(content, result);
    }

    [Fact]
    public void GetContent_WithNonExistentSlot_ReturnsEmptyList()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var result = store.GetContent("nonExistentSlot");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetContent_WithInvalidSlotName_ThrowsArgumentException(string? slotName)
    {
        // Arrange
        var store = CreateStore();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => store.GetContent(slotName!));
    }

    [Fact]
    public void RegisterSlotDefined_WithTrackingEnabled_RegistersSlot()
    {
        // Arrange
        var options = new SlottyOptions { ValidationMode = SlottyValidationMode.Log };
        var store = CreateStore(options);
        var slotName = "testSlot";

        // Act
        store.RegisterSlotDefined(slotName);

        // Assert
        var definedSlots = store.GetDefinedSlots();
        Assert.Contains(slotName, definedSlots);
    }

    [Fact]
    public void RegisterSlotDefined_WithTrackingDisabled_DoesNothing()
    {
        // Arrange
        var options = new SlottyOptions { ValidationMode = SlottyValidationMode.Silent };
        var store = CreateStore(options);
        var slotName = "testSlot";

        // Act
        store.RegisterSlotDefined(slotName);

        // Assert - Should not throw and should work normally
        var definedSlots = store.GetDefinedSlots();
        Assert.Empty(definedSlots); // Nothing should be tracked in Silent mode
    }

    [Fact]
    public void AddContent_WithTrackingEnabled_TracksFilledSlots()
    {
        // Arrange
        var options = new SlottyOptions { ValidationMode = SlottyValidationMode.Log };
        var store = CreateStore(options);
        var slotName = "testSlot";

        // Act
        store.AddContent(slotName, "content");

        // Assert
        var filledSlots = store.GetFilledSlots();
        Assert.Contains(slotName, filledSlots);
    }

    [Fact]
    public void ValidateSlotUsage_WithOrphanedFillsInThrowMode_ThrowsException()
    {
        // Arrange
        var options = new SlottyOptions { ValidationMode = SlottyValidationMode.Throw };
        var store = CreateStore(options);
        
        // Add a fill without defining the slot
        store.AddContent("orphanedSlot", "content");

        // Act & Assert
        Assert.Throws<SlotNotFoundException>(() => store.ValidateSlotUsage());
    }

    [Fact]
    public void ValidateSlotUsage_WithNoOrphanedFills_DoesNotThrow()
    {
        // Arrange
        var options = new SlottyOptions { ValidationMode = SlottyValidationMode.Throw };
        var store = CreateStore(options);
        
        // Define a slot and fill it
        store.RegisterSlotDefined("validSlot");
        store.AddContent("validSlot", "content");

        // Act & Assert - Should not throw
        store.ValidateSlotUsage();
    }

    [Fact]
    public void ValidateSlotUsage_InSilentMode_DoesNothing()
    {
        // Arrange
        var options = new SlottyOptions { ValidationMode = SlottyValidationMode.Silent };
        var store = CreateStore(options);
        
        // Add a fill without defining the slot
        store.AddContent("orphanedSlot", "content");

        // Act & Assert - Should not throw because tracking is disabled in Silent mode
        store.ValidateSlotUsage();
    }

    [Theory]
    [InlineData("1invalidName")]
    [InlineData("_invalidName")]
    [InlineData("-invalidName")]
    [InlineData("invalid name")]
    [InlineData("invalid@name")]
    [InlineData("header:middle")] // Only :before and :after are supported
    [InlineData("header:custom")] // Only :before and :after are supported
    [InlineData("header:")] // Can't end with colon
    public void AddContent_WithInvalidSlotName_ThrowsInvalidSlotNameException(string invalidSlotName)
    {
        // Arrange
        var store = CreateStore();

        // Act & Assert
        Assert.Throws<InvalidSlotNameException>(() => store.AddContent(invalidSlotName, "content"));
    }

    [Theory]
    [InlineData("validSlotName")]
    [InlineData("valid-slot-name")]
    [InlineData("valid_slot_name")]
    [InlineData("ValidSlotName")]
    [InlineData("a")]
    [InlineData("slot123")]
    [InlineData("header:before")] // Only :before is supported
    [InlineData("sidebar:after")] // Only :after is supported
    public void AddContent_WithValidSlotName_DoesNotThrow(string validSlotName)
    {
        // Arrange
        var store = CreateStore();

        // Act & Assert - Should not throw
        store.AddContent(validSlotName, "content");
    }
} 