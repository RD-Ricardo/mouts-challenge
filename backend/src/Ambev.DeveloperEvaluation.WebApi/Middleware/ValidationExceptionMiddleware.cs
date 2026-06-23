using Ambev.DeveloperEvaluation.Domain.Exceptions;
using FluentValidation;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware
{
    public class ValidationExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ValidationExceptionMiddleware> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ValidationExceptionMiddleware(RequestDelegate next, ILogger<ValidationExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                await WriteAsync(context, StatusCodes.Status400BadRequest, "ValidationError",
                    "Invalid input data", string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)));
            }
            catch (KeyNotFoundException ex)
            {
                await WriteAsync(context, StatusCodes.Status404NotFound, "ResourceNotFound",
                    "Resource not found", ex.Message);
            }
            catch (DomainException ex)
            {
                await WriteAsync(context, StatusCodes.Status400BadRequest, "DomainRuleViolation",
                    "Business rule violated", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await WriteAsync(context, StatusCodes.Status409Conflict, "ConflictError",
                    "Operation conflict", ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                await WriteAsync(context, StatusCodes.Status401Unauthorized, "AuthenticationError",
                    "Authentication failed", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                await WriteAsync(context, StatusCodes.Status500InternalServerError, "InternalServerError",
                    "An unexpected error occurred", ex.Message);
            }
        }

        private static Task WriteAsync(HttpContext context, int statusCode, string type, string error, string detail)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            var payload = new { type, error, detail };
            return context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
        }
    }
}
