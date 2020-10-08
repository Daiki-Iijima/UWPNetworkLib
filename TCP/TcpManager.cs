using MultiNetworkLib.TCP.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace MultiNetworkLib.TCP
{
    class TcpManager : ITCPController
    {
        #region シングルトン
        private static TcpManager _singleInstance = new TcpManager();

        public static TcpManager GetInstance()
        {
            return _singleInstance;
        }

        #endregion

        private TcpManager()
        {
            TcpClientList = new List<ITCPClient>();
            TcpServerList = new List<ITCPServer>();
        }

        public List<ITCPClient> TcpClientList { get; set; }
        public List<ITCPServer> TcpServerList { get; set; }

        public bool StartConnect(string ip, string port, int timeout, Action<ITCPClient> SuccessEvent, Action<string> OnReceived, Action timeoutEvent, Action<string> ErrorEvent)
        {
            ITCPClient tcpController = null;

            try
            {
                var localHost = NetworkInformation.GetHostNames().Where(q => q.Type == Windows.Networking.HostNameType.Ipv4).First();
            }catch
            {
                
                return false;
            }

            var remoteInfo = new NetworkInfo(ip, port);

            foreach(var client in TcpClientList)
            {
                if(remoteInfo.CheckEqual(client.TcpNetworkInfoPair.Remote))
                {
                    string errorStr = client.TcpNetworkInfoPair.Remote.HostName + "::" + client.TcpNetworkInfoPair.Remote.ServiceName+"への接続がすでに存在しています。";

                    ErrorEvent?.Invoke(errorStr);

                    return false;
                }
            }

            int nowConectCount = TcpClientList.Count;

            tcpController = UwpTcpClientInit(remoteInfo, timeout, SuccessEvent, OnReceived, () => { timeoutEvent(); TcpClientList.RemoveAt(nowConectCount); }, a => { ErrorEvent(a); TcpClientList.RemoveAt(nowConectCount); });

            if (tcpController == null) return false;
            
            return true;
        }

        public bool StartServer(string port, Action<ITCPServer> SuccessEvent, Action<string> OnReceived, Action<string> OnError)
        {
            ITCPServer tcpServer = null;

            string localHost = GetLocalHost();

            if(localHost == null)
            {
                OnError?.Invoke("Wifiに接続されていません");
                return false;
            }

            var remoteInfo = new NetworkInfo(localHost, port);

            foreach(var server in TcpServerList)
            {
                if(remoteInfo.CheckEqual(server.TcpNetworkInfoPair.Local))
                {
                    string errorStr = server.TcpNetworkInfoPair.Local.HostName + "::" + server.TcpNetworkInfoPair.Local.ServiceName + "";

                    OnError?.Invoke(errorStr);

                    return false;
                }
            }

            int nowConectCount = TcpServerList.Count;

            tcpServer = UwpTcpServerInit(port, SuccessEvent, OnReceived, a => { OnError("Server:" + a); TcpServerList.RemoveAt(nowConectCount); });

            if (tcpServer == null) return false;

            TcpServerList.Add(tcpServer);

            return true;
        }

        private ITCPClient UwpTcpClientInit(NetworkInfo remoteInfo,int timeout,Action<ITCPClient> SuccsesEvent,Action<string> OnReceived,Action timeoutEvent,Action<string> ErrorEvent)
        {
            var tcpController = new UwpTcpClient();

            tcpController.StartClientAsync(remoteInfo, timeout, SuccsesEvent, OnReceived, timeoutEvent, ErrorEvent);

            return tcpController; 
        }

        private ITCPServer UwpTcpServerInit(string port, Action<ITCPServer> SuccessEvent, Action<string> OnReceived, Action<string> ErrorEvent)
        {
            var tcpController = new UwpTcpServer();

            tcpController.StartServerAsync(port,SuccessEvent,OnReceived,ErrorEvent);

            return tcpController;
        }

        public void DisconnectClient(ITCPClient tcp)
        {
            tcp.Disconnect();
        }

        public void DisconnectClient(NetworkInfo netInfo)
        {
            foreach(var tcp in TcpClientList)
            {
                if (tcp.TcpNetworkInfoPair.Remote.CheckEqual(netInfo))
                {
                    tcp.Disconnect();
                }
            }
        }

        public void DisconnectServert(ITCPServer tcp)
        {
            tcp.Disconnect();
        }

        public void DisconnectServert(NetworkInfo netInfo)
        {
            foreach (var tcp in TcpServerList)
            {
                if (tcp.TcpNetworkInfoPair.Remote.CheckEqual(netInfo))
                {
                    tcp.Disconnect();
                }
            }
        }

        public string GetLocalHost()
        {
            try
            {
                return NetworkInformation.GetHostNames().Where(q => q.Type == Windows.Networking.HostNameType.Ipv4).First().ToString();
            }catch
            {
                return null;
            }
        }

    }
}
