using Acsp.Core.Lib.Abstraction;
using Acsp.Core.Lib.Extension;
using Acsp.Core.Lib.Master;
using Clio.ProjectManager.DTO;
using Clio.ProjectManagerModel.ViewModel.Content;
using Clio.ProjectManagerModel.ViewModel.Element;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Clio.ProjectManagerModel.ViewModel
{
    #region content (datatemplate) types

    public enum ContentType { Project, Task, Static, None }

    public abstract class PresentationContent              // vm composition is used instead of inheritance, 
    {                                                      // because vm is injected as singleton and itself depends on constructor injection
        protected readonly IPMViewModel _viewModel;

        protected PresentationContent(IPMViewModel viewModel)
        {
            _viewModel = viewModel;
        }
        protected ContentType ContentType { get; set; } = ContentType.None;
    }

    #endregion content (datatemplate) types

    public partial class ProjectManagerViewModel : ObservableObject, IPMViewModel
    {
        #region fields

        private ProjectContent _projectContent = null;
        private StaticContent _staticContent = null;

        #endregion fields

        public ProjectManagerViewModel Initialize()
        {
            ContentSwitchCommand = MvvmMaster.CreateAsyncCommand<string>(ResolveContent);
            ProjectSelectCommand = MvvmMaster.CreateAsyncCommand<object>(OnProjectSelect);
            OpenExcelFileCommand = MvvmMaster.CreateCommand<string>     (OpenExcelFile);
            OpenCsvFileCommand   = MvvmMaster.CreateCommand<string>     (OpenCsvFile);
            AddTaskCommand       = MvvmMaster.CreateCommand<object>     (OnAddTask);
            AddSubTaskCommand    = MvvmMaster.CreateCommand<object>     (OnAddSubTask);
            SaveCommand          = MvvmMaster.CreateAsyncCommand<object>(OnSave);
            DeleteCommand        = MvvmMaster.CreateAsyncCommand<object>(OnDelete);

            _projectContent = new ProjectContent(this);
            _staticContent  = new StaticContent(this);

            return this;
        }

        #region props 

        public object Content
        {
            get { return _content; }
            private set { _content = value; this.OnPropertyChanged(nameof(Content)); }
        }
        private object _content;

        #endregion props 

        #region observable collections

        public ObservableCollection<ProjectElement> ProjectElements { get; private set; } = new ObservableCollection<ProjectElement>();
        public ObservableCollection<TaskElement> TaskElements { get; private set; } = new ObservableCollection<TaskElement>();

        private async Task GetProjects()
        {
            await GetStaticEntities();
            IEnumerable<Project> projects = await _processor.GetProjects();

            ProjectElements.Clear();

            foreach (Project project in projects)
            {
                ProjectElements.Add(ProjectElement.Create(project, this));
            }
        }

        #endregion observable collections

        #region commands

        public ICommand ContentSwitchCommand { get; private set; }
        public ICommand OpenExcelFileCommand { get; private set; }
        public ICommand OpenCsvFileCommand   { get; private set; }

        public ICommand ProjectSelectCommand { get; private set; }
        public ICommand AddTaskCommand       { get; private set; }
        public ICommand AddSubTaskCommand    { get; private set; }
        public ICommand SaveCommand          { get; private set; }
        public ICommand DeleteCommand        { get; private set; }

        private async Task ResolveContent(string name)
        {
            ContentType content = name.ToEnum<ContentType>(ContentType.None);

            switch (content)
            {
                case ContentType.Project:
                    await GetProjects();
                    Content = _projectContent;
                    break;
                case ContentType.Static:
                    await GetStaticEntities();
                    Content = _staticContent;
                    break;
                default:
                    throw new System.Exception($"Cannot resolve content '{name}'");
            }
        }

        private async void OpenExcelFile(string name)
        {
            if (null != (name = _presenter.SelectFile()))
            {
                IEnumerable<Project> existing = await _processor.GetProjects();
                IEnumerable<Project> imported = await _excelMaster.ImportFromExcel<Project>(name);
                IEnumerable<Project> unsaved = ProjectElements.Select(x => x.Entity as Project);

                List<Project> clean = imported.Distinct(x => x.Code)
                                              .NotInTarget(existing, x => x.Code)
                                              .NotInTarget(unsaved, x => x.Code)
                                              .ToList();

                foreach (Project project in clean)
                {
                    ProjectElements.Add(ProjectElement.Create(project, this));
                }
                _presenter.ShowNotification($"{clean.Count()} of {imported.Count()} Excel project records imported");
            }
        }

        private async Task OnSave(object element)
        {
            ProjectElement projectElement = element as ProjectElement ?? _projectContent.SelectedItem;
            try
            {
                await PersistTasks(projectElement.Entity as Project);

                string action = await PersistEntity(projectElement.Entity);

                _presenter.ShowNotification($"{action} project '{projectElement.Name}'");
            }
            catch (Exception ex)
            {
                _presenter.ShowNotification($"Error create/update '{projectElement.Name}' ({ex.Message})", Notification.Error);
            }
            finally
            {
                Project project = await _processor.GetProject($" WHERE [Code]='{projectElement.Code}'");

                int index = ProjectElements.IndexOf(projectElement);

                ProjectElements.RemoveAt(index);
                ProjectElements.Insert(index, ProjectElement.Create(project, this));
            }
        }

        private async Task<string> PersistEntity(IEntity entity)
        {
            if (entity.Id <= 0)
            {
                await _processor.Create(entity);
            }
            else
            {
                await _processor.Update(entity);
            }
            return entity.Id <= 0 ? "Create" : "Update";
        }

        private async Task PersistTasks(IEntity project)
        {
            IEnumerable<TaskElement> tasks = TaskElements.Where(x => x.ProjectId == project.Id);

            foreach (TaskElement task in tasks)
            {
                foreach (TaskElement subTask in task.SubTasks)
                {
                    await PersistEntity(subTask.Entity);
                }
                await PersistEntity(task.Entity);
            }
        }

        private async Task OnDelete(object element)
        {
            ProjectElement project = element as ProjectElement ?? _projectContent.SelectedItem;
            bool isDeleted = false;
            try
            {
                if (project.Entity.Id <= 0)
                {
                    throw new InvalidOperationException($"Cannot delete '{project.Name}', there is no database Id");
                }
                if (_presenter.Confirmation($"Project '{project.Name}' is about to be deleted", "Project Delete"))
                {
                    await _processor.Delete(project.Entity);
                    isDeleted = true;
                }
            }
            catch (Exception ex)
            {
                _presenter.ShowNotification($"Error deleting '{project.Name}' ({ex.Message})", Notification.Error);
            }
            finally
            {
                if (isDeleted)
                {
                    ProjectElements.Remove(project);
                }
            }
        }

        private void OnAddTask(object element)
        {
            ProjectElement project = element as ProjectElement ?? _projectContent.SelectedItem;

            if (project.Id <= 0)
            {
                _presenter.ShowNotification($"Cannot add task to an unsaved project", Notification.Error);
                return;
            }
            ProjectTask task = new ProjectTask().SetCode("T:", 4);

            task.Name = $"Task for {project.Code}";
            task.ProjectId = project.Id;
            task.StartDate = DateTime.Now;
            task.Duration = NumericEx.RandomInRange(1, 5);

            TaskElements.Add(TaskElement.Create(task));
        }

        private void OnAddSubTask(object element)
        {
            TaskElement parentTask = element as TaskElement ?? _projectContent.SelectedTask;

            ProjectTask task = new ProjectTask().SetCode("SUB-T:", 4);

            task.Name = $"Sub-Task for {parentTask.Code}";
            task.ProjectId = parentTask.ProjectId;
            task.StartDate = DateTime.Now.AddDays(parentTask.Duration);
            task.Duration = parentTask.Duration;

            task.ParentTaskId = parentTask.Id;
            task.ParentCode = parentTask.Code;

            parentTask.SubTasks.Add(TaskElement.Create(task));
        }

        private void OpenCsvFile(string name)
        {
            if (null != (name = _presenter.SelectFile()))
            {
                IEnumerable<Project> projects = _csvAdapter.Parse<Project>(name);
            }
        }

        public async Task OnProjectSelect(object selected)
        {
            ProjectElement project = selected as ProjectElement;

            if (project is not null)
            {
                IEnumerable<ProjectTask> tasks = await _processor.GetTasks(project.Id);

                TaskElements.Clear();

                foreach (ProjectTask task in tasks)
                {
                    if (task.ParentCode is null)
                    {
                        TaskElements.Add(TaskElement.Create(task));
                    }
                }
            }
        }

        #endregion commands
    }
}
