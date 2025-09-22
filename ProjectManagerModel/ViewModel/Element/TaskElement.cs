using Acsp.Core.Lib.Abstraction;
using Acsp.Core.Lib.Pattern;
using Clio.ProjectManager.DTO;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Linq;

namespace Clio.ProjectManagerModel.ViewModel.Element
{
    public sealed class TaskElement : ObservableObject, IElement
    {
        public IEntity Entity { get; private set; }

        public TaskElement()
        {
            Entity = new ProjectTask();
        }
        
        public IElement SetRelations(IEntity owner = null)
        {
            Entity = owner;

            if (owner is ProjectTask task)
            {
                SubTasks.AddRange(task.SubTasks.Select(x => TaskElement.Create(x)));
            }
            return this;
        }

        public static TaskElement Create(ProjectTask task)
        {
            TaskElement element = new TaskElement().SetRelations(task) as TaskElement;
            return element;
        }

        private ProjectTask _task => Entity as ProjectTask;

        public int    Id           => _task.Id;
        public int    ProjectId    => _task.ProjectId;
        public int    ParentTaskId => _task.ParentTaskId;
        public string ParentCode   => _task.ParentCode;
        public decimal Percent     => _task.Percent;

        public string   Name         { get { return _task.Name; }       set { _task.Name = value; OnPropertyChanged(nameof(Name)); } }
        public string   Code         { get { return _task.Code; }       set { _task.Code = value; OnPropertyChanged(nameof(Code)); } }
        public DateTime StartDate    { get { return _task.StartDate; }  set { _task.StartDate = value; OnPropertyChanged(nameof(StartDate)); } }
        public int      Duration     { get { return _task.Duration; }   set { _task.Duration = value; OnPropertyChanged(nameof(Duration)); } }

        public string   Days => $"{Duration} Days";

        public SafeObservableCollection<TaskElement> SubTasks { get; set; } = new SafeObservableCollection<TaskElement>();
    }
}
