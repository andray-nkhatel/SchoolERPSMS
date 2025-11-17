using Microsoft.AspNetCore.Mvc;

namespace BluebirdCore.Infrastructure
{
    public class FileCallbackResult : IActionResult
    {
        private readonly string _contentType;
        private readonly Func<Stream, HttpContext, Task> _callback;

        public FileCallbackResult(string contentType, Func<Stream, HttpContext, Task> callback)
        {
            _contentType = contentType;
            _callback = callback;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            response.ContentType = _contentType;

            // Ensure response buffering is disabled for streaming
            if (response.Body.CanSeek)
            {
                response.Body.Position = 0;
            }

            await _callback(response.Body, context.HttpContext);
        }
    }
}