using Cw7.DAL;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Cw7.Middleware
{
    public class IndexMiddleware
    {
        private readonly RequestDelegate _next;

        public IndexMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IStudentDbService studentDbService)
        {
            if (!context.Request.Headers.ContainsKey("Index")) {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Index header is missing");
                return;
            }
            var index = context.Request.Headers["Index"].ToString();
            if (studentDbService.GetStudent(index) == null) {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsync("Student with index " + index + " not found");
                return;
            }

            await _next(context);
        }
    }
}
