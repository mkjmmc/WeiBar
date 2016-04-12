using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonLIB.Network.TCP.Server
{
    public class SocketTcpServer
    {
        public static int 连接超时 = 0;
        public static int 连接成功 = 1;
        public static int 发送失败 = 2;
        public static int 接收超时 = 3;
        public static int 接收成功 = 4;
        public static int 断开连接 = 5;
    }
}
