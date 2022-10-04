using HandyControl.Controls;
using SkEditorPlus.Windows.Generators;

namespace SkEditorPlus.Windows
{
    public partial class GenerateWindow : Window
    {
        private readonly SkEditorAPI skEditor;

        public GenerateWindow(SkEditorAPI skEditor)
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

        private void CommandClick(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandGenerator commandGenerator = new(skEditor);
            generatorWindow.Close();
            commandGenerator.ShowDialog();
        }

        private void GUIClick(object sender, System.Windows.RoutedEventArgs e)
        {
            GuiGenerator guiGenerator = new(skEditor);
            generatorWindow.Close();
            guiGenerator.ShowDialog();
        }
    }
}
