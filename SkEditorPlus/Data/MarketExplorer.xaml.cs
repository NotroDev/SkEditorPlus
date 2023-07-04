using HandyControl.Controls;
using SkEditorPlus.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SkEditorPlus.Data
{
    public partial class MarketExplorer : UserControl
    {
        public MarketExplorer()
        {
            InitializeComponent();
        }

        private void OnImageClick(object sender, EventArgs e)
        {
            Border border = sender as Border;
            ImageBrush image = (border.Child as Border).Background as ImageBrush;
            BitmapSource source = image.ImageSource as BitmapSource;

            if (source.IsDownloading) return;

            if (border.ToolTip is ToolTip oldToolTip)
            {
                oldToolTip.IsOpen = false;
            }

            double height = 112.5 * 4.5;
            double width = 200 * 4.5;

            Image img = new()
            {
                Source = source,
                Width = width,
                Height = height
            };

            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);

            ToolTip toolTip = new()
            {
                Content = img,
                Width = width,
                Height = height
            };

            border.ToolTip = toolTip;

            toolTip.IsOpen = true;
        }

        private void OnImageMouseLeave(object sender, EventArgs e)
        {
            Border border = sender as Border;

            if (border.ToolTip is ToolTip toolTip)
            {
                toolTip.IsOpen = false;
            }
        }
    }
}
