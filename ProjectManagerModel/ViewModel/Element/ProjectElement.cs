using Acsp.Core.Lib.Abstraction;
using Clio.ProjectManager.DTO;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;

namespace Clio.ProjectManagerModel.ViewModel.Element
{
    public sealed class ProjectElement : ObservableObject, IElement
    {
        public IEntity Entity { get; private set; }

        public ProjectElement()
        {
            Entity = new Project();
        }

        public IElement SetRelations(IEntity entity, IEntity project, IElement parent = null)
        {
            Entity = entity;
            return this;
        }

        public static ProjectElement Create(Project project, IPMStatic vm)
        {
            ProjectElement element = new ProjectElement().SetRelations(project, project) as ProjectElement;

            #region set foreign key properties

            IEntity subEntity = null;

            if (null != (subEntity = vm.Clients.FirstOrDefault(x => x.Id == project.ClientId)))
            {
                element.Client = subEntity.Name;
            }
            if (null != (subEntity = vm.Employees.FirstOrDefault(x => x.Id == project.EmployeeId)))
            { 
                element.Employee = subEntity.Name;                
            }
            if (null != (subEntity = vm.ProjectTypes.FirstOrDefault(x => x.Id == project.ProjectTypeId)))
            {
                element.ProjectType = subEntity.Name;
            }
            element.AccountingName = project.AccountingName;

            #endregion set foreign key properties

            return element;
        }

        public int Id => Entity.Id;

        public string Name           { get { return Entity.Name; } set { Entity.Name = value; OnPropertyChanged(nameof(Name)); } }
        public string Code           { get { return Entity.Code; } set { Entity.Code = value; OnPropertyChanged(nameof(Code)); } }
        
        // Foreign key properties can (and should) be set using static lookup collections and combo boxes integrated into grid columns.
        // That technique omitted here because the code to implement that is too complicated for the demo mode 
        public string Client         { get; set; }
        public string ProjectType    { get; set; }
        public string Employee       { get; set; }
        public string AccountingName { get; set; }
    }
}
