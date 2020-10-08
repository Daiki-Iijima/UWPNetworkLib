using MultiNetworkLib.TCP.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace MultiNetworkLib.TCP
{
    class UwpTcpClient:ITCPClient
    {
        private StreamWriter streamWriter = null;
        private StreamSocket socket = null;

        public NetworkInfoPair TcpNetworkInfoPair { get; set; }

        public event Action<string> OnError;
        
        public void Disconnect()
        {
            if(socket != null)
            {
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
                    OnError?.Invoke("送信できませんでした"+message);
                }
            });
        }

        public void StartClientAsync(INetworkInfo _remoteTcpNetworkinfo, int _timeout, Action<ITCPClient> _successEvent, Action<string> _onReceived,Action _timeoutEvent, Action<string> _errorEvent)
        {
            OnError += _errorEvent;
            Task.Run(async () =>
            {
                //  ソケットの生成
                using (var _socket = new StreamSocket())
                {
                    socket = _socket;

                    try
                    {
                        CancellationTokenSource cts = new CancellationTokenSource();
                        cts.CancelAfter(_timeout);
                        await _socket.ConnectAsync(new HostName(_remoteTcpNetworkinfo.HostName), _remoteTcpNetworkinfo.ServiceName).AsTask(cts.Token);
                    }
                    catch(TaskCanceledException te)
                    {
                        _timeoutEvent?.Invoke();
                        return;
                    }
                    catch(UnauthorizedAccessException uae)
                    {
                        OnError?.Invoke("WiFiへのアクセスが許可されていません");
                        return;
                    }
                    catch(Exception e)
                    {
                        OnError?.Invoke("対象への接続時の不明なエラー");
                        return;
                    }

                    TcpNetworkInfoPair = new NetworkInfoPair(
                            new NetworkInfo(_socket.Information.LocalAddress.DisplayName.ToString(), _socket.Information.LocalPort),
                            new NetworkInfo(_socket.Information.RemoteAddress.DisplayName.ToString(), _socket.Information.RemotePort)
                            );

                    streamWriter = new StreamWriter(_socket.OutputStream.AsStreamForWrite());

                    _successEvent?.Invoke(this);

                    using (var streamReader = new StreamReader(_socket.InputStream.AsStreamForRead()))
                    {
                        for(; ; )
                        {
                            try
                            {
                                var str = await streamReader.ReadLineAsync();

                                if (string.IsNullOrEmpty(str)) { OnError?.Invoke("空白文字が送られてきたため、Serverから切断されたと判断しました");break; }

                                _onReceived?.Invoke(str);
                            }catch
                            {
                                OnError?.Invoke("接続が切断されました");

                                break;
                            }
                        }
                    }
                }
            });
        }
    }
}
