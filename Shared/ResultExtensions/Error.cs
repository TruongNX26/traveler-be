﻿namespace Shared.ResultExtensions;

public class Error
{
    public readonly IDictionary<string, string> ErrorDetails;

    private Error(ErrorType errorType, string code, string message, IDictionary<string, string>? errorDetails = null)
    {
        ErrorType = errorType;
        Code = code;
        Message = message;
        ErrorDetails = errorDetails ?? new Dictionary<string, string>();
    }

    public ErrorType ErrorType { get; }

    public string Code { get; }

    public string Message { get; }


    public static Error Custom(ErrorType errorType, string code, string message,
        IDictionary<string, string>? errorDetails = null)
    {
        return new(errorType, code, message, errorDetails);
    }

    public static Error Validation(IDictionary<string, string>? errorDetails = null)
    {
        return new(ErrorType.Validation, "General.Validation", "A validation error has occurred.", errorDetails);
    }

    public static Error Conflict(IDictionary<string, string>? errorDetails = null)
    {
        return new(ErrorType.Conflict, "General.Conflict", "A conflict error has occurred.", errorDetails);
    }

    public static Error NotFound(IDictionary<string, string>? errorDetails = null)
    {
        return new(ErrorType.NotFound, "General.NotFound", "A 'Not Found' error has occurred.", errorDetails);
    }

    public static Error Unexpected(IDictionary<string, string>? errorDetails = null)
    {
        return new(ErrorType.Unexpected, "General.Unexpected", "A unexpected error has occurred.", errorDetails);
    }

    public static Error Authentication(IDictionary<string, string>? errorDetails = null)
    {
        return new(ErrorType.Authentication, "General.Authentication", "Unauthorized", errorDetails);
    }

    public static Error Authorization(IDictionary<string, string>? errorDetails = null)
    {
        return new(ErrorType.Authorization, "General.Authorization", "Forbidden", errorDetails);
    }
}