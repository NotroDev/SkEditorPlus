﻿using AvalonEditB;
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
using System.Windows.Threading;
using System.Xml;
using FontFamily = System.Windows.Media.FontFamily;
using MessageBox = HandyControl.Controls.MessageBox;
using WebView2 = Microsoft.Web.WebView2.Wpf.WebView2;

namespace SkEditorPlus.Managers
{

    public delegate void TabChangedEvent();

    public class FileManager
    {
        static string newFileName = (string)Application.Current.Resources["NewFileName"];
        static readonly string filter = "{0} (*.*)|*.*|{1} (*.sk)|*.sk".Replace("{0}", (string)Application.Current.Resources["TypeAllFiles"]).Replace("{1}", (string)Application.Current.Resources["TypeScript"]);

        readonly TabControl tabControl;
        static readonly Regex regex = new(newFileName.Replace("{0}", @"[0-9]+"));

        public Popup popup = new();

        readonly string SkriptGeometry = "M157.306.684c-.529 2.196.01 207.055.548 208.038 1.112 2.035-.273 1.939 27.928 1.939 25.358 0 25.575-.005 26.467-.64 1.749-1.246 1.659.358 1.825-32.616l.155-30.626 3.867.084c14.588.318 22.295 6.763 32.616 27.278 18.45 36.672 17.393 34.703 19.195 35.759l1.298.761h27.696c27.551 0 27.701-.003 28.512-.641 1.326-1.044 1.138-2.754-.687-6.238l-4.832-9.362-5.732-11.137-4.95-9.59-5.107-9.9-5.487-10.673c-3.725-7.275-4.674-8.991-6.274-11.343-4.516-6.635-12.493-12.962-22.642-17.956-4.317-2.125-4.904-3.587-1.89-4.709 9.258-3.447 18.588-11.393 23.643-20.137 1.631-2.821 3.856-7.113 14.348-27.68l5.576-10.827c4.97-9.397 11.136-21.907 11.136-22.595 0-.482-.366-1.175-.904-1.714l-.904-.904H295.14c-29.257 0-28.596-.031-30.191 1.43-.854.783-1.627 2.164-5.866 10.48l-5.871 11.447-4.796 9.28-3.485 6.651-1.701 3.094c-.421.766-1.028 1.762-1.349 2.213s-.583.902-.583 1c0 .846-3.885 6.04-6.591 8.811-5.064 5.185-9.7 7.177-16.92 7.267l-3.558.044-.078-44.465-.079-44.464-.695-.141c-.382-.077-1.321-.218-2.087-.313s-2.297-.314-3.403-.486l-4.021-.596-22.429-3.275-12.838-1.851-7.484-1.076c-3.116-.476-3.615-.427-3.809.379zM60.942 31.591C34.694 33.989 16.17 44.821 7.491 62.846c-.548 1.138-.996 2.263-.996 2.5s-.092.525-.205.638c-1.278 1.278-3.352 10.694-3.879 17.605-2.28 29.949 13.983 47.585 51.571 55.924.85.189 3.147.755 5.104 1.258l11.446 2.921c11.423 2.908 12.651 3.265 14.582 4.247 6.873 3.492 8.487 11.187 3.32 15.833-3.646 3.277-9.946 3.879-24.553 2.345l-6.342-.637c-1.956-.19-4.323-.467-5.259-.617l-3.867-.605a157.66 157.66 0 0 1-8.043-1.428l-3.734-.755c-.693-.139-3.129-.741-5.414-1.337-20.648-5.388-18.57-5.847-21.605 4.77l-2.476 8.649L0 200.03c.004 2.276 1.588 3.167 8.744 4.918l7.96 1.984c2.203.574 6.338 1.562 9.203 2.199l2.63.594c14.408 3.309 28.593 4.776 46.326 4.791 13.19.012 14.222-.048 21.191-1.239 23.726-4.055 39.663-15.565 46.883-33.861 1.199-3.04 2.12-6.617 3.146-12.22.544-2.969.537-16.882-.009-19.799-.782-4.177-1.052-5.282-1.965-8.043-5.417-16.391-21.149-28.215-45.096-33.895l-5.898-1.399-4.485-1.053-3.248-.782c-1.106-.275-3.264-.771-4.796-1.104s-3.549-.808-4.485-1.058-3.768-.943-6.294-1.539c-18.579-4.389-16.468-19.297 2.736-19.323 4.658-.006 16.832 1.036 23.511 2.013 4.899.716 7.359 1.214 15.159 3.07 14.076 3.348 13.986 3.33 15.185 2.934 1.665-.549 1.883-1.079 5.09-12.365l1.226-4.176 1.111-3.867c.251-.936.863-3.024 1.36-4.641a170.58 170.58 0 0 0 1.519-5.259l2.027-7.239c2.222-7.736 2.655-7.331-11.431-10.7l-7.58-1.858c-2.369-.613-15.607-3.072-19.799-3.678-7.346-1.061-11.219-1.506-16.396-1.883s-18.277-.355-22.583.039z";

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

                if (ti.ToolTip != null)
                {
                    if (ti.ToolTip.ToString() != null && ti.ToolTip.ToString() != "")
                    {
                        GetTextEditor().Save(ti.ToolTip.ToString());
                        if (ti.Header.ToString().EndsWith("*"))
                        {
                            ti.Header = ti.Header.ToString()[..^1];
                        }
                        OnTabChanged();
                        return;
                    }
                    SaveDialog();
                }
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
            OpenFileDialog openFileDialog = new()
            {
                Filter = filter,
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                for (int i = 0; i < openFileDialog.FileNames.Length; i++)
                {
                    try
                    {
                        CreateFile(openFileDialog.SafeFileNames[i], openFileDialog.FileNames[i]);
                        GetTextEditor().Load(openFileDialog.FileNames[i]);
                        TabItem ti = tabControl.SelectedItem as TabItem;
                        if (ti.Header.ToString().EndsWith("*"))
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
                System.Windows.Controls.TreeViewItem subTreeViewItem = new();
                subTreeViewItem.Header = subDirectory.Name;
                subTreeViewItem.Tag = subDirectory.FullName;
                treeViewItem.Items.Add(subTreeViewItem);

                AddDirectoriesAndFiles(subDirectory, subTreeViewItem);
            }

            foreach (FileInfo file in directory.GetFiles())
            {
                System.Windows.Controls.TreeViewItem fileTreeViewItem = new();
                fileTreeViewItem.Header = file.Name;
                fileTreeViewItem.Tag = file.FullName;

                fileTreeViewItem.MouseDoubleClick += (sender, e) =>
                {
                    if (e.ChangedButton == MouseButton.Right) return;
                    CreateFile(Path.GetFileName(fileTreeViewItem.Tag.ToString()), fileTreeViewItem.Tag.ToString());
                    GetTextEditor().Load(fileTreeViewItem.Tag.ToString());
                    TabItem ti = tabControl.SelectedItem as TabItem;
                    if (ti.Header.ToString().EndsWith("*"))
                    {
                        ti.Header = ti.Header.ToString()[..^1];
                    }
                };

                fileTreeViewItem.MouseRightButtonUp += (sender, e) =>
                {
                    System.Windows.Controls.ContextMenu contextMenu = new();
                    System.Windows.Controls.MenuItem openFile = new()
                    {
                        Header = "Open File"
                    };
                    openFile.Click += (sender, e) =>
                    {
                        CreateFile(Path.GetFileName(fileTreeViewItem.Tag.ToString()), fileTreeViewItem.Tag.ToString());
                        GetTextEditor().Load(fileTreeViewItem.Tag.ToString());
                        TabItem ti = tabControl.SelectedItem as TabItem;
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

        public void OpenFolder()
        {
            string dirPath;
            using var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            dirPath = folderBrowserDialog.SelectedPath;
            //skEditor.GetMainWindow().LeftTabControl.SelectedItem = skEditor.GetMainWindow().FilesView.Parent;
            //Dispatcher.CurrentDispatcher.BeginInvoke((Action)(() => skEditor.GetMainWindow().FilesView.Path = dirPath));
            //skEditor.GetMainWindow().FilesView.LoadFiles();



            return;
            using var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                System.Windows.Controls.TreeViewItem treeViewItem = new()
                {
                    Header = Path.GetFileName(dialog.SelectedPath),
                    Tag = dialog.SelectedPath
                };
                //skEditor.GetMainWindow().fileTreeView.Items.Add(treeViewItem);
                AddDirectoriesAndFiles(new DirectoryInfo(dialog.SelectedPath), treeViewItem);
            }
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            CheckForHex();

            TabItem tabItem = (TabItem)tabControl.SelectedItem;

            if (Properties.Settings.Default.AutoSave)
            {
                string path = tabItem.ToolTip.ToString();

                if (string.IsNullOrWhiteSpace(Path.GetFileName(path)))
                {
                    AddAsterisk(tabItem);
                }

                if (!string.IsNullOrEmpty(path))
                {
                    try
                    {
                        GetTextEditor().Save(path);
                    }
                    catch { }
                    return;
                }
                AddAsterisk(tabItem);
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

                    if (Clsw(currentLineText, new string[] { "command", "trigger", "if" }))
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

        private void CheckForHex()
        {
            for (int i = 0; i < GetTextEditor().Document.LineCount; i++)
            {
                DocumentLine line = GetTextEditor().Document.GetLineByNumber(i + 1);
                string lineText = GetTextEditor().Document.GetText(line.Offset, line.Length);

                foreach (Match match in Regex.Matches(lineText, "<#(#)?(?:[0-9a-fA-F]{3}){1,2}>").Cast<Match>())
                {
                    string hex = match.Value;
                    hex = hex.Replace("<##", "#");
                    hex = hex.Replace("<", "");
                    hex = hex.Replace(">", "");

                    Color color = (Color)ColorConverter.ConvertFromString(hex);

                    HighlightingSpan span = new()
                    {
                        StartExpression = new Regex(match.Value),
                        EndExpression = new Regex(""),
                        SpanColor = new HighlightingColor()
                        {
                            Foreground = new SimpleHighlightingBrush(color)
                        },
                        SpanColorIncludesStart = true,
                        SpanColorIncludesEnd = true,
                    };

                    GetTextEditor().SyntaxHighlighting.GetNamedRuleSet("BracedExpressionAndColorsRuleSet").Spans.Add(span);
                    GetTextEditor().SyntaxHighlighting.MainRuleSet.Spans.Add(span);
                }
            }
        }

        private static bool Clsw(string text, string[] startsWith)
        {
            foreach (string s in startsWith)
            {
                if (text.Replace("\t", "").StartsWith(s))
                {
                    return true;
                }
            }
            return false;
        }

        public void OnTabChanged()
        {
            if (tabControl.SelectedItem is not TabItem ti) return;

            ChangeGeometry();

            if (ti.ToolTip != null)
            {
                string extension = Path.GetExtension(ti.ToolTip.ToString());

                if (extension.Equals(".yml") || extension.Equals(".yaml"))
                {
                    ChangeSyntax("YAML");
                }
                else
                {
                    ChangeSyntax("Skript");
                }
            }
            if (ti.Header != null)
            {
                RPCManager.SetFile(ti.Header.ToString());
            }
            else
            {
                RPCManager.SetFile("none");
            }

            if (skEditor.IsFileOpen())
            {
                completionManager ??= new(skEditor);
                completionManager.LoadCompletionManager(GetTextEditor());
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
            TabItem tabItem = tabControl.SelectedItem as TabItem;

            if (tabItem.Header.ToString().EndsWith("*"))
            {
                string attention = (string)Application.Current.FindResource("Attention");
                string closeConfirmation = (string)Application.Current.FindResource("CloseConfirmation");
                string yeah = (string)Application.Current.FindResource("Yeah");
                string cancel = (string)Application.Current.FindResource("Cancel");

                MessageBoxResult result = MessageBox.Show(new MessageBoxInfo
                {
                    Message = closeConfirmation,
                    Caption = attention,
                    ConfirmContent = yeah,
                    CancelContent = cancel,
                    IconBrushKey = ResourceToken.DarkWarningBrush,
                    IconKey = ResourceToken.WarningGeometry,
                    Button = MessageBoxButton.OKCancel

                });

                if (result != MessageBoxResult.OK)
                {
                    return;
                }
            }

            if (tabControl.Items.IndexOf(tabItem) - 1 >= 0)
            {
                tabControl.SelectedIndex = tabControl.Items.IndexOf(tabItem) - 1;
            }
            else
            {
                tabControl.SelectedIndex = tabControl.Items.IndexOf(tabItem) + 1;
            }
            tabControl.Items.Remove(tabItem);
        }

        private void EditorMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                double fontSize = GetTextEditor().FontSize + e.Delta / 25.0;

                if (fontSize < 6)
                    GetTextEditor().FontSize = 6;
                else
                {
                    if (fontSize > 200)
                        GetTextEditor().FontSize = 200;
                    else
                        GetTextEditor().FontSize = fontSize;
                }

                // TODO: We need to make some space at bottom, so user can scroll past the end of the file


                e.Handled = true;
            }
        }

        private void ChangeGeometry()
        {
            TabItem tabItem = tabControl.SelectedItem as TabItem;

            string extension = Path.GetExtension(tabItem.ToolTip.ToString());
            string header = tabItem.Header.ToString();

            string geometry;
            int size;

            string documentation = (string)Application.Current.Resources["DocumentationTitle"];
            if (header.Equals(documentation) || header.Equals("Parser"))
            {
                geometry = "M11.9317 32.369C24.3737 13.608 37.6557 3.875 54.4347 1.222C54.8907 1.149 132.752 1.07 227.459 1.045L399.653 1L399.482 1.43333C399.021 4.93716 393.861 34.3824 375.262 139.751C368.76 176.589 368.332 179.007 367.635 182.905C367.39 184.274 366.942 186.813 366.639 188.548C366.336 190.282 365.888 192.822 365.643 194.191C365.398 195.56 364.95 198.1 364.647 199.834C364.345 201.568 363.896 204.108 363.651 205.477C363.406 206.846 362.958 209.386 362.656 211.12C362.353 212.855 361.905 215.394 361.66 216.763C361.415 218.133 360.967 220.672 360.664 222.407C360.361 224.141 359.912 226.68 359.665 228.05C359.418 229.419 358.968 231.959 358.665 233.693C358.362 235.427 356.188 247.676 353.833 260.913C351.478 274.149 347.355 297.34 344.672 312.448L339.792 339.917L194.211 339.914C96.919 339.912 47.645 339.799 45.659 339.571C24.018 337.092 8.11699 319.277 8.19099 297.593C8.20699 293.113 8.38099 291.964 13.061 265.56C15.731 250.498 18.153 236.83 18.444 235.187C18.735 233.544 19.174 231.079 19.419 229.71C19.664 228.34 20.112 225.801 20.415 224.066C20.718 222.332 21.166 219.793 21.411 218.423C21.656 217.054 22.104 214.515 22.407 212.78C22.709 211.046 23.157 208.506 23.402 207.137C23.647 205.768 24.096 203.228 24.398 201.494C24.701 199.759 25.149 197.22 25.394 195.851C25.639 194.481 26.087 191.942 26.39 190.207C26.693 188.473 27.141 185.934 27.386 184.564C28.136 180.373 28.714 177.101 34.011 147.054C39.568 115.528 40.729 108.928 41.161 106.39C41.316 105.477 41.765 102.938 42.158 100.747C42.55 98.556 42.999 96.017 43.154 95.104C43.309 94.191 43.757 91.651 44.149 89.461C44.542 87.27 44.99 84.73 45.145 83.817C45.3 82.905 45.748 80.365 46.141 78.174C46.534 75.983 46.982 73.444 47.138 72.531C47.7 69.23 49.162 60.85 49.657 58.091L49.9911 56.2285C19.1771 56.2528 0.0798671 56.2573 0.0496572 56.236C-0.724343 55.695 7.74366 38.685 11.9317 32.369ZM186.224 56.0815V68.777C186.224 77.424 186.105 82.358 185.892 82.49C185.411 82.787 185.434 269.212 185.914 269.212C186.125 269.212 186.178 269.499 186.045 269.917C185.894 270.392 185.941 270.548 186.189 270.395C186.517 270.192 186.704 270.601 186.595 271.286C186.574 271.423 186.668 271.506 186.805 271.47C187.239 271.355 188.563 271.905 188.418 272.14C188.152 272.57 235.757 272.263 236.153 271.831C236.419 271.542 236.404 271.5 236.1 271.672C235.457 272.035 235.609 271.705 236.406 271.006C236.802 270.658 237.003 270.583 236.852 270.84C236.661 271.164 236.701 271.23 236.984 271.055C237.207 270.917 237.309 270.595 237.211 270.34C237.113 270.084 237.141 269.876 237.272 269.876C237.403 269.876 237.47 257.477 237.42 242.324L237.33 214.772H240.344C242.535 214.772 243.294 214.876 243.122 215.154C242.95 215.432 243.269 215.476 244.289 215.312C245.327 215.146 245.629 215.19 245.45 215.48C245.274 215.764 245.499 215.813 246.272 215.658C247.001 215.513 247.265 215.56 247.112 215.808C246.963 216.05 247.124 216.096 247.593 215.947C248.039 215.805 248.299 215.866 248.299 216.113C248.299 216.327 248.523 216.417 248.797 216.311C249.071 216.206 249.295 216.281 249.295 216.477C249.295 216.674 249.526 216.746 249.81 216.637C250.146 216.508 250.243 216.572 250.087 216.824C249.929 217.081 250.033 217.138 250.402 216.996C250.706 216.88 250.954 216.938 250.954 217.125C250.954 217.313 251.111 217.369 251.303 217.251C251.768 216.964 255.291 219.116 255.035 219.53C254.925 219.708 254.992 219.757 255.184 219.638C255.376 219.52 255.884 219.732 256.314 220.11C256.744 220.488 256.886 220.688 256.629 220.555C256.32 220.394 256.244 220.443 256.403 220.701C256.535 220.915 256.779 221.006 256.946 220.903C257.331 220.665 260.34 223.651 260.102 224.035C260.004 224.194 260.184 224.527 260.502 224.776C261.031 225.191 261.041 225.173 260.622 224.564C260.371 224.199 260.56 224.322 261.042 224.837C261.524 225.353 261.818 225.936 261.696 226.133C261.568 226.342 261.628 226.397 261.841 226.266C262.218 226.033 263.003 227.133 262.727 227.508C262.642 227.624 262.672 227.644 262.792 227.553C263.144 227.29 265.281 230.683 265.025 231.097C264.898 231.303 264.939 231.382 265.116 231.273C265.293 231.163 265.711 231.536 266.045 232.1C266.379 232.665 266.482 233.023 266.276 232.895C266.034 232.746 265.97 232.846 266.097 233.176C266.205 233.458 266.408 233.619 266.547 233.532C266.687 233.446 267.737 235.186 268.88 237.399C270.024 239.611 270.789 241.316 270.581 241.188C270.3 241.014 270.301 241.17 270.585 241.794C270.796 242.257 271.094 242.557 271.248 242.462C271.402 242.367 271.429 242.545 271.309 242.859C271.162 243.243 271.216 243.351 271.475 243.191C271.734 243.031 271.788 243.139 271.641 243.523C271.494 243.907 271.548 244.015 271.807 243.855C272.066 243.694 272.12 243.803 271.973 244.187C271.826 244.57 271.88 244.679 272.139 244.519C272.398 244.358 272.452 244.467 272.305 244.85C272.158 245.234 272.212 245.343 272.471 245.182C272.73 245.022 272.784 245.131 272.637 245.514C272.49 245.898 272.544 246.007 272.803 245.846C273.062 245.686 273.116 245.794 272.969 246.178C272.822 246.562 272.876 246.67 273.135 246.51C273.394 246.35 273.448 246.458 273.301 246.842C273.153 247.226 273.208 247.334 273.467 247.174C273.726 247.014 273.78 247.122 273.633 247.506C273.485 247.89 273.54 247.998 273.799 247.838C274.058 247.678 274.112 247.786 273.965 248.17C273.817 248.554 273.871 248.662 274.131 248.502C274.39 248.342 274.444 248.45 274.297 248.834C274.149 249.218 274.203 249.326 274.463 249.166C274.722 249.006 274.776 249.114 274.629 249.498C274.481 249.882 274.535 249.99 274.795 249.83C275.054 249.67 275.108 249.778 274.961 250.162C274.813 250.546 274.867 250.654 275.127 250.494C275.386 250.333 275.44 250.442 275.292 250.826C275.145 251.209 275.199 251.318 275.458 251.158C275.718 250.997 275.772 251.106 275.624 251.489C275.477 251.873 275.531 251.982 275.79 251.821C276.05 251.661 276.104 251.77 275.956 252.153C275.809 252.537 275.863 252.646 276.122 252.485C276.382 252.325 276.436 252.433 276.288 252.817C276.141 253.201 276.195 253.309 276.454 253.149C276.713 252.989 276.768 253.097 276.62 253.481C276.473 253.865 276.527 253.973 276.786 253.813C277.045 253.653 277.1 253.761 276.952 254.145C276.805 254.529 276.859 254.637 277.118 254.477C277.377 254.317 277.431 254.425 277.284 254.809C277.137 255.193 277.191 255.301 277.45 255.141C277.709 254.981 277.763 255.089 277.616 255.473C277.469 255.857 277.523 255.965 277.782 255.805C278.041 255.645 278.095 255.753 277.948 256.137C277.801 256.521 277.855 256.629 278.114 256.469C278.373 256.309 278.427 256.417 278.28 256.801C278.12 257.218 278.179 257.298 278.501 257.099C278.793 256.918 278.861 256.955 278.706 257.207C278.576 257.417 278.623 257.851 278.81 258.172C278.997 258.493 279.158 258.606 279.168 258.423C279.178 258.241 280.522 260.686 282.153 263.856C283.784 267.027 285.088 269.738 285.049 269.881C285.011 270.024 285.076 270.119 285.193 270.091C285.612 269.994 286.786 271.298 286.58 271.631C286.46 271.825 286.534 271.869 286.754 271.733C286.964 271.603 287.137 271.65 287.137 271.837C287.137 272.025 287.386 272.083 287.689 271.966C287.993 271.85 288.157 271.892 288.053 272.06C287.939 272.245 297.862 272.365 313.195 272.365C328.528 272.365 338.451 272.245 338.337 272.06C338.233 271.891 338.403 271.852 338.718 271.973C339.031 272.093 339.207 272.061 339.108 271.901C338.923 271.601 339.706 270.771 339.948 271.013C340.022 271.087 340.083 270.414 340.083 269.516C340.083 268.618 339.962 267.884 339.814 267.884C339.667 267.884 339.63 267.748 339.733 267.582C339.965 267.206 339.596 266.178 339.296 266.364C339.175 266.438 338.349 265.146 337.46 263.493C336.571 261.839 336.014 260.591 336.223 260.72C336.506 260.895 336.505 260.741 336.22 260.115C336.009 259.652 335.711 259.352 335.557 259.447C335.403 259.542 335.376 259.363 335.496 259.05C335.643 258.666 335.589 258.558 335.33 258.718C335.071 258.878 335.017 258.77 335.164 258.386C335.311 258.002 335.257 257.894 334.998 258.054C334.739 258.214 334.685 258.106 334.832 257.722C334.979 257.338 334.925 257.23 334.666 257.39C334.407 257.55 334.353 257.442 334.5 257.058C334.66 256.641 334.601 256.561 334.279 256.76C334 256.933 333.919 256.903 334.058 256.679C334.313 256.266 333.976 255.21 333.653 255.409C333.532 255.484 332.705 254.192 331.816 252.538C330.927 250.885 330.371 249.637 330.58 249.766C330.863 249.941 330.862 249.787 330.576 249.16C330.366 248.698 330.068 248.397 329.914 248.492C329.76 248.587 329.733 248.409 329.853 248.096C330 247.712 329.946 247.603 329.687 247.764C329.428 247.924 329.373 247.816 329.521 247.432C329.668 247.048 329.614 246.94 329.355 247.1C329.096 247.26 329.042 247.152 329.189 246.768C329.336 246.384 329.282 246.276 329.023 246.436C328.764 246.596 328.71 246.488 328.857 246.104C329.017 245.687 328.958 245.607 328.636 245.806C328.357 245.979 328.276 245.949 328.415 245.724C328.67 245.312 328.333 244.255 328.009 244.455C327.889 244.53 327.062 243.238 326.173 241.584C325.284 239.93 324.728 238.683 324.937 238.812C325.22 238.987 325.219 238.832 324.933 238.206C324.723 237.743 324.424 237.443 324.271 237.538C324.117 237.633 324.089 237.455 324.21 237.141C324.357 236.757 324.303 236.649 324.044 236.809C323.784 236.969 323.73 236.861 323.878 236.477C324.025 236.093 323.971 235.985 323.712 236.145C323.453 236.306 323.398 236.197 323.546 235.813C323.693 235.43 323.639 235.321 323.38 235.481C323.121 235.642 323.066 235.533 323.214 235.15C323.374 234.732 323.315 234.653 322.993 234.852C322.713 235.024 322.633 234.994 322.771 234.77C323.026 234.357 322.689 233.301 322.366 233.501C322.245 233.575 321.419 232.283 320.53 230.63C319.641 228.976 319.085 227.728 319.293 227.857C319.576 228.032 319.576 227.878 319.29 227.251C319.079 226.789 318.781 226.489 318.627 226.584C318.474 226.679 318.446 226.5 318.566 226.187C318.714 225.803 318.66 225.695 318.4 225.855C318.141 226.015 318.087 225.907 318.234 225.523C318.382 225.139 318.328 225.031 318.069 225.191C317.809 225.351 317.755 225.243 317.903 224.859C318.05 224.475 317.996 224.367 317.737 224.527C317.477 224.687 317.423 224.579 317.571 224.195C317.731 223.778 317.671 223.698 317.35 223.897C317.07 224.07 316.99 224.04 317.128 223.816C317.383 223.403 317.046 222.347 316.723 222.546C316.602 222.621 315.776 221.329 314.887 219.675C313.998 218.021 313.441 216.774 313.65 216.903C313.933 217.078 313.932 216.924 313.647 216.297C313.436 215.835 313.138 215.534 312.984 215.629C312.831 215.724 312.803 215.546 312.923 215.232C313.071 214.849 313.016 214.74 312.757 214.901C312.498 215.061 312.444 214.952 312.591 214.569C312.751 214.152 312.692 214.072 312.37 214.271C312.079 214.451 312.01 214.414 312.166 214.162C312.295 213.952 312.248 213.519 312.061 213.198C311.874 212.877 311.717 212.759 311.711 212.937C311.706 213.114 311.469 212.928 311.184 212.521C310.9 212.115 310.774 211.597 310.905 211.369C311.036 211.141 311.019 211.068 310.867 211.206C310.555 211.489 309.33 209.806 309.568 209.421C309.652 209.286 309.545 209.066 309.332 208.934C309.074 208.775 309.025 208.851 309.185 209.16C309.318 209.417 309.196 209.343 308.915 208.997C308.633 208.65 308.475 208.25 308.564 208.107C308.652 207.964 308.55 207.739 308.336 207.607C308.078 207.447 308.029 207.523 308.188 207.833C308.321 208.089 308.052 207.856 307.591 207.316C307.129 206.775 306.823 206.261 306.911 206.174C307.132 205.952 304.006 202.819 303.733 202.989C303.61 203.064 303.001 202.668 302.378 202.107C301.755 201.547 301.544 201.292 301.909 201.54C302.517 201.954 302.535 201.943 302.12 201.414C301.871 201.097 301.553 200.908 301.412 200.995C301.272 201.082 300.729 200.83 300.205 200.437C298.896 199.452 298.466 199.016 299.087 199.303C299.361 199.429 299.224 199.267 298.782 198.942C298.341 198.617 297.743 198.349 297.454 198.346C297.166 198.343 296.962 198.168 297.002 197.957C297.042 197.746 296.847 197.486 296.57 197.38C296.264 197.262 296.151 197.325 296.283 197.539C296.403 197.733 296.196 197.701 295.824 197.468C295.451 197.236 295.235 196.903 295.342 196.729C295.45 196.555 295.366 196.518 295.156 196.648C294.752 196.898 293.321 196.084 293.626 195.779C293.724 195.681 293.566 195.509 293.275 195.398C292.973 195.281 292.831 195.333 292.945 195.518C293.055 195.696 291.98 195.216 290.556 194.453C289.132 193.689 287.92 192.953 287.862 192.817C287.805 192.68 287.571 192.64 287.344 192.727C287.116 192.815 286.646 192.574 286.3 192.191C285.588 191.405 285.97 190.337 286.934 190.42C287.228 190.445 287.469 190.324 287.469 190.151C287.469 189.978 287.605 189.92 287.771 190.023C288.176 190.273 289.167 189.875 288.963 189.544C288.788 189.261 290.209 188.774 291.721 188.598C291.96 188.57 292.084 188.432 291.996 188.29C291.908 188.148 292.28 187.777 292.822 187.466C293.364 187.155 293.718 187.046 293.608 187.223C293.497 187.403 293.635 187.458 293.923 187.347C294.205 187.239 294.372 187.047 294.293 186.92C294.215 186.793 294.456 186.499 294.828 186.266C295.296 185.974 295.431 185.963 295.264 186.233C295.082 186.529 295.193 186.532 295.728 186.246C296.115 186.038 296.432 185.791 296.432 185.695C296.432 185.599 296.282 185.613 296.1 185.726C295.917 185.839 295.768 185.865 295.768 185.784C295.768 185.551 297.465 184.5 297.958 184.428C298.204 184.392 298.52 184.234 298.663 184.078C298.805 183.922 298.697 183.904 298.423 184.038C298.149 184.173 298.277 183.974 298.707 183.596C299.137 183.217 299.602 182.978 299.741 183.064C299.88 183.15 300.276 182.888 300.62 182.482L301.245 181.743L300.498 182.375C298.971 183.668 299.826 182.648 302.149 180.406C303.468 179.133 304.178 178.539 303.726 179.087L302.905 180.083L303.807 179.288C304.304 178.851 304.641 178.381 304.556 178.244C304.471 178.107 304.711 177.643 305.089 177.213C305.467 176.783 305.668 176.641 305.534 176.898C305.373 177.207 305.422 177.283 305.68 177.124C305.894 176.992 305.997 176.767 305.908 176.623C305.82 176.48 305.978 176.08 306.259 175.734C306.546 175.381 306.68 175.323 306.565 175.602C306.452 175.876 306.625 175.738 306.95 175.297C307.275 174.855 307.544 174.258 307.546 173.969C307.549 173.68 307.724 173.477 307.935 173.516C308.146 173.556 308.406 173.362 308.512 173.084C308.63 172.778 308.567 172.665 308.353 172.798C308.159 172.917 308.191 172.711 308.424 172.338C308.656 171.966 308.95 171.725 309.077 171.804C309.204 171.882 309.397 171.715 309.505 171.433C309.631 171.104 309.567 171.003 309.326 171.152C309.119 171.28 309.223 170.922 309.557 170.358C309.89 169.793 310.27 169.396 310.4 169.477C310.735 169.684 311.084 168.642 310.821 168.218C310.682 167.993 310.763 167.963 311.042 168.136C311.364 168.335 311.424 168.255 311.264 167.838C311.116 167.454 311.17 167.346 311.43 167.506C311.689 167.666 311.743 167.558 311.595 167.174C311.448 166.79 311.502 166.682 311.761 166.842C312.021 167.002 312.075 166.894 311.927 166.51C311.78 166.126 311.834 166.018 312.093 166.178C312.353 166.338 312.407 166.23 312.259 165.846C312.112 165.462 312.166 165.354 312.425 165.514C312.685 165.675 312.739 165.566 312.591 165.182C312.444 164.799 312.498 164.69 312.757 164.85C313.016 165.011 313.071 164.902 312.923 164.519C312.803 164.205 312.831 164.027 312.984 164.122C313.138 164.217 313.436 163.916 313.647 163.454C313.925 162.843 313.928 162.676 313.656 162.844C313.451 162.971 314.084 161.576 315.063 159.743C316.042 157.91 316.941 156.47 317.059 156.543C317.379 156.741 317.713 155.681 317.46 155.271C317.321 155.047 317.402 155.017 317.681 155.19C318.003 155.389 318.063 155.309 317.903 154.892C317.755 154.508 317.809 154.4 318.069 154.56C318.328 154.72 318.382 154.612 318.234 154.228C318.087 153.844 318.141 153.736 318.4 153.896C318.66 154.056 318.714 153.948 318.566 153.564C318.419 153.18 318.473 153.072 318.732 153.232C318.992 153.392 319.046 153.284 318.898 152.9C318.751 152.516 318.805 152.408 319.064 152.568C319.324 152.729 319.378 152.62 319.23 152.236C319.083 151.853 319.137 151.744 319.396 151.904C319.655 152.065 319.71 151.956 319.562 151.572C319.442 151.259 319.47 151.081 319.623 151.176C319.777 151.271 320.075 150.97 320.286 150.508C320.564 149.897 320.567 149.73 320.295 149.898C320.09 150.025 320.723 148.63 321.702 146.797C322.681 144.964 323.58 143.524 323.698 143.597C324.018 143.795 324.352 142.735 324.099 142.325C323.96 142.101 324.041 142.071 324.32 142.244C324.642 142.443 324.702 142.363 324.542 141.946C324.394 141.562 324.448 141.454 324.708 141.614C324.967 141.774 325.021 141.666 324.873 141.282C324.726 140.898 324.78 140.79 325.039 140.95C325.299 141.11 325.353 141.002 325.205 140.618C325.058 140.234 325.112 140.126 325.371 140.286C325.631 140.446 325.685 140.338 325.537 139.954C325.39 139.57 325.444 139.462 325.703 139.622C325.963 139.782 326.017 139.674 325.869 139.29C325.722 138.906 325.776 138.798 326.035 138.958C326.294 139.119 326.349 139.01 326.201 138.626C326.081 138.313 326.109 138.135 326.262 138.23C326.416 138.325 326.714 138.024 326.925 137.562C327.203 136.951 327.206 136.784 326.934 136.952C326.729 137.079 327.362 135.684 328.341 133.851C329.32 132.018 330.219 130.578 330.337 130.651C330.657 130.849 330.991 129.789 330.738 129.379C330.6 129.155 330.68 129.125 330.959 129.298C331.281 129.497 331.341 129.417 331.181 129C331.033 128.616 331.087 128.508 331.347 128.668C331.606 128.828 331.66 128.72 331.512 128.336C331.365 127.952 331.419 127.844 331.678 128.004C331.938 128.164 331.992 128.056 331.844 127.672C331.697 127.288 331.751 127.18 332.01 127.34C332.27 127.5 332.324 127.392 332.176 127.008C332.029 126.624 332.083 126.516 332.342 126.676C332.602 126.836 332.656 126.728 332.508 126.344C332.361 125.96 332.415 125.852 332.674 126.012C332.933 126.172 332.988 126.064 332.84 125.68C332.72 125.367 332.748 125.189 332.901 125.284C333.055 125.379 333.353 125.078 333.564 124.616C333.859 123.968 333.856 123.831 333.551 124.02C333.333 124.155 333.801 123.081 334.592 121.634C335.383 120.188 336.149 118.957 336.295 118.9C336.441 118.842 336.478 118.581 336.377 118.319C336.277 118.057 336.36 117.842 336.562 117.842C336.764 117.842 336.929 117.385 336.929 116.827C336.929 116.234 336.791 115.897 336.598 116.017C336.415 116.129 336.266 115.984 336.266 115.694C336.266 115.403 336.042 115.08 335.77 114.976C335.497 114.871 335.355 114.653 335.455 114.492C335.555 114.33 335.38 114.297 335.067 114.417C334.751 114.538 334.581 114.499 334.686 114.33C334.8 114.145 324.942 114.025 309.71 114.025C295.814 114.025 284.623 114.152 284.721 114.309C284.817 114.466 284.56 114.487 284.149 114.357C283.64 114.195 283.475 114.237 283.629 114.487C283.754 114.689 283.698 114.873 283.505 114.896C283.312 114.919 283.017 114.956 282.849 114.979C282.459 115.033 282.003 115.488 281.95 115.878C281.935 115.985 281.915 116.145 281.896 116.296C281.885 116.381 281.875 116.464 281.867 116.534C281.844 116.727 281.673 116.791 281.486 116.675C281.298 116.56 281.179 116.588 281.219 116.739C281.354 117.236 280.787 118.519 280.509 118.347C280.146 118.123 279.772 119.133 280.05 119.583C280.189 119.808 280.108 119.837 279.829 119.665C279.507 119.466 279.448 119.546 279.608 119.963C279.755 120.347 279.701 120.455 279.442 120.295C279.183 120.135 279.129 120.243 279.276 120.627C279.423 121.011 279.369 121.119 279.11 120.959C278.851 120.798 278.797 120.907 278.944 121.291C279.064 121.604 279.037 121.782 278.883 121.687C278.729 121.592 278.431 121.893 278.22 122.355C277.935 122.982 277.934 123.136 278.217 122.961C278.426 122.832 277.869 124.08 276.98 125.733C276.091 127.387 275.265 128.679 275.144 128.604C274.821 128.405 274.484 129.461 274.739 129.874C274.878 130.098 274.797 130.128 274.518 129.955C274.196 129.756 274.137 129.836 274.297 130.253C274.444 130.637 274.39 130.745 274.131 130.585C273.871 130.425 273.817 130.533 273.965 130.917C274.112 131.301 274.058 131.409 273.799 131.249C273.54 131.089 273.485 131.197 273.633 131.581C273.78 131.965 273.726 132.073 273.467 131.913C273.208 131.753 273.153 131.861 273.301 132.245C273.421 132.558 273.394 132.737 273.24 132.642C273.086 132.547 272.788 132.847 272.577 133.31C272.349 133.81 272.33 134.066 272.531 133.942C272.732 133.818 272.713 134.074 272.485 134.574C272.274 135.037 271.976 135.337 271.822 135.242C271.669 135.147 271.641 135.326 271.761 135.639C271.921 136.056 271.862 136.136 271.54 135.937C271.267 135.768 271.181 135.794 271.312 136.007C271.428 136.194 271.08 137.151 270.539 138.133C269.998 139.114 269.548 139.768 269.539 139.585C269.531 139.402 269.37 139.516 269.184 139.837C268.997 140.158 268.95 140.591 269.079 140.801C269.235 141.053 269.166 141.09 268.875 140.91C268.553 140.711 268.493 140.791 268.653 141.208C268.801 141.591 268.747 141.7 268.488 141.54C268.228 141.379 268.174 141.488 268.322 141.871C268.469 142.255 268.415 142.364 268.156 142.203C267.896 142.043 267.842 142.152 267.99 142.535C268.11 142.849 268.082 143.027 267.929 142.932C267.775 142.837 267.477 143.138 267.266 143.6C266.976 144.237 266.976 144.383 267.268 144.202C267.48 144.071 267.081 145.025 266.381 146.321C265.681 147.618 265.01 148.618 264.891 148.544C264.771 148.471 264.423 148.937 264.115 149.581C263.808 150.225 263.707 150.659 263.891 150.545C264.085 150.425 264.142 150.554 264.028 150.852C263.919 151.135 263.674 151.269 263.482 151.15C263.279 151.024 263.232 151.094 263.37 151.317C263.62 151.721 262.806 153.152 262.501 152.847C262.403 152.749 262.231 152.907 262.119 153.198C261.988 153.541 262.048 153.646 262.292 153.495C262.499 153.367 262.395 153.725 262.062 154.29C261.728 154.854 261.35 155.252 261.221 155.172C261.093 155.093 261.008 155.288 261.033 155.605C261.058 155.923 260.906 156.15 260.695 156.11C260.484 156.07 260.225 156.265 260.118 156.542C259.991 156.873 260.059 156.964 260.314 156.806C260.529 156.673 260.057 157.487 259.267 158.614C258.476 159.742 257.767 160.664 257.692 160.664C257.616 160.664 257.661 160.491 257.791 160.281C257.939 160.041 257.882 159.988 257.639 160.138C257.425 160.27 257.336 160.516 257.44 160.685C257.702 161.108 253.422 165.439 253 165.179C252.813 165.063 252.552 165.143 252.42 165.357C252.26 165.616 252.336 165.663 252.646 165.5C253.428 165.087 251.564 166.528 250.512 167.149C249.971 167.469 249.633 167.561 249.761 167.354C249.91 167.113 249.809 167.049 249.48 167.176C249.197 167.284 249.065 167.532 249.185 167.726C249.322 167.948 249.198 168.001 248.851 167.868C248.547 167.751 248.299 167.796 248.299 167.967C248.299 168.138 248.05 168.183 247.746 168.066C247.354 167.915 247.27 167.976 247.456 168.278C247.652 168.596 247.531 168.642 246.971 168.465C246.428 168.293 246.292 168.338 246.473 168.631C246.652 168.92 246.523 168.971 246.017 168.81C245.605 168.679 245.311 168.73 245.311 168.932C245.311 169.143 244.903 169.196 244.264 169.069C243.569 168.929 243.289 168.977 243.432 169.209C243.579 169.446 242.632 169.543 240.499 169.51L237.35 169.461L237.347 112.448L237.344 56.0179L186.224 56.0815ZM340.5 341C340.427 340.922 342.174 328.316 344.846 313.118L350.628 285.483H374.837L399.97 285.481L398.801 288.22C388.426 312.525 370.054 332.868 353.356 338.542C349.483 339.858 341.001 341.539 340.5 341ZM118.694 110.751C116.997 110.881 115.851 110.844 115.851 110.659C115.851 110.478 113.366 110.354 109.876 110.361C106.589 110.367 103.9 110.505 103.9 110.668C103.9 110.834 102.802 110.863 101.381 110.735C99.505 110.565 98.922 110.605 99.1 110.892C99.276 111.177 98.883 111.215 97.593 111.037C96.258 110.854 95.907 110.892 96.099 111.203C96.289 111.51 96.01 111.551 94.938 111.369C93.894 111.193 93.589 111.232 93.767 111.52C93.943 111.805 93.686 111.849 92.813 111.685C92.037 111.54 91.618 111.588 91.618 111.822C91.618 112.05 91.232 112.105 90.571 111.973C89.829 111.825 89.592 111.874 89.757 112.142C89.923 112.411 89.682 112.459 88.928 112.308C88.147 112.152 87.929 112.201 88.109 112.493C88.288 112.782 88.108 112.828 87.447 112.662C86.79 112.497 86.607 112.543 86.783 112.828C86.96 113.113 86.777 113.159 86.119 112.994C85.462 112.829 85.279 112.875 85.455 113.16C85.632 113.445 85.449 113.491 84.792 113.326C84.11 113.155 83.948 113.202 84.141 113.514C84.332 113.822 84.205 113.869 83.651 113.693C83.129 113.527 82.975 113.57 83.14 113.837C83.306 114.106 83.11 114.151 82.468 113.99C81.786 113.819 81.625 113.866 81.818 114.178C82.008 114.486 81.881 114.532 81.328 114.357C80.785 114.185 80.649 114.23 80.83 114.523C81.011 114.816 80.875 114.861 80.332 114.689C79.772 114.511 79.651 114.558 79.847 114.876C80.033 115.177 79.949 115.238 79.557 115.088C79.253 114.971 79.004 115.016 79.004 115.187C79.004 115.358 78.756 115.402 78.452 115.286C78.048 115.131 77.973 115.194 78.174 115.519C78.375 115.844 78.3 115.906 77.897 115.751C77.218 115.491 76.285 115.811 76.536 116.217C76.637 116.381 76.449 116.532 76.119 116.553C75.789 116.574 75.369 116.626 75.187 116.668C75.004 116.711 74.659 116.768 74.42 116.796C74.181 116.824 74.068 116.98 74.169 117.144C74.404 117.524 73.415 117.915 72.948 117.626C72.753 117.506 72.688 117.558 72.801 117.742C72.915 117.927 72.63 118.25 72.168 118.46C71.667 118.688 71.411 118.707 71.535 118.506C71.658 118.308 71.392 118.329 70.891 118.558C70.422 118.771 70.113 119.02 70.204 119.111C70.501 119.408 69.055 120.211 68.662 119.968C68.425 119.822 68.369 119.877 68.516 120.115C68.731 120.463 68.317 120.652 67.61 120.529C67.46 120.503 67.431 120.635 67.547 120.822C67.739 121.133 67.292 121.311 66.614 121.193C66.464 121.167 66.435 121.298 66.551 121.486C66.743 121.797 66.296 121.975 65.618 121.857C65.468 121.831 65.435 121.955 65.545 122.133C65.733 122.438 64.89 123.082 64.288 123.093C64.136 123.096 63.911 123.209 63.79 123.345C63.668 123.481 63.793 123.482 64.066 123.348C64.34 123.214 64.281 123.335 63.934 123.616C63.588 123.898 63.197 124.062 63.066 123.981C62.935 123.899 62.546 124.166 62.202 124.572L61.577 125.311L62.407 124.659C62.863 124.3 62.144 125.074 60.808 126.379C59.473 127.684 58.278 128.775 58.153 128.802C57.709 128.901 57.105 129.539 57.243 129.763C57.453 130.103 56.845 130.954 56.392 130.954C56.175 130.954 55.956 131.235 55.905 131.579C55.855 131.922 55.632 132.37 55.41 132.575C55.104 132.857 55.064 132.826 55.246 132.448C55.557 131.799 54.495 133.089 54.05 133.899C53.798 134.358 53.811 134.457 54.099 134.279C54.369 134.113 54.358 134.247 54.066 134.716C53.833 135.088 53.5 135.305 53.326 135.197C53.152 135.089 53.116 135.173 53.246 135.384C53.376 135.594 53.25 136.099 52.965 136.505C52.681 136.911 52.442 137.098 52.435 136.92C52.428 136.743 52.177 137.046 51.879 137.593C51.385 138.498 51.228 139.091 51.148 140.352C51.133 140.591 50.987 140.703 50.823 140.602C50.417 140.351 50.097 141.285 50.357 141.963C50.49 142.31 50.437 142.434 50.216 142.297C49.764 142.018 49.416 142.9 49.693 143.623C49.844 144.016 49.783 144.1 49.481 143.913C49.164 143.717 49.117 143.839 49.295 144.398C49.471 144.955 49.425 145.079 49.112 144.886C48.806 144.697 48.743 144.821 48.879 145.342C48.982 145.736 48.921 146.058 48.743 146.058C48.565 146.058 48.52 146.374 48.644 146.764C48.808 147.28 48.757 147.4 48.452 147.212C48.139 147.019 48.093 147.18 48.264 147.862C48.43 148.523 48.384 148.703 48.094 148.524C47.803 148.344 47.754 148.562 47.91 149.342C48.061 150.097 48.013 150.338 47.744 150.172C47.476 150.007 47.426 150.244 47.575 150.986C47.705 151.635 47.65 152.033 47.431 152.033C47.202 152.033 47.159 152.512 47.31 153.403C47.472 154.364 47.433 154.706 47.176 154.547C46.918 154.387 46.874 154.922 47.027 156.383C47.169 157.747 47.128 158.374 46.905 158.236C46.684 158.099 46.568 159.445 46.568 162.158C46.568 164.862 46.684 166.216 46.904 166.08C47.126 165.943 47.164 166.52 47.017 167.772C46.856 169.144 46.898 169.608 47.168 169.442C47.436 169.276 47.474 169.61 47.303 170.622C47.136 171.612 47.173 171.967 47.428 171.81C47.677 171.656 47.724 171.919 47.578 172.649C47.422 173.43 47.471 173.648 47.762 173.468C48.052 173.289 48.098 173.468 47.932 174.13C47.767 174.787 47.812 174.97 48.098 174.793C48.383 174.617 48.429 174.8 48.264 175.457C48.093 176.139 48.139 176.301 48.452 176.108C48.76 175.917 48.806 176.044 48.631 176.598C48.458 177.14 48.504 177.276 48.797 177.095C49.089 176.914 49.135 177.051 48.963 177.593C48.79 178.136 48.836 178.272 49.129 178.091C49.421 177.91 49.467 178.047 49.295 178.589C49.117 179.149 49.164 179.271 49.481 179.074C49.783 178.888 49.844 178.972 49.693 179.365C49.577 179.668 49.635 179.917 49.823 179.917C50.01 179.917 50.08 180.053 49.977 180.219C49.726 180.624 50.125 181.616 50.456 181.411C50.811 181.192 51.178 182.211 50.904 182.654C50.784 182.848 50.836 182.914 51.021 182.8C51.205 182.687 51.528 182.972 51.738 183.434C51.957 183.913 51.979 184.187 51.791 184.07C51.598 183.951 51.545 184.087 51.663 184.395C51.775 184.687 51.949 184.843 52.049 184.742C52.328 184.463 53.434 186.303 53.208 186.67C53.099 186.846 53.182 186.884 53.393 186.754C53.603 186.623 53.795 186.675 53.817 186.868C53.84 187.062 53.878 187.357 53.9 187.524C53.923 187.692 54.078 187.953 54.244 188.105C54.422 188.268 54.446 188.177 54.302 187.884C54.168 187.61 54.289 187.669 54.571 188.016C54.852 188.362 55.014 188.756 54.931 188.891C54.847 189.026 55.039 189.34 55.356 189.589C55.891 190.008 55.897 189.992 55.436 189.38C55.162 189.016 56.125 189.912 57.575 191.371C59.025 192.831 60.221 194.137 60.231 194.274C60.241 194.411 60.324 194.498 60.415 194.467C60.803 194.336 62.061 195.24 61.988 195.596C61.945 195.811 62.095 196.012 62.324 196.043C62.552 196.074 62.897 196.118 63.09 196.141C63.283 196.164 63.347 196.336 63.231 196.523C63.116 196.71 63.144 196.842 63.295 196.815C63.973 196.698 64.42 196.876 64.227 197.187C64.112 197.374 64.14 197.505 64.291 197.479C64.969 197.362 65.415 197.539 65.223 197.851C65.108 198.038 65.136 198.169 65.287 198.143C65.994 198.02 66.407 198.21 66.192 198.558C66.046 198.795 66.101 198.851 66.338 198.704C66.731 198.461 68.177 199.264 67.88 199.561C67.789 199.652 68.099 199.901 68.567 200.115C69.068 200.343 69.334 200.364 69.212 200.166C69.088 199.965 69.343 199.984 69.844 200.212C70.306 200.423 70.591 200.746 70.478 200.93C70.364 201.114 70.43 201.166 70.624 201.046C71.058 200.778 72.089 201.136 71.876 201.48C71.672 201.809 73.324 202.552 73.718 202.308C73.887 202.204 74.025 202.272 74.025 202.46C74.025 202.647 74.273 202.705 74.577 202.589C74.924 202.456 75.048 202.509 74.911 202.73C74.632 203.182 75.514 203.53 76.237 203.253C76.641 203.098 76.715 203.16 76.515 203.485C76.314 203.811 76.388 203.873 76.792 203.718C77.096 203.602 77.344 203.646 77.344 203.817C77.344 203.989 77.593 204.033 77.897 203.917C78.29 203.766 78.373 203.827 78.187 204.129C77.991 204.446 78.112 204.493 78.672 204.315C79.215 204.143 79.351 204.189 79.17 204.481C78.989 204.774 79.125 204.82 79.668 204.647C80.211 204.475 80.347 204.521 80.166 204.813C79.985 205.106 80.121 205.151 80.664 204.979C81.207 204.807 81.343 204.852 81.162 205.145C80.981 205.438 81.117 205.483 81.66 205.311C82.202 205.139 82.339 205.184 82.158 205.477C81.977 205.77 82.113 205.815 82.656 205.643C83.198 205.471 83.334 205.516 83.154 205.809C82.973 206.102 83.109 206.147 83.651 205.975C84.205 205.799 84.332 205.846 84.141 206.154C83.948 206.466 84.11 206.513 84.792 206.342C85.434 206.181 85.63 206.226 85.463 206.495C85.299 206.762 85.453 206.805 85.975 206.639C86.528 206.463 86.656 206.51 86.465 206.818C86.272 207.13 86.434 207.177 87.115 207.006C87.773 206.841 87.955 206.887 87.779 207.172C87.603 207.457 87.786 207.503 88.443 207.338C89.1 207.173 89.283 207.219 89.107 207.504C88.931 207.789 89.113 207.835 89.771 207.67C90.428 207.505 90.611 207.551 90.435 207.836C90.258 208.121 90.441 208.167 91.099 208.002C91.756 207.837 91.939 207.883 91.763 208.168C91.586 208.453 91.769 208.499 92.426 208.334C93.084 208.169 93.267 208.215 93.09 208.5C92.914 208.785 93.097 208.831 93.754 208.666C94.412 208.501 94.594 208.547 94.418 208.832C94.242 209.117 94.425 209.163 95.082 208.998C95.739 208.833 95.922 208.878 95.746 209.164C95.57 209.449 95.752 209.495 96.41 209.33C97.067 209.165 97.25 209.21 97.074 209.496C96.897 209.781 97.08 209.827 97.738 209.662C98.395 209.497 98.578 209.542 98.402 209.828C98.225 210.113 98.408 210.159 99.065 209.994C99.723 209.829 99.906 209.874 99.729 210.16C99.553 210.445 99.736 210.491 100.393 210.326C101.051 210.161 101.233 210.206 101.057 210.492C100.881 210.777 101.064 210.823 101.721 210.658C102.378 210.493 102.561 210.538 102.385 210.823C102.209 211.109 102.391 211.154 103.049 210.989C103.706 210.824 103.889 210.87 103.713 211.155C103.536 211.441 103.719 211.486 104.377 211.321C105.034 211.156 105.217 211.202 105.041 211.487C104.864 211.773 105.047 211.818 105.704 211.653C106.362 211.488 106.545 211.534 106.368 211.819C106.192 212.105 106.375 212.15 107.032 211.985C107.69 211.82 107.872 211.866 107.696 212.151C107.52 212.437 107.703 212.482 108.36 212.317C109.017 212.152 109.2 212.198 109.024 212.483C108.848 212.769 109.03 212.814 109.688 212.649C110.33 212.488 110.526 212.533 110.36 212.802C110.197 213.065 110.342 213.114 110.83 212.959C111.266 212.821 111.535 212.879 111.535 213.112C111.535 213.338 111.802 213.404 112.199 213.278C112.597 213.152 112.863 213.218 112.863 213.444C112.863 213.67 113.129 213.736 113.527 213.61C113.925 213.484 114.191 213.55 114.191 213.776C114.191 214.002 114.457 214.068 114.855 213.942C115.252 213.816 115.519 213.882 115.519 214.108C115.519 214.334 115.785 214.4 116.183 214.274C116.58 214.148 116.846 214.214 116.846 214.44C116.846 214.673 117.116 214.731 117.552 214.593C118.072 214.428 118.189 214.479 117.998 214.789C117.809 215.094 117.933 215.157 118.454 215.021C118.848 214.918 119.17 214.965 119.17 215.124C119.17 215.284 119.419 215.319 119.723 215.203C120.108 215.055 120.198 215.115 120.021 215.401C119.862 215.658 119.935 215.779 120.216 215.725C120.88 215.595 122.193 216.105 122 216.418C121.908 216.567 122.093 216.668 122.41 216.643C122.73 216.618 122.947 216.794 122.896 217.037C122.846 217.279 122.94 217.393 123.105 217.291C123.523 217.033 125.183 218.757 124.923 219.179C124.804 219.371 124.871 219.422 125.077 219.295C125.279 219.17 125.616 219.39 125.826 219.782C126.085 220.266 126.091 220.423 125.846 220.272C125.621 220.133 125.559 220.242 125.682 220.562C125.79 220.844 126.028 220.982 126.211 220.869C126.42 220.74 126.456 220.942 126.307 221.411C126.177 221.822 126.198 222.079 126.354 221.982C126.511 221.885 126.639 222.749 126.639 223.9C126.639 225.052 126.511 225.916 126.354 225.819C126.198 225.722 126.177 225.979 126.307 226.39C126.456 226.859 126.42 227.061 126.211 226.931C126.028 226.818 125.79 226.957 125.682 227.239C125.551 227.579 125.617 227.67 125.878 227.509C126.095 227.375 125.938 227.735 125.529 228.309C124.822 229.302 122.432 231.272 122.949 230.436C123.14 230.127 123.006 230.123 122.355 230.419C121.893 230.63 121.603 230.946 121.712 231.121C121.831 231.314 121.613 231.346 121.162 231.203C120.619 231.031 120.483 231.077 120.664 231.369C120.843 231.659 120.714 231.709 120.208 231.548C119.796 231.418 119.502 231.469 119.502 231.671C119.502 231.882 119.094 231.935 118.455 231.807C117.706 231.657 117.475 231.707 117.644 231.981C117.819 232.265 116.697 232.365 113.361 232.365C110.233 232.365 108.908 232.256 109.06 232.01C109.214 231.762 108.553 231.726 106.873 231.891C105.033 232.073 104.524 232.035 104.713 231.729C104.901 231.424 104.466 231.385 102.887 231.564C101.262 231.748 100.869 231.708 101.072 231.381C101.273 231.055 100.947 231.016 99.585 231.203C98.231 231.39 97.898 231.351 98.096 231.03C98.294 230.71 97.994 230.669 96.775 230.852C95.579 231.032 95.259 230.992 95.448 230.686C95.636 230.381 95.316 230.341 94.12 230.52C92.941 230.697 92.605 230.657 92.787 230.363C92.968 230.069 92.67 230.03 91.618 230.207C90.547 230.388 90.267 230.348 90.456 230.041C90.646 229.735 90.366 229.695 89.295 229.876C88.207 230.059 87.941 230.019 88.139 229.7C88.335 229.383 88.103 229.34 87.152 229.519C86.215 229.695 85.971 229.652 86.16 229.347C86.346 229.046 86.14 228.996 85.363 229.152C84.695 229.285 84.315 229.229 84.315 228.996C84.315 228.766 83.941 228.706 83.303 228.833C82.59 228.976 82.36 228.924 82.525 228.657C82.691 228.388 82.45 228.34 81.695 228.491C80.94 228.642 80.699 228.594 80.865 228.325C81.031 228.056 80.79 228.008 80.035 228.159C79.286 228.309 79.04 228.261 79.203 227.996C79.366 227.733 79.163 227.689 78.528 227.849C77.87 228.014 77.688 227.968 77.864 227.683C78.039 227.399 77.862 227.351 77.228 227.51C76.681 227.647 76.349 227.594 76.349 227.37C76.349 227.16 76.07 227.097 75.685 227.22C75.287 227.346 75.021 227.28 75.021 227.054C75.021 226.828 74.755 226.762 74.357 226.888C73.959 227.014 73.693 226.948 73.693 226.722C73.693 226.489 73.424 226.431 72.987 226.569C72.471 226.733 72.351 226.682 72.539 226.377C72.732 226.065 72.571 226.018 71.889 226.189C71.231 226.354 71.049 226.308 71.225 226.023C71.401 225.738 71.219 225.692 70.561 225.857C69.904 226.022 69.721 225.976 69.897 225.691C70.074 225.406 69.891 225.36 69.233 225.525C68.576 225.69 68.393 225.644 68.569 225.359C68.746 225.074 68.563 225.028 67.906 225.193C67.248 225.358 67.065 225.312 67.242 225.027C67.418 224.742 67.235 224.696 66.578 224.861C65.92 225.026 65.737 224.98 65.914 224.695C66.09 224.41 65.907 224.364 65.25 224.529C64.592 224.694 64.41 224.649 64.586 224.363C64.762 224.078 64.58 224.032 63.922 224.197C63.265 224.362 63.082 224.317 63.258 224.031C63.435 223.746 63.252 223.7 62.594 223.865C61.952 224.026 61.756 223.982 61.922 223.712C62.085 223.449 61.94 223.401 61.452 223.555C61.064 223.678 60.747 223.654 60.747 223.501C60.747 223.349 59.79 223.227 58.621 223.23C57.452 223.234 56.549 223.322 56.613 223.426C56.791 223.716 55.521 224.705 55.28 224.464C55.164 224.348 55.177 224.534 55.309 224.878C55.49 225.349 55.439 225.436 55.104 225.228C54.779 225.027 54.716 225.102 54.871 225.506C54.988 225.81 54.928 226.058 54.739 226.058C54.539 226.058 54.489 226.354 54.619 226.764C54.783 227.28 54.732 227.4 54.427 227.212C54.114 227.019 54.068 227.18 54.239 227.862C54.4 228.504 54.355 228.701 54.086 228.534C53.819 228.369 53.776 228.523 53.942 229.046C54.118 229.599 54.071 229.726 53.763 229.536C53.451 229.342 53.404 229.504 53.575 230.186C53.736 230.828 53.691 231.024 53.422 230.858C53.155 230.693 53.112 230.847 53.278 231.369C53.454 231.923 53.407 232.05 53.099 231.859C52.787 231.666 52.74 231.828 52.911 232.509C53.072 233.152 53.027 233.348 52.758 233.181C52.492 233.017 52.448 233.171 52.614 233.693C52.79 234.246 52.743 234.373 52.435 234.183C52.123 233.99 52.076 234.151 52.247 234.833C52.408 235.475 52.363 235.672 52.094 235.505C51.831 235.342 51.782 235.488 51.937 235.975C52.076 236.411 52.017 236.68 51.784 236.68C51.551 236.68 51.493 236.95 51.631 237.386C51.792 237.893 51.742 238.021 51.452 237.842C51.163 237.663 51.112 237.792 51.273 238.299C51.412 238.735 51.353 239.004 51.12 239.004C50.887 239.004 50.829 239.273 50.968 239.71C51.132 240.229 51.081 240.347 50.772 240.156C50.466 239.967 50.403 240.091 50.539 240.612C50.642 241.006 50.581 241.328 50.403 241.328C50.224 241.328 50.18 241.644 50.304 242.033C50.469 242.553 50.417 242.67 50.108 242.479C49.802 242.29 49.739 242.414 49.875 242.936C49.978 243.329 49.917 243.651 49.739 243.651C49.561 243.651 49.516 243.967 49.64 244.357C49.804 244.874 49.752 244.994 49.448 244.805C49.135 244.612 49.088 244.774 49.259 245.455C49.421 246.098 49.376 246.294 49.106 246.127C48.84 245.963 48.797 246.117 48.963 246.639C49.138 247.192 49.092 247.319 48.784 247.129C48.471 246.936 48.425 247.098 48.596 247.779C48.757 248.421 48.712 248.618 48.442 248.451C48.176 248.286 48.133 248.44 48.299 248.963C48.474 249.516 48.428 249.643 48.12 249.453C47.807 249.259 47.761 249.421 47.932 250.103C48.093 250.745 48.048 250.941 47.779 250.775C47.512 250.61 47.469 250.764 47.635 251.286C47.81 251.84 47.764 251.967 47.456 251.776C47.144 251.583 47.097 251.745 47.268 252.426C47.429 253.069 47.384 253.265 47.115 253.098C46.852 252.936 46.803 253.081 46.958 253.568C47.096 254.005 47.038 254.274 46.805 254.274C46.572 254.274 46.514 254.543 46.652 254.979C46.813 255.486 46.762 255.615 46.473 255.436C46.184 255.257 46.133 255.385 46.294 255.892C46.432 256.328 46.374 256.598 46.141 256.598C45.908 256.598 45.85 256.867 45.988 257.303C46.153 257.823 46.102 257.94 45.792 257.749C45.487 257.56 45.423 257.684 45.56 258.205C45.663 258.599 45.602 258.921 45.424 258.921C45.245 258.921 45.201 259.237 45.324 259.627C45.479 260.114 45.431 260.259 45.167 260.097C44.898 259.93 44.853 260.126 45.014 260.769C45.175 261.41 45.131 261.607 44.862 261.441C44.607 261.283 44.481 261.547 44.481 262.241C44.481 262.913 44.61 263.196 44.849 263.049C45.101 262.893 45.14 263.069 44.974 263.61C44.84 264.044 44.836 264.299 44.966 264.177C45.25 263.909 46.679 265.274 46.542 265.684C46.489 265.843 46.676 265.885 46.957 265.777C47.239 265.669 47.469 265.736 47.469 265.925C47.469 266.124 47.765 266.175 48.174 266.045C48.691 265.881 48.811 265.932 48.623 266.237C48.431 266.548 48.587 266.597 49.245 266.432C49.792 266.295 50.124 266.348 50.124 266.572C50.124 266.782 50.403 266.844 50.788 266.722C51.186 266.596 51.452 266.662 51.452 266.888C51.452 267.114 51.719 267.18 52.116 267.054C52.502 266.931 52.78 266.994 52.78 267.204C52.78 267.428 53.112 267.481 53.659 267.344C54.33 267.176 54.476 267.225 54.274 267.552C54.071 267.88 54.222 267.927 54.916 267.753C55.573 267.588 55.756 267.634 55.58 267.919C55.404 268.204 55.586 268.25 56.244 268.085C56.901 267.92 57.084 267.966 56.908 268.251C56.731 268.536 56.914 268.582 57.572 268.417C58.229 268.252 58.412 268.298 58.236 268.583C58.059 268.868 58.242 268.914 58.899 268.749C59.557 268.584 59.74 268.629 59.563 268.915C59.387 269.2 59.57 269.246 60.227 269.081C60.885 268.916 61.067 268.961 60.891 269.247C60.715 269.532 60.898 269.578 61.555 269.413C62.213 269.248 62.395 269.293 62.219 269.579C62.043 269.864 62.225 269.91 62.883 269.745C63.54 269.58 63.723 269.625 63.547 269.911C63.37 270.196 63.553 270.242 64.211 270.077C64.868 269.912 65.051 269.957 64.875 270.243C64.698 270.528 64.881 270.574 65.538 270.409C66.196 270.244 66.379 270.289 66.202 270.575C66.027 270.858 66.204 270.907 66.838 270.747C67.4 270.606 67.718 270.662 67.718 270.901C67.718 271.138 68.033 271.196 68.576 271.06C69.192 270.905 69.365 270.956 69.19 271.238C69.014 271.522 69.192 271.571 69.826 271.411C70.388 271.27 70.705 271.326 70.705 271.565C70.705 271.802 71.021 271.86 71.563 271.724C72.183 271.568 72.353 271.619 72.175 271.906C71.995 272.197 72.213 272.246 72.994 272.09C73.749 271.939 73.99 271.987 73.824 272.256C73.658 272.525 73.899 272.573 74.654 272.422C75.399 272.273 75.649 272.321 75.487 272.582C75.323 272.847 75.616 272.888 76.499 272.722C77.436 272.546 77.68 272.588 77.491 272.894C77.306 273.194 77.511 273.245 78.289 273.089C78.956 272.956 79.336 273.012 79.336 273.245C79.336 273.473 79.708 273.536 80.332 273.411C80.95 273.287 81.328 273.349 81.328 273.572C81.328 273.806 81.746 273.854 82.522 273.709C83.418 273.541 83.654 273.586 83.467 273.888C83.279 274.193 83.557 274.238 84.596 274.072C85.511 273.925 85.975 273.974 85.975 274.216C85.975 274.46 86.43 274.504 87.345 274.35C88.383 274.174 88.654 274.217 88.464 274.524C88.273 274.834 88.623 274.873 89.959 274.689C91.257 274.51 91.642 274.548 91.463 274.838C91.283 275.129 91.747 275.168 93.296 274.992C94.854 274.816 95.309 274.855 95.126 275.15C94.944 275.445 95.426 275.487 97.068 275.319C98.584 275.164 99.253 275.208 99.253 275.464C99.253 275.723 100.191 275.771 102.407 275.626C104.21 275.507 105.56 275.547 105.56 275.718C105.56 275.886 108.941 276.017 113.329 276.017C117.9 276.017 121.022 275.893 120.912 275.715C120.802 275.537 121.71 275.505 123.124 275.637C124.864 275.798 125.458 275.757 125.291 275.486C125.125 275.217 125.546 275.18 126.805 275.353C128.141 275.536 128.491 275.498 128.299 275.187C128.109 274.88 128.388 274.84 129.461 275.021C130.504 275.197 130.81 275.158 130.632 274.87C130.455 274.584 130.721 274.542 131.634 274.714C132.517 274.879 132.809 274.839 132.646 274.574C132.484 274.312 132.733 274.265 133.479 274.414C134.289 274.576 134.481 274.526 134.284 274.207C134.085 273.886 134.277 273.843 135.104 274.025C135.878 274.195 136.116 274.154 135.947 273.881C135.781 273.612 135.977 273.567 136.619 273.728C137.277 273.893 137.459 273.847 137.283 273.562C137.107 273.277 137.29 273.231 137.947 273.396C138.604 273.561 138.787 273.515 138.611 273.23C138.435 272.945 138.617 272.899 139.275 273.064C139.956 273.235 140.118 273.188 139.925 272.876C139.735 272.568 139.862 272.521 140.415 272.697C140.958 272.869 141.094 272.824 140.913 272.531C140.734 272.242 140.863 272.191 141.369 272.352C141.779 272.482 142.075 272.432 142.075 272.232C142.075 272.043 142.299 271.974 142.573 272.079C142.846 272.184 143.071 272.11 143.071 271.913C143.071 271.717 143.295 271.642 143.568 271.747C143.842 271.852 144.066 271.778 144.066 271.581C144.066 271.385 144.315 271.319 144.619 271.436C145.012 271.587 145.095 271.526 144.909 271.224C144.713 270.907 144.834 270.86 145.394 271.037C145.954 271.215 146.076 271.168 145.879 270.851C145.693 270.549 145.777 270.488 146.17 270.639C146.473 270.755 146.722 270.697 146.722 270.509C146.722 270.322 146.858 270.252 147.024 270.355C147.453 270.62 148.415 270.197 148.194 269.841C148.093 269.677 148.281 269.526 148.611 269.505C149.69 269.436 149.873 269.385 151.284 268.75C152.051 268.405 152.617 268.022 152.54 267.898C152.464 267.775 152.863 267.401 153.428 267.067C153.993 266.733 154.351 266.63 154.223 266.836C154.074 267.076 154.173 267.142 154.495 267.019C154.773 266.912 154.967 266.652 154.927 266.442C154.887 266.231 155.115 266.079 155.432 266.104C155.75 266.129 155.928 266.017 155.827 265.855C155.659 265.582 156.145 265.417 156.788 265.529C156.939 265.555 156.967 265.424 156.852 265.236C156.663 264.931 157.092 264.752 157.759 264.857C157.896 264.879 157.971 264.71 157.925 264.481C157.88 264.253 158.092 264.108 158.398 264.158C158.704 264.209 159.063 264.074 159.196 263.858C159.358 263.596 159.283 263.547 158.972 263.708C158.716 263.842 158.858 263.642 159.288 263.264C159.717 262.886 160.186 262.648 160.329 262.737C160.472 262.825 160.792 262.637 161.041 262.32C161.456 261.791 161.438 261.781 160.83 262.194C160.465 262.443 160.676 262.188 161.299 261.627C161.922 261.067 162.532 260.671 162.656 260.747C162.936 260.92 165.386 258.443 165.159 258.216C165.068 258.124 165.455 257.536 166.019 256.909C166.583 256.281 166.841 256.066 166.593 256.432C166.179 257.04 166.189 257.058 166.718 256.643C167.036 256.394 167.227 256.08 167.144 255.945C167.06 255.81 167.223 255.416 167.504 255.07C167.786 254.723 167.907 254.65 167.774 254.906C167.614 255.215 167.663 255.291 167.921 255.132C168.135 255 168.237 254.775 168.149 254.632C168.06 254.489 168.218 254.088 168.5 253.742C168.782 253.395 168.915 253.336 168.797 253.61C168.532 254.223 169.545 253.017 170.016 252.159C170.268 251.7 170.255 251.601 169.967 251.779C169.698 251.945 169.708 251.811 170 251.343C170.233 250.97 170.527 250.73 170.654 250.808C170.781 250.886 170.973 250.72 171.082 250.437C171.19 250.155 171.148 250.005 170.989 250.103C170.83 250.201 171.168 249.33 171.74 248.166C172.312 247.003 172.876 246.107 172.994 246.177C173.336 246.378 173.61 245.365 173.377 244.759C173.244 244.412 173.297 244.288 173.519 244.425C173.971 244.704 174.318 243.822 174.041 243.099C173.89 242.706 173.952 242.622 174.253 242.809C174.571 243.005 174.617 242.883 174.44 242.324C174.268 241.781 174.313 241.645 174.606 241.826C174.899 242.007 174.944 241.87 174.772 241.328C174.595 240.771 174.642 240.647 174.954 240.84C175.26 241.029 175.323 240.905 175.187 240.384C175.084 239.99 175.145 239.668 175.323 239.668C175.502 239.668 175.546 239.352 175.422 238.963C175.262 238.456 175.312 238.327 175.602 238.506C175.903 238.692 175.945 238.482 175.768 237.676C175.587 236.854 175.63 236.659 175.947 236.854C176.258 237.047 176.307 236.89 176.142 236.232C175.999 235.665 176.056 235.353 176.301 235.353C176.546 235.353 176.61 234.999 176.481 234.357C176.36 233.75 176.42 233.361 176.635 233.361C176.863 233.361 176.908 232.89 176.763 232.033C176.64 231.303 176.664 230.705 176.817 230.705C176.97 230.705 177.095 227.657 177.095 223.932C177.095 220.207 176.972 217.235 176.821 217.329C176.67 217.422 176.653 216.789 176.783 215.922C176.952 214.792 176.914 214.41 176.647 214.575C176.389 214.735 176.337 214.496 176.478 213.792C176.609 213.136 176.547 212.78 176.301 212.78C176.056 212.78 175.999 212.468 176.142 211.901C176.301 211.266 176.253 211.089 175.969 211.265C175.683 211.441 175.638 211.258 175.803 210.601C175.974 209.919 175.927 209.757 175.615 209.951C175.31 210.139 175.258 210.019 175.422 209.502C175.561 209.066 175.503 208.797 175.27 208.797C175.037 208.797 174.978 208.528 175.117 208.091C175.278 207.585 175.227 207.456 174.938 207.635C174.645 207.816 174.6 207.68 174.772 207.137C174.932 206.632 174.891 206.463 174.645 206.615C174.417 206.756 174.355 206.647 174.479 206.324C174.588 206.041 174.516 205.809 174.32 205.809C174.122 205.809 174.056 205.551 174.171 205.228C174.286 204.909 174.251 204.764 174.094 204.907C173.938 205.05 173.363 204.229 172.817 203.082C171.838 201.025 171.732 200.737 171.657 199.917C171.635 199.689 171.432 199.539 171.203 199.585C170.975 199.631 170.808 199.412 170.832 199.1C170.856 198.788 170.707 198.564 170.5 198.602C170.294 198.641 170.123 198.421 170.12 198.115C170.118 197.808 169.982 197.435 169.818 197.285C169.644 197.126 169.632 197.22 169.788 197.51C169.992 197.89 169.96 197.92 169.652 197.637C169.43 197.432 169.202 196.984 169.145 196.641C169.087 196.298 168.878 196.052 168.68 196.096C168.481 196.14 168.159 195.879 167.965 195.515C167.771 195.152 167.724 194.855 167.863 194.855C168.001 194.855 167.158 193.891 165.991 192.713C164.823 191.535 163.775 190.629 163.661 190.699C163.356 190.888 162.49 190.287 162.49 189.888C162.49 189.698 162.415 189.569 162.324 189.6C161.893 189.745 160.672 188.804 160.872 188.48C160.997 188.278 160.948 188.206 160.764 188.32C160.58 188.433 160.146 188.296 159.8 188.015C159.454 187.733 159.394 187.599 159.668 187.718C160.281 187.983 159.075 186.969 158.217 186.499C157.758 186.247 157.659 186.259 157.837 186.547C158.004 186.817 157.869 186.807 157.401 186.514C157.028 186.281 156.788 185.987 156.866 185.86C156.944 185.734 156.778 185.541 156.495 185.433C156.207 185.322 156.069 185.377 156.181 185.557C156.29 185.734 155.936 185.625 155.394 185.314C154.853 185.003 154.464 184.66 154.53 184.552C154.716 184.252 150.376 182.2 150.021 182.419C149.85 182.525 149.71 182.458 149.71 182.271C149.71 182.083 149.461 182.025 149.157 182.142C148.811 182.274 148.686 182.222 148.823 182C149.103 181.548 148.22 181.2 147.497 181.478C147.151 181.611 147.027 181.558 147.164 181.336C147.443 180.884 146.56 180.536 145.838 180.814C145.434 180.969 145.359 180.906 145.56 180.581C145.761 180.256 145.686 180.193 145.283 180.348C144.978 180.465 144.73 180.4 144.73 180.203C144.73 180.007 144.506 179.932 144.232 180.037C143.959 180.142 143.734 180.088 143.734 179.917C143.734 179.746 143.486 179.701 143.182 179.818C142.789 179.969 142.705 179.907 142.892 179.606C143.088 179.288 142.966 179.241 142.407 179.419C141.864 179.591 141.728 179.546 141.909 179.253C142.088 178.964 141.959 178.913 141.452 179.074C141.006 179.215 140.747 179.154 140.747 178.908C140.747 178.694 140.523 178.604 140.249 178.709C139.975 178.814 139.751 178.746 139.751 178.556C139.751 178.357 139.455 178.306 139.046 178.436C138.539 178.597 138.41 178.547 138.589 178.257C138.768 177.968 138.639 177.917 138.133 178.078C137.696 178.217 137.427 178.158 137.427 177.925C137.427 177.692 137.158 177.634 136.722 177.773C136.205 177.937 136.085 177.885 136.274 177.581C136.467 177.268 136.305 177.221 135.623 177.392C134.966 177.557 134.783 177.512 134.959 177.226C135.136 176.941 134.953 176.895 134.296 177.06C133.638 177.225 133.455 177.18 133.632 176.894C133.808 176.609 133.625 176.563 132.968 176.728C132.31 176.893 132.128 176.848 132.304 176.562C132.48 176.277 132.297 176.231 131.64 176.396C130.983 176.561 130.8 176.516 130.976 176.23C131.152 175.947 130.974 175.898 130.34 176.057C129.812 176.19 129.461 176.141 129.461 175.934C129.461 175.727 129.109 175.677 128.581 175.81C127.911 175.978 127.765 175.928 127.967 175.602C128.17 175.273 128.019 175.226 127.325 175.401C126.667 175.566 126.484 175.52 126.661 175.235C126.837 174.949 126.654 174.904 125.997 175.069C125.339 175.234 125.157 175.188 125.333 174.903C125.508 174.619 125.331 174.571 124.697 174.73C124.169 174.862 123.817 174.813 123.817 174.606C123.817 174.399 123.466 174.349 122.938 174.482C122.268 174.65 122.122 174.601 122.324 174.274C122.527 173.946 122.376 173.898 121.681 174.073C121.024 174.238 120.841 174.192 121.018 173.907C121.194 173.622 121.011 173.576 120.354 173.741C119.696 173.906 119.513 173.86 119.69 173.575C119.865 173.291 119.688 173.243 119.054 173.402C118.525 173.534 118.174 173.485 118.174 173.278C118.174 173.071 117.823 173.022 117.295 173.154C116.625 173.322 116.479 173.273 116.68 172.946C116.883 172.618 116.733 172.571 116.038 172.745C115.381 172.91 115.198 172.864 115.374 172.579C115.551 172.294 115.368 172.248 114.71 172.413C114.053 172.578 113.87 172.532 114.047 172.247C114.222 171.963 114.045 171.915 113.411 172.074C112.882 172.207 112.531 172.157 112.531 171.95C112.531 171.743 112.18 171.694 111.652 171.826C110.982 171.995 110.835 171.945 111.037 171.618C111.24 171.29 111.09 171.243 110.395 171.417C109.738 171.582 109.555 171.536 109.731 171.251C109.908 170.966 109.725 170.92 109.067 171.085C108.41 171.25 108.227 171.205 108.403 170.919C108.579 170.635 108.402 170.587 107.767 170.746C107.239 170.879 106.888 170.829 106.888 170.622C106.888 170.415 106.537 170.366 106.009 170.499C105.351 170.664 105.194 170.614 105.386 170.303C105.577 169.995 105.45 169.949 104.896 170.124C104.337 170.302 104.215 170.255 104.411 169.938C104.598 169.636 104.514 169.575 104.121 169.726C103.471 169.975 102.504 169.673 102.739 169.295C102.828 169.15 102.67 168.944 102.387 168.835C102.068 168.713 101.959 168.775 102.098 169C102.251 169.248 102.101 169.243 101.617 168.984C101.23 168.777 100.913 168.529 100.913 168.433C100.913 168.338 101.085 168.366 101.296 168.496C101.541 168.648 101.589 168.587 101.429 168.327C101.291 168.104 100.976 167.999 100.729 168.094C100.155 168.314 97.172 165.028 97.402 164.428C97.496 164.184 97.442 163.983 97.282 163.983C97.122 163.983 97.07 163.685 97.165 163.32C97.262 162.948 97.175 162.656 96.968 162.656C96.764 162.656 96.598 162.348 96.598 161.972C96.598 161.543 96.739 161.376 96.978 161.524C97.247 161.69 97.292 161.493 97.131 160.852C96.974 160.226 97.017 160.015 97.27 160.171C97.476 160.299 97.611 160.201 97.576 159.95C97.452 159.045 97.602 158.639 97.977 158.87C98.192 159.003 98.271 158.962 98.156 158.778C97.891 158.348 98.856 156.975 99.361 157.063C99.992 157.173 102.23 155.936 102.019 155.594C101.904 155.409 102.131 155.378 102.573 155.519C103.095 155.684 103.249 155.641 103.084 155.375C102.918 155.105 103.114 155.061 103.756 155.222C104.372 155.376 104.592 155.334 104.44 155.089C104.288 154.842 104.602 154.791 105.438 154.926C106.317 155.069 106.593 155.017 106.422 154.741C106.247 154.457 107.45 154.357 111.037 154.357C114.472 154.357 115.824 154.463 115.665 154.721C115.504 154.981 116.178 155.016 118.015 154.843C120.031 154.655 120.535 154.692 120.336 155.015C120.137 155.337 120.539 155.375 122.158 155.187C123.759 155.001 124.177 155.039 123.984 155.352C123.791 155.664 124.139 155.703 125.477 155.519C126.789 155.338 127.161 155.377 126.978 155.673C126.795 155.969 127.13 156.009 128.311 155.831C129.528 155.649 129.829 155.689 129.632 156.008C129.435 156.326 129.702 156.366 130.788 156.183C131.876 155.999 132.141 156.039 131.944 156.358C131.748 156.675 131.98 156.718 132.931 156.539C133.859 156.365 134.111 156.407 133.927 156.705C133.742 157.003 133.995 157.045 134.922 156.871C135.859 156.695 136.103 156.738 135.915 157.043C135.729 157.344 135.937 157.394 136.728 157.235C137.483 157.084 137.724 157.133 137.558 157.401C137.392 157.67 137.633 157.718 138.388 157.567C139.138 157.417 139.384 157.465 139.22 157.73C139.058 157.993 139.26 158.037 139.895 157.877C140.553 157.712 140.736 157.758 140.559 158.043C140.384 158.327 140.561 158.376 141.195 158.216C141.757 158.075 142.075 158.131 142.075 158.37C142.075 158.607 142.39 158.665 142.932 158.529C143.548 158.374 143.722 158.425 143.547 158.707C143.37 158.993 143.553 159.038 144.211 158.873C144.868 158.708 145.051 158.754 144.875 159.039C144.698 159.325 144.881 159.37 145.538 159.205C146.196 159.04 146.379 159.086 146.202 159.371C146.027 159.655 146.204 159.703 146.838 159.544C147.4 159.403 147.718 159.459 147.718 159.698C147.718 159.934 148.033 159.993 148.576 159.857C149.192 159.702 149.365 159.752 149.19 160.035C149.014 160.32 149.196 160.366 149.854 160.201C150.511 160.036 150.694 160.082 150.518 160.367C150.341 160.652 150.524 160.698 151.182 160.533C151.839 160.368 152.022 160.414 151.846 160.699C151.67 160.983 151.847 161.031 152.482 160.872C153.043 160.731 153.361 160.786 153.361 161.026C153.361 161.262 153.676 161.321 154.219 161.185C154.835 161.03 155.008 161.08 154.833 161.363C154.657 161.648 154.84 161.694 155.497 161.529C156.091 161.38 156.331 161.42 156.191 161.646C156.056 161.864 156.482 161.992 157.344 161.992C158.259 161.992 158.636 161.87 158.484 161.624C158.333 161.379 158.483 161.328 158.935 161.472C159.323 161.595 159.717 161.517 159.857 161.29C160.027 161.016 159.959 160.98 159.636 161.174C159.24 161.413 159.238 161.371 159.623 160.897C159.872 160.59 160.211 160.423 160.378 160.526C160.544 160.629 160.659 160.59 160.633 160.44C160.494 159.643 160.703 158.989 161.031 159.192C161.282 159.347 161.323 159.181 161.162 158.672C160.986 158.119 161.033 157.992 161.341 158.182C161.653 158.375 161.7 158.214 161.529 157.532C161.368 156.89 161.412 156.694 161.682 156.86C161.948 157.025 161.991 156.871 161.826 156.349C161.649 155.792 161.696 155.668 162.008 155.861C162.314 156.05 162.377 155.926 162.241 155.405C162.138 155.011 162.199 154.689 162.377 154.689C162.556 154.689 162.6 154.373 162.476 153.983C162.311 153.464 162.363 153.346 162.672 153.537C162.978 153.726 163.041 153.602 162.905 153.081C162.802 152.687 162.863 152.365 163.041 152.365C163.22 152.365 163.264 152.049 163.14 151.66C162.98 151.153 163.03 151.024 163.32 151.203C163.609 151.382 163.659 151.254 163.499 150.747C163.36 150.311 163.418 150.041 163.651 150.041C163.884 150.041 163.943 149.772 163.804 149.336C163.643 148.829 163.694 148.701 163.983 148.88C164.273 149.059 164.323 148.93 164.163 148.423C164.024 147.987 164.082 147.718 164.315 147.718C164.548 147.718 164.607 147.449 164.468 147.012C164.307 146.506 164.358 146.377 164.647 146.556C164.94 146.737 164.985 146.601 164.813 146.058C164.638 145.505 164.684 145.378 164.992 145.568C165.305 145.761 165.351 145.6 165.18 144.918C165.019 144.276 165.064 144.08 165.333 144.246C165.6 144.411 165.643 144.257 165.477 143.734C165.302 143.181 165.348 143.054 165.656 143.244C165.969 143.438 166.015 143.276 165.844 142.594C165.683 141.952 165.728 141.756 165.997 141.922C166.264 142.087 166.307 141.933 166.141 141.411C165.965 140.857 166.012 140.73 166.32 140.921C166.632 141.114 166.679 140.952 166.508 140.271C166.347 139.628 166.392 139.432 166.661 139.599C166.927 139.763 166.971 139.609 166.805 139.087C166.628 138.531 166.675 138.406 166.988 138.6C167.293 138.788 167.357 138.664 167.22 138.143C167.117 137.749 167.179 137.427 167.356 137.427C167.535 137.427 167.579 137.112 167.456 136.722C167.291 136.202 167.342 136.085 167.652 136.276C167.957 136.465 168.02 136.341 167.884 135.82C167.781 135.426 167.842 135.104 168.02 135.104C168.199 135.104 168.243 134.788 168.12 134.398C167.959 133.892 168.009 133.763 168.299 133.942C168.588 134.121 168.639 133.992 168.478 133.486C168.339 133.049 168.398 132.78 168.631 132.78C168.864 132.78 168.922 132.511 168.783 132.075C168.623 131.568 168.673 131.439 168.963 131.618C169.252 131.797 169.303 131.668 169.142 131.162C169.003 130.725 169.062 130.456 169.295 130.456C169.528 130.456 169.586 130.187 169.447 129.751C169.287 129.244 169.337 129.116 169.627 129.295C169.919 129.476 169.965 129.339 169.793 128.797C169.617 128.243 169.663 128.116 169.971 128.307C170.284 128.5 170.331 128.338 170.16 127.657C169.998 127.014 170.043 126.818 170.313 126.985C170.579 127.149 170.622 126.995 170.456 126.473C170.28 125.917 170.326 125.792 170.639 125.985C170.945 126.174 171.008 126.05 170.872 125.529C170.769 125.135 170.801 124.813 170.943 124.813C171.294 124.813 171.221 122.043 170.863 121.792C170.706 121.683 170.537 121.309 170.486 120.963C170.435 120.616 170.221 120.313 170.01 120.29C169.799 120.268 169.468 120.23 169.275 120.207C169.082 120.185 169.026 120.001 169.151 119.798C169.305 119.548 169.14 119.507 168.631 119.668C168.077 119.844 167.95 119.797 168.141 119.489C168.334 119.177 168.172 119.13 167.491 119.301C166.833 119.466 166.65 119.42 166.827 119.135C167.003 118.85 166.82 118.804 166.163 118.969C165.505 119.134 165.323 119.088 165.499 118.803C165.675 118.518 165.492 118.472 164.835 118.637C164.178 118.802 163.995 118.756 164.171 118.471C164.347 118.186 164.165 118.14 163.507 118.305C162.85 118.47 162.667 118.424 162.843 118.139C163.019 117.855 162.841 117.807 162.207 117.966C161.679 118.099 161.328 118.049 161.328 117.842C161.328 117.635 160.977 117.586 160.448 117.718C159.778 117.887 159.632 117.837 159.834 117.51C160.037 117.182 159.886 117.135 159.192 117.309C158.534 117.474 158.352 117.429 158.528 117.143C158.704 116.858 158.521 116.812 157.864 116.977C157.207 117.142 157.024 117.097 157.2 116.811C157.376 116.526 157.194 116.48 156.536 116.645C155.879 116.81 155.696 116.765 155.872 116.479C156.049 116.194 155.866 116.148 155.208 116.313C154.551 116.478 154.368 116.433 154.545 116.147C154.72 115.864 154.543 115.815 153.908 115.975C153.38 116.107 153.029 116.058 153.029 115.851C153.029 115.644 152.678 115.594 152.15 115.727C151.48 115.895 151.333 115.845 151.535 115.519C151.738 115.19 151.588 115.143 150.893 115.318C150.232 115.484 150.052 115.438 150.231 115.148C150.411 114.857 150.194 114.808 149.413 114.964C148.684 115.11 148.419 115.063 148.573 114.814C148.728 114.564 148.464 114.525 147.718 114.689C146.891 114.87 146.7 114.828 146.898 114.507C147.095 114.188 146.903 114.138 146.093 114.3C145.338 114.451 145.097 114.403 145.263 114.134C145.429 113.865 145.188 113.817 144.433 113.968C143.679 114.119 143.438 114.071 143.604 113.802C143.769 113.534 143.532 113.485 142.79 113.633C142.123 113.766 141.743 113.71 141.743 113.477C141.743 113.247 141.368 113.187 140.73 113.315C140.025 113.455 139.788 113.404 139.948 113.144C140.112 112.879 139.82 112.839 138.936 113.004C138.001 113.18 137.756 113.137 137.943 112.834C138.132 112.529 137.85 112.485 136.79 112.654C135.763 112.819 135.451 112.775 135.625 112.493C135.799 112.213 135.533 112.169 134.654 112.334C133.84 112.487 133.444 112.438 133.444 112.185C133.444 111.928 133.01 111.882 132.074 112.04C131.023 112.218 130.764 112.176 130.96 111.859C131.157 111.54 130.856 111.5 129.638 111.682C128.458 111.859 128.123 111.819 128.306 111.524C128.489 111.228 128.125 111.188 126.847 111.363C125.655 111.527 125.145 111.484 125.145 111.218C125.145 110.949 124.549 110.906 123.104 111.069C121.595 111.24 121.125 111.2 121.3 110.916C121.477 110.631 120.809 110.588 118.694 110.751ZM255.021 163.337C255.705 162.708 256.266 162.148 256.266 162.093C256.266 161.839 255.985 162.078 254.92 163.237L253.776 164.481L255.021 163.337ZM100.415 167.801L99.44 166.722C98.904 166.129 98.418 165.643 98.361 165.643C98.105 165.643 98.33 165.917 99.336 166.826L100.415 167.801ZM167.614 194.855C167.523 194.855 166.561 193.959 165.477 192.863C164.393 191.768 163.581 190.871 163.672 190.871C163.764 190.871 164.725 191.768 165.809 192.863C166.893 193.959 167.705 194.855 167.614 194.855ZM306.625 206.141C306.572 206.141 305.862 205.432 305.048 204.564L303.568 202.988L305.145 204.467C306.611 205.843 306.874 206.141 306.625 206.141ZM162.49 260.747L163.634 259.502C164.263 258.817 164.823 258.257 164.878 258.257C165.132 258.257 164.893 258.538 163.734 259.603L162.49 260.747Z";
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
            else
            {
                geometry = OtherGeometry;
                size = 14;
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
                CheckForHex();

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
            TabItem tabItem = new()
            {
                Header = header,
                ToolTip = "",
                IsSelected = true,
            };
            if (tooltip != null)
            {
                tabItem.ToolTip = tooltip;
            }

            IconElement.SetGeometry(tabItem,
                Geometry.Parse(OtherGeometry));
            IconElement.SetHeight(tabItem, 16);
            IconElement.SetWidth(tabItem, 16);

            System.Windows.Controls.ToolTipService.SetIsEnabled(tabItem, false);

            TextEditor codeEditor = new()
            {
                Style = Application.Current.FindResource("TextEditorStyle") as Style,
            };

            SearchPanel searchPanel = SearchPanel.Install(codeEditor.TextArea);
            searchPanel.ShowReplace = true;
            searchPanel.Style = (Style)Application.Current.FindResource("SearchPanelStyle");

            searchPanel.Localization = new Data.Localization();

            codeEditor.PreviewMouseWheel += EditorMouseWheel;
            codeEditor.MouseHover += TextEditorMouseHover;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.Font))
            {
                codeEditor.FontFamily = new FontFamily(Properties.Settings.Default.Font);
            }

            codeEditor.TextChanged += OnTextChanged;
            codeEditor.TextArea.TextEntering += OnTextEntering;

            codeEditor.TextArea.TextView.LinkTextForegroundBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#1a94c4");
            codeEditor.TextArea.TextView.LinkTextUnderline = true;

            codeEditor.WordWrap = Properties.Settings.Default.Wrapping;

            tabItem.Content = codeEditor;

            tabControl.Items.Add(tabItem);
            ChangeGeometry();
        }
    }
}
