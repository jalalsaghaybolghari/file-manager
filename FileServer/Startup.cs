using FileServer.Database;
using FileServer.Extention;
using FileServer.Framework.Authentication;
using FileServer.Framework.Swagger;
using FileServer.ViewModels.Setting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Globalization;

namespace FileServer
{
    public class Startup
    {
        private static SiteSettings _siteSettings;

        public Startup(IConfiguration configuration)
        {
            configRoot = configuration;
            _siteSettings = configuration.GetSection(nameof(SiteSettings)).Get<SiteSettings>();
        }
        public IConfiguration configRoot
        {
            get;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            var allowOrigins = configRoot.GetValue<string>("AllowedOrigins")?.Split(",") ?? new string[0];

            services.Configure<SiteSettings>(configRoot.GetSection(nameof(SiteSettings)));

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(configRoot.GetConnectionString("DefaultConnection")));

            services.AddDistributedMemoryCache();

            services.AddControllers();
            //services.AddControllersWithViews();
            services.AddAuthentication();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo{ Title = "File Service API", Version = "v1" });
                c.OperationFilter<MyHeaderFilter>();
            });
            services.AddMvc().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });
            //services.AddRazorPages();

            services.AddScoped<ApiKeyAuthFilter>();

            services.AddHttpContextAccessor();
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    policy =>
                    {
                        policy
                            .WithOrigins(allowOrigins)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
            });

        }
        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
            var defaultCultureInfo = new CultureInfo("fa");
            CultureInfo.DefaultThreadCurrentCulture = defaultCultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = defaultCultureInfo;

            if (!app.Environment.IsDevelopment())
            {
                //app.UseStatusCodePagesWithRedirects("/Error/{0}");
                app.UseHsts();
            }
            else
            {
                //app.UseDeveloperExceptionPage();
                //app.UseSwagger();
                //app.UseSwaggerUI(c =>
                //{
                //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "File Service API V1");
                //});
            }
            app.UseAuthorization();
            app.UseSwaggerAuthorized();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "File Service API V1");
            });

            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors();

            app.UseTransactionsPerRequest();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.Run();
        }

        public void SeedingDatabase(IServiceCollection services)
        {
            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

                // Seed the database
                SeedData.Seed(context);
            }
        }
    }

}
