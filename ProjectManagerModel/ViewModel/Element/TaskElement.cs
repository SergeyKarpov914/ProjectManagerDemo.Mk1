using Acsp.Core.Lib.Abstraction;
using Acsp.Core.Lib.Extension;
using Acsp.Core.Lib.Pattern;
using Clio.ProjectManager.DTO;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Clio.ProjectManagerModel.ViewModel.Element
{
    public sealed class TaskElement : ObservableObject, IElement
    {
        #region element infrastructure

        public IEntity Entity { get; private set; }

        public TaskElement()
        {
            Entity = new ProjectTask(); //.SetCode("T:", 4);
        }
        public IElement SetRelations(IEntity entity, IEntity project, IElement parent = null)    
        {
            if (entity is ProjectTask task)
            {
                string code = $"Top";
                string name = $"Top task ({project.Name}) ";

                _task.StartDate = DateTime.Now;
                _task.EndDate   = DateTime.Now;
                _task.ProjectId = project.Id;

                if (parent is TaskElement parentTask)
                {
                    IEnumerable<TaskElement> subTasks = (parentTask.Entity as ProjectTask).SubTasks.Select(x => TaskElement.Create(x));

                    parentTask.SubTasks.AddRange(subTasks);
                    parentTask.GanttSubTasks.AddRange(subTasks);

                    Parent = parentTask;

                    ParentTaskId = parentTask.Id;
                    _task.ParentCode = parentTask.Code;

                    code = $"Sub-task-{subTasks.Count() + 1}";
                    name = $"Sub-task-{subTasks.Count() + 1}";

                    Parent.End = parentTask.SubTasks.Max(x => x.End);
                    Parent.PercentComplete = parentTask.SubTasks.Sum(x => x.PercentComplete) / parentTask.SubTasks.Count();
                }
                Entity = entity.SetCode(code, 4);
                Name = name;
            }
            return this;
        }

        public IElement SetRelations(IEntity entity)
        {
            Entity = entity;

            if (entity is ProjectTask task)
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

        public int TaskId { get; set; } // in case Id needs to be restored 

        #endregion element infrastructure

        public int    Id   { get { return _task.Id; } set { } }
        public string Name { get { return _task.Name; } set { _task.Name = value; OnPropertyChanged(nameof(Name)); } }
        public string Code { get { return _task.Code; } set { if (_task.Code.IsEmpty()) { _task.Code = value; OnPropertyChanged(nameof(Code)); } } }

        public int?   ParentTaskId { get { return _task.ParentTaskId == 0 ? null : _task.ParentTaskId; } set { _task.ParentTaskId = value.HasValue ? value.Value : 0; } }
        
        public int    ProjectId  => _task.ProjectId;
        public string ParentCode => _task.ParentCode;
        public bool   IsParent   => ParentCode.IsEmpty();

        public TaskElement       Parent { get; set; }
        public List<TaskElement> GanttSubTasks  { get; set; } = new List<TaskElement>();

        public DateTime Start { get { return _task.StartDate; }  set { _task.StartDate = value; OnPropertyChanged(nameof(Start)); } }
        public DateTime End   { get { return _task.EndDate; }    set { _task.EndDate = value; OnPropertyChanged(nameof(End)); } }
        public double PercentComplete { get { return _task.PercentComplete; } set { _task.PercentComplete = value; OnPropertyChanged(nameof(PercentComplete)); } }

        public SafeObservableCollection<TaskElement> SubTasks { get; set; } = new SafeObservableCollection<TaskElement>();
    }
}
