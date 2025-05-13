using System.Text.RegularExpressions;

namespace Slotty.Validation;

/// <summary>
/// Provides validation for slot names according to Slotty's naming conventions.
/// </summary>
public static class SlotNameValidator
{
    /// <summary>
    /// Regex pattern for valid slot names.
    /// Supports base names and optional :before or :after modifiers.
    /// </summary>
    private static readonly Regex SlotNameRegex = new(@"^[a-zA-Z][a-zA-Z0-9_-]*(?::(?:before|after))?$", RegexOptions.Compiled);

    /// <summary>
    /// Validates that a slot name conforms to Slotty's naming conventions.
    /// </summary>
    /// <param name="slotName">The slot name to validate.</param>
    /// <returns>True if the slot name is valid, false otherwise.</returns>
    public static bool IsValidSlotName(string slotName)
    {
        if (string.IsNullOrWhiteSpace(slotName))
            return false;

        return SlotNameRegex.IsMatch(slotName);
    }

    /// <summary>
    /// Gets the regex pattern used for slot name validation.
    /// This is exposed for testing purposes.
    /// </summary>
    public static Regex GetSlotNameRegex() => SlotNameRegex;
} 