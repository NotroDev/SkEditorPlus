using HandyControl.Controls;
using SkEditorPlus.Utilities;
using SkEditorPlus.Utilities.Vaults;
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
            BackgroundFixer.FixBackground(this);

        }

        private void OnKey(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                commandGeneratorWindow.Close();
            }
        }

        private void Generate(object sender, System.Windows.RoutedEventArgs e)
        {
            string nameError = (string)FindResource("CmdGenNameError");
            string cooldownError = (string)FindResource("CmdGenCooldownError");
            string unitError = (string)FindResource("CmdGenCooldownUnitError");
            string error = (string)FindResource("Error");

            if (string.IsNullOrWhiteSpace(nameTextbox.Text))
            {
                MessageBox.Error(nameError, error);
                return;
            }

            if (!string.IsNullOrWhiteSpace(cooldownTextbox.Text))
            {
                if (!float.TryParse(cooldownTextbox.Text, out _))
                {
                    MessageBox.Error(cooldownError, error);
                    return;
                }
                if (cooldownComboBox.SelectedIndex == -1)
                {
                    MessageBox.Error(unitError, error);
                    return;
                }
            }

            var editor = skEditor.GetMainWindow().GetFileManager().GetTextEditor();
            var offset = editor.CaretOffset;
            var line = editor.Document.GetLineByOffset(offset);
            var code = string.Empty;

            if (!string.IsNullOrEmpty(editor.Document.GetText(line.Offset, line.Length)))
                code += "\n";

            code += $"command /{nameTextbox.Text}:";

            if (!string.IsNullOrEmpty(permTextbox.Text)) code += $"\n\tpermission: {permTextbox.Text}";

            if (!string.IsNullOrEmpty(permMessTextbox.Text)) code += $"\n\tpermission message: {permMessTextbox.Text}";

            if (!string.IsNullOrEmpty(aliasesTextbox.Text)) code += $"\n\taliases: {aliasesTextbox.Text}";

            if (!string.IsNullOrEmpty(usageTextbox.Text)) code += $"\n\tusage: {usageTextbox.Text}";

            var checkComboBox = executableByComboBox;
            var executableBy = (checkComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();

            if (!string.IsNullOrEmpty(executableBy)) code += $"\n\texecutable by: {executableBy}";

            if (!string.IsNullOrEmpty(cooldownTextbox.Text))
            {
                var item = (ComboBoxItem)cooldownComboBox.SelectedItem;
                code += $"\n\tcooldown: {cooldownTextbox.Text} {item.Tag}";
            }

            code += "\n\ttrigger:\n\t\t";
            editor.Document.Insert(offset, code);

            commandGeneratorWindow.Close();
            editor.CaretOffset = editor.Document.TextLength;

            AddonVault.addons.ForEach(addon =>
            {
                addon.OnGenerate(ISkEditorPlusAddon.GenerateType.COMMAND);
            });
        }

        private void OnComboBoxKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                executableByComboBox.IsDropDownOpen = true;
            }
        }
    }
}
