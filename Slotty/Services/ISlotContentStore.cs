namespace Slotty.Services;

/// <summary>
/// Interface for managing slot content within an HTTP request context.
/// </summary>
public interface ISlotContentStore
{
    /// <summary>
    /// Gets the slot content dictionary for the current request.
    /// </summary>
    /// <returns>A dictionary of slot content keyed by slot name.</returns>
    /// <exception cref="InvalidOperationException">Thrown when HttpContext is not available.</exception>
    Dictionary<string, List<string>> GetSlotContent();

    /// <summary>
    /// Adds content to a named slot for the current request.
    /// </summary>
    /// <param name="slotName">The name of the slot.</param>
    /// <param name="content">The content to add.</param>
    /// <exception cref="ArgumentException">Thrown when slotName is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when HttpContext is not available.</exception>
    /// <exception cref="Slotty.Exceptions.InvalidSlotNameException">Thrown when slot name is invalid.</exception>
    void AddContent(string slotName, string content);

    /// <summary>
    /// Gets all content for a named slot.
    /// </summary>
    /// <param name="slotName">The name of the slot.</param>
    /// <returns>A list of content strings for the slot.</returns>
    /// <exception cref="ArgumentException">Thrown when slotName is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when HttpContext is not available.</exception>
    List<string> GetContent(string slotName);

    /// <summary>
    /// Registers that a slot has been defined in the current request.
    /// This is used for tracking slot usage when validation is enabled.
    /// </summary>
    /// <param name="slotName">The name of the slot that was defined.</param>
    /// <exception cref="ArgumentException">Thrown when slotName is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when HttpContext is not available.</exception>
    void RegisterSlotDefined(string slotName);

    /// <summary>
    /// Gets the set of slot names that have been defined in the current request.
    /// </summary>
    /// <returns>A set of defined slot names.</returns>
    /// <exception cref="InvalidOperationException">Thrown when HttpContext is not available.</exception>
    HashSet<string> GetDefinedSlots();

    /// <summary>
    /// Gets the set of slot names that have been filled in the current request.
    /// </summary>
    /// <returns>A set of filled slot names.</returns>
    /// <exception cref="InvalidOperationException">Thrown when HttpContext is not available.</exception>
    HashSet<string> GetFilledSlots();

    /// <summary>
    /// Validates slot usage and handles validation according to the configured mode.
    /// This should be called at the end of request processing to check for orphaned fills.
    /// </summary>
    void ValidateSlotUsage();
}

/// <summary>
/// Interface for determining when Slotty development assets should be injected.
/// </summary>
public interface ISlottyAssetService
{
    /// <summary>
    /// Determines whether Slotty development assets should be injected based on configuration and environment.
    /// </summary>
    /// <returns>True if assets should be injected, false otherwise.</returns>
    bool ShouldInjectAssets();

    /// <summary>
    /// Gets the CSS content for Slotty development mode.
    /// </summary>
    /// <returns>The CSS content as a string.</returns>
    string GetCssContent();

    /// <summary>
    /// Gets the JavaScript content for Slotty development mode.
    /// </summary>
    /// <returns>The JavaScript content as a string.</returns>
    string GetJavaScriptContent();
} 