namespace PersonalExpenseManager.Application.Common;

// A small, reusable outcome type for command methods that can fail in one of
// three ways a controller needs to distinguish: not found (404), a validation-ish
// failure like a duplicate name (422), or success with the resulting value.
public class MutationResult<T>
{
    public T? Value { get; }
    public string? Error { get; }
    public bool IsNotFound { get; }

    private MutationResult(T? value, string? error, bool isNotFound)
    {
        Value = value;
        Error = error;
        IsNotFound = isNotFound;
    }

    public static MutationResult<T> Ok(T value) => new(value, null, false);

    public static MutationResult<T> Fail(string error) => new(default, error, false);

    public static MutationResult<T> NotFound() => new(default, null, true);
}
