using System;

namespace MultiNetworkLib.TCP.Interface
{
    public interface ITCPClient
    {
        NetworkInfoPair TcpNetworkInfoPair { get; set; }

        event Action<string> OnError;

        void StartClientAsync(INetworkInfo _remoteTcpNetworkinfo, int _timeout, Action<ITCPClient> _successEvent, Action<string> _onReceived,Action _timeoutEvent, Action<string> _errorEvent);
        void SendDataToServerAsync(string message);
        void Disconnect();

    }
}
