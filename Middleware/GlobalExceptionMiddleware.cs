namespace SchoolErpSMS.Middleware
{
    namespace SchoolManagementSystem.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            // Determine the error message based on exception type
            string errorMessage;
            int statusCode;
            
            switch (exception)
            {
                case ArgumentException argEx:
                    statusCode = 400;
                    errorMessage = argEx.Message;
                    break;
                case UnauthorizedAccessException:
                    statusCode = 401;
                    errorMessage = "Access denied";
                    break;
                case KeyNotFoundException:
                    statusCode = 404;
                    errorMessage = exception.Message ?? "Resource not found";
                    break;
                case InvalidOperationException ioEx:
                    // For InvalidOperationException, use the actual message (usually contains helpful details)
                    statusCode = 500;
                    errorMessage = ioEx.Message ?? "An operation failed";
                    break;
                default:
                    statusCode = 500;
                    // For other exceptions, include the message if it's not too technical
                    errorMessage = !string.IsNullOrWhiteSpace(exception.Message) 
                        ? exception.Message 
                        : "An error occurred while processing your request.";
                    break;
            }
            
            var response = new
            {
                message = errorMessage,
                error = statusCode >= 500 ? "Internal server error" : "Client error",
                details = exception.Message // Keep details for debugging
            };

            context.Response.StatusCode = statusCode;
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
}