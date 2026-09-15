namespace PersonalExpenseManager.Application.Common;

public record ApiError(string Field, string Message);

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public IReadOnlyList<ApiError> Errors { get; init; } = [];

    public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };

    public static ApiResponse<T> Fail(IEnumerable<ApiError> errors) =>
        new() { Success = false, Errors = errors.ToList() };

    public static ApiResponse<T> Fail(string message, string field = "") =>
        Fail([new ApiError(field, message)]);
}
