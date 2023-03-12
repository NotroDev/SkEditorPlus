using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace Functionalities
{
    public class NamedPipeManager
    {
        public string NamedPipeName { get; set; } = "SkEditor+";
        public event Action<string> ReceiveString;

        private const string EXIT_STRING = "__EXIT__";
        private bool _isRunning = false;
        private CancellationTokenSource _cancellationTokenSource;

        public NamedPipeManager(string name)
        {
            NamedPipeName = name;
        }

        public async Task StartServer()
        {
            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();

            while (_isRunning)
            {
                try
                {
                    using var server = new NamedPipeServerStream(NamedPipeName);
                    await server.WaitForConnectionAsync(_cancellationTokenSource.Token);

                    using var reader = new StreamReader(server);
                    var text = await reader.ReadToEndAsync();

                    if (text == EXIT_STRING)
                    {
                        break;
                    }

                    OnReceiveString(text);
                }
                catch (OperationCanceledException)
                {
                    // Ignore cancellation
                }
            }
        }

        protected virtual void OnReceiveString(string text) => ReceiveString?.Invoke(text);

        public async void StopServer()
        {
            _isRunning = false;
            _cancellationTokenSource?.Cancel();
            await Write(EXIT_STRING);
        }

        public async ValueTask<bool> Write(string text, int connectTimeout = 300)
        {
            try
            {
                using var client = new NamedPipeClientStream(".", NamedPipeName, PipeDirection.Out);
                await client.ConnectAsync(connectTimeout);

                using var writer = new StreamWriter(client);
                await writer.WriteAsync(text);
                await writer.FlushAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
