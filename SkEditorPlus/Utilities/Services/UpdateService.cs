using HandyControl.Data;
using Octokit;
using System.Diagnostics;
using System.Net.Http;
using System.Windows;
using System;
using Application = System.Windows.Application;
using MessageBox = HandyControl.Controls.MessageBox;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using FileMode = System.IO.FileMode;
using System.Windows.Threading;
using SkEditorPlus.Utilities.Vaults;
using SkEditorPlus.Utilities.Controllers;

namespace SkEditorPlus.Utilities.Services
{
    public class UpdateService
    {
        public static async void CheckUpdate(bool infoIfNot = true)
        {
            try
            {
                var github = new GitHubClient(new ProductHeaderValue("SkEditorPlus"));
                var releases = await github.Repository.Release.GetAll("NotroDev", "SkEditorPlus");
                string latest = releases.FirstOrDefault(r => !r.Prerelease)?.TagName.Replace("v", "");

                var current = MainWindow.Version;

                AddonVault.addons.ForEach(addon =>
                {
                    addon.OnCheckForUpdates(current, latest);
                });

                if (latest != current)
                {
                    string newVersionTitle = (string)Application.Current.FindResource("NewVersion");
                    string updateAvailable = (string)Application.Current.FindResource("UpdateAvailable");
                    string download = (string)Application.Current.FindResource("Download");
                    string ignore = (string)Application.Current.FindResource("Ignore");

                    MessageBoxResult result = MessageBox.Show(new MessageBoxInfo
                    {
                        Message = updateAvailable.Replace("{0}", current).Replace("{1}", latest).Replace("{n}", Environment.NewLine),
                        Caption = newVersionTitle,
                        Button = MessageBoxButton.YesNo,
                        YesContent = download,
                        NoContent = ignore,
                        IconBrushKey = ResourceToken.DarkInfoBrush,
                        IconKey = ResourceToken.InfoGeometry
                    });

                    if (result == MessageBoxResult.Yes)
                    {
                        var release = await github.Repository.Release.Get("NotroDev", "SkEditorPlus", "v" + latest);
                        string msiUrl = release.Assets.FirstOrDefault(a => a.BrowserDownloadUrl.EndsWith(".msi"))?.BrowserDownloadUrl;

                        using HttpClient client = new();
                        var response = await client.GetAsync(msiUrl);
                        var bytes = await response.Content.ReadAsByteArrayAsync();
                        var tempFile = Path.GetTempFileName();
                        File.WriteAllBytes(tempFile, bytes);

                        var installer = tempFile.Replace(".tmp", "skeditor.msi");
                        File.Move(tempFile, installer);

                        Process.Start(new ProcessStartInfo { FileName = installer, UseShellExecute = true });
                        Application.Current.Shutdown();
                    }
                }
                else
                {
                    if (!infoIfNot) return;
                    string noNewVersionTitle = (string)Application.Current.FindResource("NoNewVersion");
                    string noNewVersion = (string)Application.Current.FindResource("UpdateNotAvailable");
                    MessageBox.Show(new MessageBoxInfo
                    {
                        Message = noNewVersion.Replace("{0}", current).Replace("{n}", Environment.NewLine),
                        Caption = noNewVersionTitle,
                        Button = MessageBoxButton.OK,
                        ConfirmContent = "OK",
                        IconBrushKey = ResourceToken.DarkInfoBrush,
                        IconKey = ResourceToken.InfoGeometry
                    });
                }
            }
            catch
            {
                // i should add error message here later, but i'll probably forgot lol
            }
        }

        public static async Task UpdateSyntaxFiles()
        {
            var appPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\SkEditor Plus";
            var syntaxPath = Path.Combine(appPath, "Syntax Highlighting");

            Directory.CreateDirectory(syntaxPath);

            using var client = new HttpClient();
            var skriptUri = new Uri("https://marketplace.notro.tech/Default.xshd");
            var yamlUri = new Uri("https://notro.tech/resources/YAMLHighlighting.xshd");
            try
            {
                File.Delete($"{syntaxPath}\\Default.xshd");
                File.Delete($"{appPath}\\YAMLHighlighting.xshd");
                await DownloadFileTaskAsync(client, skriptUri, $"{syntaxPath}\\Default.xshd");
                await DownloadFileTaskAsync(client, yamlUri, $"{appPath}\\YAMLHighlighting.xshd");
                TabController.OnTabChanged();
            }
            catch
            {
                // do nothing ;p
            }
        }

        public static async Task UpdateItemFiles()
        {
            var appPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\SkEditor Plus";

            using var client = new HttpClient();
            var jsonUri = new Uri("https://notro.tech/resources/items.json");
            var zipUri = new Uri("https://notro.tech/resources/items.zip");
            try
            {
                File.Delete($"{appPath}\\items.json");
                if (Directory.Exists($"{appPath}\\items")) Directory.Delete($"{appPath}\\items", true);
                await DownloadFileTaskAsync(client, jsonUri, $"{appPath}\\items.json");
                await DownloadFileTaskAsync(client, zipUri, $"{appPath}\\items.zip");
                System.IO.Compression.ZipFile.ExtractToDirectory($"{appPath}\\items.zip", $"{appPath}");
                File.Delete($"{appPath}\\items.zip");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + " " + e.StackTrace);
            }
        }

        public static async Task DownloadFileTaskAsync(HttpClient client, Uri uri, string fileName)
        {
            using var s = await client.GetStreamAsync(uri);
            using var fs = new FileStream(fileName, FileMode.CreateNew);
            await s.CopyToAsync(fs);
        }
    }
}
