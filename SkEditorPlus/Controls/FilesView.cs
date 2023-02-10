using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Input;
using SkEditorPlus.Managers;
using HandyControl.Controls;
using System.Windows.Media;
using HandyControl.Data;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SkEditorPlus.Controls
{
    public class FilesView : Control
    {
        private TreeView treeView;
        private FilesViewItem parentFolder;

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(string), typeof(FilesView));

        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { 
                SetValue(PathProperty, value);
                LoadFiles();
            }
        }

        public void LoadFiles()
        {
            if (Path == null) return;
            if (treeView == null)
            {
                MessageBox.Error("Something went wrong! Please try again", "GlitchCode");
                return;
            }
            treeView.Items.Clear();
            parentFolder = new FilesViewItem()
            {
                Header = System.IO.Path.GetFileNameWithoutExtension(Path),
                Tag = Path,
                IsDirectory = true,
                FilesView = this,
                IsExpanded = true,
                IsTopItem = true
            };
            treeView.Items.Add(parentFolder);
            foreach (DirectoryInfo dir in new DirectoryInfo(Path).GetDirectories())
            {
                UpdateDir(parentFolder, dir.FullName, WatcherChangeTypes.Created);
            }
            foreach (FileInfo file in new DirectoryInfo(Path).GetFiles())
            {
                UpdateFile(parentFolder, file.FullName, WatcherChangeTypes.Created);
            }
        }

        public void Update(FilesViewItem parent, string path, WatcherChangeTypes type)
        {
            FileAttributes attr = File.GetAttributes(path);
            if (attr.HasFlag(FileAttributes.Directory))
                UpdateDir(parent, path, type);
            else
                UpdateFile(parent, path, type);
        }

        public void UpdateFile(FilesViewItem parent, string path, WatcherChangeTypes type)
        {
            if (type == WatcherChangeTypes.Created)
            {
                FilesViewItem item = new FilesViewItem()
                {
                    Header = System.IO.Path.GetFileName(path),
                    Tag = path,
                    FilesView = this
                };
                item.MouseDoubleClick += Item_MouseDoubleClick;
                parent.Items.Add(item);
            }
            else if (type == WatcherChangeTypes.Deleted)
            {
                parent.Items.Remove(parent.Items.OfType<FilesViewItem>().Single(item => item.Tag.Equals(path)));
                File.Delete(path);
            }
        }

        public void UpdateDir(FilesViewItem parent, string path, WatcherChangeTypes type)
        {
            if (type == WatcherChangeTypes.Created)
            {
                FilesViewItem item = new FilesViewItem()
                {
                    Header = System.IO.Path.GetFileName(path),
                    Tag = path,
                    IsDirectory = true,
                    FilesView = this
                };
                foreach (DirectoryInfo dir in new DirectoryInfo(path).GetDirectories())
                {
                    UpdateDir(item, dir.FullName, WatcherChangeTypes.Created);
                }
                foreach (FileInfo file in new DirectoryInfo(path).GetFiles())
                {
                    UpdateFile(item, file.FullName, WatcherChangeTypes.Created);
                }
                parent.Items.Add(item);
            }
            else if (type == WatcherChangeTypes.Deleted)
            {
                parent.Items.Remove(parent.Items.OfType<FilesViewItem>().Single(item => item.Tag.ToString().Equals(path)));
                Directory.Delete(path, true);
            }
        }

        private void Item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FilesViewItem item = (FilesViewItem)sender;
            //FileManager.OpenFile(item.Tag.ToString());
        }

        public override void OnApplyTemplate()
        {
            treeView = GetTemplateChild("PART_TreeView") as TreeView;
            ((Button)GetTemplateChild("PART_ReloadButton")).Click += delegate { LoadFiles(); };
            //((Button)GetTemplateChild("PART_OpenProjectButton")).Click += delegate { FileManager.OpenProject(); };
            base.OnApplyTemplate();
        }
    }
}
