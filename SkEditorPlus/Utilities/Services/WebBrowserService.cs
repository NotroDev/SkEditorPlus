using HandyControl.Controls;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using SkEditorPlus.Utilities.Vaults;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace SkEditorPlus.Utilities.Services
{
    public class WebBrowserService
    {
        public static async Task<string> GetCodeFromSkript(string id)
        {
            string url = $"https://code.skript.pl/{id}/raw";

            using HttpClient client = new();
            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync(url);
            }
            catch (HttpRequestException)
            {
                HandyControl.Controls.MessageBox.Error(Application.Current.FindResource("FailedToSendRequestError") as string);
                return null;
            }
            string content = await response.Content.ReadAsStringAsync();

            if (response.Content.Headers.ContentType.MediaType != "text/plain")
            {
                HandyControl.Controls.MessageBox.Error((Application.Current.FindResource("ScriptWithIDNotFoundError") as string).Replace("{0}", id));
                return null;
            }

            string path = Path.Combine(Path.GetTempPath(), $"{id}.sk");
            File.WriteAllText(path, content);
            return path;
        }

        public static async void OpenSite(string header, string url)
        {
            WebView2 webBrowser = new();

            TabItem tabItem = new()
            {
                Header = header,
                ToolTip = "",
                Content = webBrowser,
                IsSelected = true,
            };

            System.Windows.Controls.ToolTipService.SetIsEnabled(tabItem, false);

            APIVault.GetAPIInstance().GetTabControl().Items.Add(tabItem);

            var userDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\SkEditor+";
            var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
            await webBrowser.EnsureCoreWebView2Async(env);
            webBrowser.Source = new Uri(url);
            AddonVault.addons.ForEach(addon =>
            {
                addon.OnSiteOpen(header, url);
            });
        }
    }
}
