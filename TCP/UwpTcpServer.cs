using MultiNetworkLib.TCP.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;

namespace MultiNetworkLib.TCP
{
    class UwpTcpServer : ITCPServer
    {
        public NetworkInfoPair TcpNetworkInfoPair { get; set; }

        public event Action<string> OnError;

        private StreamWriter streamWriter = null;
        private StreamSocket socket;
        private StreamSocketListener streamSocketListener;


        public void Disconnect()
        {
            if (socket != null)
            {
                streamSocketListener.Dispose();

                streamWriter.Close();
                streamWriter.Dispose();
                socket.Dispose();
            }
        }

        public void SendDataToServerAsync(string message)
        {
            Task.Run(async () =>
                {
                    try
                    {
                        await streamWriter.WriteLineAsync(message);
                        await streamWriter.FlushAsync();
                    }
                    catch
                    {
                        OnError?.Invoke("送信できませんでした::" + message);
                    }
                });
        }

        public void StartServerAsync(string _serviceName, Action<ITCPServer> _successEvent, Action<string> _onReceived, Action<string> _errorEvent)
        {
            OnError += _errorEvent;

            Task.Run(async () =>
            {
                try
                {
                    streamSocketListener = new StreamSocketListener();

                    streamSocketListener.ConnectionReceived += async (sender, args) =>
                    await StreamSocketListener_ConnectionReceivedAsync(sender, args,
                        _serviceName, _successEvent, _onReceived, _errorEvent);

                    await streamSocketListener.BindServiceNameAsync(_serviceName);

                    var localHost = NetworkInformation.GetHostNames().Where(q => q.Type == HostNameType.Ipv4).First();

                    Debug.WriteLine("IP:" + localHost + "Port:" + _serviceName);
                }
                catch(System.UnauthorizedAccessException uae)
                {
                    OnError?.Invoke("ネットワーク機能にアクセスできませんでした。マニフェストの設定を見直してください");

                    return;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke("接続待ちの状態のエラー");
                    return;
                }
            });
        }

        private async Task StreamSocketListener_ConnectionReceivedAsync(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args,
            string _serviceName, Action<ITCPServer> _successEvent, Action<string> _onReceived, Action<string> _errorEvent)
        {
            streamSocketListener.Dispose();

            StartServerAsync(_serviceName, _successEvent, _onReceived, _errorEvent);

            string request;

            streamWriter = new StreamWriter(args.Socket.OutputStream.AsStreamForWrite());

            TcpNetworkInfoPair = new NetworkInfoPair(
                new NetworkInfo(args.Socket.Information.RemoteAddress.DisplayName, args.Socket.Information.RemotePort),
                new NetworkInfo(args.Socket.Information.LocalAddress.DisplayName, args.Socket.Information.LocalPort));

            _successEvent?.Invoke(this);

            using (var streamreader = new StreamReader(args.Socket.InputStream.AsStreamForRead()))
            {
                for (; ; )
                {
                    try
                    {
                        request = await streamreader.ReadLineAsync();

                        _onReceived?.Invoke(request);

                        await streamWriter.WriteLineAsync("レスポンス::" + request);
                        await streamWriter.FlushAsync();
                    }
                    catch
                    {
                        streamreader.Close();
                        streamreader.Dispose();

                        OnError?.Invoke("接続が切断されました");
                        break;
                    }
                }
            }
        }
    }
}
