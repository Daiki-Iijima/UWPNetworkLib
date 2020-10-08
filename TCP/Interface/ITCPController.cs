using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiNetworkLib.TCP.Interface
{
    interface ITCPController
    {
        List<ITCPClient> TcpClientList { get; set; }
        List<ITCPServer> TcpServerList { get; set; }

        bool StartConnect(string ip,string port,int timeout, Action<ITCPClient> SuccessEvent, Action<string> OnReceived, Action timeoutEvent,Action<string>ErrorEvent);
        bool StartServer(string port, Action<ITCPServer> SuccessEvent, Action<string> OnReceived, Action<string> ErrorEvent);

        void DisconnectClient(ITCPClient tcp);
        void DisconnectClient(NetworkInfo netInfo);
        void DisconnectServert(ITCPServer tcp);
        void DisconnectServert(NetworkInfo netInfo);
    }
}
