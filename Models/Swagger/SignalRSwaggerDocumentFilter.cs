using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace backtimetracker.Swagger
{
    /// <summary>
    /// این DocumentFilter صرفاً مسیر هاب SignalR را به Swagger اضافه می‌کند
    /// و هیچ جزئیاتی درباره پیام‌ها نمایش نمی‌دهد.
    /// </summary>
    public class SignalRSwaggerDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var hubPath = "/hubs/task";

            // تعریف یک OpenApiOperation ساده برای متد GET (فقط به عنوان نشانهٔ وجود هاب)
            var getOperation = new OpenApiOperation
            {
                Tags = new List<OpenApiTag> { new OpenApiTag { Name = "SignalR Hubs" } },
                Summary = "",
                Description = "",
                Responses = new OpenApiResponses
                {
                    {
                        "200", new OpenApiResponse
                        {
                            Description = "مسیر هاب آمادهٔ اتصال WebSocket است."
                        }
                    }
                }
            };

            var pathItem = new OpenApiPathItem
            {
                Description = "SignalR TaskHub endpoint",
                Operations = new Dictionary<OperationType, OpenApiOperation>
                {
                    { OperationType.Get, getOperation }
                }
            };

            swaggerDoc.Paths.Add(hubPath, pathItem);
        }
    }
}
