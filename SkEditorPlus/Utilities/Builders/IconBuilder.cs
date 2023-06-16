using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace SkEditorPlus.Utilities.Builders
{
    public class IconBuilder
    {
        public static TextBlock Build(string iconString, string text)
        {
            TextBlock tempTextBlock = new();
            var icon = new TextBlock()
            {
                Text = iconString,
                FontFamily = new FontFamily("Segoe Fluent Icons"),
                Margin = new Thickness(0, 2, 0, 0),
            };
            icon.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.HighQuality);
            tempTextBlock.Inlines.Add(icon);
            tempTextBlock.Inlines.Add(" " + text);

            tempTextBlock.VerticalAlignment = VerticalAlignment.Center;
            return tempTextBlock;
        }
    }
}
