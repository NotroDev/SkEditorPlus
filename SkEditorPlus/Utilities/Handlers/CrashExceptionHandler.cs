using AvalonEditB;
using HandyControl.Controls;
using HandyControl.Data;
using SkEditorPlus.Utilities.Vaults;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SkEditorPlus.Utilities.Handlers
{
    public class CrashExceptionHandler : Application
    {
        private static readonly SkEditorAPI skEditor = APIVault.GetAPIInstance();

        public static void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            AddonVault.addons.ForEach(addon => addon.OnUnhandledException());

            Exception e = args.Exception;

            ISkEditorPlusAddon addon = AddonVault.addons.FirstOrDefault(addon => addon.GetType().Assembly.GetName().Name.Equals(e.Source));
            if (addon != null)
            {
                MessageBox.Error($"Addon \"{addon.Name}\" encountered an error.\n\nPlease check if there is an update available in the Marketplace. If not, reach out to the author: {addon.Author}.", "Addon Error");
                MessageBox.Error($"The error will be ignored, but the addon may not function correctly. Also, the stack trace has been copied to the clipboard.", "Addon error");
                Clipboard.SetText(e.Message + Environment.NewLine + e.StackTrace);
                args.Handled = true;
                return;
            }

            string crashTitle = "Crash";
            string crashDescription = "Sorry, but the program has crashed.{n}Don't worry, though, all your files will be saved!";
            string crashDescription2 = "If you can, please report the error in the issues section on GitHub (be sure that you're on the latest version before!).{n}{n}Error:{n}{0}";
            string copyAndOpenWebsite = "Copy and open website";
            string ok = "OK";
            string ignore = "Ignore (can cause problems)";

            MessageBox.Show(new MessageBoxInfo
            {
                Message = crashDescription.Replace("{n}", Environment.NewLine),
                Caption = crashTitle,
                Button = MessageBoxButton.OK,
                ConfirmContent = ok,
                IconBrushKey = ResourceToken.DangerBrush,
                IconKey = ResourceToken.ErrorGeometry
            });

            string error = e.Message + Environment.NewLine + e.StackTrace;

            MessageBoxResult result = MessageBox.Show(new MessageBoxInfo
            {
                Message = crashDescription2.Replace("{n}", Environment.NewLine).Replace("{0}", error),
                Caption = crashTitle,
                Button = MessageBoxButton.YesNoCancel,
                YesContent = copyAndOpenWebsite,
                NoContent = ok,
                CancelContent = ignore,
                IconBrushKey = ResourceToken.DangerBrush,
                IconKey = ResourceToken.ErrorGeometry
            });
            if (result == MessageBoxResult.OK)
            {
                Clipboard.SetText(error);
                skEditor.OpenUrl("https://github.com/NotroDev/SkEditorPlus/issues/new");
            }
            else if (result == MessageBoxResult.Cancel)
            {
                args.Handled = true;
                return;
            }

            SaveFilesToTemp();


            if (System.Diagnostics.Debugger.IsAttached)
            {
                args.Handled = false;
                return;
            }

            args.Handled = true;
            Environment.Exit(0);
        }

        public static void SaveFilesToTemp()
        {
            try
            {
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

                    if (string.IsNullOrWhiteSpace(textEditor.Document.Text))
                    {
                        continue;
                    }

                    string tempFile = Path.Combine(skEditorFolder, fileName);

                    if (!string.IsNullOrEmpty(tempFile))
                    {
                        StringBuilder fileContent = new();
                        fileContent.AppendLine("# This file was saved to the temp folder by SkEditor+.");
                        fileContent.AppendLine("# It could be saved because of a crash or a file restoring feature.");
                        fileContent.AppendLine("# If you don't need it, you can remove it.");
                        if (!string.IsNullOrWhiteSpace(tabItem.ToolTip.ToString()))
                        {
                            fileContent.AppendLine("# Original path: " + tabItem.ToolTip.ToString());
                        }
                        else
                        {
                            fileContent.AppendLine("# Original path: null");
                        }
                        fileContent.AppendLine("# Saved: " + !tabItem.Header.ToString().EndsWith('*'));
                        fileContent.AppendLine();
                        fileContent.Append(textEditor.Document.Text);


                        textEditor.Document.Text = fileContent.ToString();

                        textEditor.Save(tempFile);
                        continue;
                    }
                }
            }
            catch { }
        }
    }
}
