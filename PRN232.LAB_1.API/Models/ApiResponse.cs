namespace PRN232.LAB_1.API.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public object? Pagination { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, object? pagination = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = "Success",
            Data = data,
            Pagination = pagination,
            Errors = null
        };
    }

    public static ApiResponse<T> Created(T data)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = "Created",
            Data = data,
            Pagination = null,
            Errors = null
        };
    }

    public static ApiResponse<T> Fail(string message, Dictionary<string, string[]>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default,
            Pagination = null,
            Errors = errors
        };
    }
}
