using Acsp.Core.Lib.Abstraction;
using Acsp.Core.Lib.Data;
using Acsp.Core.Lib.Master;
using Acsp.Core.Lib.Util;
using Clio.ProjectManager.DTO;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Clio.ProjectManagerModel.ViewModel.Presentation
{
    public class ProjectManagerViewModelBlazor : ProjectManagerViewModel
    {
        public static DependencyMaster CascadeDependencies()
        {
            return new ViewModelDependencies();
        }

        #region ctor

        public ProjectManagerViewModelBlazor(ProjectManagerProcessor processor) : base(processor)
        {
        }

        #endregion ctor

        #region crud

        public async Task Create(object item)
        {
            IEntity entity = resolveEntity(item);
            try
            {
                await _processor.Create(entity);
                await refresh(entity, CudAction.Create);
            }
            catch (Exception ex)
            {
                Log.Error(this, ex);
            }
        }

        public async Task Update(object item)
        {
            try
            {
                if (item is not null)
                {
                    IEntity entity = resolveEntity(item);
                
                    await _processor.Update(entity);
                    await refresh(entity, CudAction.Update);
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, ex);
            }
        }

        public async Task Delete(object item)
        {
            IEntity entity = resolveEntity(item);
            try
            {
                await _processor.Delete(entity);
                await refresh(entity, CudAction.Delete);
            }
            catch (Exception ex)
            {
                Log.Error(this, ex);
            }
        }

        #endregion crud
    }
}
