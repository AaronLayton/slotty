namespace Slotty.Services;

/// <summary>
/// Manages slot extensions for before/after content and provides utilities for slot naming.
/// </summary>
public static class SlotExtensions
{
    private const string BeforeSuffix = ":before";
    private const string AfterSuffix = ":after";

    /// <summary>
    /// Gets the base slot name by removing any :before or :after suffixes.
    /// </summary>
    /// <param name="slotName">The full slot name.</param>
    /// <returns>The base slot name without extensions.</returns>
    /// <exception cref="ArgumentException">Thrown when slotName is null or whitespace.</exception>
    public static string GetBaseSlotName(string slotName)
    {
        if (string.IsNullOrWhiteSpace(slotName))
            throw new ArgumentException("Slot name cannot be null or whitespace.", nameof(slotName));

        if (slotName.EndsWith(BeforeSuffix))
            return slotName[..^BeforeSuffix.Length];
        
        if (slotName.EndsWith(AfterSuffix))
            return slotName[..^AfterSuffix.Length];
        
        return slotName;
    }

    /// <summary>
    /// Gets the before slot name for a given base slot name.
    /// </summary>
    /// <param name="baseSlotName">The base slot name.</param>
    /// <returns>The before slot name.</returns>
    public static string GetBeforeSlotName(string baseSlotName) => $"{baseSlotName}{BeforeSuffix}";

    /// <summary>
    /// Gets the after slot name for a given base slot name.
    /// </summary>
    /// <param name="baseSlotName">The base slot name.</param>
    /// <returns>The after slot name.</returns>
    public static string GetAfterSlotName(string baseSlotName) => $"{baseSlotName}{AfterSuffix}";

    /// <summary>
    /// Gets all content for a slot, including before and after extensions.
    /// </summary>
    /// <param name="slotContentStore">The slot content store service.</param>
    /// <param name="slotName">The slot name.</param>
    /// <returns>Combined content from before, main, and after slots.</returns>
    public static IEnumerable<string> GetExtendedContent(ISlotContentStore slotContentStore, string slotName)
    {
        var baseSlotName = GetBaseSlotName(slotName);
        var isExtension = slotName != baseSlotName;

        // If this is already an extension slot (e.g., sidebar:before),
        // just return its content directly
        if (isExtension)
            return slotContentStore.GetContent(slotName);

        // Otherwise, combine before, main, and after content
        var beforeContent = slotContentStore.GetContent(GetBeforeSlotName(baseSlotName));
        var mainContent = slotContentStore.GetContent(baseSlotName);
        var afterContent = slotContentStore.GetContent(GetAfterSlotName(baseSlotName));

        return beforeContent.Concat(mainContent).Concat(afterContent);
    }
} 