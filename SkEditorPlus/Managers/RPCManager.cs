using DiscordRPC;
using System.Windows;

namespace SkEditorPlus.Managers
{
    public class RPCManager
    {
        public static DiscordRpcClient client;

        public static void Initialize()
        {
            if (!Properties.Settings.Default.DiscordRPC) return;

            client = new DiscordRpcClient("926880446246187089");

            client.Initialize();

            string file = (string)Application.Current.FindResource("DiscordRPCFile");
            string none = (string)Application.Current.FindResource("DiscordRPCNone");
            string download = (string)Application.Current.FindResource("DiscordRPCDownload");

            client.SetPresence(new RichPresence()
            {
                Details = file.Replace("{0}", none),
                Timestamps = Timestamps.Now,
                Buttons = new Button[]
                {
                        new Button()
                        {
                            Label = download,
                            Url = "https://github.com/NotroDev/SkEditorPlus"
                        }
                },
                Assets = new Assets()
                {
                    LargeImageKey = "large"
                }
            });
        }

        public static void Uninitialize()
        {
            try
            {
                if (!Properties.Settings.Default.DiscordRPC) return;
                client.Dispose();
            }
            catch { }
        }

        public static void SetFile(string name)
        {
            try
            {
                client.UpdateDetails("Pracuje nad nową funkcją :)");
                return;
                
                if (!Properties.Settings.Default.DiscordRPC) return;
                if (name.EndsWith("*")) name = name[..^1];
                string file = (string)Application.Current.FindResource("DiscordRPCFile");
                client.UpdateDetails(file.Replace("{0}", name));
            }
            catch { }
        }
    }
}
