using HandyControl.Tools.Extension;
using Newtonsoft.Json.Linq;
using SkEditorPlus.Utilities;
using SkEditorPlus.Utilities.Vaults;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SkEditorPlus.Windows
{
    public partial class PublishWindow : HandyControl.Controls.Window
    {
        private SkEditorAPI skEditor = APIVault.GetAPIInstance();

        public PublishWindow()
        {
            InitializeComponent();
            BackgroundFixer.FixBackground(this);

            helpText.Text = helpText.Text.Replace("{0}", "pastebin.com/doc_api");
            websiteComboBox.SelectionChanged += OnWebsiteChange;
            OnWebsiteChange(null, null);
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

            switch (websiteComboBox.Text)
            {
                case "code.skript.pl":
                    Post();
                    break;
                case "Pastebin":
                    PostPastebin();
                    break;
            }
        }

        private async void PostPastebin()
        {
            string apiKey = apiTextBox.Text;
            string code = skEditor.GetTextEditor().Text;

            var values = new Dictionary<string, string>
            {
                { "api_dev_key", apiKey },
                { "api_option", "paste" },
                { "api_paste_code", code },
            };

            var client = new HttpClient();

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync("https://pastebin.com/api/api_post.php", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                urlTextBox.Text = responseString;
                AddonVault.addons.ForEach(addon =>
                {
                    addon.OnPublish(responseString);
                });
            }
        }

        private async void Post()
        {
            string code = skEditor.GetMainWindow().GetFileManager().GetTextEditor().Text;

            try
            {
                string json = JsonSerializer.Serialize(new
                {
                    key = apiTextBox.Text,
                    language = langComboBox.Text.ToLower(),
                    content = code,
                    anonymous = anonymousCheckBox.IsChecked
                });

                HttpClient client = new();
                HttpResponseMessage response = await client.PostAsync("https://code.skript.pl/api/v1/codes/create", new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
                string responseString = await response.Content.ReadAsStringAsync();

                JObject jsonResult = JObject.Parse(responseString);
                string url = jsonResult["url"].ToString();
                urlTextBox.Text = url;
                AddonVault.addons.ForEach(addon =>
                {
                    addon.OnPublish(url);
                });
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
            switch (websiteComboBox.Text)
            {
                case "code.skript.pl":
                    Properties.Settings.Default.CodeSkriptApiKey = apiTextBox.Text;
                    break;
                case "Pastebin":
                    Properties.Settings.Default.PastebinApiKey = apiTextBox.Text;
                    break;
            }
            Properties.Settings.Default.Save();
        }

        private void HelpClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string website = "https://pastebin.com/doc_api";
            if (websiteComboBox.Text.Equals("code.skript.pl"))
            {
                website = "https://code.skript.pl/api-key";
            }

            skEditor.OpenUrl(website);
        }

        private void CopyClick(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(urlTextBox.Text);
            }
            catch { }
        }

        private void OnKey(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                publishWindow.Close();
            }
        }

        private void OnWebsiteChange(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem comboBoxItem = (ComboBoxItem)websiteComboBox.SelectedItem;
            string website = comboBoxItem.Content.ToString();
            switch (website)
            {
                case "code.skript.pl":
                    apiTextBox.Text = Properties.Settings.Default.CodeSkriptApiKey;
                    helpText.Text = helpText.Text.Replace("pastebin.com/doc_api", "code.skript.pl/api-key");
                    break;
                case "Pastebin":
                    apiTextBox.Text = Properties.Settings.Default.PastebinApiKey;
                    helpText.Text = helpText.Text.Replace("code.skript.pl/api-key", "pastebin.com/doc_api");
                    break;
            }

            anonymousCheckBox.IsEnabled = langComboBox.IsEnabled = !website.Equals("Pastebin");
        }
    }
}
