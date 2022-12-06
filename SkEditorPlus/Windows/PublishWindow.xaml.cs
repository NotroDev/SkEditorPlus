using HandyControl.Controls;
using Newtonsoft.Json.Linq;
using SkEditorPlus.Managers;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;

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
            BackgroundFixManager.FixBackground(this);

        }

        private void PublishClick(object sender, RoutedEventArgs e)
        {
            string error = (string)Application.Current.FindResource("Error");
            string emptyCodeError = (string)Application.Current.FindResource("EmptyCodeError");
            string emptyAPIKeyError = (string)Application.Current.FindResource("EmptyAPIKeyError");

            if (string.IsNullOrEmpty(apiTextBox.Text))
            {
                MessageBox.Error(emptyAPIKeyError, error);
                return;
            }

            if (string.IsNullOrEmpty(skEditor.GetMainWindow().GetFileManager().GetTextEditor().Text))
            {
                MessageBox.Error(emptyCodeError, error);
                return;
            }

            Post();
        }

        private async void Post()
        {
            try
            {
                string json = JsonSerializer.Serialize(new
                {
                    key = apiTextBox.Text,
                    language = langComboBox.Text.ToLower(),
                    content = skEditor.GetMainWindow().GetFileManager().GetTextEditor().Text,
                    anonymous = anonymousCheckBox.IsChecked
                });

                HttpClient client = new();
                HttpResponseMessage response = await client.PostAsync("https://code.skript.pl/api/v1/codes/create", new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
                string responseString = await response.Content.ReadAsStringAsync();

                JObject jsonResult = JObject.Parse(responseString);
                string url = jsonResult["url"].ToString();
                urlTextBox.Text = url;
            }
            catch (Exception e)
            {
                string error = (string)Application.Current.FindResource("Error");
                string publishingError = (string)Application.Current.FindResource("PublishingError");
                MessageBox.Error(publishingError.Replace("{n}", Environment.NewLine).Replace("{0}", e.Message), error);
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
