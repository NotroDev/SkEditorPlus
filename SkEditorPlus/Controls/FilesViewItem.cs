using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SkEditorPlus.Controls
{
    public class FilesViewItem : TreeViewItem
    {

        public static readonly DependencyProperty IsDirectoryProperty =
            DependencyProperty.Register("IsDirectory", typeof(bool), typeof(FilesViewItem), new PropertyMetadata(false));
        public static readonly DependencyProperty IsTopItemProperty =
            DependencyProperty.Register("IsTopItem", typeof(bool), typeof(FilesViewItem), new PropertyMetadata(false));
        public static readonly DependencyProperty FilesViewProperty =
            DependencyProperty.Register("FilesView", typeof(FilesView), typeof(FilesViewItem));

        public bool IsDirectory
        {
            get { return (bool)GetValue(IsDirectoryProperty); }
            set
            {
                SetValue(IsDirectoryProperty, value);
            }
        }

        public bool IsTopItem
        {
            get { return (bool)GetValue(IsTopItemProperty); }
            set
            {
                SetValue(IsTopItemProperty, value);
            }
        }

        public FilesView FilesView
        {
            get { return (FilesView)GetValue(FilesViewProperty); }
            set
            {
                SetValue(FilesViewProperty, value);
            }
        }

        public override void OnApplyTemplate()
        {
            DockPanel ButtonPanel = GetTemplateChild("PART_ButtonPanel") as DockPanel;
            foreach (Button button in ButtonPanel.Children.OfType<Button>())
            {
                button.Click += Button_Click;
            }
            
            base.OnApplyTemplate();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Button Clicked");
            Button button = sender as Button;
            switch (button.Name)
            {
                case "PART_NewFileButton":
                    File.Create(Path.Combine(Tag.ToString(), "New File.txt"));
                    FilesView.UpdateFile(GetFixedParent(), Path.Combine(Tag.ToString(), "New File.txt"), WatcherChangeTypes.Created);
                    break;
                case "PART_NewDirButton":
                    Directory.CreateDirectory(Path.Combine(Tag.ToString(), "New Directory"));
                    FilesView.UpdateDir(GetFixedParent(), Path.Combine(Tag.ToString(), "New Directory"), WatcherChangeTypes.Created);
                    break;
                case "PART_DelButton":
                    FilesView.Update(GetFixedParent(), Tag.ToString(), WatcherChangeTypes.Deleted);
                    break;
            }
        }

        FilesViewItem GetFixedParent()
        {
            //return Parent.GetType() == typeof(FilesViewItem) ? (FilesViewItem) Parent : this;
            return this;
        }
    }
}
