using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace CognitoPOC.Infrastructure.Extensions
{
    public static class HttpHelpers
    {

        public static string? GetHeaderValue(this HttpRequest request, string header)
        {
            return request.Headers.TryGetValue(header, out var value) ? value.ToString() : null;
        }

        public static string GetIpAddress(this HttpRequest request) 
            => request.HttpContext.Connection.RemoteIpAddress != null 
                ? request.HttpContext.Connection.RemoteIpAddress.ToString() 
                : "0.0.0.0";

        public static string? GetUserAgent(this HttpRequest request) 
            => request.GetHeaderValue("User-Agent");

        public static string? GetCustomUserAgent(this HttpRequest request)
            => request.GetHeaderValue("stu-user-agent");

        public static bool IsLocal(this IWebHostEnvironment env)
        {
            return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Local";
        }

        public static string? GetUsername(this HttpContext context)
            => context
                .User
                .Claims
                .Where(p => p.Type == "username")
                .Select(p => p.Value)
                .FirstOrDefault();
    }
}
