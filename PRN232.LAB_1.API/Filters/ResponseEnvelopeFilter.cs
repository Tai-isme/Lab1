using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PRN232.LAB_1.API.Models;

namespace PRN232.LAB_1.API.Filters;

public class ResponseEnvelopeFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        // Skip if already wrapped
        if (context.Result is ObjectResult objectResult
            && objectResult.Value != null
            && objectResult.Value.GetType().IsGenericType
            && objectResult.Value.GetType().GetGenericTypeDefinition() == typeof(ApiResponse<>))
        {
            return;
        }

        // Get pagination metadata from HttpContext.Items (set by controller)
        var pagination = context.HttpContext.Items["Pagination"];

        // Handle NotFoundResult (ASP.NET Core's 404 helper with no body)
        if (context.Result is NotFoundResult)
        {
            var notFoundResponse = typeof(ApiResponse<>)
                .MakeGenericType(typeof(object))
                .GetMethod("Fail", new[] { typeof(string), typeof(Dictionary<string, string[]>) })?
                .Invoke(null, new object?[] { "Resource not found", null });

            context.Result = new ObjectResult(notFoundResponse)
            {
                StatusCode = 404
            };
            return;
        }

        // Handle ObjectResult (the common case)
        if (context.Result is ObjectResult result)
        {
            var statusCode = ResolveStatusCode(context.Result, result.StatusCode);
            var data = result.Value;

            object? envelope = null;

            if (statusCode >= 200 && statusCode < 300)
            {
                if (statusCode == 201)
                {
                    envelope = typeof(ApiResponse<>)
                        .MakeGenericType(data?.GetType() ?? typeof(object))
                        .GetMethod("Created")?
                        .Invoke(null, new[] { data });
                }
                else
                {
                    var okMethod = typeof(ApiResponse<>)
                        .MakeGenericType(data?.GetType() ?? typeof(object))
                        .GetMethods()
                        .First(m => m.Name == "Ok" && m.GetParameters().Length == 2);

                    envelope = okMethod
                        .Invoke(null, new object?[] { data, pagination });
                }
            }
            else if (statusCode == 400)
            {
                var errors = new Dictionary<string, string[]>();
                foreach (var (key, entry) in context.ModelState)
                {
                    if (entry.Errors.Count > 0)
                    {
                        errors[key] = entry.Errors.Select(e => e.ErrorMessage).ToArray();
                    }
                }

                var message = result.Value is ProblemDetails pd
                    ? pd.Title ?? "Bad request"
                    : errors.Count > 0
                        ? "Validation failed"
                        : (result.Value?.ToString() ?? "Bad request");

                envelope = typeof(ApiResponse<>)
                    .MakeGenericType(typeof(object))
                    .GetMethod("Fail", new[] { typeof(string), typeof(Dictionary<string, string[]>) })?
                    .Invoke(null, new object?[] { message, errors.Count > 0 ? errors : null });
            }
            else if (statusCode == 404)
            {
                var message = result.Value is ProblemDetails pd
                    ? pd.Title ?? "Resource not found"
                    : result.Value?.ToString() ?? "Resource not found";

                envelope = typeof(ApiResponse<>)
                    .MakeGenericType(typeof(object))
                    .GetMethod("Fail", new[] { typeof(string), typeof(Dictionary<string, string[]>) })?
                    .Invoke(null, new object?[] { message, null });
            }
            else
            {
                envelope = typeof(ApiResponse<>)
                    .MakeGenericType(typeof(object))
                    .GetMethod("Fail", new[] { typeof(string), typeof(Dictionary<string, string[]>) })?
                    .Invoke(null, new object?[] { "An error occurred", null });
            }

            if (envelope != null)
            {
                var wrappedResult = new ObjectResult(envelope)
                {
                    StatusCode = statusCode
                };

                // Preserve Location header for CreatedAtAction results
                if (context.Result is CreatedAtActionResult createdAtAction)
                {
                    var urlHelper = createdAtAction.UrlHelper;
                    if (urlHelper != null)
                    {
                        var url = urlHelper.Action(new Microsoft.AspNetCore.Mvc.Routing.UrlActionContext
                        {
                            Action = createdAtAction.ActionName,
                            Controller = createdAtAction.ControllerName,
                            Values = createdAtAction.RouteValues,
                            Protocol = context.HttpContext.Request.Scheme
                        });
                        if (!string.IsNullOrEmpty(url))
                        {
                            context.HttpContext.Response.Headers["Location"] = url;
                        }
                    }
                }

                context.Result = wrappedResult;
            }
        }
    }

    private static int ResolveStatusCode(IActionResult result, int? explicitStatusCode)
    {
        if (explicitStatusCode.HasValue)
            return explicitStatusCode.Value;

        return result switch
        {
            CreatedResult or CreatedAtActionResult or CreatedAtRouteResult => 201,
            BadRequestObjectResult or BadRequestResult => 400,
            NotFoundObjectResult => 404,
            _ => 200
        };
    }

    public void OnResultExecuted(ResultExecutedContext context) { }
}
