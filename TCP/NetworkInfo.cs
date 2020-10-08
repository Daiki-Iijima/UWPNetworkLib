using MultiNetworkLib.TCP.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiNetworkLib.TCP
{
    public class NetworkInfo:INetworkInfo
    {
        public string HostName { get; set; }
        public string  ServiceName { get; set; }

        public NetworkInfo(string hostName,string serviceName)
        {
            HostName = hostName;
            ServiceName = serviceName;
        }

        public bool CheckEqual(NetworkInfo info)
        {
            return ((this.HostName == info.HostName) && (this.ServiceName == info.ServiceName));
        }
    }
}
