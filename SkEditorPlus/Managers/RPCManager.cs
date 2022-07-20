using DiscordRPC;

namespace SkEditorPlus.Managers
{
    public class RPCManager
    {
        public static DiscordRpcClient client;

        public static void Initialize()
        {
            client = new DiscordRpcClient("926880446246187089");

            client.Initialize();

            client.SetPresence(new RichPresence()
            {
                Details = "Plik: brak",
                Timestamps = Timestamps.Now,
                Buttons = new Button[]
                {
                        new Button()
                        {
                            Label = "Pobierz SkEditor+",
                            Url = "https://skript.pl/temat/49197-skeditor-remake-edytor-u%C5%82atwiaj%C4%85cy-pisanie-skrypt%C3%B3w/"
                        }
                },
                Assets = new Assets()
                {
                    LargeImageKey = "large"
                }
            });
        }

        public static void SetFile(string name)
        {
            client.UpdateDetails($"Plik: {name}");
        }
    }
}
