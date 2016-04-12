using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace CommonLIB.Network.TCP.Client
{
     public delegate void SocketTcpClientEvent(TcpClient Socket,int Type, byte[] Data);
}
