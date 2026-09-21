namespace TaskTracker.Domain.Common;

public static class Guard
{
    /// Validates a required text field: not blank, trimmed, within max length.
    public static string RequiredText(string? value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.FieldRequired, $"{fieldName} is required.");

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new DomainException(DomainErrors.FieldTooLong, $"{fieldName} cannot exceed {maxLength} characters.");

        return trimmed;
    }

    /// Validates an optional text field: blank becomes null, otherwise trimmed and within max length.
    public static string? OptionalText(string? value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new DomainException(DomainErrors.FieldTooLong, $"{fieldName} cannot exceed {maxLength} characters.");

        return trimmed;
    }
}