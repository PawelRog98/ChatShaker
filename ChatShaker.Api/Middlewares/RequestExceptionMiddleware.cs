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
                    resp = new Response<object>
                    {
                        Success = false,
                        Errors = validationException.Errors
                        .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
                        .ToArray(),
                        Message = validationException.Message
                    };

                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    break;

                case MainHttpException mainHttpException:
                    resp = new Response<object>
                    {
                        Success = false,
                        Errors = [mainHttpException.Message],
                        Message = "HttpError"
                    };

                    context.Response.StatusCode = mainHttpException.StatusCode;
                    break;

                case UnauthorizedAccessException unauthorizedAccessException:
                    resp = new Response<object>
                    {
                        Success = false,
                        Errors = [unauthorizedAccessException.Message],
                        Message = "Unauthorized"
                    };

                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    break;

                default:
                    resp = new Response<object>
                    {
                        Success = false,
                        Errors = [ex.Message],
                        Message = "Internal Error"
                    };

                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    break;
            }

            var result = JsonSerializer.Serialize(resp);
            await context.Response.WriteAsync(result);
        }
    }
}
