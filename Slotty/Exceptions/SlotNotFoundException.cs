namespace Slotty.Exceptions;

/// <summary>
/// Exception thrown when a fill operation references a slot that doesn't exist.
/// This exception is only thrown when <see cref="Configuration.SlottyOptions.ValidationMode"/> is set to <see cref="Configuration.SlottyValidationMode.Throw"/>.
/// </summary>
public class SlotNotFoundException : Exception
{
    /// <summary>
    /// Gets the name of the slot that was not found.
    /// </summary>
    public string SlotName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SlotNotFoundException"/> class.
    /// </summary>
    /// <param name="slotName">The name of the slot that was not found.</param>
    public SlotNotFoundException(string slotName) 
        : base($"Slot '{slotName}' was not found. Ensure a corresponding <slot name=\"{slotName}\"> tag exists.")
    {
        SlotName = slotName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SlotNotFoundException"/> class.
    /// </summary>
    /// <param name="slotName">The name of the slot that was not found.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public SlotNotFoundException(string slotName, string message) : base(message)
    {
        SlotName = slotName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SlotNotFoundException"/> class.
    /// </summary>
    /// <param name="slotName">The name of the slot that was not found.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public SlotNotFoundException(string slotName, string message, Exception innerException) : base(message, innerException)
    {
        SlotName = slotName;
    }
} 