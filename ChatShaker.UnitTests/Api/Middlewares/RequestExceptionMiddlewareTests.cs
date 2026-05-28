using ChatShaker.Api.Middlewares;
using ChatShaker.Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using ChatShaker.Api.Helpers;
using FluentAssertions;
using Xunit;

namespace ChatShaker.UnitTests.Api.Middlewares;

public class RequestExceptionMiddlewareTests
{
    private readonly Mock<ILogger<RequestExceptionMiddleware>> _loggerMock;
    private readonly RequestExceptionMiddleware _middleware;

    public RequestExceptionMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<RequestExceptionMiddleware>>();
        _middleware = new RequestExceptionMiddleware(_loggerMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_ShouldCallNext()
    {
        var context = new DefaultHttpContext();
        var nextCalled = false;
        Task Next(HttpContext innerContext)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        await _middleware.InvokeAsync(context, Next);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenValidationException_ShouldReturnBadRequest()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new ValidationException("Validation failed", new[]
        {
            new ValidationFailure("Prop1", "Error1")
        });

        await _middleware.InvokeAsync(context, _ => throw exception);

        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Response.ContentType.Should().Be("application/json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var response = JsonSerializer.Deserialize<Response<object>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        response.Should().NotBeNull();
        response.Errors.Should().NotBeNull("Response body was: " + responseBody);
        response.Errors.Should().Contain("Prop1: Error1");
    }

    private class TestHttpException : MainHttpException
    {
        public TestHttpException(string message, int statusCode) : base(message, statusCode) { }
    }

    [Fact]
    public async Task InvokeAsync_WhenMainHttpException_ShouldReturnCustomStatusCode()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new TestHttpException("Custom error", StatusCodes.Status403Forbidden);

        await _middleware.InvokeAsync(context, _ => throw exception);

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task InvokeAsync_WhenUnauthorizedAccessException_ShouldReturnUnauthorized()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new UnauthorizedAccessException("Unauthorized");

        await _middleware.InvokeAsync(context, _ => throw exception);

        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task InvokeAsync_WhenGenericException_ShouldReturnInternalServerError()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new Exception("Generic error");

        await _middleware.InvokeAsync(context, _ => throw exception);

        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }
}
