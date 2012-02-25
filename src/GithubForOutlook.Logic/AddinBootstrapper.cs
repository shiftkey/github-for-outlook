using System;
using Analects.SettingsService;
using Autofac;
using GithubForOutlook.Logic.Ribbons.Task;
using GithubForOutlook.Logic.Ribbons.Email;
using GithubForOutlook.Logic.Ribbons.MainExplorer;
using NGitHub;
using NGitHub.Authentication;
using VSTOContrib.Core.RibbonFactory.Interfaces;

namespace GithubForOutlook.Logic
{
    public class AddinBootstrapper : IDisposable
    {
        private readonly IContainer container;

        public AddinBootstrapper()
        {
            var containerBuilder = new ContainerBuilder();

            RegisterComponents(containerBuilder);

            container = containerBuilder.Build();
        }

        private static void RegisterComponents(ContainerBuilder containerBuilder)
        {
            var assembly = typeof (GithubTask).Assembly;

            var settingsService = new SettingsService();

            if (!settingsService.ContainsKey("client"))
            {
                settingsService.Set("client", "9e96382c3109d9f35371");
                settingsService.Set("secret", "60d6c49b946ba4ddc52a34aa0dc1cf43e6077ba6");
                settingsService.Set("redirect", "http://code52.org");
                settingsService.Save();
            }

            containerBuilder.RegisterInstance(settingsService)
                            .AsImplementedInterfaces()
                            .SingleInstance();

            // TODO: initial setup needs to call out to external endpoint to get app credentials
            // TODO: yes, i'm hardcoding it here for now. hatters gonna hat.
            


            containerBuilder.RegisterAssemblyTypes(assembly)
                .Where(t => t.Name.EndsWith("ViewModel"))
                .AsSelf();

            containerBuilder.RegisterType<GitHubOAuthAuthorizer>()
                            .AsImplementedInterfaces();
            containerBuilder.RegisterType<GitHubClient>()
                            .AsImplementedInterfaces();

            containerBuilder.RegisterType<GithubMailItem>()
                .As<IRibbonViewModel>()
                .AsSelf();

            containerBuilder.RegisterType<GithubTask>()
                .As<IRibbonViewModel>()
                .AsSelf();

            containerBuilder.RegisterType<GithubExplorerRibbon>()
                .As<IRibbonViewModel>()
                .AsSelf();
        }

        public object Resolve(Type type)
        {
            return container.Resolve(type);
        }

        public T Resolve<T>()
        {
            return container.Resolve<T>();
        }

        public void Dispose()
        {
            container.Dispose();
        }
    }
}