using HandyControl.Data;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SkEditorPlus.Utilities
{
    public class RegistryUtility
    {
        public static void PerformRegistryOperations()
        {
            RegistryKey skEditorProtocol = Registry.ClassesRoot.OpenSubKey("skeditor");
            if (skEditorProtocol == null)
            {
                var isAdmin = IsRunningAsAdministrator();
                if (!isAdmin)
                {
                    MessageBoxResult result = MessageBox.Show(new MessageBoxInfo
                    {
                        Message = "Looks like it's your first time using SkEditor+ or you've recently updated the app.\nJust for this first time, you'll need to run the application as an administrator to be able to add registry keys.\n\nAfter that, run app again.",
                        Caption = "SkEditor+",
                        Button = MessageBoxButton.YesNo,
                        YesContent = "Run as admin",
                        NoContent = "Close app",
                        IconBrushKey = ResourceToken.DarkInfoBrush,
                        IconKey = ResourceToken.InfoGeometry
                    });

                    if (result == MessageBoxResult.Yes)
                    {
                        var processInfo = new ProcessStartInfo(Environment.ProcessPath)
                        {
                            UseShellExecute = true,
                            Verb = "runas"
                        };

                        try
                        {
                            Process.Start(processInfo);
                        }
                        catch
                        {
                            MessageBox.Show(new MessageBoxInfo
                            {
                                Message = "Oops, looks like you might have denied running the app as an administrator.\nI'm sorry, but it's necessary. The app will close now.",
                                Caption = "SkEditor+",
                                Button = MessageBoxButton.OK,
                                IconBrushKey = ResourceToken.DarkDangerBrush,
                                IconKey = ResourceToken.ErrorGeometry
                            });
                        }
                        Application.Current.Shutdown();
                        return;
                    }
                    else
                    {
                        Application.Current.Shutdown();
                    }
                }
                else
                {
                    skEditorProtocol = Registry.ClassesRoot.CreateSubKey("skeditor");
                    skEditorProtocol.SetValue("", "URL:skeditor Protocol");
                    skEditorProtocol.SetValue("URL Protocol", "");

                    RegistryKey shell = skEditorProtocol.CreateSubKey("shell");
                    RegistryKey open = shell.CreateSubKey("open");
                    RegistryKey command = open.CreateSubKey("command");

                    command.SetValue("", $"\"{Environment.ProcessPath}\" \"%1\"");
                }
            }
        }

        public static bool IsRunningAsAdministrator()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
