using AvalonEditB.Search;
using AvalonEditB;
using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Text.RegularExpressions;
using SkEditorPlus.Utilities.Vaults;
using SkEditorPlus.Utilities.Services;

namespace SkEditorPlus.Utilities.Builders
{
    public class FileBuilder
    {
        public static string newFileName = (string)Application.Current.Resources["NewFileName"];
        public static Regex regex = new(newFileName.Replace("{0}", @"[0-9]+"));

        public static void Build(string header, string tooltip = null)
        {
            var tabItem = new TabItem()
            {
                Header = header,
                ToolTip = tooltip ?? "",
                IsSelected = true,
            };
            IconElement.SetGeometry(tabItem, Geometry.Parse(GeometryUtility.Other));
            IconElement.SetHeight(tabItem, 16);
            IconElement.SetWidth(tabItem, 16);
            System.Windows.Controls.ToolTipService.SetIsEnabled(tabItem, false);

            var codeEditor = new TextEditor()
            {
                Style = Application.Current.FindResource("TextEditorStyle") as Style,
                FontFamily = new FontFamily(Properties.Settings.Default.Font),
                WordWrap = Properties.Settings.Default.Wrapping,
            };
            codeEditor.PreviewMouseWheel += TextEditorService.OnScrolling;
            codeEditor.TextChanged += TextEditorService.OnTextChanged;
            codeEditor.TextArea.TextEntering += TextEditorService.OnTextEntering;
            codeEditor.TextArea.TextView.LinkTextForegroundBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#1a94c4");
            codeEditor.TextArea.TextView.LinkTextUnderline = true;
            codeEditor.Options.AllowScrollBelowDocument = true;

            tabItem.Content = codeEditor;

            var searchPanel = SearchPanel.Install(codeEditor.TextArea);
            searchPanel.ShowReplace = true;
            searchPanel.Style = (Style)Application.Current.FindResource("SearchPanelStyle");
            searchPanel.Localization = new Data.Localization();

            APIVault.GetAPIInstance().GetTabControl().Items.Add(tabItem);
            GeometryUtility.ChangeGeometry();

            AddonVault.addons.ForEach(addon =>
            {
                addon.OnTabCreate();
            });
        }

        public static int UntitledCount()
        {
            int tabIndex = 1;
            foreach (TabItem ti in APIVault.GetAPIInstance().GetTabControl().Items)
            {
                if (regex.IsMatch(ti.Header.ToString()))
                {
                    tabIndex++;
                }
            }
            return tabIndex;
        }
    }
}
