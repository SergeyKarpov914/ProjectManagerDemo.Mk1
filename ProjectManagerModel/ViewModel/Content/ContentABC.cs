namespace Clio.ProjectManagerModel.ViewModel.Content
{
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
}
