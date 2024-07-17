
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FileServer.Framework.Swagger
{
    public class MyHeaderFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();


            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "x-api-key",
                In = ParameterLocation.Header,
                Description = "api key",
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = "string"
                    //Default = new OpenApiString("Bearer ")
                }

            });
        }
    }
}