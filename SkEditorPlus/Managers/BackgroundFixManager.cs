using HandyControl.Tools;
using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace SkEditorPlus.Managers
{
    public class BackgroundFixManager
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public static void FixBackground(HandyControl.Controls.Window window)
        {
            window.SystemBackdropType = (MicaHelper.IsSupported(BackdropType.Mica) && Properties.Settings.Default.Mica) ? BackdropType.Mica : BackdropType.Disable;
            
            
            var hwnd = new WindowInteropHelper(window).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }
    }
}
