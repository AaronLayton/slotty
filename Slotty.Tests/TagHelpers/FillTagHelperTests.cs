using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Slotty.Configuration;
using Slotty.Services;
using Slotty.TagHelpers;
using Xunit;

namespace Slotty.Tests.TagHelpers;

public class FillTagHelperTests
{
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ILogger<SlotContentStore>> _mockLogger;
    private readonly Mock<IOptions<SlottyOptions>> _mockOptions;
    private readonly DefaultHttpContext _httpContext;
    private readonly SlotContentStore _slotContentStore;

    public FillTagHelperTests()
    {
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockLogger = new Mock<ILogger<SlotContentStore>>();
        _mockOptions = new Mock<IOptions<SlottyOptions>>();
        _httpContext = new DefaultHttpContext();
        
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_httpContext);
        _mockOptions.Setup(x => x.Value).Returns(new SlottyOptions());
        _slotContentStore = new SlotContentStore(_mockHttpContextAccessor.Object, _mockLogger.Object, _mockOptions.Object);
    }

    private (FillTagHelper helper, ISlotContentStore store) CreateFillTagHelper(HttpContext? httpContext = null)
    {
        httpContext ??= new DefaultHttpContext();
        
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        
        var logger = new Mock<ILogger<SlotContentStore>>();
        var options = new Mock<IOptions<SlottyOptions>>();
        options.Setup(x => x.Value).Returns(new SlottyOptions());
        
        var slotContentStore = new SlotContentStore(httpContextAccessor.Object, logger.Object, options.Object);
        var helper = new FillTagHelper(slotContentStore);
        
        return (helper, slotContentStore);
    }

    [Fact]
    public async Task ProcessAsync_StoresContentAndSuppressesOutput()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var slotName = "test-slot";
        var content = "<div>Test Content</div>";

        var context = new TagHelperContext(
            new TagHelperAttributeList { new TagHelperAttribute("name", slotName) },
            new Dictionary<object, object> { ["HttpContext"] = httpContext },
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput("fill",
            new TagHelperAttributeList(),
            (useCachedResult, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                tagHelperContent.SetHtmlContent(content);
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            });

        var (helper, store) = CreateFillTagHelper(httpContext);
        helper.Name = slotName;

        // Act
        await helper.ProcessAsync(context, output);

        // Assert
        Assert.True(output.IsContentModified);
        Assert.Empty(output.Content.GetContent());
        var storedContent = store.GetContent(slotName);
        Assert.Single(storedContent);
        Assert.Equal(content, storedContent.First());
    }

    [Fact]
    public async Task ProcessAsync_WithNullName_ThrowsArgumentException()
    {
        // Arrange
        var context = new TagHelperContext(
            new TagHelperAttributeList(),
            new Dictionary<object, object> { ["HttpContext"] = new DefaultHttpContext() },
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput("fill",
            new TagHelperAttributeList(),
            (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        var (helper, _) = CreateFillTagHelper();
        helper.Name = null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => helper.ProcessAsync(context, output));
    }

    [Fact]
    public async Task ProcessAsync_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var context = new TagHelperContext(
            new TagHelperAttributeList(),
            new Dictionary<object, object> { ["HttpContext"] = new DefaultHttpContext() },
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput("fill",
            new TagHelperAttributeList(),
            (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        var (helper, _) = CreateFillTagHelper();
        helper.Name = string.Empty;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => helper.ProcessAsync(context, output));
    }

    [Fact]
    public async Task ProcessAsync_WithNoHttpContext_ThrowsInvalidOperationException()
    {
        // Arrange
        var context = new TagHelperContext(
            new TagHelperAttributeList { new TagHelperAttribute("name", "test-slot") },
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput("fill",
            new TagHelperAttributeList(),
            (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        // Create a tag helper with null HttpContext to trigger the exception
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);
        
        var logger = new Mock<ILogger<SlotContentStore>>();
        var options = new Mock<IOptions<SlottyOptions>>();
        options.Setup(x => x.Value).Returns(new SlottyOptions());
        
        var slotContentStore = new SlotContentStore(httpContextAccessor.Object, logger.Object, options.Object);
        var helper = new FillTagHelper(slotContentStore);
        helper.Name = "test-slot";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => helper.ProcessAsync(context, output));
    }

    [Fact]
    public async Task ProcessAsync_MultipleFills_PreservesOrder()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var slotName = "test-slot";
        var content1 = "<div>First Content</div>";
        var content2 = "<div>Second Content</div>";

        var context1 = new TagHelperContext(
            new TagHelperAttributeList { new TagHelperAttribute("name", slotName) },
            new Dictionary<object, object> { ["HttpContext"] = httpContext },
            Guid.NewGuid().ToString("N"));

        var output1 = new TagHelperOutput("fill",
            new TagHelperAttributeList(),
            (useCachedResult, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                tagHelperContent.SetHtmlContent(content1);
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            });

        var context2 = new TagHelperContext(
            new TagHelperAttributeList { new TagHelperAttribute("name", slotName) },
            new Dictionary<object, object> { ["HttpContext"] = httpContext },
            Guid.NewGuid().ToString("N"));

        var output2 = new TagHelperOutput("fill",
            new TagHelperAttributeList(),
            (useCachedResult, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                tagHelperContent.SetHtmlContent(content2);
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            });

        var (helper1, store) = CreateFillTagHelper(httpContext);
        helper1.Name = slotName;
        var (helper2, _) = CreateFillTagHelper(httpContext);
        helper2.Name = slotName;

        // Act
        await helper1.ProcessAsync(context1, output1);
        await helper2.ProcessAsync(context2, output2);

        // Assert
        var storedContent = store.GetContent(slotName).ToList();
        Assert.Equal(2, storedContent.Count);
        Assert.Equal(content1, storedContent[0]);
        Assert.Equal(content2, storedContent[1]);
    }
} 