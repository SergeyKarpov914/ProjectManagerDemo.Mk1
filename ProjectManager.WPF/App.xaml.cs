using Acsp.Core.Lib.Data;
using Acsp.Core.Lib.Extension;
using Acsp.Core.Lib.Master;
using Acsp.Core.Lib.Util;
using Acsp.Core8.Asp.Net;
using Clio.ProjectManager.DTO;
using Clio.ProjectManagerModel.ViewModel;
using Clio.ProjectManagerModel.ViewModel.Presentation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Syncfusion.Licensing;
using System.IO;
using System.Windows;


namespace Clio.ProjectManagerDemo.WPF
{
    public partial class App : Application
    {
        #region fields
        private IHost _host;
        private IServiceCollection _container;
        private IConfiguration _configuration;
        #endregion fields

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                        .ConfigureAppConfiguration((context, config) =>
                        {
                            config.SetBasePath(Directory.GetCurrentDirectory());
                            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                            config.AddUserSecrets<MainWindow>();
                        })
                        .ConfigureServices((context, services) =>
                        {
                            #region configure custom dependencies

                            services.Configure<Connection>(context.Configuration.GetSection("SqlConnection"));
                            typeof(MainWindow).RegisterCascading(services, () => services.AddSingleton<MainWindow>());

                            #endregion configure custom dependencies

                            #region activate license(s)
                            _container = services;
                            _configuration = context.Configuration;

                            SyncfusionLicenseProvider.RegisterLicense(_configuration.GetValue<string>("Syncfusion"));
                            #endregion activate license(s)
                        })
                        .Build();
        }

        protected override async void OnStartup(StartupEventArgs e) // to override OnStartup remove StartupUri="MainWindow.xaml" from app.xaml
        {
            await _host!.StartAsync();

            MainWindow mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            #region startup log
            LogUtil.RuntimeInfo(typeof(App), $"WPF Demo Start", Env.Staging, null, new Type[]
            {
                typeof(EntityMaster),// CoreLib
                typeof(AspNetCore),  // Core8
                typeof(Project),     //
                typeof(ProjectManagerViewModel),
            });
            AspNetCore.ProcessDIContainer(typeof(ProjectManagerViewModelWpf), _configuration.GetValue<string>("prefix"), _container);
            #endregion startup log

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host!.StopAsync(TimeSpan.FromSeconds(5));
            
            _host!.Dispose();
            base.OnExit(e);
        }
    }
}
