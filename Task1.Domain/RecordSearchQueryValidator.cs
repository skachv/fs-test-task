namespace Task1.Domain;

internal static class RecordSearchQueryValidator
{
    public static void ValidateGreaterThan(this List<string> errors, int? parameter, int greaterThan, string parameterName)
    {
        if (parameter.HasValue && parameter.Value < 0)
        {
            errors.Add($"{parameterName} should not be less than {greaterThan}");
        }
    }

    public static void ValidateBoundaries(this List<string> errors, int? lower, int? upper, string nameLower, string nameUpper)
    {
        errors.ValidateGreaterThan(lower, 0, nameLower);
        errors.ValidateGreaterThan(upper, 0, nameUpper);

        if (lower.HasValue
            && upper.HasValue
            && upper < lower)
        {
            errors.Add($"{nameUpper} should be greater or equal than {nameLower}");
        }
    }

    public static void ValidateArrayNotEmpty(this List<string> errors, int[]? array, string arrayName)
    {
        if (array != null && array.Length == 0)
        {
            errors.Add($"{arrayName} should not be empty array");
        }
    }
}
