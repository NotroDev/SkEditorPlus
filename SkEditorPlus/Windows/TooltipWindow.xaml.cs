using SkEditorPlus.Utilities;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace SkEditorPlus.Windows
{
    public partial class TooltipWindow : UserControl
    {
        private readonly FileManager fileManager;
        public enum FixType
        {
            URL,
            FUNCTION
        }
        string fixUrlOrFunction = "";
        readonly FixType fixType;

        public TooltipWindow(string title, string description, string fix = null, string url = null, bool isFix = true, FixType fixType = FixType.URL, FileManager fm = null)
        {
            InitializeComponent();
            titleText.Text = title;
            descText.Text = description;
            fixText.Text = fix;
            fixUrlOrFunction = url;
            this.fixType = fixType;
            if (!isFix)
            {
                Height = 85;
            }
            if (fm != null)
            {
                fileManager = fm;
            }


            BrushConverter bc = new();
            Brush brush = (Brush)bc.ConvertFrom("#21201f");

            string[] textSplit = descText.Text.Split("`");
            if (textSplit.Length != 3) return;
            descText.Inlines.Clear();
            descText.Inlines.Add(new Run(textSplit[0]));
            descText.Inlines.Add(new Run(textSplit[1]) { Background = brush, FontFamily = new FontFamily("Cascadia Mono") });
            descText.Inlines.Add(new Run(textSplit[2]));
        }

        private void OnFixMouseEnter(object sender, MouseEventArgs e)
        {
            fixText.TextDecorations = TextDecorations.Underline;
        }

        private void OnFixMouseLeave(object sender, MouseEventArgs e)
        {
            fixText.TextDecorations = null;
        }

        private void OnFixClick(object sender, MouseButtonEventArgs e)
        {
            if (fixType == FixType.URL)
            {
                if (fixUrlOrFunction != null)
                {
                    try
                    {
                        Process.Start(fixUrlOrFunction);
                    }
                    catch
                    {
                        fixUrlOrFunction = fixUrlOrFunction.Replace("&", "^&");
                        Process.Start(new ProcessStartInfo(fixUrlOrFunction) { UseShellExecute = true });
                    }
                }
            }
            else if (fixType == FixType.FUNCTION)
            {
                Type thisType = GetType();
                MethodInfo theMethod = thisType.GetMethod(fixUrlOrFunction);
                theMethod.Invoke(this, null);
            }
            fileManager.popup.IsOpen = false;
        }

        public void FixDotVariable()
        {
            string code = fileManager.GetTextEditor().Text;

            Regex regex = new("{([^.}]*)\\.([^}]*)}");
            foreach (Match variableMatch in regex.Matches(code).Cast<Match>())
            {
                string variable = variableMatch.Value.Replace(".", "::");
                code = code.Replace(variableMatch.Value, variable);
                fileManager.GetTextEditor().Text = code;
            }
        }

        public void BroadcastToSend()
        {
            string code = fileManager.GetTextEditor().Text;
            Regex regex = new("broadcast \"(.*)\"");
            foreach (Match broadcastMatch in regex.Matches(code).Cast<Match>())
            {
                string broadcast = broadcastMatch.Value.Replace("broadcast ", "");
                code = "send " + code.Replace(broadcastMatch.Value, broadcast) + " to all players";
                fileManager.GetTextEditor().Text = code;
            }
        }
    }
}
