using HandyControl.Controls;
using SkEditorPlus.Utilities.Managers;
using SkEditorPlus.Utilities.Vaults;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                //completionManager ??= new(skEditor);
                //completionManager.LoadCompletionManager(GetTextEditor());
            }

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
