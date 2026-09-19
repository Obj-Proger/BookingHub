using Microsoft.Extensions.Logging;

namespace BookingHub.Application.Common.Behaviors;

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Handling {RequestName}")]
    public static partial void HandlingRequest(ILogger logger, string requestName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handled {RequestName}")]
    public static partial void HandledRequest(ILogger logger, string requestName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{RequestName} failed: [{ErrorCode}] {ErrorMessage}")]
    public static partial void RequestFailed(ILogger logger, string requestName, string errorCode, string errorMessage);
}