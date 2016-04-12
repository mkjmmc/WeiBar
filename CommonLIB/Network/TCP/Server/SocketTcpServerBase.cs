using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace CommonLIB.Network.TCP.Server
{
    public class SocketTcpServerBase
    {
        public int Type;
        public byte ID;
        public Socket Socket;
        public TcpServer TcpServer;
        public SocketTcpServerBase(TcpServer TcpServer,Socket Socket, int Type, byte ID)
        {
            this.Type = Type;
            this.ID = ID;
            this.Socket = Socket;
            this.TcpServer = TcpServer;
        }
        public void SendData(byte[] Data)
        {
            TcpServer.SendData(ID, Socket, Data);
        }
    }
}
