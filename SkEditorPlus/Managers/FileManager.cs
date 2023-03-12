using AvalonEditB;
using AvalonEditB.Document;
using AvalonEditB.Highlighting;
using AvalonEditB.Search;
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using Renci.SshNet;
using Renci.SshNet.Async;
using SkEditorPlus.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using FontFamily = System.Windows.Media.FontFamily;
using MessageBox = HandyControl.Controls.MessageBox;
using WebView2 = Microsoft.Web.WebView2.Wpf.WebView2;

namespace SkEditorPlus.Managers
{

    public delegate void TabChangedEvent();

    public class FileManager
    {
        public static string newFileName = (string)Application.Current.Resources["NewFileName"];
        private static readonly string filter = "{0} (*.*)|*.*|{1} (*.sk)|*.sk".Replace("{0}", (string)Application.Current.Resources["TypeAllFiles"]).Replace("{1}", (string)Application.Current.Resources["TypeScript"]);

        readonly TabControl tabControl;
        public static Regex regex = new(newFileName.Replace("{0}", @"[0-9]+"));

        public Popup popup = new();

        readonly string SkriptGeometry = "M225.923 542.8C245.723 547.75 254.633 559.63 254.633 572.5C254.633 589.33 240.773 599.23 216.023 599.23C197.213 599.23 136.823 594.28 94.2531 583.39L47.7231 571.51C39.8031 569.53 34.8531 572.5 32.8731 580.42L3.1731 685.36C0.203099 693.28 4.1631 699.22 12.0831 701.2L64.5531 714.07C119.993 727.93 172.463 731.89 216.023 731.89C336.803 731.89 408.083 672.49 408.083 572.5C408.083 495.28 365.513 450.73 270.473 427.96L186.323 408.16C169.493 403.21 160.583 392.32 160.583 382.42C160.583 366.58 175.433 357.67 201.173 357.67C224.933 357.67 266.513 361.63 293.243 367.57L343.733 379.45C351.653 381.43 356.603 378.46 358.583 370.54L388.283 267.58C391.253 259.66 387.293 253.72 379.373 251.74L329.873 239.86C294.233 231.94 246.713 224.02 201.173 224.02C79.4031 224.02 8.1231 282.43 8.1231 382.42C8.1231 457.66 51.6831 501.22 147.713 523L225.923 542.8ZM821.284 542.8C805.444 511.12 773.764 490.33 743.074 476.47C739.114 474.49 740.104 469.54 743.074 468.55C773.764 456.67 797.524 437.86 815.344 404.2L894.544 249.76C899.494 240.85 895.534 234.91 885.634 234.91H744.064C736.144 234.91 732.184 237.88 728.224 243.82L679.714 337.87C660.904 373.51 643.084 406.18 600.514 406.18H591.604V13.15C591.604 5.22995 586.654 0.279956 578.734 0.279956H447.064C439.144 0.279956 434.194 5.22995 434.194 13.15V708.13C434.194 716.05 439.144 721 447.064 721H578.734C586.654 721 591.604 716.05 591.604 708.13V543.79H599.524C645.064 543.79 665.854 569.53 685.654 606.16L739.114 712.09C743.074 718.03 747.034 721 754.954 721H896.524C906.424 721 910.384 715.06 905.434 706.15L821.284 542.8Z";

        readonly string YmlGeometry = "M94.107 153c-1.833.5-4.667 1-8.5 1.5s-7.583.75-11.25.75c-7.667 0-13.417-1.25-17.25-3.75-3.833-2.667-5.75-8.167-5.75-16.5v-33.75l-13.5-20.5c-4.833-7.5-9.583-15.167-14.25-23s-9-15.5-13-23c-4-7.667-7.25-14.667-9.75-21 2.167-3 5.083-5.75 8.75-8.25 3.833-2.5 8.5-3.75 14-3.75 6.5 0 11.75 1.333 15.75 4 4.167 2.667 8.083 7.667 11.75 15l20.75 41.75h1.5c2.333-5.167 4.333-9.833 6-14l5.25-12.75 5.25-13.25 6.25-16c3-1.5 6.334-2.667 10-3.5s7.167-1.25 10.5-1.25c5.834 0 10.75 1.583 14.75 4.75 4.167 3 6.25 7.583 6.25 13.75 0 2-.416 4.417-1.25 7.25s-2.75 7.167-5.75 13c-3 5.667-7.416 13.417-13.25 23.25-5.666 9.833-13.416 22.667-23.25 38.5V153zm165.143-36.25c-2.167 1.833-5 3.333-8.5 4.5-3.333 1-7.25 1.5-11.75 1.5-6 0-11-.75-15-2.25-3.833-1.5-6.333-4.083-7.5-7.75L205 77.25c-3-9.167-5.333-17.167-7-24h-1.25l-1.25 27-.5 23.5a547.39 547.39 0 0 1-.5 23.25c-.167 7.833-.5 16.5-1 26-2.167.833-5 1.5-8.5 2-3.5.667-7 1-10.5 1-7.333 0-13.083-1.167-17.25-3.5-4-2.333-6-6.583-6-12.75l7.25-127.5c1.333-2.167 4.25-4.5 8.75-7s10.833-3.75 19-3.75c8.833 0 15.75 1.417 20.75 4.25 5 2.667 8.667 7.083 11 13.25 1.5 4 3.083 8.583 4.75 13.75 1.833 5.167 3.583 10.5 5.25 16l5.25 16.5 4.5 14.5H239l12-40.5c4-13.167 7.667-24.167 11-33 2.5-1.333 5.667-2.417 9.5-3.25 4-1 8.417-1.5 13.25-1.5 8.167 0 14.833 1.25 20 3.75 5.167 2.333 8.167 5.75 9 10.25.667 3.333 1.333 8.5 2 15.5l2.5 23.25 2.5 27.5 2.25 28 2 24.5 1 17c-2.833 1.667-5.833 2.833-9 3.5-3 .833-6.917 1.25-11.75 1.25-6.333 0-11.667-1.083-16-3.25s-6.667-6.417-7-12.75l-2-49c-.167-14.5-.5-26.667-1-36.5H278c-1.667 6.5-4.083 14.667-7.25 24.5l-11.5 37.75zm117.719 38c-7.167 0-12.834-2.083-17-6.25s-6.25-9.833-6.25-17V2.25c1.833-.333 4.75-.75 8.75-1.25 4-.667 7.833-1 11.5-1 3.833 0 7.166.333 10 1 3 .5 5.5 1.5 7.5 3s3.5 3.583 4.5 6.25 1.5 6.167 1.5 10.5v98.75h57.25c1.166 1.833 2.25 4.333 3.25 7.5 1 3 1.5 6.167 1.5 9.5 0 6.667-1.417 11.417-4.25 14.25-2.834 2.667-6.584 4-11.25 4h-67z";

        readonly string OtherGeometry = "M74.6484 36.8359C70.2539 32.832 64.1016 30.8301 56.1914 30.8301C48.2812 30.8301 39.4434 33.125 29.6777 37.7148C19.9121 42.207 13.7598 44.4531 11.2207 44.4531C8.7793 44.4531 6.43555 42.8906 4.18945 39.7656C1.94336 36.6406 0.820312 33.3203 0.820312 29.8047V27.4609C0.820312 20.625 7.75391 13.9355 21.6211 7.39258C32.0703 2.50977 43.6426 0.0683594 56.3379 0.0683594L59.2676 0.214844H62.1973C77.334 0.214844 90.2246 4.75586 100.869 13.8379C111.611 22.9199 116.982 34.8828 116.982 49.7266V52.5098C116.982 64.6191 114.59 74.3359 109.805 81.6602C105.117 88.8867 97.4023 97.334 86.6602 107.002C75.918 116.572 70.4004 121.553 70.1074 121.943C67.0801 126.045 65.0781 131.172 64.1016 137.324C63.125 143.477 62.3926 146.943 61.9043 147.725C60.6348 149.775 57.7051 150.801 53.1152 150.801H50.4785C44.2285 150.801 39.9805 149.824 37.7344 147.871C35.4883 146.113 34.3652 141.816 34.3652 134.98C34.3652 120.039 41.7383 105.977 56.4844 92.793C66.3477 84.0039 72.9395 77.1191 76.2598 72.1387C79.5801 67.1582 81.2402 61.0547 81.2402 53.8281C81.2402 46.5039 79.043 40.8398 74.6484 36.8359ZM69.375 186.689L68.9355 192.549C68.9355 197.139 67.3242 200.801 64.1016 203.535C60.9766 206.27 56.3867 207.637 50.332 207.637H47.2559C46.2793 207.441 44.9121 207.344 43.1543 207.344C41.4941 207.344 38.3691 206.025 33.7793 203.389C32.0215 202.412 31.1426 200.41 31.1426 197.383C30.6543 195.43 30.4102 194.014 30.4102 193.135L30.1172 190.938C30.1172 190.449 30.1172 189.961 30.1172 189.473L29.9707 187.129V181.855C29.9707 175.703 30.6055 171.699 31.875 169.844C33.9258 166.719 38.6621 165.156 46.084 165.156H52.2363L57.3633 165.742C61.5625 165.742 64.6387 167.109 66.5918 169.844C68.5449 172.578 69.5215 177.412 69.5215 184.346V185.811L69.375 186.689Z";

        private readonly SkEditorAPI skEditor;

        public CompletionManager completionManager;

        public event TabChangedEvent TabChanged;

        public static FileManager instance;

        public FileManager(SkEditorAPI skEditor)
        {
            tabControl = skEditor.GetMainWindow().tabControl;
            this.skEditor = skEditor;
            instance = this;
        }

        public TextEditor GetTextEditor()
        {
            TextEditor textEditor = null;
            if (tabControl.SelectedItem is TabItem tp)
                textEditor = tp.Content as TextEditor;
            return textEditor;
        }

        public TabControl GetTabControl()
        {
            return tabControl;
        }

        int UntitledCount()
        {
            int tabIndex = 1;
            foreach (TabItem ti in tabControl.Items)
            {
                if (regex.IsMatch(ti.Header.ToString()))
                {
                    tabIndex++;
                }
            }
            return tabIndex;
        }

        public void NewFile()
        {
            CreateFile(newFileName.Replace("{0}", UntitledCount().ToString()));
        }

        public void Save()
        {
            try
            {
                if (GetTextEditor() == null) return;
                if (tabControl.SelectedItem is not TabItem ti) return;

                var toolTip = ti.ToolTip ?? "";
                if (!string.IsNullOrEmpty(toolTip.ToString()))
                {
                    GetTextEditor().Save(toolTip.ToString());
                    ti.Header = ti.Header.ToString().TrimEnd('*');
                    OnTabChanged();
                    return;
                }
                SaveDialog();
            }
            catch { }
        }

        public void SaveDialog()
        {
            try
            {
                if (GetTextEditor() == null) return;
                if (tabControl.SelectedItem is not TabItem ti) return;
                SaveFileDialog saveFile = new()
                {
                    Filter = filter
                };
                if (saveFile.ShowDialog() == true)
                {
                    GetTextEditor().Save(saveFile.FileName);
                    ti.ToolTip = saveFile.FileName.ToString();
                    ti.Header = saveFile.SafeFileName;
                    OnTabChanged();
                }
            }
            catch { }
        }

        public void OpenFile()
        {
            OpenFileDialog openFileDialog = new() { Filter = filter, Multiselect = true };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var (fileName, index) in openFileDialog.FileNames.Select((v, i) => (v, i)))
                {
                    try
                    {
                        CreateFile(openFileDialog.SafeFileNames[index], fileName);
                        GetTextEditor().Load(fileName);
                        if (tabControl.SelectedItem is TabItem ti && ti.Header.ToString().EndsWith("*", StringComparison.Ordinal))
                        {
                            ti.Header = ti.Header.ToString()[..^1];
                        }
                    }
                    catch { }
                }
            }
        }

        private void AddDirectoriesAndFiles(DirectoryInfo directory, System.Windows.Controls.TreeViewItem treeViewItem)
        {
            foreach (DirectoryInfo subDirectory in directory.GetDirectories())
            {
                var subTreeViewItem = new System.Windows.Controls.TreeViewItem
                {
                    Header = CreateIcon("\ue8b7", subDirectory.Name),
                    Tag = subDirectory.FullName
                };
                treeViewItem.Items.Add(subTreeViewItem);
                AddDirectoriesAndFiles(subDirectory, subTreeViewItem);
            }

            foreach (FileInfo file in directory.GetFiles())
            {
                var fileTreeViewItem = new System.Windows.Controls.TreeViewItem
                {
                    Header = CreateIcon("\ue8a5", file.Name),
                    Tag = file.FullName
                };

                fileTreeViewItem.MouseDoubleClick += (sender, e) =>
                {
                    if (e.ChangedButton == MouseButton.Right) return;
                    var tabItem = tabControl.Items.Cast<TabItem>().FirstOrDefault(ti => ti.ToolTip.ToString() == fileTreeViewItem.Tag.ToString());
                    if (tabItem != null)
                    {
                        tabControl.SelectedItem = tabItem;
                        return;
                    }

                    CreateFile(Path.GetFileName(fileTreeViewItem.Tag.ToString()), fileTreeViewItem.Tag.ToString());
                    GetTextEditor().Load(fileTreeViewItem.Tag.ToString());
                    if (tabControl.SelectedItem is TabItem ti && ti.Header.ToString().EndsWith("*"))
                    {
                        ti.Header = ti.Header.ToString()[..^1];
                    }
                };

                fileTreeViewItem.MouseRightButtonUp += (sender, e) =>
                {
                    var contextMenu = new System.Windows.Controls.ContextMenu();
                    var openFile = new System.Windows.Controls.MenuItem
                    {
                        Header = "Open File"
                    };
                    openFile.Click += (sender, e) =>
                    {
                        CreateFile(Path.GetFileName(fileTreeViewItem.Tag.ToString()), fileTreeViewItem.Tag.ToString());
                        GetTextEditor().Load(fileTreeViewItem.Tag.ToString());
                        var ti = tabControl.SelectedItem as TabItem;
                        if (ti.Header.ToString().EndsWith("*"))
                        {
                            ti.Header = ti.Header.ToString()[..^1];
                        }
                    };
                    contextMenu.Items.Add(openFile);
                    fileTreeViewItem.ContextMenu = contextMenu;
                };

                treeViewItem.Items.Add(fileTreeViewItem);
            }
        }

        private static System.Windows.Controls.TextBlock CreateIcon(string iconString, string text)
        {
            System.Windows.Controls.TextBlock tempTextBlock = new();
            var icon = new System.Windows.Controls.TextBlock()
            {
                Text = iconString,
                FontFamily = new FontFamily("Segoe Fluent Icons"),
                Margin = new Thickness(0, 2, 0, 0)
            };
            icon.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.HighQuality);
            tempTextBlock.Inlines.Add(icon);
            tempTextBlock.Inlines.Add(" " + text);

            tempTextBlock.VerticalAlignment = VerticalAlignment.Center;
            return tempTextBlock;
        }

        public void OpenFolder()
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                System.Windows.Controls.TreeViewItem treeViewItem = new()
                {
                    Header = CreateIcon("\ue8b7", Path.GetFileName(dialog.SelectedPath)),
                    Tag = dialog.SelectedPath,
                    IsExpanded = true
                };
                skEditor.GetMainWindow().fileTreeView.Items.Add(treeViewItem);
                AddDirectoriesAndFiles(new DirectoryInfo(dialog.SelectedPath), treeViewItem);

                skEditor.GetMainWindow().leftTabControl.SelectedIndex = 1;
            }
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            TabItem tabItem = (TabItem)tabControl.SelectedItem;
            string path = tabItem.ToolTip.ToString();

            if (Properties.Settings.Default.AutoSave && !string.IsNullOrEmpty(path))
            {
                try
                {
                    GetTextEditor().Save(path);
                }
                catch { }
            }
            else
            {
                AddAsterisk(tabItem);
            }
        }

        private static void AddAsterisk(TabItem tabItem)
        {
            if (!tabItem.Header.ToString().EndsWith("*"))
            {
                tabItem.Header += "*";
            }
        }

        private void OnTextEntering(object sender, TextCompositionEventArgs e)
        {
            char ch = e.Text[0];
            if (Properties.Settings.Default.AutoSecondCharacter)
            {

                string textToReplace = "";
                switch (ch)
                {
                    case '"':
                        textToReplace = "\"\"";
                        break;
                    case '{':
                        textToReplace = "{}";
                        break;
                    case '(':
                        textToReplace = "()";
                        break;
                    case '[':
                        textToReplace = "[]";
                        break;
                    case '%':
                        textToReplace = "%%";
                        break;
                }
                if (!string.IsNullOrEmpty(textToReplace))
                {
                    TextEditor codeEditor = GetTextEditor();
                    int caretOffset = codeEditor.CaretOffset;
                    int lineStartOffset = codeEditor.Document.GetLineByOffset(caretOffset).Offset;
                    string textBeforeCaret = codeEditor.Document.GetText(lineStartOffset, caretOffset - lineStartOffset);
                    int quotesCount = textBeforeCaret.Count(c => c == '"');
                    if (quotesCount % 2 == 1)
                    {
                        return;
                    }
                    GetTextEditor().Document.Insert(GetTextEditor().CaretOffset, textToReplace);
                    e.Handled = true;
                    GetTextEditor().CaretOffset--;
                }
            }

            if (!Properties.Settings.Default.AutoNewLineAndTab) return;

            switch (ch)
            {
                case ':':
                    DocumentLine currentLine = GetTextEditor().Document.GetLineByOffset(GetTextEditor().CaretOffset);
                    string currentLineText = GetTextEditor().Document.GetText(currentLine.Offset, currentLine.Length);

                    int tabsCount = 0;
                    for (int i = 0; i < currentLineText.Length; i++)
                    {
                        if (currentLineText[i] == '\t')
                        {
                            tabsCount++;
                        }
                    }

                    if (StartsWithAny(currentLineText, new string[] { "command", "trigger", "if" }))
                    {
                        e.Handled = true;
                        GetTextEditor().Document.Insert(GetTextEditor().CaretOffset, ":\n");
                        for (int i = 0; i <= tabsCount; i++)
                        {
                            GetTextEditor().Document.Insert(GetTextEditor().CaretOffset, "\t");
                        }
                    }
                    break;

            }
        }

        private static bool StartsWithAny(string text, params string[] startsWith)
        {
            return startsWith.Any(s => text.TrimStart().StartsWith(s));
        }

        public void OnTabChanged()
        {
            if (tabControl.SelectedItem is not TabItem ti) return;

            ChangeGeometry();

            var toolTip = ti.ToolTip?.ToString();
            var extension = Path.GetExtension(toolTip);

            ChangeSyntax(extension.Equals(".yml") || extension.Equals(".yaml") ? "YAML" : "Skript");

            RPCManager.SetFile(ti.Header?.ToString() ?? "none");

            if (skEditor.IsFileOpen())
            {
                //completionManager ??= new(skEditor);
                //completionManager.LoadCompletionManager(GetTextEditor());
            }

            OnTabChangedEvent();
        }

        protected virtual void OnTabChangedEvent()
        {
            TabChanged?.Invoke();
        }

        public void CloseFile()
        {
            if (!skEditor.IsFileOpen()) return;
            var tabItem = tabControl.SelectedItem as TabItem;

            if (tabItem.Header.ToString().EndsWith("*"))
            {
                if (!ConfirmCloseFile()) return;
            }

            tabControl.SelectedIndex = tabControl.Items.IndexOf(tabItem) - 1 >= 0
                ? tabControl.Items.IndexOf(tabItem) - 1
                : tabControl.Items.IndexOf(tabItem) + 1;
            tabControl.Items.Remove(tabItem);
        }

        private bool ConfirmCloseFile()
        {
            var attention = (string)Application.Current.FindResource("Attention");
            var closeConfirmation = (string)Application.Current.FindResource("CloseConfirmation");
            var yeah = (string)Application.Current.FindResource("Yeah");
            var cancel = (string)Application.Current.FindResource("Cancel");

            var result = MessageBox.Show(new MessageBoxInfo
            {
                Message = closeConfirmation,
                Caption = attention,
                ConfirmContent = yeah,
                CancelContent = cancel,
                IconBrushKey = ResourceToken.DarkWarningBrush,
                IconKey = ResourceToken.WarningGeometry,
                Button = MessageBoxButton.OKCancel
            });

            return result == MessageBoxResult.OK;
        }

        private void EditorMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control) return;
            var textEditor = GetTextEditor();

            double fontSize = textEditor.FontSize + e.Delta / 25.0;

            textEditor.FontSize = fontSize < 6 ? 6 : fontSize > 200 ? 200 : fontSize;

            e.Handled = true;
        }

        private void ChangeGeometry()
        {
            TabItem tabItem = tabControl.SelectedItem as TabItem;

            string extension = Path.GetExtension(tabItem.ToolTip.ToString());
            string header = tabItem.Header.ToString();

            string geometry = OtherGeometry;
            int size = 14;

            string documentation = (string)Application.Current.Resources["DocumentationTitle"];
            if (header.Equals(documentation) || header.Equals("Parser"))
            {
                string geometry1 = "M150.344 166H0L15 129L37.5 91.5L51.5 72L69 52.5L89 33L117 16L142.5 4L157.5 1.5L175.5 0H1203L1052.28 857H1200.5L1191.5 880.5L1179.5 902.5L1165 925.5L1150.5 946L1132 968L1111 987L1091 1003L1066 1014.5L1044.5 1021L1023.11 1022.9L1023 1023.5L138 1023L115.5 1019L96 1011L82.5 1004L71.5 996L61 987L52.5 977.5L42 964L35 950L24 917V884.5L149.164 173.586L150.344 166Z";
                string geometry2 = "M351.923 652.8C371.723 657.75 380.633 669.63 380.633 682.5C380.633 699.33 366.773 709.23 342.023 709.23C323.213 709.23 262.823 704.28 220.253 693.39L173.723 681.51C165.803 679.53 160.853 682.5 158.873 690.42L129.173 795.36C126.203 803.28 130.163 809.22 138.083 811.2L190.553 824.07C245.993 837.93 298.463 841.89 342.023 841.89C462.803 841.89 534.083 782.49 534.083 682.5C534.083 605.28 491.513 560.73 396.473 537.96L312.323 518.16C295.493 513.21 286.583 502.32 286.583 492.42C286.583 476.58 301.433 467.67 327.173 467.67C350.933 467.67 392.513 471.63 419.243 477.57L469.733 489.45C477.653 491.43 482.603 488.46 484.583 480.54L514.283 377.58C517.253 369.66 513.293 363.72 505.373 361.74L455.873 349.86C420.233 341.94 372.713 334.02 327.173 334.02C205.403 334.02 134.123 392.43 134.123 492.42C134.123 567.66 177.683 611.22 273.713 633L351.923 652.8Z";
                string geometry3 = "M947.284 652.8C931.444 621.12 899.764 600.33 869.074 586.47C865.114 584.49 866.104 579.54 869.074 578.55C899.764 566.67 923.524 547.86 941.344 514.2L1020.54 359.76C1025.49 350.85 1021.53 344.91 1011.63 344.91H870.064C862.144 344.91 858.184 347.88 854.224 353.82L805.714 447.87C786.904 483.51 769.084 516.18 726.514 516.18H717.604L715 164.5C715 156.58 715.42 158.5 707.5 158.5C707.5 158.5 582.42 158.5 574.5 158.5C566.58 158.5 560 156.58 560 164.5C560 172.42 560.194 818.13 560.194 818.13C560.194 826.05 565.144 831 573.064 831H704.734C712.654 831 717.604 826.05 717.604 818.13V653.79H725.524C771.064 653.79 791.854 679.53 811.654 716.16L865.114 822.09C869.074 828.03 873.034 831 880.954 831H1022.52C1032.42 831 1036.38 825.06 1031.43 816.15L947.284 652.8Z";

                geometry = geometry1 + geometry2 + geometry3;
                size = 21;
            }
            else if (extension.Equals(".sk"))
            {
                geometry = SkriptGeometry;
                size = 16;
            }
            else if (extension.Equals(".yml") || extension.Equals(".yaml"))
            {
                geometry = YmlGeometry;
                size = 24;
            }

            IconElement.SetGeometry(tabItem,
                Geometry.Parse(geometry));
            IconElement.SetHeight(tabItem, size);
            IconElement.SetWidth(tabItem, size);
        }

        public void FormatCode()
        {
            if (GetTextEditor() == null) return;

            FormattingWindow formattingWindow = new(skEditor);
            formattingWindow.ShowDialog();
        }

        void TextEditorMouseHover(object sender, MouseEventArgs e)
        {
            // Quick fixes need rewriting, so they are currently disabled
            return;

            var mousePosition = GetTextEditor().GetPositionFromPoint(e.GetPosition(GetTextEditor()));

            if (mousePosition == null) return;

            int offset = GetTextEditor().Document.GetOffset(mousePosition.Value.Location);

            DocumentLine line = GetTextEditor().Document.GetLineByOffset(offset);

            string lineText = GetTextEditor().Document.GetText(line.Offset, line.Length);

            if (string.IsNullOrEmpty(lineText)) return;

            string wordAtMouse = GetWordAtMousePosition(e);

            TooltipWindow tooltipWindow = null;

            string formatSlotDescription = (string)Application.Current.FindResource("FormatSlotBadVeryBad");
            string openTutorial = (string)Application.Current.FindResource("OpenTutorial");
            string formatSlotTutorialUrl = (string)Application.Current.FindResource("FormatSlotTutorialLink");

            switch (wordAtMouse)
            {
                case "format":
                case "slot":
                    if (lineText.Contains("format slot"))
                    {
                        tooltipWindow = new("Format slot", formatSlotDescription, openTutorial, formatSlotTutorialUrl);
                    }
                    break;
                case "broadcast":
                    tooltipWindow = new("Broadcast", "W większości przypadków, użycie `send \"wiadomość\" to all players` będzie lepsze - nie wyśle wiadomości do konsoli.", "Zamień na send", "BroadcastToSend", true, TooltipWindow.FixType.FUNCTION, this);
                    break;
                default:
                    break;
            }

            if (tooltipWindow == null)
            {
                return;
                if (lineText.Contains('.'))
                {
                    Regex rg = new("(?<=\\{)(.*?)(?=\\})");
                    if (rg.IsMatch(lineText))
                        tooltipWindow = new("Zmienne z kropką", "Zmienne używające kropek mają utrudniony dostęp do każdej wartości, ponieważ przez takie ustawienie nie jest możliwe używanie pętli, więc każdą nazwę wartości należy pamiętać. Aby zapobiec temu problemowi, wśród skripterów przyjęło się używanie zmiennych grupowych.", "Napraw", "FixDotVariable", true, TooltipWindow.FixType.FUNCTION, this);
                    else return;
                }
                else return;
            }

            popup.PlacementTarget = sender as UIElement;
            popup.Placement = PlacementMode.MousePoint;
            popup.HorizontalOffset = -5;
            popup.VerticalOffset = -5;
            popup.AllowsTransparency = true;
            popup.StaysOpen = false;
            popup.Child = tooltipWindow;
            popup.PopupAnimation = PopupAnimation.Fade;
            popup.IsOpen = true;

            tooltipWindow.MouseLeave += (s, ee) =>
            {
                popup.IsOpen = false;
            };
            e.Handled = true;

        }

        private string GetWordAtMousePosition(MouseEventArgs e)
        {
            var mousePosition = GetTextEditor().GetPositionFromPoint(e.GetPosition(GetTextEditor()));

            if (mousePosition == null)
                return string.Empty;

            var line = mousePosition.Value.Line;
            var column = mousePosition.Value.Column;
            var offset = GetTextEditor().Document.GetOffset(line, column);

            if (offset >= GetTextEditor().Document.TextLength)
                offset--;

            int offsetStart = TextUtilities.GetNextCaretPosition(GetTextEditor().Document, offset, LogicalDirection.Backward, CaretPositioningMode.WordBorderOrSymbol);
            int offsetEnd = TextUtilities.GetNextCaretPosition(GetTextEditor().Document, offset, LogicalDirection.Forward, CaretPositioningMode.WordBorderOrSymbol);

            if (offsetEnd == -1 || offsetStart == -1)
                return string.Empty;

            var currentChar = GetTextEditor().Document.GetText(offset, 1);

            if (string.IsNullOrWhiteSpace(currentChar))
                return string.Empty;

            return GetTextEditor().Document.GetText(offsetStart, offsetEnd - offsetStart);
        }

        public void ChangeSyntax(string syntax)
        {
            if (GetTextEditor() == null) return;

            try
            {
                if (syntax.Equals("none"))
                {
                    GetTextEditor().SyntaxHighlighting = null;
                    return;
                }
                var appFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var highlightingFile = Path.Combine(appFolderPath, "SkEditor Plus", $"{syntax}Highlighting.xshd");
                using StreamReader s = new(highlightingFile);
                using XmlTextReader reader = new(s);
                GetTextEditor().SyntaxHighlighting =
                    AvalonEditB.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);

                Color color = (Color)ColorConverter.ConvertFromString("#f1ff63");

                HighlightingSpan span = new()
                {
                    StartExpression = new Regex("\""),
                    EndExpression = new Regex("\""),
                    StartColor = GetTextEditor().SyntaxHighlighting.GetNamedColor("String"),
                    EndColor = GetTextEditor().SyntaxHighlighting.GetNamedColor("String"),
                    RuleSet = GetTextEditor().SyntaxHighlighting.GetNamedRuleSet("ColorsPreview")
                };
                GetTextEditor().SyntaxHighlighting.MainRuleSet.Spans.Add(span);
            }
            catch (Exception e)
            {
                string message = (string)Application.Current.FindResource("SyntaxError");
                string error = (string)Application.Current.FindResource("Error");
                MessageBox.Show(message.Replace("{n}", Environment.NewLine).Replace("{0}", e.Message), error);
            }
        }

        public void OpenParser()
        {
            OpenSite("Parser", "https://parser.skunity.com/");
        }

        public void OpenDocs()
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

            OpenSite(documentation, link);
        }


        private async void OpenSite(string header, string url)
        {
            WebView2 webBrowser = new();

            TabItem tabItem = new()
            {
                Header = header,
                ToolTip = "",
                Content = webBrowser,
                IsSelected = true,
            };

            System.Windows.Controls.ToolTipService.SetIsEnabled(tabItem, false);

            tabControl.Items.Add(tabItem);

            var userDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\SkEditor+";
            var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
            await webBrowser.EnsureCoreWebView2Async(env);
            webBrowser.Source = new Uri(url);
        }

        public void Export()
        {
            if (GetTextEditor() == null) return;
            if (GetTextEditor().Document.TextLength == 0) return;

            UploadFileToFtp();
            return;

            string text = GetTextEditor().Document.Text;
            string[] lines = text.Split("\n", StringSplitOptions.RemoveEmptyEntries);

            List<int> sectionStartIndexes = new();
            List<int> sectionEndIndexes = new();

            foreach (string line in lines)
            {
                if (line.StartsWith("# Section:"))
                {
                    sectionStartIndexes.Add(Array.IndexOf(lines, line));
                }
                else if (line.StartsWith("# End"))
                {
                    sectionEndIndexes.Add(Array.IndexOf(lines, line));
                }
            }

            if (sectionStartIndexes.Count != sectionEndIndexes.Count)
            {
                MessageBox.Error("Nie udało się wyeksportować skryptu, ponieważ niektóre sekcje nie zostały poprawnie zamknięte.", "Błąd!");
                return;
            }

            List<string> sections = new();
            for (int i = 0; i < sectionStartIndexes.Count; i++)
            {
                int startIndex = sectionStartIndexes[i];
                int endIndex = sectionEndIndexes[i];

                string section = string.Join("\n", lines[startIndex..(endIndex + 1)]);

                sections.Add(section);
            }
            foreach (string section in sections)
            {
                string[] sectionLines = section.Split("\n", StringSplitOptions.RemoveEmptyEntries);

                string sectionName = sectionLines[0].Replace("# Section:", "").Trim();

                sectionLines = sectionLines[1..^1];

                while (string.IsNullOrWhiteSpace(sectionLines[0]))
                {
                    sectionLines = sectionLines[1..];
                }
                while (string.IsNullOrWhiteSpace(sectionLines[^1]))
                {
                    sectionLines = sectionLines[..^1];
                }

                string sectionText = "";
                foreach (string line in sectionLines)
                {
                    sectionText += line + "\n";
                }

                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + $"\\{sectionName}.sk";
                File.WriteAllText(path, sectionText);
            }
        }

        private async void UploadFileToFtp()
        {
            string fileName = "test.sk";
            string fileContent = GetTextEditor().Document.Text;

            try
            {
                string tempPath = Path.GetTempPath();
                string filePath = Path.Combine(tempPath, fileName);
                File.WriteAllText(filePath, fileContent);

                string hostName = "hostname";
                string userName = "username";
                int port = 2053;
                string password = "passwd";

                using var sftp = new SftpClient(hostName, port, userName, password);
                sftp.Connect();

                Stream fileStream = File.OpenRead(filePath);

                await sftp.UploadAsync(fileStream, $"plugins/scripts/{fileName}", true);
            }
            catch { }
        }

        public void CreateFile(string header, string tooltip = null)
        {
            var tabItem = new TabItem()
            {
                Header = header,
                ToolTip = tooltip ?? "",
                IsSelected = true,
            };
            IconElement.SetGeometry(tabItem, Geometry.Parse(OtherGeometry));
            IconElement.SetHeight(tabItem, 16);
            IconElement.SetWidth(tabItem, 16);
            System.Windows.Controls.ToolTipService.SetIsEnabled(tabItem, false);

            var codeEditor = new TextEditor()
            {
                Style = Application.Current.FindResource("TextEditorStyle") as Style,
                FontFamily = new FontFamily(Properties.Settings.Default.Font),
                WordWrap = Properties.Settings.Default.Wrapping,
            };
            codeEditor.PreviewMouseWheel += EditorMouseWheel;
            codeEditor.MouseHover += TextEditorMouseHover;
            codeEditor.TextChanged += OnTextChanged;
            codeEditor.TextArea.TextEntering += OnTextEntering;
            codeEditor.TextArea.TextView.LinkTextForegroundBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#1a94c4");
            codeEditor.TextArea.TextView.LinkTextUnderline = true;
            tabItem.Content = codeEditor;

            var searchPanel = SearchPanel.Install(codeEditor.TextArea);
            searchPanel.ShowReplace = true;
            searchPanel.Style = (Style)Application.Current.FindResource("SearchPanelStyle");
            searchPanel.Localization = new Data.Localization();

            tabControl.Items.Add(tabItem);
            ChangeGeometry();
        }
    }
}
