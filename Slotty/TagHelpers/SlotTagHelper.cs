using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Html;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Slotty.Services;

namespace Slotty.TagHelpers;

/// <summary>
/// TagHelper that renders content registered for a named slot.
/// If no content is registered, renders the fallback content provided within the slot tag.
/// </summary>
/// <example>
/// Basic usage:
/// <code>
/// <slot name="header" />
/// </code>
/// 
/// With fallback content:
/// <code>
/// <slot name="sidebar">
///     <div>Default sidebar content</div>
/// </slot>
/// </code>
/// </example>
[HtmlTargetElement("slot")]
public class SlotTagHelper : TagHelper
{
    private readonly ISlotContentStore _slotContentStore;
    private readonly IHostEnvironment _environment;
    private readonly ICompositeViewEngine _viewEngine;

    // Slot names that are typically used in <head> context
    private static readonly HashSet<string> HeadSlotPatterns = new(StringComparer.OrdinalIgnoreCase)
    {
        "head", "meta", "title", "style", "styles", "css", "link", "links", 
        "viewport", "description", "keywords", "author", "robots", "canonical"
    };

    /// <summary>
    /// Gets or sets the ViewContext for rendering partial views.
    /// </summary>
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = default!;

    /// <summary>
    /// Initializes a new instance of the <see cref="SlotTagHelper"/> class.
    /// </summary>
    /// <param name="slotContentStore">The slot content store service.</param>
    /// <param name="environment">The hosting environment.</param>
    /// <param name="viewEngine">The view engine.</param>
    public SlotTagHelper(
        ISlotContentStore slotContentStore, 
        IHostEnvironment environment, 
        ICompositeViewEngine viewEngine)
    {
        _slotContentStore = slotContentStore ?? throw new ArgumentNullException(nameof(slotContentStore));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _viewEngine = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));
    }

    /// <summary>
    /// Gets or sets the name of the slot to render.
    /// </summary>
    [HtmlAttributeName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Processes the slot tag helper to render the appropriate content.
    /// </summary>
    /// <param name="context">The tag helper context.</param>
    /// <param name="output">The tag helper output.</param>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Slot name is required. Use the 'name' attribute to specify a slot name.", nameof(Name));

        // Normalize slot name by trimming whitespace
        var slotName = Name.Trim();
        
        // Register this slot as defined
        _slotContentStore.RegisterSlotDefined(slotName);

        // Check if this is an extension slot (contains colon)
        if (slotName.Contains(':'))
        {
            await ProcessExtensionSlotAsync(context, output, slotName);
            return;
        }

        // Handle base slot with extensions
        await ProcessBaseSlotAsync(context, output, slotName);
    }

    private async Task ProcessExtensionSlotAsync(TagHelperContext context, TagHelperOutput output, string slotName)
    {
        var content = _slotContentStore.GetContent(slotName);
        var contentHtml = string.Join("", content);

        var fallbackContent = await output.GetChildContentAsync();
        var hasFallbackContent = !string.IsNullOrWhiteSpace(fallbackContent.GetContent());
        
        // If no content, use fallback
        if (!content.Any() && hasFallbackContent)
            contentHtml = fallbackContent.GetContent();

        output.TagName = null;

        if (_environment.IsDevelopment())
        {
            var hasContent = !string.IsNullOrWhiteSpace(contentHtml) || hasFallbackContent;
            var wrappedContent = IsHeadSlot(slotName) 
                ? RenderHeadDevModeWrapper(slotName, contentHtml, hasContent)
                : await RenderBodyDevModeWrapperAsync(slotName, contentHtml, hasContent);
            
            output.Content.SetHtmlContent(wrappedContent);
            return;
        }

        output.Content.SetHtmlContent(contentHtml);
    }

    private async Task ProcessBaseSlotAsync(TagHelperContext context, TagHelperOutput output, string baseSlotName)
    {
        // Register extension slots as defined too (for base slots)
        var beforeSlotName = SlotExtensions.GetBeforeSlotName(baseSlotName);
        var afterSlotName = SlotExtensions.GetAfterSlotName(baseSlotName);
        _slotContentStore.RegisterSlotDefined(beforeSlotName);
        _slotContentStore.RegisterSlotDefined(afterSlotName);

        // Handle base slot with extensions
        var beforeContent = _slotContentStore.GetContent(beforeSlotName);
        var mainContent = _slotContentStore.GetContent(baseSlotName);
        var afterContent = _slotContentStore.GetContent(afterSlotName);

        var beforeHtml = string.Join("", beforeContent);
        var mainHtml = string.Join("", mainContent);
        var afterHtml = string.Join("", afterContent);

        var fallbackContent = await output.GetChildContentAsync();
        var hasFallbackContent = !string.IsNullOrWhiteSpace(fallbackContent.GetContent());
        
        // If no main content, use fallback
        if (!mainContent.Any() && hasFallbackContent)
            mainHtml = fallbackContent.GetContent();

        output.TagName = null;

        if (_environment.IsDevelopment())
        {
            var hasBeforeContent = !string.IsNullOrWhiteSpace(beforeHtml);
            var hasMainContent = !string.IsNullOrWhiteSpace(mainHtml);
            var hasAfterContent = !string.IsNullOrWhiteSpace(afterHtml);

            var isHeadContext = IsHeadSlot(baseSlotName);
            
            var beforeWrapped = hasBeforeContent 
                ? (isHeadContext 
                    ? RenderHeadDevModeWrapper($"{baseSlotName}:before", beforeHtml, true)
                    : await RenderBodyDevModeWrapperAsync($"{baseSlotName}:before", beforeHtml, true))
                : "";
                
            var mainWrapped = isHeadContext
                ? RenderHeadDevModeWrapper(baseSlotName, mainHtml, hasMainContent || hasFallbackContent)
                : await RenderBodyDevModeWrapperAsync(baseSlotName, mainHtml, hasMainContent || hasFallbackContent);
                
            var afterWrapped = hasAfterContent
                ? (isHeadContext
                    ? RenderHeadDevModeWrapper($"{baseSlotName}:after", afterHtml, true)
                    : await RenderBodyDevModeWrapperAsync($"{baseSlotName}:after", afterHtml, true))
                : "";

            output.Content.SetHtmlContent($"{beforeWrapped}{mainWrapped}{afterWrapped}");
            return;
        }

        output.Content.SetHtmlContent($"{beforeHtml}{mainHtml}{afterHtml}");
    }

    /// <summary>
    /// Determines if a slot is likely to be used in a head context based on naming patterns.
    /// </summary>
    /// <param name="slotName">The slot name to check.</param>
    /// <returns>True if the slot is likely used in head context.</returns>
    private static bool IsHeadSlot(string slotName)
    {
        // Remove extension part (e.g., "head:before" -> "head")
        var baseName = slotName.Split(':')[0];
        
        // Check against known head slot patterns
        return HeadSlotPatterns.Contains(baseName);
    }

    /// <summary>
    /// Renders a head-safe dev mode wrapper using HTML comments.
    /// </summary>
    /// <param name="slotName">The name of the slot.</param>
    /// <param name="content">The slot content.</param>
    /// <param name="hasContent">Whether the slot has content.</param>
    /// <returns>The wrapped content with HTML comments.</returns>
    private static string RenderHeadDevModeWrapper(string slotName, string content, bool hasContent)
    {
        var status = hasContent ? "HAS CONTENT" : "EMPTY";
        var startComment = $"<!-- SLOTTY: {slotName} ({status}) -->";
        var endComment = $"<!-- /SLOTTY: {slotName} -->";
        
        if (hasContent && !string.IsNullOrWhiteSpace(content))
        {
            return $"{startComment}\n{content}\n{endComment}";
        }
        
        return startComment;
    }

    /// <summary>
    /// Renders the body dev mode wrapper using the existing partial view system.
    /// </summary>
    /// <param name="slotName">The name of the slot.</param>
    /// <param name="content">The slot content.</param>
    /// <param name="hasContent">Whether the slot has content.</param>
    /// <returns>The wrapped content using div elements.</returns>
    private async Task<string> RenderBodyDevModeWrapperAsync(string slotName, string content, bool hasContent)
    {
        var result = _viewEngine.FindView(ViewContext, "_SlotDevModeWrapper", false);
        if (!result.Success)
            throw new InvalidOperationException("Could not find _SlotDevModeWrapper view. This view should be embedded in the library.");

        using var writer = new StringWriter();
        var viewData = new ViewDataDictionary<(string Name, string Content, bool HasContent)>(
            ViewContext.ViewData,
            (slotName, content, hasContent)
        );
        
        var viewContext = new ViewContext(
            ViewContext,
            result.View,
            viewData,
            writer
        );
        
        await result.View.RenderAsync(viewContext);
        return writer.ToString();
    }
} 