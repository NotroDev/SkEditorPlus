using AvalonEditB;
using AvalonEditB.Highlighting;
using SkEditorPlus.Utilities.Vaults;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SkEditorPlus.Utilities.Managers
{
    public class SyntaxManager
    {
        public static void ChangeSyntax(string syntax)
        {
            TextEditor textEditor = APIVault.GetAPIInstance().GetTextEditor();
            if (textEditor == null) return;

            if (syntax.Equals("none"))
            {
                textEditor.SyntaxHighlighting = null;
                AddonVault.addons.ForEach(addon =>
                {
                    addon.OnSyntaxDisable();
                });
                return;
            }

            try
            {
                var appFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                string highlightingFile;
                if (syntax.Equals("Skript"))
                {
                    string syntaxHighlightingFile = $"{Properties.Settings.Default.SyntaxHighlighting}.xshd";
                    if (File.Exists(Path.Combine(appFolderPath, "SkEditor Plus", "Syntax Highlighting", syntaxHighlightingFile)))
                    {
                        highlightingFile = Path.Combine(appFolderPath, "SkEditor Plus", "Syntax Highlighting", syntaxHighlightingFile);
                    }
                    else
                    {
                        Properties.Settings.Default.SyntaxHighlighting = "Default";
                        highlightingFile = Path.Combine(appFolderPath, "SkEditor Plus", "Syntax Highlighting", "Default.xshd");
                    }
                }

                else
                {
                    highlightingFile = Path.Combine(appFolderPath, "SkEditor Plus", $"{syntax}Highlighting.xshd");
                }


                using var reader = new XmlTextReader(new StreamReader(highlightingFile));
                textEditor.SyntaxHighlighting = AvalonEditB.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);

                if (textEditor.SyntaxHighlighting.GetNamedColor("DefaultColor") is not null)
                    textEditor.Foreground = textEditor.SyntaxHighlighting.GetNamedColor("DefaultColor").Foreground.GetBrush(null);

                if (Properties.Settings.Default.SyntaxHighlighting.Equals("Default"))
                {
                    var span = new HighlightingSpan()
                    {
                        StartExpression = new Regex("\""),
                        EndExpression = new Regex("\""),
                        StartColor = textEditor.SyntaxHighlighting.GetNamedColor("String"),
                        EndColor = textEditor.SyntaxHighlighting.GetNamedColor("String"),
                        RuleSet = textEditor.SyntaxHighlighting.GetNamedRuleSet("ColorsPreview")
                    };
                    textEditor.SyntaxHighlighting.MainRuleSet.Spans.Add(span);
                }
                AddonVault.addons.ForEach(addon =>
                {
                    addon.OnSyntaxChange(syntax);
                });
            }
            catch (Exception e)
            {
                var message = ((string)Application.Current.FindResource("SyntaxError")).Replace("{n}", Environment.NewLine).Replace("{0}", e.Message);
                var error = (string)Application.Current.FindResource("Error");
                MessageBox.Error(message, error);
            }

        }
    }
}
