using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MyProject.Middleware;

public class ApiExceptionMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteProblem(context, StatusCodes.Status401Unauthorized, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            // Duplicate email va shunga o‘xshash business errorlar
            await WriteProblem(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (Exception ex)
        {
            await WriteProblem(context, StatusCodes.Status500InternalServerError,
                "Internal server error. Check server logs for details.");
        }
    }

    private static async Task WriteProblem(HttpContext context, int status, string message)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = status,
            Title = ((HttpStatusCode)status).ToString(),
            Detail = message
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
}
