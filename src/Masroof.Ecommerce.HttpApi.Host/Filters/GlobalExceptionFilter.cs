using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc.ExceptionHandling;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Validation;
using Masroof.Ecommerce.Application.Contracts.Dtos;

namespace Masroof.Ecommerce.Filters;

/// <summary>
/// Global exception filter to handle all unhandled exceptions in the application
/// </summary>
public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        if (context.ExceptionHandled)
        {
            return;
        }

        var exception = context.Exception;

        // Log the exception with full details
        _logger.LogError(
            exception,
            "An unhandled exception occurred. Exception Type: {ExceptionType}, Message: {Message}",
            exception.GetType().Name,
            exception.Message
        );

        // Create error response based on exception type
        var errorResponse = CreateErrorResponse(exception);

        // Set the result
        context.Result = new ObjectResult(errorResponse)
        {
            StatusCode = errorResponse.StatusCode
        };

        context.ExceptionHandled = true;
    }

    private ErrorResponseDto CreateErrorResponse(Exception exception)
    {
        var timestamp = DateTime.UtcNow;

        // Handle UserFriendlyException - return message as-is
        if (exception is UserFriendlyException userFriendlyException)
        {
            return new ErrorResponseDto
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                ErrorCode = userFriendlyException.Code ?? "USER_FRIENDLY_ERROR",
                Message = userFriendlyException.Message,
                Details = userFriendlyException.Details,
                Timestamp = timestamp
            };
        }

        // Handle EntityNotFoundException - 404 with friendly message
        if (exception is EntityNotFoundException entityNotFoundException)
        {
            return new ErrorResponseDto
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                ErrorCode = "ENTITY_NOT_FOUND",
                Message = entityNotFoundException.Message ?? "The requested resource was not found.",
                Details = $"Entity Type: {entityNotFoundException.EntityType?.Name}",
                Timestamp = timestamp
            };
        }

        // Handle AbpValidationException - 400 with validation errors
        if (exception is AbpValidationException validationException)
        {
            var validationErrors = string.Join("; ",
                validationException.ValidationErrors.Select(e =>
                    $"{string.Join(".", e.MemberNames)}: {e.ErrorMessage}"));

            return new ErrorResponseDto
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                ErrorCode = "VALIDATION_ERROR",
                Message = "One or more validation errors occurred.",
                Details = validationErrors,
                Timestamp = timestamp
            };
        }

        // Handle AbpAuthorizationException - 403 Forbidden
        if (exception is AbpAuthorizationException authorizationException)
        {
            return new ErrorResponseDto
            {
                StatusCode = (int)HttpStatusCode.Forbidden,
                ErrorCode = "AUTHORIZATION_ERROR",
                Message = "You do not have permission to perform this action.",
                Details = authorizationException.Message,
                Timestamp = timestamp
            };
        }

        // Handle UnauthorizedAccessException - 401
        if (exception is UnauthorizedAccessException)
        {
            return new ErrorResponseDto
            {
                StatusCode = (int)HttpStatusCode.Unauthorized,
                ErrorCode = "UNAUTHORIZED",
                Message = "Authentication is required to access this resource.",
                Details = null,
                Timestamp = timestamp
            };
        }

        // Handle BusinessException - 400 Bad Request
        if (exception is BusinessException businessException)
        {
            return new ErrorResponseDto
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                ErrorCode = businessException.Code ?? "BUSINESS_ERROR",
                Message = businessException.Message,
                Details = businessException.Details,
                Timestamp = timestamp
            };
        }

        // Handle all other exceptions - 500 with generic message (hide technical details)
        _logger.LogCritical(exception, "Critical unhandled exception occurred");

        return new ErrorResponseDto
        {
            StatusCode = (int)HttpStatusCode.InternalServerError,
            ErrorCode = "INTERNAL_SERVER_ERROR",
            Message = "An internal server error occurred. Please try again later or contact support if the problem persists.",
            Details = null, // Don't expose internal error details to clients
            Timestamp = timestamp
        };
    }
}
