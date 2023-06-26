using AvalonEditB.Document;
using AvalonEditB.Rendering;
using AvalonEditB;
using Newtonsoft.Json.Linq;
using SkEditorPlus;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.IO;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using SkEditorPlus.Utilities.Vaults;
using MessageBox = HandyControl.Controls.MessageBox;
using HandyControl.Tools.Extension;

namespace SkEditorPlus.Utilities
{
    class CodeParsingUtility
    {
        public static SkEditorAPI skEditor = APIVault.GetAPIInstance();

        private static List<MouseEventHandler> eventListeners = new();

        public static async Task ParseCode()
        {
            try
            {
                string encodedCode = skEditor.GetTextEditor().Document.Text;

                if (string.IsNullOrEmpty(encodedCode))
                {
                    skEditor.ShowError("There is no code to parse.", "Analysis Error");
                    return;
                }

                MessageBox.Info("The code will be analyzed now. It can take some time.", "Analyzer");

                using var httpClient = new HttpClient();

                string apiKey = "apikey";

                var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.skunity.com/v1/{apiKey}/parser/parse?content={encodedCode}");

                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                RemoveLineBackgroundTransformers();
                RemovePreviousHoverEvents();

                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    try
                    {
                        JObject jsonResponse = JObject.Parse(responseContent);
                        JObject result = jsonResponse["result"] as JObject;
                        StringBuilder analysisStringBuilder = new();

                        analysisStringBuilder.Append("Analysis complete.");
                        analysisStringBuilder.Append("\n\n");

                        if (result["errors"] is JObject errorsObject)
                        {
                            string errors = ProcessErrors(errorsObject);
                            if (errors.Length > 0)
                            {
                                analysisStringBuilder.Append("Errors found:");
                                analysisStringBuilder.Append('\n');
                                analysisStringBuilder.Append(errors);
                                analysisStringBuilder.Append('\n');
                                analysisStringBuilder.Append("Run analysis again to refresh the errors.");
                            }
                        }
                        else
                        {
                            analysisStringBuilder.Append("No errors found.");
                        }
                        List<string> addons = result["addons"].ToObject<List<string>>();
                        if (addons.Count > 0)
                        {
                            StringBuilder addonsStringBuilder = new("Required addons: ");
                            foreach (string addon in addons)
                            {
                                addonsStringBuilder.Append(addon + ", ");
                            }
                            addonsStringBuilder.Remove(addonsStringBuilder.Length - 2, 2);
                            analysisStringBuilder.Append('\n');
                            analysisStringBuilder.Append(addonsStringBuilder);
                        }

                        MessageBox.Info(analysisStringBuilder.ToString(), "Analysis Results");
                    }
                    catch (Exception e)
                    {
                        MessageBox.Error("There was a problem when parsing your script.\n\nCheck if you have internet and if parser.skunity.com works for you.\n\n" + e.Message + "\n" + e.StackTrace, "Analysis Error");
                    }
                }
                else
                {
                    // failed, probably rate limit
                    MessageBox.Error($"There was a problem when parsing your script.\nYou probably got rate-limited.\n\n{response.StatusCode}", "Analysis error");
                }
            }
            catch (Exception e)
            {
                MessageBox.Error(e.Message + "\n\n" + e.StackTrace, "Analysis Error");
            }
        }

        public static void RemoveLineBackgroundTransformers()
        {
            var lineTransformers = skEditor.GetTextEditor().TextArea.TextView.LineTransformers;
            for (int i = lineTransformers.Count - 1; i >= 0; i--)
            {
                if (lineTransformers[i] is LineBackgroundTransformer)
                {
                    lineTransformers.RemoveAt(i);
                }
            }
        }

        public static void RemovePreviousHoverEvents()
        {
            foreach (var listener in eventListeners)
            {
                skEditor.GetTextEditor().PreviewMouseHover -= listener;
            }
            eventListeners.Clear();
        }

        private static string ProcessErrors(JObject errorsObject)
        {
            var errors = errorsObject.Properties()
            .SelectMany(p => p.Value)
            .Select(v => v.ToString())
            .ToArray();

            StringBuilder errorsStringBuilder = new();

            var brush = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));

            int id = 1;
            foreach (JProperty errorProperty in errorsObject.Properties())
            {
                string lineNumber = errorProperty.Name;
                string errorMessage = errorProperty.Value[0].ToString();
                string formattedError = errorMessage.Replace("(The Parser doesn't support every syntax element.)", "").Replace("event event", "event");
                string pattern = @"Apply a \[skUP:StartError.*?\s";
                string pattern2 = @"\d+,\s*\d+\[skUP:EndError\]";
                formattedError = Regex.Replace(formattedError, pattern, "");
                formattedError = Regex.Replace(formattedError, pattern2, "");

                errorsStringBuilder.Append($"#{id}: {formattedError}\n\n");
                id++;

                AddLineBackgroundTransformer(int.Parse(lineNumber), brush);
                AttachToolTipToEditor(formattedError, int.Parse(lineNumber));
            }

            return errorsStringBuilder.ToString();
        }

        private static void AddLineBackgroundTransformer(int lineNumber, SolidColorBrush brush)
        {
            var lineBackgroundTransformer = new LineBackgroundTransformer()
            {
                LineNumber = lineNumber,
                UnderlineBrush = brush
            };


            skEditor.GetTextEditor().TextArea.TextView.LineTransformers.Add(lineBackgroundTransformer);

        }

        private static void AttachToolTipToEditor(string errorWithLineNumber, int lineNumber)
        {
            TextEditor editor = skEditor.GetTextEditor();
            DocumentLine documentLine = editor.Document.GetLineByNumber(lineNumber);
            VisualLine line = editor.TextArea.TextView.GetOrConstructVisualLine(documentLine);


            System.Windows.Controls.ToolTip toolTip = new()
            {
                PlacementTarget = editor,
                Content = errorWithLineNumber
            };

            void mouseHoverHandler(object sender, MouseEventArgs e)
            {
                try
                {
                    var pos = editor.GetPositionFromPoint(e.GetPosition(editor));
                    if (pos != null && pos.Value.Location.Line == documentLine.LineNumber)
                    {
                        toolTip.IsOpen = true;
                    }
                }
                catch { }
            }

            editor.PreviewMouseHover += mouseHoverHandler;

            eventListeners.Add(mouseHoverHandler);

            editor.MouseHoverStopped += (sender, e) =>
            {
                toolTip.IsOpen = false;
            };

        }
    }

    public class LineBackgroundTransformer : DocumentColorizingTransformer
    {
        public int LineNumber { get; set; }
        public Brush UnderlineBrush { get; set; }

        protected override void ColorizeLine(DocumentLine line)
        {
            if (line.LineNumber == LineNumber)
            {
                string text = CurrentContext.Document.GetText(line);
                int lineOffset = line.Offset + text.TakeWhile(c => c == '\t' || c == ' ').Count();
                ChangeLinePart(lineOffset, line.EndOffset, ApplyChanges);


            }
        }

        private void ApplyChanges(VisualLineElement element)
        {
            element.TextRunProperties.SetTextDecorations(new TextDecorationCollection(new[]
            {
            new TextDecoration
            {
                Pen = new Pen(UnderlineBrush, 0.1),
                Location = TextDecorationLocation.Underline,
                PenThicknessUnit = TextDecorationUnit.FontRenderingEmSize,
            }
        }));
        }
    }
}
