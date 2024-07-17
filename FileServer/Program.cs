using Autofac;
using Autofac.Extensions.DependencyInjection;
using FileServer;
using FileServer.Framework.Autofac;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(builder =>
    {
        builder.AddAutofacDependencyServices();
    });
var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services); // calling ConfigureServices method
startup.SeedingDatabase(builder.Services);

var app = builder.Build();
startup.Configure(app, builder.Environment); // calling Configure method
