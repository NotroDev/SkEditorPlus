using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SkEditorPlus.Windows
{
    public partial class ServerWindow : HandyControl.Controls.Window
    {
        Process ServerProc;

        public ServerWindow()
        {
            InitializeComponent();
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            InitializeComponent();

            string ServerFile;
            string ServerPath;


            // If the values are already there then just load them.
            ServerFile = "server.jar";
            ServerPath = @"C:\Users\notro\Desktop\serwer";


            var startInfo = new ProcessStartInfo(@"C:\Program Files\Java\jdk-17.0.1\bin\java.exe", "-Xmx1024M -Xms1024M -jar " + ServerFile + " nogui");
            // Replace the following with the location of your Minecraft Server
            startInfo.WorkingDirectory = ServerPath;
            // Notice that the Minecraft Server uses the Standard Error instead of the Standard Output

            startInfo.RedirectStandardInput = startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false; // Necessary for Standard Stream Redirection
            startInfo.CreateNoWindow = true; // You can do either this or open it with "javaw" instead of "java"

            ServerProc = new Process();
            ServerProc.StartInfo = startInfo;
            ServerProc.EnableRaisingEvents = true;
            ServerProc.ErrorDataReceived += new DataReceivedEventHandler(ServerProc_ErrorDataReceived);
            ServerProc.Exited += new EventHandler(ServerProc_Exited);

            ServerProc.Start();

            Console.WriteLine("Test Run");


        }

        private void ServerProc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            // You have to do this through the Dispatcher because this method is called by a different Thread
            Dispatcher.Invoke(new Action(() =>
            {
                ConsoleTextBlock.Text += e.Data + "\r\n";

            }));

        }

        private void ServerProc_Exited(object sender, EventArgs e)
        {
            // The order of these 2 lines is very important, reversing them will cause an exception
            // and you wont be able to read from the stream when you start the Process again !
            ServerProc.CancelErrorRead();
            ServerProc.Close();
        }

        private void CommandTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            ServerProc.StandardInput.WriteLine(CommandTextBox.Text);
            CommandTextBox.Clear();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                ServerProc.StandardInput.WriteLine("stop");
                ServerProc.WaitForExit(10000);
                if (!ServerProc.HasExited)
                {
                    ConsoleTextBlock.Text += "ERROR: The Server doesn't want to Stop !\r\n";
                    e.Cancel = true;
                }
            }
            catch { }
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            // This is my dirty way of making sure that I don't start the Server Twice :D
            // If the server isn't running then an exception will be thrown when accessing
            // ServerProc.StartTime and the method wont return ;-)
            try { var x = ServerProc.StartTime; return; }
            catch { }

            ServerProc.Start();
            ServerProc.BeginErrorReadLine();
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            try { ServerProc.StandardInput.WriteLine("stop"); }
            catch { }
        }
    }
}