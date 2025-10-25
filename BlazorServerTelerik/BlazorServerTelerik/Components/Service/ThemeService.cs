using Microsoft.JSInterop;

namespace BlazorServerTelerik.Components.Service
{
    public class ThemeService
    {
        private readonly IJSRuntime _js;

        public ThemeService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task SetThemeAsync(string theme)
        {
            string cssPath = $"_content/Telerik.UI.for.Blazor/css/kendo-theme-{theme}/all.css";
            await _js.InvokeVoidAsync("changeTelerikTheme", cssPath);
        }
    }
}
