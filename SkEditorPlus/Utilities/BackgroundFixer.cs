using HandyControl.Tools;
using SkEditorPlus.Utilities.Vaults;
using SkEditorPlus.Windows;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace SkEditorPlus.Utilities
{
    public class BackgroundFixer
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        static extern IntPtr GetWindow(IntPtr hWnd, uint wCmd);
        const uint GW_HWNDNEXT = 2;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        public static void FixBackground(HandyControl.Controls.Window window)
        {
            window.SystemBackdropType = (MicaHelper.IsSupported(BackdropType.Mica) && Properties.Settings.Default.Mica) ? BackdropType.Mica : BackdropType.Disable;


            var hwnd = new WindowInteropHelper(window).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }

		public static void OnStateChanged(object sender, EventArgs e)
		{
			Window window = sender as Window;
			if (window.WindowState == WindowState.Maximized)
			{
				IntPtr hWnd = new WindowInteropHelper(Application.Current.MainWindow).Handle;

				IntPtr hNext = hWnd;
				do
					hNext = GetWindow(hNext, GW_HWNDNEXT);
				while (!IsWindowVisible(hNext));

				SetForegroundWindow(hNext);

				window.Activate();
			}
		}
	}
}
