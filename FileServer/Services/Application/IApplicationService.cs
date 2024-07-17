using FileServer.ViewModels;
using FileServer.ViewModels.Interface;

namespace FileServer.Services.Application
{
    public interface IApplicationService : IScopedDependency
    {
        Task<Result<Models.ApplicationApiKey>> GetApplicationApiKey(int? id = null, string? apiKey = null, bool? isActive = null, DateTime? expireTime = null, string remoteIpAddress = null);
        Task<Result<Models.Application>> GetApplication(int? id = null, string? apiKey = null, bool? isActive = null, string remoteIpAddress = null);
        Task<Result<Models.ApplicationApiKey>> AddApplicationApiKey(int appId, CancellationToken cancellationToken, string[] permittedIPs = null, DateTime? expireTime = null);
        Task<Result<AppSettingResult>> GetApplicationSettings(int applicationId, CancellationToken cancellationToken);
    }
}
