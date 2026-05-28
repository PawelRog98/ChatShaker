using ChatShaker.Api.Helpers;
using ChatShaker.Domain.Exceptions;
using FluentValidation;
using System.Text.Json;

namespace ChatShaker.Api.Middlewares
{
    public class RequestExceptionMiddleware : IMiddleware
    {
        private readonly ILogger<RequestExceptionMiddleware> _logger;

        public RequestExceptionMiddleware(ILogger<RequestExceptionMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate request)
        {
            try
            {
                await request.Invoke(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            Response<object> resp;

            context.Response.ContentType = "application/json";

            switch(ex)
            {
                case ValidationException validationException:
                    
                    var errors = validationException.Errors
                        .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
                        .ToArray();
                    
                    resp = new ErrorResponse(validationException.Message, errors);

                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    break;

                case MainHttpException mainHttpException:
                    resp = new ErrorResponse("HttpError", [mainHttpException.Message]);

                    context.Response.StatusCode = mainHttpException.StatusCode;
                    break;

                case UnauthorizedAccessException unauthorizedAccessException:
                    resp = new ErrorResponse("Unauthorized", [unauthorizedAccessException.Message]);

                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    break;

                default:
                    resp = new ErrorResponse("Internal Error",  [ex.Message]);

                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    break;
            }

            var result = JsonSerializer.Serialize(resp);
            await context.Response.WriteAsync(result);
        }
    }
}
