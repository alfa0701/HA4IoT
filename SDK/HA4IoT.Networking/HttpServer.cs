using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using HA4IoT.Contracts.Networking;

namespace HA4IoT.Networking
{
    public sealed class HttpServer : IDisposable
    {
        private readonly StreamSocketListener _serverSocket = new StreamSocketListener();

        public async Task StartAsync(int port)
        {
            _serverSocket.Control.KeepAlive = true;
            _serverSocket.ConnectionReceived += HandleConnection;

            await _serverSocket.BindServiceNameAsync(port.ToString(), SocketProtectionLevel.PlainSocket);
        }

        public event EventHandler<HttpRequestReceivedEventArgs> RequestReceived;

        public void Dispose()
        {
            _serverSocket.Dispose();
        }

        private void HandleConnection(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            ////Debug.WriteLine("Received HTTP connection on thread " + Environment.CurrentManagedThreadId);

            Task.Factory.StartNew(() => HandleRequests(args.Socket), TaskCreationOptions.LongRunning);
        }

        private void HandleRequests(StreamSocket client)
        {
            using (var clientHandler = new HttpClientHandler(client, HandleClientRequest))
            {
                try
                {
                    clientHandler.HandleRequests();
                }
                catch (Exception exception)
                {
                    Debug.WriteLine("ERROR: Error while handling HTTP client requests. " + exception);
                }
            }
        }

        private bool HandleClientRequest(HttpClientHandler clientHandler, HttpContext httpContext)
        {
            var eventArgs = new HttpRequestReceivedEventArgs(httpContext);
            RequestReceived?.Invoke(clientHandler, eventArgs);

            return eventArgs.IsHandled;
        }
    }
}