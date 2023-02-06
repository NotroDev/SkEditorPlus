using AvalonEditB;
using SkEditorPlus.Managers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TabItem = HandyControl.Controls.TabItem;
using System;
using CheckBox = System.Windows.Controls.CheckBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using SkEditorPlus.Data;
using Application = System.Windows.Application;
using System.IO;
using System.Linq;
using HandyControl.Tools;
using HandyControl.Controls;
using HandyControl.Data;
using System.Net.Http;
using System.Threading.Tasks;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SkEditorPlus.Windows
{
    public partial class NewOptionsWindow : HandyControl.Controls.Window
    {
        private readonly SkEditorAPI skEditor;
        private SettingsBindings settingsBindings;

        // This is polish easter egg :)
        private static readonly string pozdrowieniaSwirek = "pozdrowieniaswiateczne";
        private static readonly bool[] pozdrowieniaSwirekBools = new bool[pozdrowieniaSwirek.Length];

        public NewOptionsWindow(SkEditorAPI skEditor)
        {
            InitializeComponent();
            this.skEditor = skEditor;
            BackgroundFixManager.FixBackground(this);
            settingsBindings = new();
            DataContext = settingsBindings;
            
            foreach (var property in settingsBindings.GetType().GetProperties())
            {
                if (property.PropertyType == typeof(bool))
                {
                    var name = property.Name.Replace("Is", string.Empty).Replace("Enabled", string.Empty);

                    if (Properties.Settings.Default[name] != null)
                    {
                        property.SetValue(settingsBindings, Properties.Settings.Default[name]);
                    }
                }
            }

            string appDirectory = Path.GetDirectoryName(Application.ResourceAssembly.Location);
            string[] files = Directory.GetFiles(appDirectory + @"\Languages");

            foreach (string file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                if (fileName != null)
                if (fileName != null)
                {
                    if (languageComboBox.Items.Cast<ComboBoxItem>().All(item => item.Content.ToString() != fileName))
                    {
                        ComboBoxItem comboBoxItem = new()
                        {
                            Content = fileName
                        };
                        languageComboBox.Items.Add(comboBoxItem);
                    }
                }
            }
            ComboBoxItem item = languageComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString() == Properties.Settings.Default.Language);
            if (item != null)
            {
                languageComboBox.SelectedItem = item;
            }

            string versionLabel = (string)Application.Current.FindResource("Version");
            versionText.Text = $"{versionLabel} {MainWindow.Version}";

            fontPickerButton.Content = Properties.Settings.Default.Font;
            if (!MicaHelper.IsSupported(BackdropType.Mica))
            {
                micaCheckbox.IsEnabled = false;
            }
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
                    if (!skEditor.IsFileOpen()) continue;
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

            foreach (TabItem ti in skEditor.GetMainWindow().tabControl.Items)
            {
                if (!skEditor.IsFileOpen()) continue;
                TextEditor textEditor = (TextEditor)ti.Content;
                textEditor.Background = new SolidColorBrush(Color.FromArgb((byte)Properties.Settings.Default.EditorTransparency, 30, 30, 30));
            }
            skEditor.GetMainWindow().SetUpMica(false);
        }


        private void OnKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                newOptionsWindow.Close();
            }

            // This is polish easter egg :)
            
            if (e.Key.ToString().ToLower() == pozdrowieniaSwirek[Array.IndexOf(pozdrowieniaSwirekBools, false)].ToString())
            {
                pozdrowieniaSwirekBools[Array.IndexOf(pozdrowieniaSwirekBools, false)] = true;
            }
            else
            {
                Array.Clear(pozdrowieniaSwirekBools, 0, pozdrowieniaSwirekBools.Length);
            }
            if (pozdrowieniaSwirekBools.All(x => x))
            {
                DateTime christmas = new(DateTime.Now.Year, 12, 25);
                if (DateTime.Now > christmas)
                {
                    christmas = christmas.AddYears(1);
                }
                int daysLeft = (christmas - DateTime.Now).Days;
                MessageBox.Show($"pozdrowienia świąteczne byniu, {daysLeft} dni do świąt", "Siema świrek", MessageBoxButton.OK, MessageBoxImage.Information);
                Array.Clear(pozdrowieniaSwirekBools, 0, pozdrowieniaSwirekBools.Length);
            }
        }

        private void CheckboxClicked(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            string checkBoxName = checkBox.Name;
            checkBoxName = checkBoxName.Replace("Checkbox", "");
            checkBoxName = string.Concat(checkBoxName.Substring(0, 1).ToUpper(), checkBoxName.AsSpan(1));
            bool value = (bool)checkBox.IsChecked;

            Properties.Settings.Default[checkBoxName] = value;
            Properties.Settings.Default.Save();
        }

        private void OnLanguageChange(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem typeItem = (ComboBoxItem)languageComboBox.SelectedItem;
            string content = typeItem.Content.ToString();

            Properties.Settings.Default.Language = content;
            Properties.Settings.Default.Save();

            string appDirectory = Path.GetDirectoryName(Application.ResourceAssembly.Location);

            ResourceDictionary dict = new()
            {
                Source = new Uri(appDirectory + @"\Languages\" + content + ".xaml", UriKind.Absolute)
            };
            Application.Current.Resources.MergedDictionaries.Add(dict);
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
            Uri skriptUri = new("https://notro.tech/resources/SkriptHighlighting.xshd");
            Uri yamlUri = new("https://notro.tech/resources/YAMLHighlighting.xshd");

            string updateSuccess = (string)Application.Current.FindResource("UpdateSuccess");

            try
            {
                File.Delete(appPath + @"\SkriptHighlighting.xshd");
                File.Delete(appPath + @"\YAMLHighlighting.xshd");
                await DownloadFileTaskAsync(client, skriptUri, appPath + @"\SkriptHighlighting.xshd");
                await DownloadFileTaskAsync(client, yamlUri, appPath + @"\YAMLHighlighting.xshd");
                Growl.Success(updateSuccess, "SuccessMsg");
                skEditor.GetMainWindow().GetFileManager().OnTabChanged();
            }
            catch (Exception e)
            {
                string updateFailed = (string)Application.Current.FindResource("UpdateFailed");
                updateFailed = updateFailed.Replace("{0}", e.Message).Replace("{n}", Environment.NewLine);
                string error = (string)Application.Current.FindResource("Error");
                MessageBox.Error(updateFailed, error);
            }
        }

        public static async Task DownloadFileTaskAsync(HttpClient client, Uri uri, string FileName)
        {
            using var s = await client.GetStreamAsync(uri);
            using var fs = new FileStream(FileName, FileMode.CreateNew);
            await s.CopyToAsync(fs);
        }
    }
}
