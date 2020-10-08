using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiNetworkLib.TCP.Interface
{
    public interface ITCPServer
    {
        NetworkInfoPair TcpNetworkInfoPair { get; set; }

        event Action<string> OnError;

        void StartServerAsync(string _serviceName, Action<ITCPServer> _successEvent, Action<string> _onReceived,Action<string> _errorEvent);
        void SendDataToServerAsync(string message);
        void Disconnect();

    }
}
