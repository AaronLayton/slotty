using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Moq;
using Slotty.Configuration;
using Slotty.Services;
using Slotty.TagHelpers;
using Xunit;

namespace Slotty.Tests.TagHelpers;

public class SlotTagHelperTests
{
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ILogger<SlotContentStore>> _mockLogger;
    private readonly Mock<IOptions<SlottyOptions>> _mockOptions;
    private readonly DefaultHttpContext _httpContext;
    private readonly SlotContentStore _slotContentStore;

    public SlotTagHelperTests()
    {
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockLogger = new Mock<ILogger<SlotContentStore>>();
        _mockOptions = new Mock<IOptions<SlottyOptions>>();
        _httpContext = new DefaultHttpContext();
        
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_httpContext);
        _mockOptions.Setup(x => x.Value).Returns(new SlottyOptions());
        _slotContentStore = new SlotContentStore(_mockHttpContextAccessor.Object, _mockLogger.Object, _mockOptions.Object);
    }

    private (SlotTagHelper helper, ISlotContentStore store) CreateSlotTagHelper(HttpContext? httpContext = null)
    {
        httpContext ??= new DefaultHttpContext();
        
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        
        var logger = new Mock<ILogger<SlotContentStore>>();
        var options = new Mock<IOptions<SlottyOptions>>();
        options.Setup(x => x.Value).Returns(new SlottyOptions());
        
        var slotContentStore = new SlotContentStore(httpContextAccessor.Object, logger.Object, options.Object);
        var environment = new Mock<IHostEnvironment>();
        var viewEngine = new Mock<ICompositeViewEngine>();
        
        var helper = new SlotTagHelper(slotContentStore, environment.Object, viewEngine.Object);
        return (helper, slotContentStore);
    }

    [Fact]
    public async Task ProcessAsync_WithNoContent_RetainsFallbackContent()
    {
        // Arrange
        var context = new TagHelperContext(
            new TagHelperAttributeList { new TagHelperAttribute("name", "test-slot") },
            new Dictionary<object, object> { ["HttpContext"] = new DefaultHttpContext() },
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput("slot",
            new TagHelperAttributeList(),
            (useCachedResult, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                tagHelperContent.SetHtmlContent("<div>Fallback Content</div>");
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            });

        var (helper, _) = CreateSlotTagHelper();
        helper.Name = "test-slot";

        // Act
        await helper.ProcessAsync(context, output);

        // Assert
        Assert.Equal("<div>Fallback Content</div>", output.Content.GetContent());
    }

    [Fact]
    public async Task ProcessAsync_WithRegisteredContent_RendersContent()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var slotName = "test-slot";
        var content = "<div>Injected Content</div>";
        
        var (helper, store) = CreateSlotTagHelper(httpContext);
        store.AddContent(slotName, content);

        var context = new TagHelperContext(
            new TagHelperAttributeList { new TagHelperAttribute("name", slotName) },
            new Dictionary<object, object> { ["HttpContext"] = httpContext },
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput("slot",
            new TagHelperAttributeList(),
            (useCachedResult, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                tagHelperContent.SetHtmlContent("<div>Fallback Content</div>");
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            });

        helper.Name = slotName;

        // Act
        await helper.ProcessAsync(context, output);

        // Assert
        Assert.Equal(content, output.Content.GetContent());
        Assert.Null(output.TagName);
    }

    [Fact]
    public async Task ProcessAsync_WithMultipleContent_RendersConcatenatedContent()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var slotName = "test-slot";
        var content1 = "<div>First Content</div>";
        var content2 = "<div>Second Content</div>";
        
        var (helper, store) = CreateSlotTagHelper(httpContext);
        store.AddContent(slotName, content1);
        store.AddContent(slotName, content2);

        var context = new TagHelperContext(
            new TagHelperAttributeList { new TagHelperAttribute("name", slotName) },
            new Dictionary<object, object> { ["HttpContext"] = httpContext },
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput("slot",
            new TagHelperAttributeList(),
            (useCachedResult, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                tagHelperContent.SetHtmlContent("<div>Fallback Content</div>");
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            });

        helper.Name = slotName;

        // Act
        await helper.ProcessAsync(context, output);

        // Assert
        Assert.Equal(content1 + content2, output.Content.GetContent());
        Assert.Null(output.TagName);
    }

    [Fact]
    public async Task ProcessAsync_WithNullName_ThrowsArgumentException()
    {
        // Arrange
        var context = new TagHelperContext(
            new TagHelperAttributeList(),
            new Dictionary<object, object> { ["HttpContext"] = new DefaultHttpContext() },
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput("slot",
            new TagHelperAttributeList(),
            (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        var (helper, _) = CreateSlotTagHelper();
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

        var output = new TagHelperOutput("slot",
            new TagHelperAttributeList(),
            (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        var (helper, _) = CreateSlotTagHelper();
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

        var output = new TagHelperOutput("slot",
            new TagHelperAttributeList(),
            (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        // Create a tag helper with null HttpContext to trigger the exception
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);
        
        var logger = new Mock<ILogger<SlotContentStore>>();
        var options = new Mock<IOptions<SlottyOptions>>();
        options.Setup(x => x.Value).Returns(new SlottyOptions());
        
        var slotContentStore = new SlotContentStore(httpContextAccessor.Object, logger.Object, options.Object);
        var environment = new Mock<IHostEnvironment>();
        var viewEngine = new Mock<ICompositeViewEngine>();
        
        var helper = new SlotTagHelper(slotContentStore, environment.Object, viewEngine.Object);
        helper.Name = "test-slot";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => helper.ProcessAsync(context, output));
    }
} 