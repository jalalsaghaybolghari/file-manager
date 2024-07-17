using FileServer.Database;
using FileServer.StaticData;
using FileServer.ViewModels;
using FileServer.ViewModels.Setting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace FileServer.Services.Application
{
    public class ApplicationService : IApplicationService
    {
        #region Fields
        private IRepository<Models.ApplicationApiKey> _applicationApiKeyRepository;
        private IRepository<Models.Application> _applicationRepository;
        private readonly IOptionsSnapshot<SiteSettings> _siteSettings;


        #endregion
        public ApplicationService(
            IRepository<Models.ApplicationApiKey> applicationApiKeyRepository,
            IRepository<Models.Application> applicationRepository,
            IOptionsSnapshot<SiteSettings> siteSettings
        )
        {
            _applicationApiKeyRepository = applicationApiKeyRepository;
            _applicationRepository = applicationRepository;
            _siteSettings = siteSettings;
        }

        #region GetApplicationApiKey
        public async Task<Result<Models.ApplicationApiKey>> GetApplicationApiKey(
            int? id = null,
            string? apiKey = null,
            bool? isActive = null ,
            DateTime? expireTime = null,
            string remoteIpAddress = null
        )
        {
            var result = new Result<Models.ApplicationApiKey>();
            var applicationApiKeys = _applicationApiKeyRepository.GetQuery();


            var applicationApiKey = new Models.ApplicationApiKey();

            if (isActive != null)
                applicationApiKeys = applicationApiKeys.Where(i => i.IsActive == isActive);
            if (expireTime != null)
                applicationApiKeys = applicationApiKeys.Where(i => i.ExpireTime > expireTime);
            if (id != null)
                applicationApiKey = applicationApiKeys.Where(i => i.Id == id).FirstOrDefault();
            if (apiKey != null)
                applicationApiKey = applicationApiKeys.Where(i => i.ApiKey == apiKey).FirstOrDefault();
            if(remoteIpAddress != null && applicationApiKey?.PermittedIPs != null)
            {
                var permittedIPs = applicationApiKey.PermittedIPs.Split(",");
                if(!permittedIPs.Any(ip => ip == remoteIpAddress))
                {
                    result.Success = false;
                    result.Message = Resources.Global.AppApiKeyNotFound;
                    return result;
                }
            }
            if (applicationApiKey == null)
            {
                result.Success = false;
                result.Message = Resources.Global.AppApiKeyNotFound;
                return result;
            }
            result.Data = applicationApiKey;
            return result;
        }
        #endregion
        #region GetApplication
        public async Task<Result<Models.Application>> GetApplication(
            int? id = null,
            string? apiKey = null,
            bool? isActive = null,
            string remoteIpAddress = null
        )
        {
            var result = new Result<Models.Application>();
            var applications = _applicationRepository.GetQuery();

            var application = new Models.Application();

            if (isActive != null)
                applications = applications.Where(i => i.IsActive == isActive);
            if (id != null)
                application = applications.Where(i => i.Id == id).FirstOrDefault();
            if (apiKey != null)
            {
                var appApiKey = await GetApplicationApiKey(
                    apiKey: apiKey ,
                    isActive: isActive,
                    expireTime : DateTime.UtcNow,
                    remoteIpAddress: remoteIpAddress
                    );
                if (!appApiKey.Success)
                {
                    result.Success = appApiKey.Success;
                    result.Message = appApiKey.Message;
                    return result;
                }
                application = applications.Where(i => i.Id == appApiKey.Data.ApplicationId).FirstOrDefault();
            }

            if (application == null)
            {
                result.Success = false;
                result.Message = Resources.Global.AppNotFound;
                return result;
            }
            result.Data = application;
            return result;
        }
        #endregion


        #region AddApplicationApiKey
        public async Task<Result<Models.ApplicationApiKey>> AddApplicationApiKey(
            int appId,
            CancellationToken cancellationToken,
            string[] permittedIPs = null,
            DateTime? expireTime = null
        )
        {
            var result = new Result<Models.ApplicationApiKey>();

            var parameters = new List<string>();
            parameters.Add(appId.ToString());

            var apiKey = GenerateRandomKey(parameters.ToArray());

            var ips = string.Empty;
            if(permittedIPs != null)
                ips = String.Join(",", permittedIPs);

            var applicationApiKey = new Models.ApplicationApiKey
            {
                ApplicationId = appId,
                ApiKey = apiKey,
                IsActive = true,
                ExpireTime = expireTime,
                PermittedIPs = permittedIPs != null ? ips : null
            };
            await _applicationApiKeyRepository.AddAsync(applicationApiKey, cancellationToken);
            result.Data = applicationApiKey;
            return result;
        }
        #endregion


        #region GetApplicationSettings
        public async Task<Result<AppSettingResult>> GetApplicationSettings(
            int applicationId,
            CancellationToken cancellationToken
        )
        {
            var result = new Result<AppSettingResult>();

            try
            {
                var application = await _applicationRepository.GetAsync(predicate: x => x.Id == applicationId, cancellationToken: cancellationToken);
                var applicationSettings = application.AppSettings;
                var appSettingObject = new AppSettingResult();

                if (applicationSettings != null)
                    appSettingObject = JsonConvert.DeserializeObject<AppSettingResult>(applicationSettings);

                result.Data = appSettingObject;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;    
            }
            return result;
        }
        #endregion

        #region GenerateRandomKey
        private string GenerateRandomKey(
            string[] parameters
        )
        {
            string hash = String.Empty;
            var textInput = String.Empty;
            var separator = StaticVariables.CharacterSeparator;
            foreach (var parameter in parameters)
            {
                if(textInput != String.Empty) textInput += separator;
                textInput += parameter;
            }
            textInput += separator + _siteSettings.Value.EncryptKey + separator + DateTime.UtcNow;

            // Initialize a SHA256 hash object
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashValue = sha256.ComputeHash(Encoding.UTF8.GetBytes(textInput));

                // Convert the byte array to string format
                foreach (byte b in hashValue)
                {
                    hash += $"{b:X2}";
                }
            }
            return hash.ToLower();
        }
        #endregion
    }
}