using AvalonEditB;
using HandyControl.Controls;
using SkEditorPlus.Utilities.Managers;
using SkEditorPlus.Utilities.Vaults;
using System.IO;
using System.Windows;

namespace SkEditorPlus.Utilities.Controllers
{
    public class TabController
    {
        public static void OnTabChanged()
        {
            if (APIVault.GetAPIInstance().GetTabControl().SelectedItem is not TabItem ti) return;

            GeometryUtility.ChangeGeometry();

            var toolTip = ti.ToolTip?.ToString();
            var extension = Path.GetExtension(toolTip);

            SyntaxManager.ChangeSyntax(extension.Equals(".yml") || extension.Equals(".yaml") ? "YAML" : "Skript");

            DiscordRPCManager.Instance.SetFile(ti.Header?.ToString() ?? "none");

            if (APIVault.GetAPIInstance().IsFileOpen())
            {
                TextEditor textEditor = APIVault.GetAPIInstance().GetTextEditor();
                APIVault.GetAPIInstance().GetMainWindow().BottomBarLenght.Text = (Application.Current.FindResource("BottomBarLenght") as string).Replace("{0}", textEditor.Text.Length.ToString());
                APIVault.GetAPIInstance().GetMainWindow().BottomBarLines.Text = (Application.Current.FindResource("BottomBarLines") as string).Replace("{0}", textEditor.LineCount.ToString());
                APIVault.GetAPIInstance().GetMainWindow().BottomBarPos.Text = (Application.Current.FindResource("BottomBarPosition") as string).Replace("{0}", textEditor.CaretOffset.ToString());

                if (Properties.Settings.Default.CompletionExperiment)
                {
                    FileManager.instance.completionManager ??= new();
                    FileManager.instance.completionManager.LoadCompletionManager(textEditor);
                }
            }

            APIVault.GetAPIInstance().GetMainWindow().BottomBar.Visibility = APIVault.GetAPIInstance().IsFileOpen() && Properties.Settings.Default.BottomBarExperiment ? Visibility.Visible : Visibility.Collapsed;

            AddonVault.addons.ForEach(a => a.OnTabChanged());
        }

        public static void AddAsterisk(TabItem tabItem)
        {
            if (!tabItem.Header.ToString().EndsWith("*"))
            {
                tabItem.Header += "*";
            }
        }

        public static void RemoveAsterisk(TabItem tabItem)
        {
            if (tabItem.Header.ToString().EndsWith("*"))
            {
                tabItem.Header = tabItem.Header.ToString().TrimEnd('*');
            }
        }
    }
}
