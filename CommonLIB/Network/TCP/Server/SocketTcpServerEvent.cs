using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonLIB.Network.TCP.Server
{
    public delegate void SocketTcpServerEvent(SocketTcpServerBase SocketServerBase, byte[] Data);
}
