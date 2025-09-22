using Clio.ProjectManager.DTO;
using Clio.ProjectManagerModel.ViewModel.Element;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Clio.ProjectManagerModel.ViewModel
{
    public interface IPMViewModel
    {
        ObservableCollection<ProjectElement> ProjectElements { get; }
        ObservableCollection<TaskElement> TaskElements { get; }

        IEnumerable<Client> Clients { get; }
        IEnumerable<Employee> Employees { get; }
        IEnumerable<ProjectType> ProjectTypes { get; }

        ICommand OpenExcelFileCommand { get; }
        ICommand OpenCsvFileCommand   { get; }
        ICommand SaveCommand { get; }
        ICommand DeleteCommand { get; }

        ICommand AddTaskCommand { get; }
        ICommand AddSubTaskCommand { get; }

        ICommand ProjectSelectCommand { get; }
    }
}
