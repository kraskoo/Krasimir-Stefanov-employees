namespace Employees.Application.Models;

public struct Result<T> where T : class
{
    private Result(T data, string errorMessage)
    {
        Data = data;
        ErrorMessage = errorMessage;
    }

    private Result(T data) : this(data, string.Empty)
    {
    }

    private Result(string errorMessage) : this(default!, errorMessage)
    {
    }

    public T Data { get; private set; } = default!;

    public string? ErrorMessage { get; private set; } = default;

    public readonly bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);

    public static implicit operator Result<T>(T data) => new(data);

    public static implicit operator Result<T>(string message) => new(message);
}