using AvalonEditB;
using AvalonEditB.Document;
using AvalonEditB.Highlighting;
using AvalonEditB.Search;
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using Renci.SshNet;
using Renci.SshNet.Async;
using SkEditorPlus.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using FontFamily = System.Windows.Media.FontFamily;
using MessageBox = HandyControl.Controls.MessageBox;
using WebView2 = Microsoft.Web.WebView2.Wpf.WebView2;

namespace SkEditorPlus.Utilities.Vaults
{
    public class AddonVault
    {

        public static List<ISkEditorPlusAddon> addons = new();
    }
}
