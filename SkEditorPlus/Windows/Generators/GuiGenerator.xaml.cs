using AvalonEditB;
using AvalonEditB.Document;
using HandyControl.Controls;
using SkEditorPlus.Managers;

namespace SkEditorPlus.Windows.Generators
{
    public partial class GuiGenerator : Window
    {
        private SkEditorAPI skEditor;

        public GuiGenerator(SkEditorAPI skEditor)
        {
            InitializeComponent();
            this.skEditor = skEditor;
            BackgroundFixManager.FixBackground(this);

        }

        private void OnKey(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                guiGeneratorWindow.Close();
            }
        }

        private void Generate(object sender, System.Windows.RoutedEventArgs e)
        {
            string nameError = (string)FindResource("GUIGenNameError");
            string titleError = (string)FindResource("GUIGenTitleError");
            string rowsError = (string)FindResource("GUIGenRowsError");
            string rowsError2 = (string)FindResource("GUIGenRowsError2");
            string error = (string)FindResource("Error");

            if (string.IsNullOrWhiteSpace(nameTextbox.Text))
            {
                MessageBox.Error(nameError, error);
                return;
            }

            if (int.TryParse(rowsTextBox.Text, out int rows))
            {
                if (rows < 1 || rows > 6)
                {
                    MessageBox.Error(rowsError, error);
                    return;
                }
            }
            else
            {
                MessageBox.Error(rowsError2, error);
                return;
            }

            if (string.IsNullOrEmpty(titleTextbox.Text))
            {
                MessageBox.Error(titleError, error);
                return;
            }

            TextEditor editor = skEditor.GetMainWindow().GetFileManager().GetTextEditor();

            int offset = editor.CaretOffset;
            DocumentLine line = editor.Document.GetLineByOffset(offset);

            string exampleItemName = (string)FindResource("GUIGenExampleItemName");
            string exampleItemLore = (string)FindResource("GUIGenExampleItemLore");
            string exampleItemClicked = (string)FindResource("GUIGenExampleItemClicked");

            string code = "";

            if (!string.IsNullOrEmpty(editor.Document.GetText(line.Offset, line.Length)))
            {
                code += "\n";
            }

            code += $"function {nameTextbox.Text}(p: player):";

            code += $"\n\tset {{_gui}} to chest inventory with {rowsTextBox.Text} rows named \"{titleTextbox.Text}\"\n";
            if ((bool)backgroundCheckbox.IsChecked)
                code += $"\n\tset slot (numbers between 0 and {int.Parse(rowsTextBox.Text) * 9}) of {{_gui}} to black stained glass pane";
            if ((bool)exampleItemCheckbox.IsChecked)
                code += $"\n\tset slot {int.Parse(rowsTextBox.Text) * 9 / 2} of {{_gui}} to diamond named \"&a{exampleItemName}\" with lore \"{exampleItemLore}\"\n\t";
            code += "\n\topen {_gui} to {_p}";

            code += "\n\non inventory click:";
            code += $"\n\tif name of event-inventory is \"{titleTextbox.Text}\":";
            code += "\n\t\tcancel event";
            code += "\n\t\tevent-inventory is not player's inventory";
            if ((bool)exampleItemCheckbox.IsChecked)
            {
                code += $"\n\t\tif clicked slot is {int.Parse(rowsTextBox.Text) * 9 / 2}:";
                code += $"\n\t\t\tsend \"&7{exampleItemClicked}\"";
            }
            editor.Text += code;

            guiGeneratorWindow.Close();

            editor.CaretOffset = editor.Document.TextLength;

            AddonManager.addons.ForEach(addon =>
            {
                    addon.OnGenerate(ISkEditorPlusAddon.GenerateType.GUI);
            });
        }

        private void FunctionNameChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(nameTextbox.Text))
            {
                howToOpenText.Text = "";
                return;
            }

            string hint = (string)System.Windows.Application.Current.Resources["GUIGenHint"];

            howToOpenText.Text = hint.Replace("{0}", nameTextbox.Text + "(player)");
        }
    }
}
