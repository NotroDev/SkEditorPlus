using AvalonEditB;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SkEditorPlus.Windows
{
    public partial class BackpackWindow : HandyControl.Controls.Window
    {
        private SkEditorAPI skEditor;

        public BackpackWindow(SkEditorAPI skEditor)
        {
            InitializeComponent();
            this.skEditor = skEditor;

            StringCollection codes = Properties.Settings.Default.BackpackCodes;
            codes ??= new StringCollection();

            foreach (string code in codes)
            {
                ListBoxItem item = new()
                {
                    Content = code,
                    MaxHeight = 16
                };

                backpackListbox.Items.Add(item);
            }
        }

        private void OnKey(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                backpackWindow.Close();
            }
        }

        private void AddCode(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Zaznacz kod, a następnie kliknij przycisk OK.", "Dodawanie kodu", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Information);

            if (result == MessageBoxResult.Cancel) return;

            StringCollection codes = Properties.Settings.Default.BackpackCodes;

            codes ??= new StringCollection();

            TextEditor textEditor = skEditor.GetMainWindow().GetFileManager().GetTextEditor();

            string selectedCode = textEditor.SelectedText;

            ListBoxItem item = new()
            {
                Content = selectedCode,
                MaxHeight = 16
            };

            backpackListbox.Items.Add(item);
            codes.Add(item.Content.ToString());

            Properties.Settings.Default.BackpackCodes = codes;
            Properties.Settings.Default.Save();
        }

        private void DeleteCode(object sender, System.Windows.RoutedEventArgs e)
        {
            if (backpackListbox.SelectedItem != null)
            {
                StringCollection codes = Properties.Settings.Default.BackpackCodes;

                ListBoxItem item = (ListBoxItem)backpackListbox.SelectedItem;
                codes.Remove(item.Content.ToString());

                Properties.Settings.Default.BackpackCodes = codes;
                Properties.Settings.Default.Save();

                backpackListbox.Items.Remove(backpackListbox.SelectedItem);
            }
        }

        private void PasteCode(object sender, System.Windows.RoutedEventArgs e)
        {
            TextEditor textEditor = skEditor.GetMainWindow().GetFileManager().GetTextEditor();
            ListBoxItem item = (ListBoxItem)backpackListbox.SelectedItem;
            string codeToPaste = item.Content.ToString();

            textEditor.Document.Insert(textEditor.CaretOffset, codeToPaste);
        }
    }
}
