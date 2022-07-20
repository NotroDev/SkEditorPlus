using HandyControl.Controls;
using Newtonsoft.Json.Linq;
using SkEditorPlus.Managers;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Net.Http;
using ICSharpCode.AvalonEdit;

namespace SkEditorPlus.Windows
{
    public partial class GeneratorWindow : Window
    {
        public GeneratorWindow()
        {
            InitializeComponent();
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
            TextEditor editor = FileManager.GetTextEditor();

            string code = $"\ncommand /{nameTextbox.Text}:";
            if (!string.IsNullOrEmpty(permTextbox.Text))
            {
                code += $"\n\tpermission: {permTextbox.Text}";
            }
            if (!string.IsNullOrEmpty(permTextbox.Text))
            {
                code += $"\n\tpermission message: {permMessTextbox.Text}";
            }
            if (!string.IsNullOrEmpty(permTextbox.Text))
            {
                code += $"\n\taliases: {aliasesTextbox.Text}";
            }
            if (!string.IsNullOrEmpty(permTextbox.Text))
            {
                code += $"\n\tusage: {usageTextbox.Text}";
            }
            if (!string.IsNullOrEmpty(permTextbox.Text))
            {
                code += $"\n\texecutable by: {executableByTextbox.Text}";
            }
            code += "\n\ttrigger:";

            editor.Text += code;
        }
    }
}
