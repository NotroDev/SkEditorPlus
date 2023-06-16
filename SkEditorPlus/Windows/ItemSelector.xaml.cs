using Newtonsoft.Json;
using SkEditorPlus.Utilities;
using SkEditorPlus.Windows.Generators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace SkEditorPlus.Windows
{
    public partial class ItemSelector : HandyControl.Controls.Window, INotifyPropertyChanged
    {
        private static List<Item> itemList = null;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<string> filteredItems;

        public ObservableCollection<string> FilteredItems
        {
            get { return filteredItems; }
            set
            {
                filteredItems = value;
                OnPropertyChanged(nameof(FilteredItems));
            }
        }

        public Item ResultItem { get; private set; }
        public bool HaveCustomName { get; private set; }
        public bool HaveAction { get; private set; }
        public string OriginalDisplayName { get; set; }


        private CancellationTokenSource cancellationTokenSource;

        public ItemSelector(bool itemNameTextBoxVisible = true)
        {
            InitializeComponent();
            BackgroundFixer.FixBackground(this);

            Loaded += (s, e) =>
            {
                Keyboard.Focus(searchTextBox);
                searchTextBox.CaretIndex = searchTextBox.Text.Length;
            };

            itemNameTextBox.Visibility = itemNameTextBoxVisible ? Visibility.Visible : Visibility.Hidden;

            if (itemList == null)
            {
                string exePath = Environment.ProcessPath;

                string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SkEditor Plus");

                string json = File.ReadAllText(Path.Combine(appDataPath, "items.json"));
                itemList = JsonConvert.DeserializeObject<List<Item>>(json);
                itemList.Sort((a, b) => a.DisplayName.CompareTo(b.DisplayName));
            }

            DataContext = this;
            filteredItems = new ObservableCollection<string>();
            RefreshItems();
        }

        private void OnApply(object sender, RoutedEventArgs e)
        {
            if (ItemListBox.SelectedItem is null)
            {
                DialogResult = false;
                return;
            }
            ResultItem = itemList.FirstOrDefault(item => item.DisplayName == ItemListBox.SelectedItem.ToString())?.Clone() as Item;

            DialogResult = ResultItem is not null;
            OriginalDisplayName = ResultItem.DisplayName;
            if (!string.IsNullOrEmpty(itemNameTextBox.Text))
            {
                ResultItem.DisplayName = itemNameTextBox.Text;
                HaveCustomName = true;
            }
            else
            {
                HaveCustomName = false;
            }

            HaveAction = addActionCheckbox.IsChecked == true;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshItems();
        }

        private async void RefreshItems()
        {
            try
            {
                string searchQuery = searchTextBox.Text.ToLower().Replace("_", "").Trim();
                string[] searchTerms = searchQuery.Split(' ');

                cancellationTokenSource?.Cancel();
                cancellationTokenSource = new CancellationTokenSource();

                var filteredItemsList = itemList.Where(item => ContainsAllSearchTerms(item.DisplayName, searchTerms) || ContainsAllSearchTerms(item.Name, searchTerms))
                    .Select(item => item.DisplayName);

                if (filteredItemsList.Any(item => item.ToLower() == searchQuery))
                {
                    var item = filteredItemsList.First(item => item.ToLower() == searchQuery);
                    filteredItemsList = filteredItemsList.Where(item => item.ToLower() != searchQuery);
                    filteredItemsList = new List<string>() { item }.Concat(filteredItemsList);
                }

                FilteredItems = new ObservableCollection<string>();

                foreach (var item in filteredItemsList)
                {
                    if (cancellationTokenSource.Token.IsCancellationRequested)
                        break;

                    if (FilteredItems.Count > 100)
                        break;

                    FilteredItems.Add(item);
                    await Task.Delay(10, cancellationTokenSource.Token);

                    if (FilteredItems.Count == 1)
                    {
                        ItemListBox.SelectedItem = item;
                        ItemListBox.ScrollIntoView(ItemListBox.SelectedItem);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }

        private static bool ContainsAllSearchTerms(string text, string[] searchTerms)
        {
            return searchTerms.All(term => text.ToLower().Replace("_", "").Contains(term));
        }

        private void OnSearchKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up)
            {
                int nextSelection;
                if (e.Key == Key.Down)
                {
                    nextSelection = ItemListBox.SelectedIndex + 1;
                    if (nextSelection >= ItemListBox.Items.Count)
                        return;

                    ItemListBox.SelectedIndex = nextSelection;
                    ItemListBox.ScrollIntoView(ItemListBox.SelectedItem);
                }
                else
                {
                    nextSelection = ItemListBox.SelectedIndex - 1;
                    if (nextSelection < 0)
                        return;
                }
                ItemListBox.SelectedIndex = nextSelection;
                ItemListBox.ScrollIntoView(ItemListBox.SelectedItem);
            }
            if (e.Key == Key.Right)
            {
                Keyboard.Focus(itemNameTextBox);
                itemNameTextBox.CaretIndex = itemNameTextBox.Text.Length;
            }
            else if (e.Key == Key.Left)
            {
                Keyboard.Focus(searchTextBox);
                searchTextBox.CaretIndex = searchTextBox.Text.Length;
            }
            else
            {
                return;
            }
            e.Handled = true;
        }
    }



    public class Item
    {
        public string DisplayName { get; set; }
        public string Name { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
