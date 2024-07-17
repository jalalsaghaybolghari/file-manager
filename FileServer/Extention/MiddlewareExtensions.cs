using FileServer.Framework.Swagger;
using FileServer.Framework.Transaction;

namespace FileServer.Extention
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseTransactionsPerRequest(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TransactionMiddleware>();
        }
        public static IApplicationBuilder UseSwaggerAuthorized(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SwaggerBasicAuthMiddleware>();
        }
    }
}
