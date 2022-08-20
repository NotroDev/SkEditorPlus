using HandyControl.Controls;
using AvalonEditB;
using AvalonEditB.Document;
using System.Collections.Generic;

namespace SkEditorPlus.Windows.Generators
{
    public partial class GuiGenerator : Window
    {
        private SkEditorAPI skEditor;

        public GuiGenerator(SkEditorAPI skEditor)
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
                MessageBox.Error("Nazwa GUI jest wymagana!", "Błąd");
                return;
            }

            if (int.TryParse(rowsTextBox.Text, out int rows))
            {
                if (rows < 1 || rows > 6)
                {
                    MessageBox.Error("Liczba rzędów musi być w zakresie od 1 do 6!", "Błąd");
                    return;
                }
            }
            else
            {
                MessageBox.Error("Liczba wierszy musi być liczbą!", "Błąd");
                return;
            }

            if (string.IsNullOrEmpty(titleTextbox.Text))
            {
                MessageBox.Error("Tytuł GUI jest wymagany!", "Błąd");
                return;
            }

            TextEditor editor = skEditor.GetMainWindow().GetFileManager().GetTextEditor();

            int offset = editor.CaretOffset;
            DocumentLine line = editor.Document.GetLineByOffset(offset);

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
                code += $"\n\tset slot {int.Parse(rowsTextBox.Text) * 9/2} of {{_gui}} to diamond named \"&aPrzykładowy przedmiot\" with lore \"&7Oto opis tego\" and \"&7wspaniałego przedmiotu!\"\n\t";
            code += "\n\topen {_gui} to {_p}";

            code += "\n\non inventory click:";
            code += $"\n\tif name of event-inventory is \"{titleTextbox.Text}\":";
            code += "\n\t\tcancel event";
            code += "\n\t\tevent-inventory is not player's inventory";
            if ((bool)exampleItemCheckbox.IsChecked)
            {
                code += "\n\t\tif clicked slot is 1:";
                code += "\n\t\t\tsend \"&7Kliknąłeś przykładowy przedmiot!\"";
            }
            editor.Text += code;

            generatorWindow.Close();

            editor.CaretOffset = editor.Document.TextLength;
        }

        private void FunctionNameChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(nameTextbox.Text))
            {
                howToOpenText.Text = "";
                return;
            }
            howToOpenText.Text = $"Aby wyświetlić GUI, użyj {nameTextbox.Text}(player)";
        }
    }
}
