using Microsoft.AspNetCore.Razor.TagHelpers;
using Slotty.Services;

namespace Slotty.TagHelpers;

/// <summary>
/// TagHelper that registers content to be rendered in a named slot.
/// The content is stored in the current request's HttpContext.Items collection.
/// </summary>
/// <example>
/// Basic usage:
/// <code>
/// <fill name="header">
///     <h1>My Site Header</h1>
/// </fill>
/// </code>
/// 
/// Multiple fills for the same slot:
/// <code>
/// <fill name="sidebar">
///     <div>First sidebar widget</div>
/// </fill>
/// <fill name="sidebar">
///     <div>Second sidebar widget</div>
/// </fill>
/// </code>
/// </example>
[HtmlTargetElement("fill")]
public class FillTagHelper : TagHelper
{
    private readonly ISlotContentStore _slotContentStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="FillTagHelper"/> class.
    /// </summary>
    /// <param name="slotContentStore">The slot content store service.</param>
    public FillTagHelper(ISlotContentStore slotContentStore)
    {
        _slotContentStore = slotContentStore;
    }

    /// <summary>
    /// Gets or sets the name of the slot to fill with content.
    /// </summary>
    [HtmlAttributeName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Asynchronously executes the TagHelper.
    /// </summary>
    /// <param name="context">Contains information associated with the current HTML tag.</param>
    /// <param name="output">A stateful HTML element used to generate an HTML tag.</param>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // Get the content
        var childContent = await output.GetChildContentAsync();
        var content = childContent.GetContent();

        // Store the content using the injected service
        // The service will handle validation of Name and content parameters
        _slotContentStore.AddContent(Name, content);

        // Suppress output of the fill tag
        output.SuppressOutput();
    }
}