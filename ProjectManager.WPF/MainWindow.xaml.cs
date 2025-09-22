using Acsp.Core.Lib.Abstraction;
using Acsp.Core.Lib.Extension;
using Acsp.Core.Lib.Master;
using Clio.ProjectManager.WPF.WinUtil;
using Clio.ProjectManagerModel.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Clio.ProjectManagerDemo.WPF
{
    public sealed class MainWindowDependencies : DependencyMaster
    {
        protected override void cascadeDependencies(IServiceCollection container)
        {
            container.AddTransient<IPresenter, Presenter>();
            typeof(ProjectManagerViewModel).RegisterCascading(container, () => container.AddSingleton<ProjectManagerViewModel>());
        }
    }

    public partial class MainWindow : Window
    {
        public static DependencyMaster CascadeDependencies()
        {
            return new MainWindowDependencies();
        }

        public MainWindow(ProjectManagerViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel.Initialize();
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            navigationDrawer.ToggleDrawer();
        }
    }
}