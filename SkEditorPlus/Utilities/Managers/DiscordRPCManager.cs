using DiscordRPC;
using System.Windows;

namespace SkEditorPlus.Utilities.Managers
{
    public class DiscordRPCManager
    {
        private readonly string DiscordRPCClientId = "926880446246187089";
        private readonly string DiscordRPCFileKey = "DiscordRPCFile";
        private readonly string DiscordRPCNoneKey = "DiscordRPCNone";
        private readonly string DiscordRPCDownloadKey = "DiscordRPCDownload";
        private static DiscordRpcClient client;
        private static DiscordRPCManager instance;

        public static DiscordRPCManager Instance
        {
            get
            {
                instance ??= new DiscordRPCManager();
                return instance;
            }
        }

        public void InitializeClient()
        {
            if (!Properties.Settings.Default.DiscordRPC)
                return;

            client = new DiscordRpcClient(DiscordRPCClientId);
            client.Initialize();

            string file = (string)Application.Current.FindResource(DiscordRPCFileKey);
            string none = (string)Application.Current.FindResource(DiscordRPCNoneKey);
            string download = (string)Application.Current.FindResource(DiscordRPCDownloadKey);

            UpdatePresence(file, none, download);
        }

        private static void UpdatePresence(string file, string none, string download)
        {
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
                if (!Properties.Settings.Default.DiscordRPC)
                    return;
                client.Dispose();
            }
            catch { }
        }

        public void SetFile(string name)
        {
            try
            {
                if (!Properties.Settings.Default.DiscordRPC)
                    return;
                if (name.EndsWith("*"))
                    name = name[..^1];
                string file = (string)Application.Current.FindResource(DiscordRPCFileKey);

                client.UpdateDetails(file.Replace("{0}", name));
            }
            catch { }
        }

    }
}
