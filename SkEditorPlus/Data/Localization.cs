using System.Windows;

namespace SkEditorPlus.Data
{
    // It is class that should override old one
    public class Localization : AvalonEditB.Search.Localization
    {
        public override string NoMatchesFoundText
        {
            get { return (string)Application.Current.FindResource("NoResults"); }
        }
    }
}
