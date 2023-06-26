using AvalonEditB;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools;
using SkEditorPlus.Data;
using SkEditorPlus.Utilities;
using SkEditorPlus.Utilities.Builders;
using SkEditorPlus.Utilities.Controllers;
using SkEditorPlus.Utilities.Services;
using SkEditorPlus.Utilities.Vaults;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Application = System.Windows.Application;
using CheckBox = System.Windows.Controls.CheckBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = HandyControl.Controls.MessageBox;
using TabItem = HandyControl.Controls.TabItem;

namespace SkEditorPlus.Windows
{
    public partial class NewOptionsWindow : HandyControl.Controls.Window
    {
        private readonly SkEditorAPI skEditor = APIVault.GetAPIInstance();
        private readonly SettingsBindings settingsBindings;


        public NewOptionsWindow()
        {
            InitializeComponent();
            BackgroundFixer.FixBackground(this);
            settingsBindings = new();
            DataContext = settingsBindings;

            docsLinkTextBox.Text = Properties.Settings.Default.DocsLink;
            editorTransparency.Value = Properties.Settings.Default.EditorTransparency;

            foreach (var property in settingsBindings.GetType().GetProperties().Where(p => p.PropertyType == typeof(bool)))
            {
                var name = property.Name.Replace("Is", "").Replace("Enabled", "");
                var value = Properties.Settings.Default[name];
                property.SetValue(settingsBindings, value);
            }

            string directory = Path.Combine(Path.GetDirectoryName(Application.ResourceAssembly.Location), "Languages");

            foreach (string file in Directory.GetFiles(directory))
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                if (fileName != null && languageComboBox.Items.Cast<ComboBoxItem>().All(item => item.Tag.ToString() != fileName))
                {
                    var langName = new ResourceDictionary { Source = new Uri(file) }["LangName"] as string;
                    languageComboBox.Items.Add(new ComboBoxItem { Content = langName, Tag = fileName });
                }
            }

            ComboBoxItem item = languageComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(x => x.Tag.ToString() == Properties.Settings.Default.Language);
            languageComboBox.SelectedItem = item;


            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SkEditor Plus", "Syntax Highlighting");
            Directory.CreateDirectory(path);

            foreach (string file in Directory.GetFiles(path, "*.xshd"))
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                if (fileName != null && syntaxComboBox.Items.Cast<ComboBoxItem>().All(item => item.Tag.ToString() != fileName))
                {
                    ComboBoxItem comboBoxItem = new() { Content = fileName, Tag = fileName };
                    syntaxComboBox.Items.Add(comboBoxItem);
                }
            }

            syntaxComboBox.SelectedItem = syntaxComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(x => x.Tag.ToString() == Properties.Settings.Default.SyntaxHighlighting);


            string versionLabel = (string)Application.Current.FindResource("Version");
            versionText.Text = $"{versionLabel} {MainWindow.Version}";

            fontPickerButton.Content = Properties.Settings.Default.Font;

            if (string.IsNullOrEmpty(fontPickerButton.Content.ToString()))
            {
                fontPickerButton.Content = "Cascadia Mono";
            }

            if (!MicaHelper.IsSupported(BackdropType.Mica))
            {
                micaCheckbox.IsEnabled = false;
            }

            LoadAddons();
        }

        private void OnFontButtonClick(object sender, RoutedEventArgs e)
        {
            FontSelector fontSelector = new();

            if (fontSelector.ShowDialog().Equals(true))
            {
                Properties.Settings.Default.Font = fontSelector.ResultFontFamily.Source;
                Properties.Settings.Default.Save();
                fontPickerButton.Content = Properties.Settings.Default.Font;

                foreach (TabItem ti in skEditor.GetMainWindow().tabControl.Items)
                {
                    if (!skEditor.IsFile(ti)) continue;
                    TextEditor textEditor = (TextEditor)ti.Content;
                    textEditor.FontFamily = new FontFamily(fontSelector.ResultFontFamily.Source);
                }
            }
        }

        private void TransparencyChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = (Slider)sender;
            var value = slider.Value;

            Properties.Settings.Default.EditorTransparency = (int)value;
            Properties.Settings.Default.Save();

            if (skEditor == null)
            {
                return;
            }

            if (Properties.Settings.Default.Mica)
            {
                foreach (TabItem ti in skEditor.GetMainWindow().tabControl.Items)
                {
                    if (!skEditor.IsFile(ti)) continue;
                    TextEditor textEditor = (TextEditor)ti.Content;
                    textEditor.Background = new SolidColorBrush(Color.FromArgb((byte)Properties.Settings.Default.EditorTransparency, 30, 30, 30));
                }
            }
            skEditor.GetMainWindow().SetUpMica(false);
        }


        private void OnKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                newOptionsWindow.Close();
            }
        }

        private void OnExperimentClicked(object sender, RoutedEventArgs e)
        {
            CheckboxClicked(sender, e);

            CheckBox checkBox = (CheckBox)sender;
            string checkBoxName = checkBox.Name;

            var mainWindow = APIVault.GetAPIInstance().GetMainWindow();
            switch (checkBoxName)
            {
                case "projectsExperimentCheckbox":
                    mainWindow.leftTabControl.Visibility = checkBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                    break;
                case "bottomBarExperimentCheckbox":
                    mainWindow.BottomBar.Visibility = checkBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                    break;
                case "completionExperimentCheckbox":
                    TabController.OnTabChanged();
                    break;
                case "analyzerExperimentCheckbox":
                    mainWindow.ToggleAnalyzer(checkBox.IsChecked == true);
                    break;
            }
        }

        private void CheckboxClicked(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            string checkBoxName = checkBox.Name;
            checkBoxName = checkBoxName.Replace("Checkbox", "");
            checkBoxName = string.Concat(checkBoxName[..1].ToUpper(), checkBoxName.AsSpan(1));
            bool value = (bool)checkBox.IsChecked;

            Properties.Settings.Default[checkBoxName] = value;
            Properties.Settings.Default.Save();
        }

        private void OnLanguageChange(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem typeItem = (ComboBoxItem)languageComboBox.SelectedItem;
            string tag = typeItem.Tag.ToString();

            string appDirectory = Path.GetDirectoryName(Application.ResourceAssembly.Location);

            string oldLang = Properties.Settings.Default.Language;
            if (!oldLang.Equals("English"))
            {
                Uri oldLanguage = new(appDirectory + @"\Languages\" + oldLang + ".xaml", UriKind.Absolute);
                Application.Current.Resources.MergedDictionaries.Remove(Application.Current.Resources.MergedDictionaries.First(d => d.Source == oldLanguage));
            }

            Properties.Settings.Default.Language = tag;
            Properties.Settings.Default.Save();

            ResourceDictionary dict = new()
            {
                Source = new Uri(appDirectory + @"\Languages\" + tag + ".xaml", UriKind.Absolute)
            };

            Application.Current.Resources.MergedDictionaries.Add(dict);
            string versionLabel = (string)Application.Current.FindResource("Version");
            versionText.Text = $"{versionLabel} {MainWindow.Version}";

            FileBuilder.newFileName = (string)Application.Current.Resources["NewFileName"];
            FileBuilder.regex = new(FileBuilder.newFileName.Replace("{0}", @"[0-9]+"));
        }

        private void UpdateSyntaxClick(object sender, RoutedEventArgs e)
        {
            string updateConfirmation = (string)Application.Current.FindResource("UpdateConfirmation");
            string cancel = (string)Application.Current.FindResource("Cancel");
            string continueString = (string)Application.Current.FindResource("Continue");
            string attention = (string)Application.Current.FindResource("Attention");

            MessageBoxResult result = MessageBox.Show(new MessageBoxInfo
            {
                Message = updateConfirmation,
                Caption = attention,
                ConfirmContent = continueString,
                CancelContent = cancel,
                Button = MessageBoxButton.OKCancel

            });

            if (result.Equals(MessageBoxResult.OK))
            {
                UpdateSyntaxFile();
            }
        }

        public async void UpdateSyntaxFile()
        {
            string appPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SkEditor Plus";
            using var client = new HttpClient();
            Uri skriptUri = new("https://marketplace.notro.tech/Default.xshd");
            Uri yamlUri = new("https://notro.tech/resources/YAMLHighlighting.xshd");

            string updateSuccess = (string)Application.Current.FindResource("UpdateSuccess");

            try
            {
                File.Delete(appPath + @"\Syntax Highlighting\Default.xshd");
                File.Delete(appPath + @"\YAMLHighlighting.xshd");
                await UpdateService.DownloadFileTaskAsync(client, skriptUri, appPath + @"\Syntax Highlighting\Default.xshd");
                await UpdateService.DownloadFileTaskAsync(client, yamlUri, appPath + @"\YAMLHighlighting.xshd");
                Growl.Success(updateSuccess, "SuccessMsg");
                TabController.OnTabChanged();
            }
            catch (Exception e)
            {
                string updateFailed = (string)Application.Current.FindResource("UpdateFailed");
                updateFailed = updateFailed.Replace("{0}", e.Message).Replace("{n}", Environment.NewLine);
                string error = (string)Application.Current.FindResource("Error");
                MessageBox.Error(updateFailed, error);
            }
        }

        private void OnDocsLinkChanged(object sender, TextChangedEventArgs e)
        {
            string link = docsLinkTextBox.Text;
            Properties.Settings.Default.DocsLink = link;
            Properties.Settings.Default.Save();
        }


        private void OpenAddonsFolderClick(object sender, RoutedEventArgs e)
        {
            string appPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SkEditor Plus";
            string addonsPath = appPath + @"\Addons";
            if (!Directory.Exists(addonsPath))
            {
                Directory.CreateDirectory(addonsPath);
            }
            Process.Start("explorer.exe", addonsPath);
        }

        private void LoadAddons()
        {
            foreach (var addon in AddonVault.addons)
            {
                ListBoxItem item = new()
                {
                    Content = addon.Name
                };

                item.Selected += (sender, e) =>
                {
                    string name = (string)Application.Current.FindResource("OptionsAddonName");
                    string author = (string)Application.Current.FindResource("OptionsAddonAuthor");
                    string version = (string)Application.Current.FindResource("OptionsAddonVersion");
                    string description = (string)Application.Current.FindResource("OptionsAddonDescription");

                    addonNameText.Text = $"{name} {addon.Name}";
                    addonDescriptionText.Text = $"{description} {addon.Description}";
                    addonAuthorText.Text = $"{author} {addon.Author}";
                    addonVersionText.Text = $"{version} {addon.Version}";
                };

                addonListBox.Items.Add(item);
            }
        }

        private void OnSyntaxChange(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem typeItem = (ComboBoxItem)syntaxComboBox.SelectedItem;
            string tag = typeItem.Tag.ToString();

            Properties.Settings.Default.SyntaxHighlighting = tag;
            Properties.Settings.Default.Save();

            TabController.OnTabChanged();
        }
    }
}