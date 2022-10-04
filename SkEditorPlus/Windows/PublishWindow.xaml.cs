using HandyControl.Controls;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.Json;

namespace SkEditorPlus.Windows
{
    public partial class PublishWindow : HandyControl.Controls.Window
    {
        private SkEditorAPI skEditor;

        public PublishWindow(SkEditorAPI skEditor)
        {
            InitializeComponent();
            this.skEditor = skEditor;
            apiTextBox.Text = Properties.Settings.Default.ApiKey;
        }

        private void PublishClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(apiTextBox.Text))
            {
                MessageBox.Error("Nie wprowadzono klucza API.", "Błąd");
                return;
            }

            if (string.IsNullOrEmpty(skEditor.GetMainWindow().GetFileManager().GetTextEditor().Text))
            {
                MessageBox.Error("Twój kod jest pusty!", "Błąd");
                return;
            }

            PostSkript();
        }

        private async void PostSkript()
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://code.skript.pl/api/v1/codes/create");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = JsonSerializer.Serialize(new
                    {
                        key = apiTextBox.Text,
                        language = langComboBox.Text.ToLower(),
                        content = skEditor.GetMainWindow().GetFileManager().GetTextEditor().Text,
                        anonymous = anonymousCheckBox.IsChecked
                    });
                    await streamWriter.WriteAsync(json);
                    await streamWriter.FlushAsync();
                    streamWriter.Close();
                }

                var httpResponse = await httpWebRequest.GetResponseAsync() as HttpWebResponse;
                using var streamReader = new StreamReader(httpResponse.GetResponseStream());
                var result = await streamReader.ReadToEndAsync();
                dynamic resultJson = JObject.Parse(result);

                urlTextBox.Text = resultJson.url.ToString();
            }
            catch
            {
                MessageBox.Error("Coś nie zadziałało.\nMasz połączenie z internetem? Wkleiłeś prawidłowy klucz API?", "Wystąpił błąd");
            }
        }



        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.ApiKey = apiTextBox.Text;
            Properties.Settings.Default.Save();
        }

        private void HelpClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenUrl("https://code.skript.pl/api-key");
        }

        private void CopyClick(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                System.Windows.Clipboard.SetText(urlTextBox.Text);
            }
            catch { }
        }

        private static void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
        }

        private void OnKey(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                publishWindow.Close();
            }
        }
    }
}
