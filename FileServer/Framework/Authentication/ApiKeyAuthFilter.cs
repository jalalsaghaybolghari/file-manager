using FileServer.Services.Application;
using FileServer.ViewModels;
using FileServer.ViewModels.Setting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace FileServer.Framework.Authentication
{
    public class ApiKeyAuthFilter : IAsyncAuthorizationFilter
    {
        private readonly IOptionsSnapshot<SiteSettings> _siteSettings;
        private readonly IApplicationService _applicationService;
        public ApiKeyAuthFilter(
            IOptionsSnapshot<SiteSettings> siteSettings,
            IApplicationService applicationService
            )
        {
            _siteSettings = siteSettings;
            _applicationService = applicationService;

        }
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var result = new Result();
            if (!context.HttpContext.Request.Headers.TryGetValue(_siteSettings.Value.ApiKeyHeaderName, out var extractedApiKey))
            {
                result.Success = false;
                result.Message = Resources.Global.AppApiKeyNotFound;
                context.Result = new UnauthorizedObjectResult(result);
                return;
            }

            var remoteIpAddress = context.HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            var app = await _applicationService.GetApplication(apiKey: extractedApiKey, isActive: true , remoteIpAddress: remoteIpAddress);

            if (!app.Success)
            {
                result.Success = false;
                result.Message = Resources.Global.AppApiKeyIsInvalid;
                context.Result = new UnauthorizedObjectResult(result);
                return;
            }
        }
    }
}
