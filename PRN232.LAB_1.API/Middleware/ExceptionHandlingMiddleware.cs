using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using PRN232.LAB_1.API.Models;
using System.Text.Json;

namespace PRN232.LAB_1.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, IWebHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Exception] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");

            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            ApiResponse<object> envelope;

            if (_env.IsDevelopment())
            {
                var errors = new Dictionary<string, string[]>
                {
                    { "Exception", new[] { $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}" } }
                };
                envelope = ApiResponse<object>.Fail("An internal server error occurred", errors);
            }
            else
            {
                envelope = ApiResponse<object>.Fail("An internal server error occurred");
            }

            var json = JsonSerializer.Serialize(envelope);
            await context.Response.WriteAsync(json);
        }
    }
}
