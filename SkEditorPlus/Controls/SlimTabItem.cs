using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SkEditorPlus.Controls
{
    public class SlimTabItem : TabItem
    {
        Border border;

        public override void OnApplyTemplate()
        {
            border = GetTemplateChild("Border") as Border;
            border.Tag = (Parent as TabControl).Items.IndexOf(this);
            base.OnApplyTemplate();
        }

        public Border GetBorder()
        {
            return border;
        }
    }
}
