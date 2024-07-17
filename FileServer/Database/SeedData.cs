using FileServer.Models;
using FileServer.ViewModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FileServer.Database
{
    public class SeedData
    {
        public static void Seed(ApplicationDbContext context)
        {
            context.Database.EnsureCreated(); // Ensure the database is created

            // Check if the data has already been seeded
            if (context.Applications.Any())
            {
                return; // Data has already been seeded
            }

            #region create appsetting
            var appSettingObject = new AppSettingResult();
            appSettingObject.FileSettings = new ApplicationFileSettings();
            appSettingObject.FileSettings.MaximumFileSize = 50000000;
            appSettingObject.FileSettings.PermittedExtentions = "jpeg,png,jpg";
            var appSetting = JsonConvert.SerializeObject(appSettingObject);
            #endregion

            var user = new User()
            {
                Id = 1,
                Name = "sample",
                IsActive = true,
                DirectoryPath = "sample",
            };

            var application = new Application()
            {
                Id = 1,
                IsActive = true,
                UserId = user.Id,
                Description = null,
                Name = "sample",
                AppSettings = appSetting,
                DirectoryPath = "sample.com"
            };           

            var document = new Document()
            {
                FileWidth = 440,
                FileHeight = 440,
                FileLength = 24064,
                Tag = "Products",
                FileName = "default",
                FileExtention = ".png",
                ClientDocumentId = null,
                ApplicationId = application.Id,
                Id = new Guid("11111111-1111-1111-1111-111111111111")
            };

            var applicationApiKey = new ApplicationApiKey()
            {
                Id = 1,
                IsActive = true,
                PermittedIPs = null,
                ApplicationId = application.Id,
                ExpireTime = DateTime.UtcNow.AddMonths(12),
                ApiKey = "710c4fdd5118239707ced8749956ffc07bac0f0e39f518f54762311525678bc8"
            };

            context.Users.Add(user);
            context.Applications.Add(application);
            context.Documents.Add(document);
            context.ApplicationApiKeys.Add(applicationApiKey);

            context.SaveChanges();
        }
    }
}
