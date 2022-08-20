using HandyControl.Controls;
using HandyControl.Data;
using AvalonEditB;
using System;
using System.IO;
using System.Net;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SkEditorPlus.Windows
{
    public partial class OptionsWindow : HandyControl.Controls.Window
    {
        private SkEditorAPI skEditor;

        public OptionsWindow(SkEditorAPI skEditor)
        {
            InitializeComponent();
            this.skEditor = skEditor;
        }

        private void OnWindowLoad(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.Font == null) return;
            fontChooseButton.Content = Properties.Settings.Default.Font;
            wrappingCheckbox.IsChecked = Properties.Settings.Default.Wrapping;
        }

        private void FontButtonClick(object sender, RoutedEventArgs e)
        {

            FontSelector fontSelector = new();

            if (fontSelector.ShowDialog().Equals(true))
            {
                Properties.Settings.Default.Font = fontSelector.ResultFontFamily.Source;
                Properties.Settings.Default.Save();
                fontChooseButton.Content = Properties.Settings.Default.Font;
                foreach (TabItem ti in skEditor.GetMainWindow().tabControl.Items)
                {
                    TextEditor textEditor = (TextEditor)ti.Content;
                    textEditor.FontFamily = new System.Windows.Media.FontFamily(fontSelector.ResultFontFamily.Source);
                }
            }
        }

        private void OnKey(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                optionWindow.Close();
            }
        }

        private void AutoSaveChecked(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = HandyControl.Controls.MessageBox.Show(new MessageBoxInfo
            {
                Message = "Próbujesz włączyć opcję, która nie została w pełni przetestowana i może powodować problemy.\nUżywasz na własną odpowiedzialność.",
                Caption = "Uwaga!",
                ConfirmContent = "Spoko!"

            });

            if (result == MessageBoxResult.OK)
            {
                return;
            }

            System.Windows.Controls.CheckBox checkBox = (System.Windows.Controls.CheckBox)sender;
            checkBox.IsChecked = false;
        }
        
        private void WrappingChecked(object sender, RoutedEventArgs e)
        {
            ChangeWrapping(true);
        }

        private void WrappingUnChecked(object sender, RoutedEventArgs e)
        {
            ChangeWrapping(false);
        }

        private void ChangeWrapping(bool wrapping)
        {
            Properties.Settings.Default.Wrapping = wrapping;
            Properties.Settings.Default.Save();
            foreach (TabItem ti in skEditor.GetMainWindow().tabControl.Items)
            {
                TextEditor textEditor = (TextEditor)ti.Content;
                textEditor.WordWrap = wrapping;
            }
        }

        private void UpdateSyntaxClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(new MessageBoxInfo
            {
                Message = "Jeśli kontynujesz, zostanie pobrany oraz zamieniony plik podświetlania składni. Jeśli robiłeś jakieś zmiany w nim i nie chcesz ich stracić, zrób kopię.",
                Caption = "Uwaga!",
                ConfirmContent = "Kontynuuj",
                CancelContent = "Anuluj",
                Button = MessageBoxButton.OKCancel

            });

            if (result.Equals(MessageBoxResult.OK))
            {
                UpdateSyntaxFile();
            }
        }

        public static void UpdateSyntaxFile()
        {
            string appPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SkEditor Plus";
            using var client = new WebClient();
            Uri uri = new("https://notro.tech/resources/SkriptHighlighting.xshd");
            try
            {
                client.DownloadFile(uri, appPath + @"\SkriptHighlighting.xshd");
                Growl.Success("Pomyślnie zaaktualizowano plik podświetlania składni!", "SuccessMsg");
            }
            catch (Exception e)
            {
                MessageBox.Error("Nie udało się pobrać pliku podświetlania składni!\nMasz połączenie z internetem?", "Błąd");
            }
        }
    }
}
