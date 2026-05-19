namespace GpaSystem.API.Exceptions;

public class ApiException : Exception
{
    public ApiException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }

    public static ApiException NotFound(string message) => new(StatusCodes.Status404NotFound, message);

    public static ApiException Conflict(string message) => new(StatusCodes.Status409Conflict, message);

    public static ApiException BadRequest(string message) => new(StatusCodes.Status400BadRequest, message);
}
