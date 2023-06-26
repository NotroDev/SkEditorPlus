using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace SkEditorPlus.Utilities.Builders
{
    public class MinecraftTextBuilder
    {
        public static TextBlock Build(string input, int fontSize = 12, bool defaultColor = false)
        {
            try
            {
                input = (defaultColor ? "" : "&f") + input;
                string colorSymbol = "&";

                Dictionary<char, string> colorMap = new()
                {
                    {'0', "#000000"},
                    {'1', "#0000AA"},
                    {'2', "#00AA00"},
                    {'3', "#00AAAA"},
                    {'4', "#AA0000"},
                    {'5', "#AA00AA"},
                    {'6', "#FFAA00"},
                    {'7', "#AAAAAA"},
                    {'8', "#555555"},
                    {'9', "#5555FF"},
                    {'a', "#55FF55"},
                    {'b', "#55FFFF"},
                    {'c', "#FF5555"},
                    {'d', "#FF55FF"},
                    {'e', "#FFFF55"},
                    {'f', "#FFFFFF"},
                };

                StringBuilder xaml = new();
                xaml.Append("<TextBlock xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>");

                string[] segments = input.Split(colorSymbol[0]);

                string lastColor = defaultColor ? "#3F3F3F" : "#AAAAAA";

                foreach (string segment in segments)
                {
                    if (string.IsNullOrEmpty(segment))
                        continue;
                    char formatCode = segment[0];

                    string formattedSegment;
                    if (colorMap.ContainsKey(formatCode))
                    {
                        string colorHexCode = colorMap[formatCode];
                        if (colorHexCode.Length == 6)
                        {
                            colorHexCode = "#" + colorHexCode;
                        }
                        formattedSegment = $"<Run Text='{segment[1..]}' Foreground='{colorHexCode}' />";
                        lastColor = colorHexCode;
                    }

                    else
                    {
                        switch (formatCode)
                        {
                            case 'r':
                                formattedSegment = segment[1..];
                                lastColor = defaultColor ? "#3F3F3F" : "#AAAAAA";
                                break;
                            case 'l':
                                formattedSegment = $"<Run Text='{segment[1..]}' Foreground='{lastColor}' FontWeight='Bold' />";
                                break;
                            default:
                                formattedSegment = $"<Run Text='{segment}' Foreground='#3F3F3F' />";
                                break;
                        }
                    }

                    xaml.Append(formattedSegment);
                }

                xaml.Append("</TextBlock>");

                string xamlString = xaml.ToString();
                TextBlock textBlock = XamlReader.Parse(xamlString) as TextBlock;

                textBlock.Foreground = MainWindow.GetSolidColorBrush("#3F3F3F");

                textBlock.FontFamily = new FontFamily("Minecraft font 1.13");
                textBlock.FontSize = fontSize;
                textBlock.SetValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Display);
                textBlock.SetValue(TextOptions.TextRenderingModeProperty, TextRenderingMode.Aliased);
                textBlock.SetValue(TextOptions.TextHintingModeProperty, TextHintingMode.Fixed);

                return textBlock;
            }
            catch
            {
                return new TextBlock()
                {
                    Text = input
                };
            }
        }
    }
}
