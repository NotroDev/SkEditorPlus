using HandyControl.Controls;
using HandyControl.Data;
using ICSharpCode.AvalonEdit;
using SkEditorPlus.Managers;
using System;
using System.Windows;

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

        private void FontChoosed(object sender, EventArgs e)
        {
            FontSelector fontSelector = (FontSelector)sender;
            //fontSelector.ResultFontFamily.Source;
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
    }
}
