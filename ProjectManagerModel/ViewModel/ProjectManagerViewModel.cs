using Acsp.Core.Lib.Abstraction;
using Acsp.Core.Lib.Data;
using Acsp.Core.Lib.Extension;
using Acsp.Core.Lib.Gateway;
using Acsp.Core.Lib.Master;
using Acsp.Core.Lib.Util;
using Clio.ProjectManager.DTO;
using Clio.ProjectManagerModel.ViewModel.Element;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Clio.ProjectManagerModel.ViewModel
{
    public sealed class ViewModelDependenciesDesktop : DependencyMaster
    {
        protected override void cascadeDependencies(IServiceCollection container)
        {
            container.AddTransient<ExcelMaster>();
            container.AddTransient<CsvAdapter>();

            typeof(ProjectManagerProcessor).RegisterCascading(container, () => container.AddTransient<ProjectManagerProcessor>());
        }
    }

    public sealed class ViewModelDependencies : DependencyMaster
    {
        protected override void cascadeDependencies(IServiceCollection container)
        {
            typeof(ProjectManagerProcessor).RegisterCascading(container, () => container.AddTransient<ProjectManagerProcessor>());
        }
    }

    public class ProjectManagerViewModel : ObservableObject, IPMStatic
    {
        #region ctor

        protected readonly IPresenter _presenter;
        protected readonly CsvAdapter _csvAdapter;
        protected readonly ExcelMaster _excelMaster;
        
        protected readonly ProjectManagerProcessor _processor;

        public ProjectManagerViewModel(ProjectManagerProcessor processor)
        {
            processor.Inject(out _processor);
        }

        public ProjectManagerViewModel(ProjectManagerProcessor processor, IPresenter presenter, ExcelMaster excelMaster, CsvAdapter csvAdapter)
        {
            processor  .Inject(out _processor);
            excelMaster.Inject(out _excelMaster);
            csvAdapter .Inject(out _csvAdapter);
            presenter  .Inject(out _presenter);
        }

        #endregion ctor

        #region static collections

        public IEnumerable<Client> Clients { get; private set; } = Enumerable.Empty<Client>();
        public IEnumerable<Employee> Employees { get; private set; } = Enumerable.Empty<Employee>();
        public IEnumerable<ProjectType> ProjectTypes { get; private set; } = Enumerable.Empty<ProjectType>();

        protected async Task GetStaticEntities()
        {
            Clients = await _processor.GetClients();
            Employees = await _processor.GetEmployees();
            ProjectTypes = await _processor.GetProjectTypes();
        }

        #endregion static collections

        #region events

        public event PropertyChangedEventHandler DataRefreshed;
        public event PropertyChangedEventHandler ProjectChanged;

        public Project SelectedProject
        {
            get { return _selectedProject; }
            set
            {
                if (value is not null && value.Id != _selectedProject?.Id)
                {
                    _selectedProject = value;
                    ProjectChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedProject)));
                }
            }
        }
        private Project _selectedProject = null;

        public bool HasSelectedProject => SelectedProject is not null && SelectedProject.Id != 0;

        #endregion events

        #region project-task relation

        public async Task<IEnumerable<ProjectElement>> GetProjects()
        {
            List<ProjectElement> elements = new List<ProjectElement>();
            try
            {
                await GetStaticEntities();
                elements.AddRange((await _processor.GetProjects()).Select(x => ProjectElement.Create(x, this)));
            }
            catch (Exception ex)
            {
                Log.Error(this, ex);
            }
            return elements;
        }

        public async Task<IEnumerable<TaskElement>> GetTasks(int projectId = 0)
        {
            List<TaskElement> elements = new List<TaskElement>();
            try
            {
                elements.AddRange((await _processor.GetTasks(resolveProjectId(projectId))).Select(x => TaskElement.Create(x)));
            }
            catch (Exception ex)
            {
                Log.Error(this, ex);
            }
            return elements;
        }

        protected async Task refresh(IEntity entity, CudAction action = CudAction.None) // called after each grid change
        {
            switch (entity)
            {
                case Project _:
                    await GetProjects();
                    break;
                case ProjectTask _:
                    await GetTasks(SelectedProject?.Id ?? 0);
                    break;
            }
            if (entity is not null)
            {
                DataRefreshed?.Invoke(this, new OnPropertyChangeEventArgs<IEntity>(entity.GetType().Name, new DataAction<IEntity>(entity, action)));
            }
        }

        protected IEntity resolveEntity(object item)
        {
            IEntity entity = null;
            switch (item)
            {
                case IElement element:
                    entity = element?.Entity;
                    break;
                case IEntity self:
                    entity = self;
                    break;
                default:
                    break;
            }
            return entity;
        }

        protected int resolveProjectId(int projectId)
        { 
            if(projectId == 0 && SelectedProject is not null)
            {
                projectId = SelectedProject.Id;
            }
            return projectId;
        }

        #endregion project-task relation
    }
}
