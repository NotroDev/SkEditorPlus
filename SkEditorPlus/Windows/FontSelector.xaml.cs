using SkEditorPlus.Utilities;
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
        readonly List<string> listFont = new();

        public FontFamily ResultFontFamily { get; private set; }

        public FontSelector()
        {
            InitializeComponent();
            BackgroundFixer.FixBackground(this);


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
        }

        private void OnApply(object sender, RoutedEventArgs e)
        {
            ResultFontFamily = new FontFamily(listFont[lboxFont.SelectedIndex]);

            DialogResult = true;
        }

        private void OnFontChange(object sender, SelectionChangedEventArgs e)
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

            textFont.TextChanged -= OnFontTextChanged;
            textFont.Text = family.ToString();
            textFont.TextChanged += OnFontTextChanged;
        }

        private void OnFontTextChanged(object sender, TextChangedEventArgs e)
        {
            string lower = textFont.Text.ToLower();

            foreach (var item in listFont)
            {
                if (item.ToLower().StartsWith(lower))
                {
                    lboxFont.SelectionChanged -= OnFontChange;
                    lboxFont.SelectedItem = item;
                    lboxFont.SelectionChanged += OnFontChange;

                    lboxFont.ScrollIntoView(item);
                    return;
                }
            }
        }
    }
}
