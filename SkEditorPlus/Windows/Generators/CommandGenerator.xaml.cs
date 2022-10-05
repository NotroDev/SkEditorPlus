using AvalonEditB;
using AvalonEditB.Document;
using HandyControl.Controls;
using System.Collections.Generic;
using System.Windows.Controls;

namespace SkEditorPlus.Windows.Generators
{
    public partial class CommandGenerator : Window
    {
        private SkEditorAPI skEditor;

        public CommandGenerator(SkEditorAPI skEditor)
        {
            InitializeComponent();
            this.skEditor = skEditor;
        }

        private void OnKey(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                generatorWindow.Close();
            }
        }

        private void Generate(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(nameTextbox.Text))
            {
                MessageBox.Error("Nazwa komendy jest wymagana!", "Błąd");
                return;
            }

            if (string.IsNullOrEmpty(cooldownTextbox.Text))
            {
                if (cooldownComboBox.SelectedIndex == 0)
                {
                    MessageBox.Error("Wybierz jednostkę czasu!", "Błąd");
                    return;
                }
            }

            TextEditor editor = skEditor.GetMainWindow().GetFileManager().GetTextEditor();

            int offset = editor.CaretOffset;
            DocumentLine line = editor.Document.GetLineByOffset(offset);

            string code = "";

            if (!string.IsNullOrEmpty(editor.Document.GetText(line.Offset, line.Length)))
            {
                code += "\n";
            }

            code += $"command /{nameTextbox.Text}:";

            if (!string.IsNullOrEmpty(permTextbox.Text))
            {
                code += $"\n\tpermission: {permTextbox.Text}";
            }
            if (!string.IsNullOrEmpty(permMessTextbox.Text))
            {
                code += $"\n\tpermission message: {permMessTextbox.Text}";
            }
            if (!string.IsNullOrEmpty(aliasesTextbox.Text))
            {
                code += $"\n\taliases: {aliasesTextbox.Text}";
            }
            if (!string.IsNullOrEmpty(usageTextbox.Text))
            {
                code += $"\n\tusage: {usageTextbox.Text}";
            }

            CheckComboBox checkComboBox = executableByComboBox;
            List<string> selectedItems = new List<string>();
            foreach (CheckComboBoxItem item in checkComboBox.SelectedItems)
            {
                selectedItems.Add(item.Content.ToString());
            }

            string executableBy = "";
            if (selectedItems.Count == 2)
            {
                executableBy = "player, console";
            }
            else if (selectedItems.Count == 1)
            {
                executableBy = selectedItems[0];
            }

            if (!string.IsNullOrEmpty(executableBy))
            {
                code += $"\n\texecutable by: {executableBy}";
            }
            
            if (!string.IsNullOrEmpty(cooldownTextbox.Text))
            {
                ComboBoxItem item = (ComboBoxItem)cooldownComboBox.SelectedItem;
                code += $"\n\tcooldown: {cooldownTextbox.Text} {item.Content}";
            }    

            code += "\n\ttrigger:\n\t\t";

            editor.Text += code;

            generatorWindow.Close();

            editor.CaretOffset = editor.Document.TextLength;
        }
    }
}
