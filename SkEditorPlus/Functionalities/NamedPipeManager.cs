using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace Functionalities
{
    public class NamedPipeManager
    {
        public string NamedPipeName = "SkEditor+";
        public event Action<string> ReceiveString;

        private const string EXIT_STRING = "__EXIT__";
        private bool _isRunning = false;
        private Thread Thread;

        public NamedPipeManager(string name)
        {
            NamedPipeName = name;
        }

        public void StartServer()
        {
            Thread = new Thread((pipeName) =>
            {
                _isRunning = true;

                while (true)
                {
                    string text;
                    using (var server = new NamedPipeServerStream(pipeName as string))
                    {
                        server.WaitForConnection();

                        using StreamReader reader = new(server);
                        text = reader.ReadToEnd();
                    }

                    if (text == EXIT_STRING)
                        break;

                    OnReceiveString(text);

                    if (_isRunning == false)
                        break;
                }
            });
            Thread.Start(NamedPipeName);
        }

        protected virtual void OnReceiveString(string text) => ReceiveString?.Invoke(text);


        public void StopServer()
        {
            _isRunning = false;
            Write(EXIT_STRING);
            Thread.Sleep(30);
        }

        public bool Write(string text, int connectTimeout = 300)
        {
            using (var client = new NamedPipeClientStream(NamedPipeName))
            {
                try
                {
                    client.Connect(connectTimeout);
                }
                catch
                {
                    return false;
                }

                if (!client.IsConnected)
                    return false;

                using StreamWriter writer = new(client);
                writer.Write(text);
                writer.Flush();
            }
            return true;
        }
    }
}
