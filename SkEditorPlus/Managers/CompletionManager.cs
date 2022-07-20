using HandyControl.Controls;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using Newtonsoft.Json.Linq;
using SkEditorPlus.Data;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Window = System.Windows.Window;

namespace SkEditorPlus.Managers
{
    public class CompletionManager
    {
        static CompletionWindow completionWindow;

        static string apiKey = "658df780ec1e9b05";
        static string apiLink = $"https://docs.skunity.com/api/?key={apiKey}&function=getDocTypeSyntax&doctype=event";

        static TabItem tabItem = (TabItem)GetMainWindow().tabControl.SelectedItem;
        static TextEditor textEditor = tabItem.Content as TextEditor;

        public static void LoadJson()
        {
            WebClient client = new WebClient();
            client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");

            string json = client.DownloadString("https://api.skripttools.net/v4/documentation");

            dynamic data = JObject.Parse(json);
            string author = "[i332] manage (lol|112|wewxXD) xD [ewsjwe] lol [wceceaewrc] lol2";

            var regex = new Regex(@"^\[(.+?)\].+\]\s(.+?)\s\(.+\|.+\|(.+)\)");

            var m = regex.Match(author);

            if (m.Success)
            {
                textEditor.Text = m.Result("$1 $2 $3");
            }
        }

        public static void LoadCompletionManager()
        {
            textEditor.TextArea.TextEntering += TextEditor_TextArea_TextEntering;
            textEditor.TextArea.TextEntered += TextEditor_TextArea_TextEntered;
        }

        public static MainWindow GetMainWindow()
        {
            List<Window> windowList = new List<Window>();
            foreach (Window window in App.Current.Windows)
                windowList.Add(window);
            return (MainWindow)windowList.Find(window => window.GetType() == typeof(MainWindow));
        }

        static void TextEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            completionWindow = new CompletionWindow(textEditor.TextArea);
            completionWindow.StartOffset = 0;
            IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;

            data.Add(new CompletionData("Item1", "lol"));
            data.Add(new CompletionData("Item2"));
            data.Add(new CompletionData("Item3"));
            completionWindow.Show();
            completionWindow.Closed += delegate
            {
                completionWindow = null;
            };
        }

        static void TextEditor_TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }
    }
}