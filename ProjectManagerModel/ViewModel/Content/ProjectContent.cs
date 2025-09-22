using Clio.ProjectManagerModel.ViewModel.Element;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Clio.ProjectManagerModel.ViewModel.Content
{
    public sealed class ProjectContent : PresentationContent
    {
        public ProjectContent(IPMViewModel viewModel) : base(viewModel)
        {
            ContentType = ContentType.Project;
            ViewModel = viewModel;
        }
        public ObservableCollection<ProjectElement> ProjectElements => _viewModel.ProjectElements;
        public ObservableCollection<TaskElement>   TaskElements     => _viewModel.TaskElements;

        public ICommand OpenExcelFileCommand => _viewModel.OpenExcelFileCommand;
        public ICommand OpenCsvFileCommand   => _viewModel.OpenCsvFileCommand;

        #region menu commands
        public static IPMViewModel ViewModel { get; set; }
        
        public static ICommand Save       => ViewModel.SaveCommand;
        public static ICommand Delete     => ViewModel.DeleteCommand;
        public static ICommand AddTask    => ViewModel.AddTaskCommand;
        public static ICommand AddSubTask => ViewModel.AddSubTaskCommand;
        #endregion menu commands

        public ProjectElement SelectedItem 
        {
            get { return _selected; }
            set { _selected = value; (ViewModel.ProjectSelectCommand as IAsyncRelayCommand).ExecuteAsync(_selected); } 
        }
        public ProjectElement _selected;

        public TaskElement SelectedTask { get; set; }
    }
}
