using Renci.SshNet;
using Renci.SshNet.Async;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static System.Net.WebRequestMethods;
using File = System.IO.File;
using TabItem = HandyControl.Controls.TabItem;

namespace SkEditorPlus.Managers
{
    public class ProjectManager
    {

        public static ProjectManager instance;

        public static FileManager fm;

        private Dictionary<string, bool> isExpandedMap = new();

        private readonly SkEditorAPI skEditor;

        private FileSystemWatcher fileWatcher;

        public bool isFtp = false;

        public static SftpClient client;

        public ProjectManager(SkEditorAPI skEditor)
        {
            this.skEditor = skEditor;
            instance = this;
            fm = FileManager.instance;
        }

        public void OnProjectClick(object sender, MouseButtonEventArgs e)
        {
            TabControl leftTabControl = skEditor.GetSideTabControl();
            if (leftTabControl.SelectedIndex == 1)
            {
                skEditor.GetDispatcher().InvokeAsync(() => leftTabControl.SelectedIndex = -1);
            }
            else
            {
                leftTabControl.SelectedIndex = 1;
            }
        }

        public static MenuItem CreateMenuItem(string header, string iconText)
        {
            return new MenuItem
            {
                Header = header,
                Icon = new TextBlock
                {
                    Text = iconText,
                    FontFamily = new FontFamily("Segoe Fluent Icons")
                }
            };
        }

        private static SftpClient GetClient()
        {
            try
            {
                if (client.IsConnected)
                    return client;
            }
            catch { }

            //var newClient = new SftpClient();
            //newClient.Connect();
            //client = newClient;
            //return newClient;
        }

        private static async Task OpenFileInTreeView(string tabToolTip)
        {
            //if (instance.isFtp)
            //{
            //    try
            //    {
            //        using var client = GetClient();

            //        var temp = Path.GetTempPath();
            //        var tempFile = Path.Combine(temp, Path.GetFileName(tabToolTip));

            //        using (Stream fileStream = File.Create(tempFile))
            //        {
            //            await client.DownloadAsync(tabToolTip, fileStream);
            //        }
            //        client.Disconnect();

            //        fm.CreateFile(Path.GetFileName(tabToolTip), tabToolTip);
            //        fm.GetTextEditor().Load(tempFile);

            //        if (fm.tabControl.SelectedItem is TabItem selectedItem && selectedItem.Header.ToString().EndsWith("*"))
            //        {
            //            selectedItem.Header = selectedItem.Header.ToString()[..^1];
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show(ex.Message);
            //    }
            //    return;
            //}

            var tabItem = fm.tabControl.Items.Cast<TabItem>().FirstOrDefault(ti => ti.ToolTip.ToString() == tabToolTip);
            if (tabItem != null)
            {
                fm.tabControl.SelectedItem = tabItem;
                return;
            }

            fm.CreateFile(Path.GetFileName(tabToolTip), tabToolTip);
            fm.GetTextEditor().Load(tabToolTip);
            if (fm.tabControl.SelectedItem is TabItem ti && ti.Header.ToString().EndsWith("*"))
            {
                ti.Header = ti.Header.ToString()[..^1];
            }
        }

        private static string GetNewFileName(string folderPath)
        {
            var regex = FileManager.regex;
            var files = Directory.GetFiles(folderPath);

            var numbers = files
                .Where(file => regex.IsMatch(file))
                .Select(file => int.Parse(Regex.Match(Path.GetFileNameWithoutExtension(file), "[0-9]+").Value))
                .ToList();

            var count = numbers.DefaultIfEmpty(0).Max() + 1;

            return $"{FileManager.newFileName.Replace("{0}", count.ToString())}.sk";
        }

        public async Task AddDirectoriesAndFilesAsync(DirectoryInfo directory, TreeViewItem treeViewItem)
        {
            fileWatcher = new FileSystemWatcher(fm.projectPath)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName
            };
            fileWatcher.Changed += (sender, e) => OnFileSystemEventAsync(e);
            fileWatcher.Created += (sender, e) => OnFileSystemEventAsync(e);
            fileWatcher.Deleted += (sender, e) => OnFileSystemEventAsync(e);
            fileWatcher.Renamed += (sender, e) => OnFileRenamed(e);
            fileWatcher.EnableRaisingEvents = true;

            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                treeViewItem.MouseRightButtonUp += (sender, e) =>
                {
                    var contextMenu = new ContextMenu();
                    var openFile = CreateMenuItem(Application.Current.FindResource("ProjectCreateFile") as string, "\xe7c3");
                    openFile.Click += async (sender, e) =>
                    {
                        var fileName = GetNewFileName(treeViewItem.Tag.ToString());
                        var filePath = Path.Combine(treeViewItem.Tag.ToString(), fileName);
                        await Task.Run(() => File.Create(filePath));
                        await RefreshTreeViewAsync();
                    };

                    var openFileExplorer = CreateMenuItem(Application.Current.FindResource("ProjectOpenExplorer") as string, "\xec50");
                    openFileExplorer.Click += (sender, e) =>
                    {
                        string arg = "/select, " + treeViewItem.Tag;
                        Process.Start("explorer.exe", arg);
                    };

                    contextMenu.Items.Add(openFile);
                    contextMenu.Items.Add(openFileExplorer);
                    treeViewItem.ContextMenu = contextMenu;
                };

                foreach (DirectoryInfo subDirectory in directory.GetDirectories())
                {
                    var subTreeViewItem = CreateItem(treeViewItem, subDirectory.Name, subDirectory.FullName, "folder");
                    await AddDirectoriesAndFilesAsync(subDirectory, subTreeViewItem);
                }

                foreach (FileInfo file in directory.GetFiles())
                {
                    var subTreeViewItem = CreateItem(treeViewItem, file.Name, file.FullName, "file");
                }
            });
        }

        public async Task RefreshTreeViewAsync()
        {
            if (string.IsNullOrEmpty(fm.projectPath))
            {
                return;
            }

            SaveIsExpanded();

            skEditor.GetMainWindow().fileTreeView.Items.Clear();

            var rootDirectoryInfo = new DirectoryInfo(fm.projectPath);
            var rootTreeViewItem = new TreeViewItem
            {
                Header = FileManager.CreateIcon("\ue8b7", rootDirectoryInfo.Name),
                Tag = rootDirectoryInfo.FullName
            };

            rootTreeViewItem.MouseRightButtonUp += (sender, e) => {
                var contextMenu = new ContextMenu();
                var openFile = CreateMenuItem(Application.Current.FindResource("ProjectCreateFile") as string, "\xe7c3");
                openFile.Click += (sender, e) => {
                    var fileName = GetNewFileName(rootTreeViewItem.Tag.ToString());
                    var filePath = Path.Combine(rootTreeViewItem.Tag.ToString(), fileName);
                    File.Create(filePath);
                };
                contextMenu.Items.Add(openFile);
                rootTreeViewItem.ContextMenu = contextMenu;
            };

            skEditor.GetMainWindow().fileTreeView.Items.Add(rootTreeViewItem);
            await AddDirectoriesAndFilesAsync(rootDirectoryInfo, rootTreeViewItem);

            RestoreIsExpanded();

        }

        private void OnFileRenamed(RenamedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var rootTreeViewItem = skEditor.GetMainWindow().fileTreeView.Items[0] as TreeViewItem;
                RemoveTreeViewItem(rootTreeViewItem, e.OldFullPath);
                AddTreeViewItem(rootTreeViewItem, e.FullPath);
            });
        }

        private void OnFileSystemEventAsync(FileSystemEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Path.GetFullPath(e.FullPath).StartsWith(Path.GetFullPath(fm.projectPath)))
                {
                    var rootTreeViewItem = skEditor.GetMainWindow().fileTreeView.Items[0] as TreeViewItem;

                    if (File.Exists(e.FullPath) || Directory.Exists(e.FullPath))
                    {
                        if (rootTreeViewItem.Items.Cast<TreeViewItem>().Any(childTreeViewItem => e.FullPath.Equals(childTreeViewItem.Tag.ToString())))
                        {
                            return;
                        }

                        var directoryInfo = new DirectoryInfo(fm.projectPath);
                        AddTreeViewItem(rootTreeViewItem, e.FullPath);
                    }
                    else
                    {
                        RemoveTreeViewItem(rootTreeViewItem, e.FullPath);
                    }
                }
            });
        }

        private void AddTreeViewItem(TreeViewItem parentTreeViewItem, string fullPath)
        {
            if (File.Exists(fullPath))
            {
                CreateItem(parentTreeViewItem, Path.GetFileName(fullPath), fullPath, "file");
            }
            else if (Directory.Exists(fullPath))
            {
                var treeViewItem = CreateItem(parentTreeViewItem, Path.GetFileName(fullPath), fullPath, "folder");

                string[] subdirectories = Directory.GetDirectories(fullPath);
                string[] files = Directory.GetFiles(fullPath);

                foreach (string subdirectory in subdirectories)
                {
                    AddTreeViewItem(treeViewItem, subdirectory);
                }
                foreach (string file in files)
                {
                    CreateItem(treeViewItem, Path.GetFileName(file), file, "file");
                }
            }
        }


        private void RemoveTreeViewItem(TreeViewItem parentTreeViewItem, string fullPath)
        {
            foreach (TreeViewItem treeViewItem in parentTreeViewItem.Items)
            {
                if (treeViewItem.Tag.ToString() == fullPath)
                {
                    parentTreeViewItem.Items.Remove(treeViewItem);
                    break;
                }
                else if (Directory.Exists(treeViewItem.Tag.ToString()))
                {
                    RemoveTreeViewItem(treeViewItem, fullPath);
                }
            }
        }

        private TreeViewItem CreateItem(TreeViewItem parentTreeViewItem, string name, string location, string type)
        {
            if (type.Equals("file"))
            {
                var fileTreeViewItem = new TreeViewItem
                {
                    Header = FileManager.CreateIcon("\ue8a5", name),
                    Tag = location
                };

                fileTreeViewItem.MouseDoubleClick += async (sender, e) =>
                {
                    if (e.ChangedButton == MouseButton.Right) return;
                    await OpenFileInTreeView(fileTreeViewItem.Tag.ToString());
                };

                fileTreeViewItem.MouseRightButtonUp += (sender, e) =>
                {
                    var contextMenu = new ContextMenu();
                    var openFile = CreateMenuItem(Application.Current.FindResource("ProjectOpenFile") as string, "\xE8E5");
                    openFile.Click += async (sender, e) =>
                    {
                        await OpenFileInTreeView(fileTreeViewItem.Tag.ToString());
                    };

                    var openFileExplorer = CreateMenuItem(Application.Current.FindResource("ProjectOpenExplorer") as string, "\xec50");
                    openFileExplorer.Click += (sender, e) =>
                    {
                        string arg = "/select, " + location;
                        Process.Start("explorer.exe", arg);
                    };

                    contextMenu.Items.Add(openFile);
                    contextMenu.Items.Add(openFileExplorer);
                    fileTreeViewItem.ContextMenu = contextMenu;
                };

                parentTreeViewItem.Items.Add(fileTreeViewItem);
                return fileTreeViewItem;
            }
            else if (type.Equals("folder"))
            {
                var subTreeViewItem = new TreeViewItem
                {
                    Header = FileManager.CreateIcon("\ue8b7", name),
                    Tag = location
                };

                subTreeViewItem.MouseRightButtonUp += (sender, e) =>
                {
                    var contextMenu = new ContextMenu();
                    var openFile = CreateMenuItem(Application.Current.FindResource("ProjectOpenFile") as string, "\xe7c3");
                    openFile.Click += async (sender, e) =>
                    {
                        var fileName = GetNewFileName(subTreeViewItem.Tag.ToString());
                        var filePath = Path.Combine(subTreeViewItem.Tag.ToString(), fileName);
                        await Task.Run(() => File.Create(filePath));
                        await RefreshTreeViewAsync();
                    };
                    var openFileExplorer = CreateMenuItem(Application.Current.FindResource("ProjectOpenExplorer") as string, "\xec50");
                    openFileExplorer.Click += (sender, e) =>
                    {
                        string arg = "/select, " + location;
                        Process.Start("explorer.exe", arg);
                    };

                    contextMenu.Items.Add(openFile);
                    contextMenu.Items.Add(openFileExplorer);

                    subTreeViewItem.ContextMenu = contextMenu;
                };

                parentTreeViewItem.Items.Add(subTreeViewItem);
                return subTreeViewItem;
            }
            return parentTreeViewItem;
        }

        private void SaveIsExpanded()
        {
            isExpandedMap.Clear();
            foreach (TreeViewItem item in skEditor.GetMainWindow().fileTreeView.Items)
            {
                SaveIsExpanded(item);
            }
        }

        private void SaveIsExpanded(TreeViewItem item)
        {
            if (item == null) return;
            isExpandedMap[item.Tag.ToString()] = item.IsExpanded;
            foreach (TreeViewItem subItem in item.Items)
            {
                SaveIsExpanded(subItem);
            }
        }

        private void RestoreIsExpanded()
        {
            foreach (TreeViewItem item in skEditor.GetMainWindow().fileTreeView.Items)
            {
                RestoreIsExpanded(item);
            }
        }

        private void RestoreIsExpanded(TreeViewItem item)
        {
            if (item == null) return;
            if (isExpandedMap.ContainsKey(item.Tag.ToString()))
            {
                item.IsExpanded = isExpandedMap[item.Tag.ToString()];
            }
            foreach (TreeViewItem subItem in item.Items)
            {
                RestoreIsExpanded(subItem);
            }
        }


        public async Task AddDirectoriesAndFilesFTPAsync(SftpClient client, TreeViewItem item, TreeViewItem parentItem = null)
        {
            var listing = await client.ListDirectoryAsync(item.Tag.ToString());

            foreach (var file in listing)
            {
                if (file.IsDirectory)
                {
                    string[] excludedNames = { "cache", "libraries", "?", "logs", "versions", "world", "world_nether", "world_the_end" };

                    if (excludedNames.Contains(file.Name)) continue;

                    var treeViewItem = CreateItem(parentItem ?? skEditor.GetMainWindow().fileTreeView.Items[0] as TreeViewItem, file.Name, file.FullName, "folder");
                    await AddDirectoriesAndFilesFTPAsync(client, treeViewItem, treeViewItem);
                }
                else
                {
                    if (file.Name.EndsWith(".jar")) continue;
                    CreateItem(parentItem ?? skEditor.GetMainWindow().fileTreeView.Items[0] as TreeViewItem, file.Name, file.FullName, "file");
                }
            }
        }
    }
}
