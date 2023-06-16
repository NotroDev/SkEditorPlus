using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using SkEditorPlus.Data;
using System.Linq;
using System.Net.Http;
using MessageBox = HandyControl.Controls.MessageBox;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using HandyControl.Tools.Extension;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Collections.Specialized;
using HandyControl.Data;
using HandyControl.Controls;
using SkEditorPlus.Utilities.Vaults;
using SkEditorPlus.Utilities.Controllers;

namespace SkEditorPlus.Windows
{
    public partial class MarketplaceWindow : HandyControl.Controls.Window
    {
        private readonly SkEditorAPI skEditor;

        public static MarketplaceBindings marketBindings;

        static MarketExplorer[] marketExplorers = { new(), new(), new() };

        string[] buttonContents = { Application.Current.FindResource("MarketplaceButtonInstall") as string,
                                    Application.Current.FindResource("MarketplaceButtonUninstall") as string,
                                    Application.Current.FindResource("MarketplaceButtonUpdate") as string };

        private static MarketplaceWindow instance;

        public MarketplaceWindow(SkEditorAPI skEditor)
        {
            InitializeComponent();
            this.skEditor = skEditor;

            string header = BrowseTabItem.Header.ToString();
            int headerLength = header.Length;
            int spacesPerSide = 10 - headerLength;
            spacesPerSide = Math.Max(0, spacesPerSide);

            string spaces = new(' ', spacesPerSide);

            BrowseTabItem.Header = $"{spaces}{header}{spaces}";

            Tab1Content.Content = marketExplorers[0];
            Tab2Content.Content = marketExplorers[1];
            Tab3Content.Content = marketExplorers[2];

            if (instance is null)
            {
                instance = this;
                AddItems(true);
                return;
            }
            AddItems(false);
        }

        public static MarketplaceWindow GetInstance()
        {
            return instance;
        }

        private MarketExplorer MarketExplorer => marketExplorers[tabControl.SelectedIndex];

        private async void AddItems(bool firstTime)
        {
            try
            {
                if (firstTime)
                {
                    marketBindings = new MarketplaceBindings { MarketItems = new ObservableCollection<MarketplaceItem>(), FilteredItems = new ObservableCollection<MarketplaceItem>() };
                }
                DataContext = marketBindings;

                foreach (MarketExplorer addonExplorer in marketExplorers)
                {
                    addonExplorer.FilterComboBox.SelectionChanged += OnFilterChanged;
                    addonExplorer.SearchBox.TextChanged += OnSearching;
                    addonExplorer.AddonsListBox.SelectionChanged += OnAddonChanged;
                }
                tabControl.SelectionChanged += OnTabChanged;

                if (!firstTime) return;

                var client = new HttpClient();
                var responseBody = await client.GetStringAsync("https://marketplace.notro.tech/items.json");
                var addonsList = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(responseBody)["addons"];
                addonsList.Sort();

                foreach (var item in addonsList)
                {
                    var manifest = await client.GetStringAsync($"https://marketplace.notro.tech/items/{item}/manifest.json");
                    var addon = JsonConvert.DeserializeObject<MarketplaceItem>(manifest);

                    if (!string.IsNullOrEmpty(addon.Icon))
                    {
                        addon.Icon = $"https://raw.githubusercontent.com/NotroDev/SkEditorPlus-Marketplace/main/items/{item}/{addon.Icon}";
                    }

                    if (!string.IsNullOrEmpty(addon.URL))
                    {
                        addon.URL = $"https://raw.githubusercontent.com/NotroDev/SkEditorPlus-Marketplace/main/items/{item}/{addon.URL}";
                    }

                    addon.NamePlusVersion = $"{addon.Name} {addon.Version}";

                    marketBindings.MarketItems.Add(addon);

                    if (IsAddonUninstalled(addon))
                    {
                        marketBindings.FilteredItems.Add(addon);
                    }
                    marketBindings.FilteredItems = FilterItems();
                }
            }
            catch (Exception ex)
            {
                skEditor.ShowError((Application.Current.FindResource("MarketplaceDownloadError") as string).Replace("{0}", ex.Message).Replace("{n}", Environment.NewLine), Application.Current.FindResource("Error") as string);
            }
        }


        private void OnKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                marketplaceWindow.Close();
            }
        }

        private void OnAddonChanged(object sender, SelectionChangedEventArgs e)
        {
            marketExplorers.ForEach(explorer => explorer.installButton.Visibility = Visibility.Visible);

            try
            {
                int selectedIndex = MarketExplorer.AddonsListBox.SelectedIndex;
                if (selectedIndex >= 0)
                {
                    marketBindings.SelectedItem = marketBindings.FilteredItems[selectedIndex];
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                marketBindings.SelectedItem = null;
                marketExplorers.ForEach(explorer => explorer.installButton.Visibility = Visibility.Collapsed);
            }
        }

        private void OnSearching(object sender, TextChangedEventArgs e)
        {
            foreach (MarketExplorer loopAddonExplorer in marketExplorers)
            {
                loopAddonExplorer.SearchBox.Text = MarketExplorer.SearchBox.Text;
                loopAddonExplorer.SearchBox.CaretIndex = loopAddonExplorer.SearchBox.Text.Length;
            }
            marketBindings.FilteredItems = FilterItems();
        }

        private void OnFilterChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (MarketExplorer loopAddonExplorer in marketExplorers)
            {
                loopAddonExplorer.FilterComboBox.SelectedIndex = MarketExplorer.FilterComboBox.SelectedIndex;
            }
            marketBindings.FilteredItems = FilterItems();
        }

        private void OnTabChanged(object sender, SelectionChangedEventArgs e)
        {
            marketBindings.InstallButtonHeader = buttonContents[tabControl.SelectedIndex];

            marketBindings.FilteredItems = FilterItems();
        }

        private static bool IsAddonUninstalled(MarketplaceItem item)
        {
            bool shouldItemBeAdded = true;
            foreach (var addon in AddonVault.addons)
            {
                if (item.Name.Equals(addon.Name))
                {
                    shouldItemBeAdded = false;
                    break;
                }
            }
            var syntaxHighlightingPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SkEditor Plus", "Syntax Highlighting");

            if (!Directory.Exists(syntaxHighlightingPath))
                Directory.CreateDirectory(syntaxHighlightingPath);

            var files = Directory.GetFiles(syntaxHighlightingPath, "*.xshd", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                if (Path.GetFileName(file).Equals(Path.GetFileName(item.URL)))
                {
                    shouldItemBeAdded = false;
                    break;
                }
            }

            return shouldItemBeAdded;
        }

        private static bool IsOutdated(MarketplaceItem item)
        {
            try
            {
                if (item.Type.Equals("Addon"))
                {
                    return AddonVault.addons.Any(skEditorPlusAddon => item.Name.Equals(skEditorPlusAddon.Name) && !item.Version.Equals(skEditorPlusAddon.Version));
                }
                string syntax = Properties.Settings.Default.InstalledSyntaxes.Cast<string>().FirstOrDefault(s => s.Contains(item.Name));

                string version = syntax.Split('|')[2];
                return !item.Version.Equals(version);
            }
            catch
            {
                return false;
            }
        }

        private ObservableCollection<MarketplaceItem> FilterItems()
        {
            var searchQuery = MarketExplorer.SearchBox.Text?.ToLower() ?? "";
            var filtered = marketBindings.MarketItems.Where(a => a.Name.ToLower().Contains(searchQuery));

            if (tabControl.SelectedIndex == 1)
            {
                filtered = FilterInstalledAddons(filtered);
            }
            else if (tabControl.SelectedIndex == 2)
            {
                filtered = FilterOutdatedAddons(filtered);
            }
            else
            {
                filtered = FilterUninstalledAddons(filtered);
            }

            var comboBoxItem = MarketExplorer.FilterComboBox.SelectedItem as ComboBoxItem;
            var typeFilter = comboBoxItem?.Content switch
            {
                "Addons" => "Addon",
                "Syntax highlightings" => "Syntax highlighting",
                _ => null
            };

            if (typeFilter != null)
            {
                filtered = filtered.Where(a => a.Type == typeFilter);
            }

            if (!filtered.Contains(marketBindings.SelectedItem))
            {
                marketBindings.SelectedItem = null;
                marketExplorers.ForEach(explorer => explorer.installButton.Visibility = Visibility.Collapsed);
            }

            return new ObservableCollection<MarketplaceItem>(filtered);
        }

        private static IEnumerable<MarketplaceItem> FilterInstalledAddons(IEnumerable<MarketplaceItem> items)
        {
            return items.Where(item => !IsAddonUninstalled(item) && !IsOutdated(item));
        }

        private static IEnumerable<MarketplaceItem> FilterUninstalledAddons(IEnumerable<MarketplaceItem> items)
        {
            return items.Where(item => IsAddonUninstalled(item));
        }

        private static IEnumerable<MarketplaceItem> FilterOutdatedAddons(IEnumerable<MarketplaceItem> items)
        {
            return items.Where(item => IsOutdated(item));
        }

        public async Task InstallAddon()
        {
            try
            {
                MarketplaceItem item = marketBindings.SelectedItem;
                using HttpClient client = new();

                bool isSyntax = item.Type.Equals("Syntax highlighting");

                string path = isSyntax ?
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SkEditor Plus", "Syntax Highlighting") :
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SkEditor Plus", "Addons");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string filePath = Path.Combine(path, Path.GetFileName(item.URL));
                HttpResponseMessage response = await client.GetAsync(item.URL);
                response.EnsureSuccessStatusCode();
                byte[] responseBody = await response.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(filePath, responseBody);

                marketBindings.FilteredItems = FilterItems();
                
                if (isSyntax)
                {
                    MessageBoxResult result = MessageBox.Show(new MessageBoxInfo
                    {
                        Message = Application.Current.FindResource("MarketplaceSyntaxInstalled") as string,
                        Caption = Application.Current.FindResource("MarketplaceInstalledTitle") as string,
                        Button = MessageBoxButton.OKCancel,
                        ConfirmContent = Application.Current.FindResource("MarketplaceEnableNow") as string,
                        CancelContent = "OK",
                        IconBrushKey = ResourceToken.DarkInfoBrush,
                        IconKey = ResourceToken.InfoGeometry
                    });

                    if (result  == MessageBoxResult.OK)
                    {
                        Properties.Settings.Default.SyntaxHighlighting = Path.GetFileNameWithoutExtension(item.URL);
                        Properties.Settings.Default.Save();

                        TabController.OnTabChanged();
                    }
                }
                else
                {
                    MessageBox.Show(new MessageBoxInfo
                    {
                        Message = Application.Current.FindResource("MarketplaceAddonInstalled") as string,
                        Caption = Application.Current.FindResource("MarketplaceInstalledTitle") as string,
                        Button = MessageBoxButton.OK,
                        ConfirmContent = "OK",
                        IconBrushKey = ResourceToken.DarkInfoBrush,
                        IconKey = ResourceToken.InfoGeometry
                    });
                }

                if (isSyntax)
                {
                    StringCollection syntaxes = Properties.Settings.Default.InstalledSyntaxes;
                    syntaxes ??= new StringCollection();

                    syntaxes.Add($"{item.Name}|{Path.GetFileName(item.URL)}|{item.Version}");
                    Properties.Settings.Default.InstalledSyntaxes = syntaxes;
                    Properties.Settings.Default.Save();
                }
                AddonVault.addons.ForEach(addon =>
                {
                    addon.OnAddonInstall(item.Name, item.Author, item.Version);
                });
            }
            catch (Exception ex)
            {
                skEditor.ShowError((Application.Current.FindResource("MarketplaceDownloadError") as string).Replace("{0}", ex.Message).Replace("{n}", Environment.NewLine), Application.Current.FindResource("Error") as string);
            }
        }

        public void UninstallAddon()
        {
            MarketplaceItem item = marketBindings.SelectedItem;
            bool isSyntax = item.Type.Equals("Syntax highlighting");

            string path = isSyntax ?
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SkEditor Plus", "Syntax Highlighting") :
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SkEditor Plus", "Addons");

            string filePath = Path.Combine(path, Path.GetFileName(item.URL));

            if (isSyntax)
            {
                StringCollection syntaxes = Properties.Settings.Default.InstalledSyntaxes;
                syntaxes ??= new StringCollection();
                string syntax = syntaxes.Cast<string>().FirstOrDefault(s => s.StartsWith(item.Name));
                if (syntax != null)
                {
                    syntaxes.Remove(syntax);
                    Properties.Settings.Default.InstalledSyntaxes = syntaxes;
                    Properties.Settings.Default.Save();
                }

                File.Delete(filePath);
                marketBindings.FilteredItems.Remove(item);

                skEditor.ShowSuccess(Application.Current.FindResource("MarketplaceSyntaxUninstalled") as string);
                Properties.Settings.Default.SyntaxHighlighting = "Default";
                Properties.Settings.Default.Save();

                TabController.OnTabChanged();
            }
            else
            {
                StringCollection addonsToRemove = Properties.Settings.Default.AddonsToRemove;
                addonsToRemove ??= new StringCollection();

                addonsToRemove.Add(Path.GetFileName(item.URL));

                Properties.Settings.Default.AddonsToRemove = addonsToRemove;
                Properties.Settings.Default.Save();

                MessageBox.Show(new MessageBoxInfo
                {
                    Message = Application.Current.FindResource("MarketplaceAddonUninstalled") as string,
                    Caption = Application.Current.FindResource("MarketplaceUninstalledTitle") as string,
                    Button = MessageBoxButton.OK,
                    ConfirmContent = "OK",
                    IconBrushKey = ResourceToken.DarkInfoBrush,
                    IconKey = ResourceToken.InfoGeometry
                });
            }
            AddonVault.addons.ForEach(addon =>
            {
                    addon.OnAddonUninstall(item.Name, item.Author, item.Version);
            });
        }

        public async void UpdateAddon()
        {
            try
            {
                MarketplaceItem item = marketBindings.SelectedItem;
                using HttpClient client = new();

                bool isSyntax = item.Type.Equals("Syntax highlighting");

                string path = isSyntax ?
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SkEditor Plus", "Syntax Highlighting") :
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SkEditor Plus", "Addons");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                if (isSyntax)
                {
                    string filePath = Path.Combine(path, Path.GetFileName(item.URL));
                    HttpResponseMessage response = await client.GetAsync(item.URL);
                    response.EnsureSuccessStatusCode();
                    byte[] responseBody = await response.Content.ReadAsByteArrayAsync();
                    await File.WriteAllBytesAsync(filePath, responseBody);

                    TabController.OnTabChanged();
                    marketBindings.FilteredItems = FilterItems();

                    StringCollection syntaxes = Properties.Settings.Default.InstalledSyntaxes ?? new StringCollection();

                    string stringToRemove = syntaxes.Cast<string>().FirstOrDefault(syntax => syntax.StartsWith($"{item.Name}|{Path.GetFileName(item.URL)}"));
                    syntaxes.Remove(stringToRemove);

                    if (!syntaxes.Contains($"{item.Name}|{Path.GetFileName(item.URL)}|{item.Version}"))
                    {
                        syntaxes.Add($"{item.Name}|{Path.GetFileName(item.URL)}|{item.Version}");
                    }

                    Properties.Settings.Default.InstalledSyntaxes = syntaxes;
                    Properties.Settings.Default.Save();

                    MessageBox.Show(new MessageBoxInfo
                    {
                        Message = Application.Current.FindResource("MarketplaceSyntaxUpdated") as string,
                        Caption = Application.Current.FindResource("MarketplaceUpdatedTitle") as string,
                        Button = MessageBoxButton.OK,
                        ConfirmContent = "OK",
                        IconBrushKey = ResourceToken.DarkInfoBrush,
                        IconKey = ResourceToken.InfoGeometry
                    });
                }
                else
                {
                    string filePath = Path.Combine(path, "update-" + Path.GetFileName(item.URL));
                    HttpResponseMessage response = await client.GetAsync(item.URL);
                    response.EnsureSuccessStatusCode();
                    byte[] responseBody = await response.Content.ReadAsByteArrayAsync();
                    await File.WriteAllBytesAsync(filePath, responseBody);

                    StringCollection addonsToRemove = Properties.Settings.Default.AddonsToRemove;
                    addonsToRemove ??= new StringCollection();

                    if (!addonsToRemove.Contains(Path.GetFileName(item.URL)))
                    {
                        addonsToRemove.Add(Path.GetFileName(item.URL));
                    }

                    Properties.Settings.Default.AddonsToRemove = addonsToRemove;
                    Properties.Settings.Default.Save();

                    MessageBox.Show(new MessageBoxInfo
                    {
                        Message = Application.Current.FindResource("MarketplaceAddonUpdated") as string,
                        Caption = Application.Current.FindResource("MarketplaceUpdatedTitle") as string,
                        Button = MessageBoxButton.OK,
                        ConfirmContent = "OK",
                        IconBrushKey = ResourceToken.DarkInfoBrush,
                        IconKey = ResourceToken.InfoGeometry
                    });
                }
                AddonVault.addons.ForEach(addon =>
                {
                    addon.OnAddonUpdate(item.Name, item.Author, item.Version);
                });
            }
            catch (Exception ex)
            {
                skEditor.ShowError((Application.Current.FindResource("MarketplaceDownloadError") as string).Replace("{0}", ex.Message).Replace("{n}", Environment.NewLine), Application.Current.FindResource("Error") as string);
            }
        }
    }
}