using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AvalonEditB.Document;
using SkEditorPlus.Managers;
using SkEditorPlus.Utilities;
using Image = System.Windows.Controls.Image;
using MessageBox = HandyControl.Controls.MessageBox;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace SkEditorPlus.Windows.Generators
{

    public partial class GuiPreview : HandyControl.Controls.Window
    {
        public List<ItemSlot> usedSlots = new();

        private readonly SkEditorAPI skEditor;

        private Item backgroundItem;

        public GuiPreview(SkEditorAPI skEditor)
        {
            InitializeComponent();
            this.skEditor = skEditor;
            BackgroundFixManager.FixBackground(this);

            CreateGrid();
        }

        private void CreateGrid()
        {
            int rows = 6;
            int columns = 9;

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    Button button = CreateButton(row * 9 + column);
                    Grid.SetRow(button, row + 1);
                    Grid.SetColumn(button, column);
                    grid.Children.Add(button);
                }
            }
        }

        private Button CreateButton(int tag)
        {
            Button button = new()
            {
                Content = "",
                Width = 48,
                Height = 48,
                Margin = new Thickness(2),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Tag = tag
            };

            ToolTipService.SetInitialShowDelay(button, 1);

            button.Click += Button_Click;
            button.MouseRightButtonDown += (sender, e) =>
            {
                Button btn = (Button)sender;
                ContextMenu contextMenu = new();
                MenuItem edit = CreateEditMenuItem(btn);
                MenuItem remove = CreateRemoveMenuItem(btn);
                contextMenu.Items.Add(edit);
                contextMenu.Items.Add(remove);
                contextMenu.IsOpen = true;
            };

            return button;
        }

        private MenuItem CreateEditMenuItem(Button button)
        {
            MenuItem edit = ProjectManager.CreateMenuItem(Application.Current.FindResource("GUIGenEdit") as string, "\xe70f");

            edit.Click += (sender, e) =>
            {
                ItemSelector itemSelector = new();
                var tagSlot = usedSlots.FirstOrDefault(s => s.Slot == (int)button.Tag);
                if (tagSlot != null)
                {
                    itemSelector.addActionCheckbox.IsChecked = tagSlot.HaveAction;
                    itemSelector.itemNameTextBox.Text = tagSlot.Item.DisplayName;
                    itemSelector.searchTextBox.Text = tagSlot.OriginalDisplayName;
                    if (string.IsNullOrEmpty(tagSlot.OriginalDisplayName))
                    {
                        itemSelector.searchTextBox.Text = tagSlot.Item.DisplayName;
                    }

                    if (tagSlot.Item.DisplayName.Equals(tagSlot.OriginalDisplayName) || string.IsNullOrEmpty(tagSlot.OriginalDisplayName))
                    {
                        itemSelector.itemNameTextBox.Text = "";
                    }
                }

                if (itemSelector.ShowDialog().Equals(true))
                {
                    UpdateButtonWithSelectedItem(button, itemSelector.ResultItem);

                    int slot = GetSlot(button);
                    usedSlots.RemoveAll(s => s.Slot == slot);

                    ItemSlot itemSlot = new(slot, itemSelector.ResultItem)
                    {
                        HaveCustomName = itemSelector.HaveCustomName,
                        HaveAction = itemSelector.HaveAction,
                        OriginalDisplayName = itemSelector.OriginalDisplayName
                    };

                    usedSlots.Add(itemSlot);
                }
            };

            return edit;
        }

        private MenuItem CreateRemoveMenuItem(Button button)
        {
            MenuItem remove = ProjectManager.CreateMenuItem(Application.Current.FindResource("GUIGenRemove") as string, "\xE74D");

            remove.Click += (sender, e) =>
            {
                if (button.Content is Image)
                {
                    button.Content = "";
                    button.ToolTip = null;
                }
                var slotToRemove = usedSlots.FirstOrDefault(slot => slot.Slot == GetSlot(button));
                if (slotToRemove != null)
                {
                    usedSlots.Remove(slotToRemove);
                }
            };

            return remove;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ItemSelector itemSelector = new();

            if (itemSelector.ShowDialog().Equals(true))
            {
                UpdateButtonWithSelectedItem(button, itemSelector.ResultItem);

                int slot = GetSlot(button);

                usedSlots.RemoveAll(s => s.Slot == slot);

                ItemSlot itemSlot = new(slot, itemSelector.ResultItem)
                {
                    HaveCustomName = itemSelector.HaveCustomName,
                    HaveAction = itemSelector.HaveAction,
                    OriginalDisplayName = itemSelector.OriginalDisplayName
                };

                usedSlots.Add(itemSlot);
            }
        }

        private static int GetSlot(Button button)
        {
            return (Grid.GetRow(button) - 1) * 9 + Grid.GetColumn(button);
        }

        private static void UpdateButtonWithSelectedItem(Button button, Item selectedItem, int size = 32)
        {
            button.ToolTip = MinecraftFormatting.GetFormatted(selectedItem.DisplayName);

            if (!(button.ToolTip is TextBlock textBlock && textBlock.Inlines.Any()))
            {
                (button.ToolTip as TextBlock).Text = selectedItem.DisplayName;
            }

            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string path = Path.Combine(appdata, "SkEditor Plus", "Items", selectedItem.Name + ".png");

            if (File.Exists(path))
            {
                BitmapImage bitmap = new();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(path, UriKind.Relative);
                bitmap.EndInit();

                RenderOptions.SetBitmapScalingMode(bitmap, BitmapScalingMode.HighQuality);

                Image img = new()
                {
                    Source = bitmap,
                    Stretch = Stretch.UniformToFill,
                    Width = size,
                    Height = size
                };

                button.Padding = new Thickness(0);
                button.Content = img;
            }
            else
            {
                button.Content = selectedItem.DisplayName;
            }
        }

        private void OnApply(object sender, RoutedEventArgs e)
        {
            usedSlots = usedSlots.OrderBy(slot => slot.Slot).ToList();

            StringBuilder code = new();

            string functionName = string.IsNullOrEmpty(functionNameTextBox.Text) ? "openGUI" : functionNameTextBox.Text.Trim().Replace(" ", "_");
            titleTextBox.Text = string.IsNullOrEmpty(titleTextBox.Text) ? "GUI" : titleTextBox.Text.Trim();

            int offset = skEditor.GetTextEditor().CaretOffset;
            DocumentLine line = skEditor.GetTextEditor().Document.GetLineByOffset(offset);

            if (!string.IsNullOrEmpty(skEditor.GetTextEditor().Document.GetText(line.Offset, line.Length)))
            {
                code.Append("\n\n");
            }

            code.Append($"function {functionName}(p: player):");
            code.Append($"\n\tcreate a gui with virtual chest inventory with {GetRowQuantity()} rows named \"{titleTextBox.Text}\"");
            if (backgroundItem != null)
            {
                int rowQuantity = GetRowQuantity();
                int maxSlotValue = rowQuantity * 9;

                HashSet<int> excludedSlots = new(usedSlots.Select(slot => slot.Slot));
                List<int> result = Enumerable.Range(0, maxSlotValue + 1).Except(excludedSlots).ToList();

                StringBuilder shapeBuilder = new();
                for (int i = 0; i < rowQuantity; i++)
                {
                    HashSet<char> usedChars = new();
                    StringBuilder fragmentBuilder = new();
                    fragmentBuilder.Append('"');
                    for (int j = 0; j < 9; j++)
                    {
                        int slot = i * 9 + j;
                        fragmentBuilder.Append(result.Contains(slot) ? "-" : GenerateRandomChar(usedChars));
                    }
                    if (i != rowQuantity - 1)
                    {
                        fragmentBuilder.Append("\", ").Append(i == rowQuantity - 2 ? "and " : "");
                    }
                    else
                    {
                        fragmentBuilder.Append('"');
                    }
                    shapeBuilder.Append(fragmentBuilder);
                }
                code.Append(" and shape " + shapeBuilder);
            }
            code.Append(':');

            if (backgroundItem != null)
            {
                code.Append($"\n\t\tmake gui slot \"-\" with {backgroundItem.Name.Replace("_", " ")} named \"&r\"");
            }

            foreach (ItemSlot slot in usedSlots)
            {
                code.Append($"\n\t\tmake gui slot {slot.Slot} with {slot.Item.Name.Replace("_", " ")}");
                if (slot.HaveCustomName)
                {
                    code.Append($" named \"{slot.Item.DisplayName}\"");
                }
                if (slot.HaveAction)
                {
                    code.Append($":\n\t\t\tsend \"You clicked on slot {slot.Slot}\"");
                }
            }
            code.Append("\n\topen the last gui for {_p}");

            skEditor.GetTextEditor().Document.Insert(offset, code.ToString());

            skEditor.GetTextEditor().CaretOffset = offset + code.Length;
        }

        private void OnRowQuantityChanged(object sender, TextChangedEventArgs e)
        {
            string rowQuantityText = rowQuantityTextBox.Text;

            if (int.TryParse(rowQuantityText, out int newRowCount))
            {
                if (newRowCount < 1 || newRowCount > 6)
                {
                    MessageBox.Show(Application.Current.FindResource("GUIGenRowsError") as string, Application.Current.FindResource("Error") as string,
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (grid == null) return;
                grid.Children.Clear();
                usedSlots.Clear();

                int columns = 9;
                for (int row = 0; row < newRowCount; row++)
                {
                    for (int column = 0; column < columns; column++)
                    {
                        Button button = CreateButton(row * columns + column);
                        Grid.SetRow(button, row + 1);
                        Grid.SetColumn(button, column);
                        grid.Children.Add(button);
                    }
                }
            }
        }

        private void OnBackgroundClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ItemSelector itemSelector = new(false);

            if (backgroundItem != null)
            {
                itemSelector.searchTextBox.Text = backgroundItem.DisplayName;
            }

            if (itemSelector.ShowDialog().Equals(true))
            {
                UpdateButtonWithSelectedItem(button, itemSelector.ResultItem, 20);
                backgroundItem = itemSelector.ResultItem;
            }
        }

        private int GetRowQuantity()
        {
            int buttonCount = grid.Children.Count;

            int columns = 9;
            int rowCount = (int)Math.Ceiling((double)buttonCount / columns);

            return rowCount;
        }

        static char GenerateRandomChar(HashSet<char> usedChars)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new();
            char randomChar;
            do { randomChar = chars[random.Next(chars.Length)]; } while (usedChars.Contains(randomChar));
            return randomChar;
        }


        private void OnPreview(object sender, RoutedEventArgs e)
        {
            string guiPath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), "Images", "GUI");
            string guiImage = Path.Combine(guiPath, GetRowQuantity() + ".png");

            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            Bitmap guiBitmap = new(guiImage);

            int slotWidth = 48;
            int slotHeight = 48;
            int slotsPerRow = 9;

            foreach (ItemSlot itemSlot in usedSlots)
            {
                int slotIndex = itemSlot.Slot;

                int row = slotIndex / slotsPerRow;
                int column = slotIndex % slotsPerRow;

                int slotX = 24 + (column * 54);
                int slotY = 54 * (row + 1);

                using Graphics graphics = Graphics.FromImage(guiBitmap);
                Bitmap itemTexture = new(Path.Combine(appdata, "SkEditor Plus", "Items", itemSlot.Item.Name + ".png"));
                graphics.DrawImage(itemTexture, slotX, slotY, slotWidth, slotHeight);
            }

            if (backgroundItem != null)
            {
                int rowQuantity = GetRowQuantity();
                int maxSlotValue = rowQuantity * 9;

                HashSet<int> excludedSlots = new(usedSlots.Select(slot => slot.Slot));
                List<int> result = Enumerable.Range(0, maxSlotValue).Except(excludedSlots).ToList();

                foreach (int backgroundSlot in result)
                {
                    int row = backgroundSlot / slotsPerRow;
                    int column = backgroundSlot % slotsPerRow;

                    int slotX = 24 + (column * 54);
                    int slotY = 54 * (row + 1);

                    using Graphics graphics = Graphics.FromImage(guiBitmap);
                    Bitmap itemTexture = new(Path.Combine(appdata, "SkEditor Plus", "Items", backgroundItem.Name + ".png"));
                    graphics.DrawImage(itemTexture, slotX, slotY, slotWidth, slotHeight);
                }
            }

            System.Drawing.Brush titleBrush = MainWindow.GetBrush("#3F3F3F");
            PointF titlePosition = new(24, 9);

            using (Graphics graphics = Graphics.FromImage(guiBitmap))
            {
                graphics.SmoothingMode = SmoothingMode.None;
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

                string title = string.IsNullOrWhiteSpace(titleTextBox.Text) ? "GUI" : titleTextBox.Text;

                TextBlock formattedText = MinecraftFormatting.GetFormatted(title, 24, true);

                formattedText.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
                formattedText.Arrange(new Rect(formattedText.DesiredSize));

                RenderTargetBitmap rtb = new(
                    (int)formattedText.ActualWidth,
                    (int)formattedText.ActualHeight,
                    96, 96, PixelFormats.Pbgra32);

                rtb.Render(formattedText);

                BitmapSource bitmapSource = rtb.Clone();

                Bitmap renderedBitmap = new(
                    bitmapSource.PixelWidth,
                    bitmapSource.PixelHeight,
                    PixelFormat.Format32bppArgb);

                BitmapData bitmapData = renderedBitmap.LockBits(
                    new Rectangle(System.Drawing.Point.Empty, renderedBitmap.Size),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppArgb);

                bitmapSource.CopyPixels(Int32Rect.Empty, bitmapData.Scan0, bitmapData.Height * bitmapData.Stride, bitmapData.Stride);
                renderedBitmap.UnlockBits(bitmapData);

                graphics.DrawImage(renderedBitmap, titlePosition.X, titlePosition.Y);
            }

            Image image = new()
            {
                Source = BitmapToImageSource(guiBitmap),
            };

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                image.Width = guiBitmap.Width * 0.6;
                image.Height = guiBitmap.Height * 0.6;

                Popup popup = new()
                {
                    AllowsTransparency = true,
                    Placement = PlacementMode.Center,
                    PlacementTarget = this,
                    VerticalOffset = -100,
                    IsOpen = true,
                    Child = image,
                    PopupAnimation = PopupAnimation.Fade,
                };

                (sender as Button).MouseLeave += (sender, e) => popup.IsOpen = false;
                (sender as Button).Click += (sender, e) => popup.IsOpen = false;

                return;
            }

            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);

            string tempPath = Path.GetTempPath();
            string tempFile = Path.Combine(tempPath, "gui.png");
            guiBitmap.Save(tempFile, ImageFormat.Png);

            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = tempFile
            });
        }

        private static ImageSource BitmapToImageSource(Bitmap bitmap)
        {
            using MemoryStream memory = new();
            bitmap.Save(memory, ImageFormat.Png);
            memory.Position = 0;
            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            return bitmapImage;
        }
    }

    public class ItemSlot
    {
        public int Slot { get; set; }
        public Item Item { get; set; }

        public bool HaveCustomName { get; set; }
        public bool HaveAction { get; set; }

        public string OriginalDisplayName { get; set; }

        public ItemSlot(int slot, Item item)
        {
            Slot = slot;
            Item = item;
        }
    }
}