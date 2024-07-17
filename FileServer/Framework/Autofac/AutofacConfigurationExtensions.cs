using Autofac;
using FileServer.Database;
using FileServer.ViewModels.Interface;
using System.Reflection;

namespace FileServer.Framework.Autofac
{
    public static class AutofacConfigurationExtensions
    {
        public static void AddAutofacDependencyServices(this ContainerBuilder containerBuilder)
        {

            var currentAssembly = Assembly.GetExecutingAssembly();

            containerBuilder.RegisterAssemblyTypes(currentAssembly)
            .AssignableTo<IScopedDependency>()
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

            containerBuilder.RegisterGeneric(typeof(Repository<>))
            .As(typeof(IRepository<>))
            .InstancePerLifetimeScope();
        }
    }
}