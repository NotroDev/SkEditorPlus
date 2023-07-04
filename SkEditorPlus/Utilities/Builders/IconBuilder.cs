using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SkEditorPlus.Utilities.Builders
{
    public class IconBuilder
    {
        private static FontFamily segoeFluentFont = new(new Uri("pack://application:,,,/"), "./Fonts/#Segoe Fluent Icons");
        public static TextBlock Build(string iconString, string text)
        {
            TextBlock tempTextBlock = new();
            var icon = new TextBlock()
            {
                Text = iconString,
                FontFamily = segoeFluentFont,
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
