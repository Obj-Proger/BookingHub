using Microsoft.AspNetCore.Mvc;

namespace BookingHub.API.Common;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult HandleResult(Result result) =>
        result.IsSuccess ? NoContent() : MapError(result.Error);

    protected IActionResult HandleResult<T>(Result<T> result) =>
        result.IsSuccess ? Ok(result.Value) : MapError(result.Error);

    private IActionResult MapError(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        return Problem(
            detail: error.Message,
            statusCode: statusCode,
            extensions: new Dictionary<string, object?> { ["errorCode"] = error.Code });
    }
}