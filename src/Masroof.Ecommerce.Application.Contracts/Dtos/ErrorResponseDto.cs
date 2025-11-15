using System;

namespace Masroof.Ecommerce.Application.Contracts.Dtos;

/// <summary>
/// Standardized error response DTO for consistent error handling across the API
/// </summary>
public class ErrorResponseDto
{
    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Application-specific error code for categorizing errors
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>
    /// User-friendly error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Optional additional details about the error (e.g., validation errors)
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Timestamp when the error occurred (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; }
}
