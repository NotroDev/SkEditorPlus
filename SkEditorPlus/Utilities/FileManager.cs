using AvalonEditB;
using HandyControl.Controls;
using SkEditorPlus.Utilities.Builders;
using SkEditorPlus.Utilities.Managers;
using SkEditorPlus.Utilities.Services;
using SkEditorPlus.Utilities.Vaults;
using SkEditorPlus.Windows;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SkEditorPlus.Utilities
{

    public class FileManager
    {
        public static readonly string filter = "{0} (*.*)|*.*|{1} (*.sk)|*.sk".Replace("{0}", (string)Application.Current.Resources["TypeAllFiles"]).Replace("{1}", (string)Application.Current.Resources["TypeScript"]);

        public readonly TabControl tabControl;

        public Popup popup = new();

        private readonly SkEditorAPI skEditor = APIVault.GetAPIInstance();

        public CompletionManager completionManager;

        public static FileManager instance;

        public string projectPath = string.Empty;

        public FileManager()
        {
            tabControl = skEditor.GetMainWindow().tabControl;
            instance = this;
        }

        public TextEditor GetTextEditor()
        {
            TextEditor textEditor = null;
            if (tabControl.SelectedItem is TabItem tp)
                textEditor = tp.Content as TextEditor;
            return textEditor;
        }

        public static void NewFile()
        {
            FileBuilder.Build(FileBuilder.newFileName.Replace("{0}", FileBuilder.UntitledCount().ToString()));
        }

        public async void OpenFolder()
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) return;

			skEditor.GetMainWindow().fileTreeView.Items.Clear();
			projectPath = dialog.SelectedPath;
			System.Windows.Controls.TreeViewItem treeViewItem = new()
			{
				Header = IconBuilder.Build("\ue8b7", Path.GetFileName(dialog.SelectedPath)),
				Tag = dialog.SelectedPath,
				IsExpanded = true
			};
			skEditor.GetMainWindow().fileTreeView.Items.Add(treeViewItem);

			ProjectManager.instance.isFtp = false;

			await Task.Run(() => ProjectManager.instance.AddDirectoriesAndFilesAsync(new DirectoryInfo(dialog.SelectedPath), treeViewItem));

			skEditor.GetMainWindow().leftTabControl.SelectedIndex = 1;
		}


        public void FormatCode()
        {
            if (GetTextEditor() == null) return;

            QuickEditsWindow quickEditsWindow = new(skEditor);
            quickEditsWindow.ShowDialog();
        }

        public static void OpenParser()
        {
            WebBrowserService.OpenSite("Parser", "https://parser.skunity.com/");
        }

        public static void OpenDocs()
        {
            string documentation = (string)Application.Current.Resources["DocumentationTitle"];
            string link = Properties.Settings.Default.DocsLink;

            bool result = Uri.TryCreate(link, UriKind.Absolute, out Uri uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (!result)
            {
                MessageBox.Error("You provided incorrect link to documentation in the settings.\nCorrect it and try again.", "Incorrect url");
                return;
            }

            WebBrowserService.OpenSite(documentation, link);
        }

        public void OnStructureClick(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.TabControl leftTabControl = skEditor.GetSideTabControl();
            if (leftTabControl.SelectedIndex == 2)
            {
                skEditor.GetDispatcher().InvokeAsync(() => leftTabControl.SelectedIndex = -1);
            }
            else
            {
                leftTabControl.SelectedIndex = 2;
            }
        }
    }
}
