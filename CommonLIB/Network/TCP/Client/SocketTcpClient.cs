using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
namespace CommonLIB.Network.TCP.Client
{
   public class SocketTcpClient
    {
        public const int 连接超时 = 0;
        public const int 连接成功 = 1;
        public const int 发送失败 = 2;
        public const int 接收超时 = 3;
        public const int 接收成功 = 4;
        public const int 断开连接 = 5;
       
    }
}
