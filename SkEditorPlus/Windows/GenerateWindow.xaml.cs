using HandyControl.Controls;
using SkEditorPlus.Managers;
using SkEditorPlus.Windows.Generators;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkEditorPlus.Windows
{
    public partial class GenerateWindow : Window
    {
        private readonly SkEditorAPI skEditor;

        public GenerateWindow(SkEditorAPI skEditor)
        {
            InitializeComponent();
            this.skEditor = skEditor;
            BackgroundFixManager.FixBackground(this);

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
            GuiPreview guiPreview = new(skEditor);
            generatorWindow.Close();
            guiPreview.ShowDialog();
        }


        private void EventClick(object sender, System.Windows.RoutedEventArgs e)
        {
            EventGenerator eventGenerator = new(skEditor);
            generatorWindow.Close();
            eventGenerator.ShowDialog();
        }
    }
}
