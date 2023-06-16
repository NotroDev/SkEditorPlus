using AvalonEditB;
using HandyControl.Controls;
using HandyControl.Data;
using SkEditorPlus.Utilities.Vaults;
using System;
using System.IO;
using System.Windows;

namespace SkEditorPlus.Utilities.Handlers
{
    public class CrashExceptionHandler
    {
        public static SkEditorAPI skEditor;

        public static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            AddonVault.addons.ForEach(addon => addon.OnUnhandledException());

            Exception e = (Exception)args.ExceptionObject;

            string crashTitle = "Crash";
            string crashDescription = "Sorry, but the program has crashed.{n}Don't worry, though, all your files will be saved!";
            string crashDescription2 = "If you can, please report the error in the issues section on GitHub (be sure that you're on the latest version before!).{n}{n}Error:{n}{0}";
            string copyAndOpenWebsite = "Copy and open website";
            string ok = "OK";

            HandyControl.Controls.MessageBox.Show(new MessageBoxInfo
            {
                Message = crashDescription.Replace("{n}", Environment.NewLine),
                Caption = crashTitle,
                Button = MessageBoxButton.OK,
                ConfirmContent = ok,
                IconBrushKey = ResourceToken.DangerBrush,
                IconKey = ResourceToken.ErrorGeometry
            });

            string error = e.Message + Environment.NewLine + e.StackTrace;

            MessageBoxResult result = HandyControl.Controls.MessageBox.Show(new MessageBoxInfo
            {
                Message = crashDescription2.Replace("{n}", Environment.NewLine).Replace("{0}", error),
                Caption = crashTitle,
                Button = MessageBoxButton.OKCancel,
                ConfirmContent = copyAndOpenWebsite,
                CancelContent = ok,
                IconBrushKey = ResourceToken.DangerBrush,
                IconKey = ResourceToken.ErrorGeometry
            });
            if (result == MessageBoxResult.OK)
            {
                Clipboard.SetText(error);
                skEditor.OpenUrl("https://github.com/NotroDev/SkEditorPlus/issues/new");
            }

            foreach (TabItem tabItem in skEditor.GetMainWindow().tabControl.Items)
            {
                if (tabItem.Content is not TextEditor textEditor)
                {
                    continue;
                }

                string tempPath = Path.GetTempPath();

                string skEditorFolder = Path.Combine(tempPath, "SkEditorPlus");
                if (!Directory.Exists(skEditorFolder))
                {
                    Directory.CreateDirectory(skEditorFolder);
                }

                string fileName = tabItem.Header.ToString();
                if (fileName.EndsWith("*"))
                {
                    fileName = fileName[..^1];
                }

                string tempFile = Path.Combine(skEditorFolder, fileName);

                tabItem.ToolTip = tempFile;

                if (!string.IsNullOrEmpty(tabItem.ToolTip.ToString()))
                {
                    textEditor.Save(tabItem.ToolTip.ToString());
                    continue;
                }
            }

            //Environment.Exit(0);
        }
    }
}
