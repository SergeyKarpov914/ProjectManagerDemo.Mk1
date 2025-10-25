using Acsp.Core.Lib.Util;
using Clio.ProjectManagerModel.ViewModel.Element;
using Clio.ProjectManagerModel.ViewModel.Presentation;
using Telerik.Blazor.Components;

namespace BlazorServerTelerik.Components.Extension
{
    public static class ViewModelEx
    {
        public static async Task OnTreeAdd(this ProjectManagerViewModelBlazor viewModel, TreeListCommandEventArgs args)
        {
            try
            {
                switch (args.Item)
                {
                    case TaskElement taskElement:
                        taskElement.SetRelations(taskElement.Entity, viewModel.SelectedProject, args.ParentItem as TaskElement);
                        Log.Info(typeof(ViewModelEx), $"add [{taskElement}]");
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(ViewModelEx), ex);
            }
            await Task.Delay(0);
        }
        public static async Task OnTreeCreate(this ProjectManagerViewModelBlazor viewModel, TreeListCommandEventArgs args)
        {
            try
            {
                switch (args.Item)
                {
                    case TaskElement taskElement:
                        await viewModel.Update(taskElement.Parent);
                        break;
                    default:
                        break;
                }
                await viewModel.Create(args.Item);
            }
            catch (Exception ex)
            {
                Log.Error(typeof(ViewModelEx), ex);
            }
        }
        public static async Task OnTreeUpdate(this ProjectManagerViewModelBlazor viewModel, TreeListCommandEventArgs args)
        {
            try
            {
                await viewModel.Update(args.Item);
            }
            catch (Exception ex)
            {
                Log.Error(typeof(ViewModelEx), ex);
            }
        }
        public static async Task OnTreeDelete(this ProjectManagerViewModelBlazor viewModel, TreeListCommandEventArgs args)
        {
            try
            {
                await viewModel.Delete(args.Item);
            }
            catch (Exception ex)
            {
                Log.Error(typeof(ViewModelEx), ex);
            }
        }
    }
}
