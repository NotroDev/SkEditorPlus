using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SkEditorPlus.Windows
{
    public partial class FontSelector : HandyControl.Controls.Window
    {

        List<string> listFont = new List<string>();

        public FontFamily ResultFontFamily { get; private set; }

        public FontSelector()
        {
            InitializeComponent();

            Control control = this;
            ResultFontFamily = control.FontFamily;

            var cond = System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentUICulture.Name);
            foreach (FontFamily item in Fonts.SystemFontFamilies)
            {
                if (item.FamilyNames.ContainsKey(cond))
                    listFont.Add(item.FamilyNames[cond]);
                else
                    listFont.Add(item.ToString());
            }
            if (!listFont.Contains("Cascadia Mono"))
            {
                listFont.Add("Cascadia Mono");
            }
            listFont.Sort();
            lboxFont.ItemsSource = listFont;

            lboxFont.SelectedItem = Properties.Settings.Default.Font;
            lboxFont.ScrollIntoView(lboxFont.SelectedItem);
            textFont.Text = Properties.Settings.Default.Font;

            double[] listSize = { 8, 9, 10, 10.5, 11, 12, 14, 16, 18, 20, 24, 28, 32, 36, 40, 44, 48, 54, 60, 66, 72, 80, 88, 96 };
        }

        public FontSelector(Control control)
        {
            InitializeComponent();

            ResultFontFamily = control.FontFamily;

            var cond = System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentUICulture.Name);
            foreach (FontFamily item in Fonts.SystemFontFamilies)
            {
                if (item.FamilyNames.ContainsKey(cond))
                    listFont.Add(item.FamilyNames[cond]);
                else
                    listFont.Add(item.ToString());
            }
            listFont.Sort();
            lboxFont.ItemsSource = listFont;

            lboxFont.SelectedItem = control.FontFamily.ToString();
            lboxFont.ScrollIntoView(lboxFont.SelectedItem);
            textFont.Text = control.FontFamily.ToString();

            double[] listSize = { 8, 9, 10, 10.5, 11, 12, 14, 16, 18, 20, 24, 28, 32, 36, 40, 44, 48, 54, 60, 66, 72, 80, 88, 96 };
        }

        private void textFontSize_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(((Key.D0 <= e.Key) && (e.Key <= Key.D9))
                || ((Key.NumPad0 <= e.Key) && (e.Key <= Key.NumPad9))
                || e.Key == Key.Back
                || e.Key == Key.OemPeriod
                || e.Key == Key.Delete))
            {
                e.Handled = true;
            }
            else if (e.Key == Key.OemPeriod)
            {
                if ((sender as TextBox).Text.IndexOf('.') > -1)
                    e.Handled = true;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            ResultFontFamily = new FontFamily(listFont[lboxFont.SelectedIndex]);

            DialogResult = true;
        }

        private void lboxFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FontFamily family;
            List<string> tempListFontStyle = new List<string>();
            List<FamilyTypeface> tempListFontTypeface;
            try
            {
                tempListFontStyle = new List<string>();
                family = new FontFamily((sender as ListBox).SelectedItem as string);
                tempListFontTypeface = family.FamilyTypefaces.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int selectIndex = -1;

            var cond = System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentUICulture.Name);

            var list = family.GetTypefaces().ToList();
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if (item.FaceNames.ContainsKey(cond))
                {
                    tempListFontStyle.Add(item.FaceNames[cond]);
                }
                else
                {
                    tempListFontStyle.Add(item.FaceNames[System.Windows.Markup.XmlLanguage.GetLanguage("en-us")]);
                }
            }

            textFont.TextChanged -= textFont_TextChanged;
            textFont.Text = family.ToString();
            textFont.TextChanged += textFont_TextChanged;
        }

        private void textFont_TextChanged(object sender, TextChangedEventArgs e)
        {
            string lower = textFont.Text.ToLower();

            foreach (var item in listFont)
            {
                if (item.ToLower().StartsWith(lower))
                {
                    lboxFont.SelectionChanged -= lboxFont_SelectionChanged;
                    lboxFont.SelectedItem = item;
                    lboxFont.SelectionChanged += lboxFont_SelectionChanged;

                    lboxFont.ScrollIntoView(item);
                    return;
                }
            }
        }

        public void ApplyToControl(Control control)
        {
            control.FontFamily = ResultFontFamily;
        }
    }
}
