using Telerik.SvgIcons;

namespace BlazorServerTelerik.Components.Service
{
    public sealed class DrawerItem
    {
        public string   Text      { get; set; }
        public ISvgIcon Icon      { get; set; }
        public string   Url       { get; set; }
        public bool     IsSeparator { get; set; }
    }

    public sealed class ThemeModel
    {
        public int    Id      { get; set; }
        public string Theme   { get; set; }
        public string Swatch  { get; set; }
        public string FullName => $"{Theme} {Swatch}";

        public ThemeModel(int id, string themeName, string swatchName)
        {
            Id     = id;
            Theme  = themeName;
            Swatch = swatchName;
        }
    }
}
