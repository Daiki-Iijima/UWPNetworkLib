using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiNetworkLib.TCP
{
    public class NetworkInfoPair
    {
        public NetworkInfo Local { get; }
        public NetworkInfo Remote { get; }

        public NetworkInfoPair(NetworkInfo localInfo, NetworkInfo remoteInfo)
        {
            Local = localInfo;
            Remote = remoteInfo;
        }
    }
}
