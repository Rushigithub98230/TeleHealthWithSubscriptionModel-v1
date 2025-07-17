using System.Collections.Generic;

namespace SmartTelehealth.Application.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        public static ApiResponse<T> SuccessResponse(T? data, string message = "", int statusCode = 200)
            => new() { Success = true, Data = data, Message = message, StatusCode = statusCode };

        public static ApiResponse<T> ErrorResponse(string message, int statusCode = 400)
            => new() { Success = false, Data = default, Message = message, StatusCode = statusCode };

        public static ApiResponse<T> ErrorResponse(string message, List<string> errors, int statusCode = 400)
            => new() { Success = false, Data = default, Message = message, StatusCode = statusCode, Errors = errors };
    }
} 