using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SkEditorPlus.Windows
{
    /// <summary>
    /// Logika interakcji dla klasy TooltipWindow.xaml
    /// </summary>
    public partial class TooltipWindow : UserControl
    {
        public TooltipWindow()
        {
            InitializeComponent();
        }

        public TooltipWindow SetDescription(string description)
        {
            this.description.Text = description;
            return this;
        }

        public TooltipWindow AddOption(TextBlock option)
        {
            this.dockpanel.Children.Add(option);
            return this;
        }
    }
}
