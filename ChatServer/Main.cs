using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CommonLIB.Network.HTTP;
using CommonLIB.Network.TCP.Server;
using PeiduiServer;

namespace ChatServer
{
    public class Main
    {
        static string listenIP;
        static string listenPort;
        static TcpServer listenServer;
        private static Dictionary<Socket, ClientInfo> clientPool = new Dictionary<Socket, ClientInfo>();
        private static Dictionary<string, RoomInfo> roomPool = new Dictionary<string, RoomInfo>();
        private static List<SocketMessage> msgPool = new List<SocketMessage>();
        private static bool isClear = true;

        /// <summary>
        /// 用于进行锁
        /// </summary>
        static string lockDictionary = "";

        public static void Start()
        {
            listenIP = System.Configuration.ConfigurationManager.AppSettings["ListenIP"];
            listenPort = System.Configuration.ConfigurationManager.AppSettings["ListenPort"];
            StartListen();
            Console.WriteLine("Server is ready");
            Console.ReadLine();
            Broadcast();
        }
        public static void StartListen()
        {
            listenServer = new TcpServer();
            listenServer.Init(listenIP, int.Parse(listenPort), ListenEvent, 0, 0);
        }

        /// <summary>
        /// 在独立线程中不停地向所有客户端广播消息
        /// </summary>
        private static void Broadcast()
        {
            Thread broadcast = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        if (!isClear)
                        {
                            byte[] msg = PackageServerData(msgPool[0].Message);
                            var room = roomPool[msgPool[0].RoomID];
                            foreach (KeyValuePair<Socket, ClientInfo> cs in room.UserList)
                            {
                                if (!clientPool.ContainsKey(cs.Key))
                                {
                                    room.UserList.Remove(cs.Key);
                                    continue;
                                }
                                Socket client = cs.Key;
                                if (client.Poll(10, SelectMode.SelectWrite))
                                {
                                    client.Send(msg, msg.Length, SocketFlags.None);
                                }
                            }
                            msgPool.RemoveAt(0);
                            isClear = msgPool.Count == 0 ? true : false;
                        }
                        else
                        {
                            Thread.Sleep(10);
                        }
                    }
                    catch (Exception)
                    {
                        
                        throw;
                    }
                   
                }
            });

            broadcast.Start();
        }


        /// <summary>
        /// Socket回调
        /// </summary>
        /// <param name="SocketServerBase"></param>
        /// <param name="Data"></param>
        public static void ListenEvent(SocketTcpServerBase SocketServerBase, byte[] Data)
        {
            try
            {
                lock (clientPool)
                {
                    if (SocketServerBase.Type == SocketTcpServer.连接成功)
                    {
                        if (Data != null && Data.Length > 0)
                        {
                            //string msg = Encoding.UTF8.GetString(Data);
                            SocketServerBase.SendData(PackageHandShakeData(Data, Data.Length));
                        }
                        clientPool.Add(SocketServerBase.Socket, new ClientInfo()
                        {
                            Id = SocketServerBase.Socket.RemoteEndPoint,
                            handle = SocketServerBase.Socket.Handle,
                            buffer = Data,
                        });
                    }
                    else if (SocketServerBase.Type == SocketTcpServer.接收成功)
                    {
                        var client = SocketServerBase.Socket;
                        //int length = client.EndReceive(result);
                        string msg = Encoding.UTF8.GetString(Data);

                        if (!clientPool[SocketServerBase.Socket].IsHandShaked && msg.Contains("Sec-WebSocket-Key"))
                        {
                            SocketServerBase.Socket.Send(PackageHandShakeData(Data, Data.Length));
                            clientPool[SocketServerBase.Socket].IsHandShaked = true;
                            return;
                        }

                        msg = AnalyzeClientData(Data, msg.Length);
                        Console.WriteLine("{0} @ {1}\r\n    {2}", client.RemoteEndPoint, DateTime.Now, msg);
                        if (string.IsNullOrWhiteSpace(msg))
                        {
                            return;
                        }
                        // 数据解析
                        var action = SerializeUtility.JavaScriptDeserialize<ActionModel<object>>(msg);
                        if (action != null)
                        {
                            switch (action.action)
                            {
                                case "joinroom":
                                    {
                                        // 加入房间，向当前用户发送加入成功消息
                                        var joinroommodel =
                                            SerializeUtility.JavaScriptDeserialize<ActionModel<JoinRoomModel>>(msg);
                                        // 获取房间信息
                                        roomPool.TryAdd(joinroommodel.data.bar, new RoomInfo() { ID = joinroommodel.data.bar });
                                        roomPool[joinroommodel.data.bar].UserList.AddOrPeplace(client, clientPool[client]);
                                        // 返回房间加入成功消息
                                        var msg1 = PackageServerData(
                                            SerializeUtility.JavaScriptSerialize(
                                                new ActionModel<object>()
                                                {
                                                    action = "joinroomsuccess",
                                                    data = new { }
                                                }));
                                        client.Send(
                                            msg1
                                            , msg1.Length
                                            , SocketFlags.None);
                                        clientPool[client].Bar = joinroommodel.data.bar;
                                        // 加入广播消息
                                        msgPool.Add(new SocketMessage()
                                        {
                                            RoomID = joinroommodel.data.bar,
                                            Message = SerializeUtility.JavaScriptSerialize(new ActionModel<object>()
                                            {
                                                action = "joinroom",
                                                data = new
                                                {
                                                    ui = joinroommodel.data.ui,
                                                    un = joinroommodel.data.un,
                                                }
                                            }),
                                        });
                                        isClear = false;
                                        break;
                                    }
                                case "sendmessage":
                                    {
                                        var messagemodel = SerializeUtility.JavaScriptDeserialize<ActionModel<MessageModel>>(msg);
                                        // 加入广播消息
                                        msgPool.Add(new SocketMessage()
                                        {
                                            RoomID = messagemodel.data.bar,
                                            Message = SerializeUtility.JavaScriptSerialize(new ActionModel<object>()
                                            {
                                                action = "newmessage",
                                                data = new
                                                {
                                                    ui = messagemodel.data.ui,
                                                    un = messagemodel.data.un,
                                                    msg = messagemodel.data.msg,
                                                }
                                            }),
                                        });
                                        isClear = false;
                                        break;
                                    }
                            }
                        }


                    }
                    else if (SocketServerBase.Type == SocketTcpServer.断开连接)
                    {
                        if (clientPool.ContainsKey(SocketServerBase.Socket) && !string.IsNullOrWhiteSpace(clientPool[SocketServerBase.Socket].Bar))
                        {
                            roomPool[clientPool[SocketServerBase.Socket].Bar].UserList.Remove(SocketServerBase.Socket);
                        }
                        clientPool.Remove(SocketServerBase.Socket);
                    }
                    else if (SocketServerBase.Type == SocketTcpServer.连接超时)
                    {
                        if (clientPool.ContainsKey(SocketServerBase.Socket) && !string.IsNullOrWhiteSpace(clientPool[SocketServerBase.Socket].Bar))
                        {
                            roomPool[clientPool[SocketServerBase.Socket].Bar].UserList.Remove(SocketServerBase.Socket);
                        }
                        clientPool.Remove(SocketServerBase.Socket);
                    }
                }
                //if (SocketServerBase.Type == SocketTcpServer.接收成功)
                //{
                //    //HashTableXml xml = new HashTableXml(Data);
                //    //string cmd = (string)xml["CMD"];
                //    //switch (cmd)
                //    //{
                //    //    case "TeacherOnline":
                //    //        // 教师上线
                //    //        TeacherOnline(SocketServerBase, (string)xml["UserID"], (string)xml["NickName"], (int)xml["Sex"], (string)xml["Photo"]);
                //    //        break;
                //    //    case "TeacherOffline":
                //    //        // 教师下线
                //    //        TeacherOffline(SocketServerBase);
                //    //        break;
                //    //    case "UserPair":
                //    //        // 用户请求
                //    //        UserPair(SocketServerBase, (string)xml["UserID"], (string)xml["NickName"], (int)xml["Sex"], (string)xml["Photo"], (string)xml["voipAccount"]);
                //    //        break;
                //    //    case "UserPairEnd":
                //    //        // 用户取消请求
                //    //        UserPairEnd(SocketServerBase);
                //    //        break;
                //    //    case "TeacherAgree":
                //    //        // 教师接受订单
                //    //        TeacherAgree(SocketServerBase, (string)xml["UserID"]);
                //    //        break;
                //    //}
                //}
                //else if (SocketServerBase.Type == SocketTcpServer.断开连接)
                //{
                //    // 用户处理
                //    UserPairEnd(SocketServerBase);

                //    // 教师处理
                //    lock (lockDictionary)
                //    {
                //        if (SocketTeacher.ContainsKey(SocketServerBase.Socket))
                //        {
                //            // 教师断开，删除教师信息，删除教师的相关订单
                //            var id = SocketTeacher[SocketServerBase.Socket];
                //            teacherinfo.Remove(id);
                //            dicordersinfo.Remove(id);
                //            SocketTeacher.Remove(SocketServerBase.Socket);

                //            // 向服务器发送下线消息
                //            ThreadPool.QueueUserWorkItem(SendWaitStat, new object[] { id, 1 });
                //        }
                //    }
                //}
                //else if (SocketServerBase.Type == SocketTcpServer.连接超时)
                //{

                //}
                //else if (SocketServerBase.Type == SocketTcpServer.连接成功)
                //{

                //}

            }
            catch
            {
                
            }
        }

        /// <summary>
        /// 打包服务器握手数据
        /// </summary>
        /// <returns>The hand shake data.</returns>
        /// <param name="handShakeBytes">Hand shake bytes.</param>
        /// <param name="length">Length.</param>
        private static byte[] PackageHandShakeData(byte[] handShakeBytes, int length)
        {
            string handShakeText = Encoding.UTF8.GetString(handShakeBytes, 0, length);
            string key = string.Empty;
            Regex reg = new Regex(@"Sec\-WebSocket\-Key:(.*?)\r\n");
            Match m = reg.Match(handShakeText);
            if (m.Value != "")
            {
                key = Regex.Replace(m.Value, @"Sec\-WebSocket\-Key:(.*?)\r\n", "$1").Trim();
            }

            byte[] secKeyBytes = SHA1.Create().ComputeHash(
                                     Encoding.ASCII.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"));
            string secKey = Convert.ToBase64String(secKeyBytes);

            var responseBuilder = new StringBuilder();
            responseBuilder.Append("HTTP/1.1 101 Switching Protocols" + "\r\n");
            responseBuilder.Append("Upgrade: websocket" + "\r\n");
            responseBuilder.Append("Connection: Upgrade" + "\r\n");
            responseBuilder.Append("Sec-WebSocket-Accept: " + secKey + "\r\n\r\n");

            return Encoding.UTF8.GetBytes(responseBuilder.ToString());
        }
        /// <summary>
        /// 把发送给客户端消息打包处理（拼接上谁什么时候发的什么消息）
        /// </summary>
        /// <returns>The data.</returns>
        /// <param name="message">Message.</param>
        private static byte[] PackageServerData(string msg)
        {
            byte[] content = null;
            byte[] temp = Encoding.UTF8.GetBytes(msg.ToString());

            if (temp.Length < 126)
            {
                content = new byte[temp.Length + 2];
                content[0] = 0x81;
                content[1] = (byte)temp.Length;
                Array.Copy(temp, 0, content, 2, temp.Length);
            }
            else if (temp.Length < UInt16.MaxValue)
            {
                content = new byte[temp.Length + 4];
                content[0] = 0x81;
                content[1] = 126;
                content[3] = (byte)(temp.Length & 0xFF);
                content[2] = (byte)(temp.Length >> 8 & 0xFF);
                Array.Copy(temp, 0, content, 4, temp.Length);
            }
            else
            {
                // 暂不处理超长内容  
            }

            return content;
        }


        /// <summary>
        /// 解析客户端发送来的数据
        /// </summary>
        /// <returns>The data.</returns>
        /// <param name="recBytes">Rec bytes.</param>
        /// <param name="length">Length.</param>
        private static string AnalyzeClientData(byte[] recBytes, int length)
        {
            if (length < 2)
            {
                return string.Empty;
            }

            bool fin = (recBytes[0] & 0x80) == 0x80; // 1bit，1表示最后一帧  
            if (!fin)
            {
                return string.Empty;// 超过一帧暂不处理 
            }

            bool mask_flag = (recBytes[1] & 0x80) == 0x80; // 是否包含掩码  
            if (!mask_flag)
            {
                return string.Empty;// 不包含掩码的暂不处理
            }

            int payload_len = recBytes[1] & 0x7F; // 数据长度  

            byte[] masks = new byte[4];
            byte[] payload_data;

            if (payload_len == 126)
            {
                Array.Copy(recBytes, 4, masks, 0, 4);
                payload_len = (UInt16)(recBytes[2] << 8 | recBytes[3]);
                payload_data = new byte[payload_len];
                Array.Copy(recBytes, 8, payload_data, 0, payload_len);

            }
            else if (payload_len == 127)
            {
                Array.Copy(recBytes, 10, masks, 0, 4);
                byte[] uInt64Bytes = new byte[8];
                for (int i = 0; i < 8; i++)
                {
                    uInt64Bytes[i] = recBytes[9 - i];
                }
                UInt64 len = BitConverter.ToUInt64(uInt64Bytes, 0);

                payload_data = new byte[len];
                for (UInt64 i = 0; i < len; i++)
                {
                    payload_data[i] = recBytes[i + 14];
                }
            }
            else
            {
                Array.Copy(recBytes, 2, masks, 0, 4);
                payload_data = new byte[payload_len];
                Array.Copy(recBytes, 6, payload_data, 0, payload_len);

            }

            for (var i = 0; i < payload_len; i++)
            {
                payload_data[i] = (byte)(payload_data[i] ^ masks[i % 4]);
            }

            return Encoding.UTF8.GetString(payload_data);
        }

        #region 将时间转换成毫秒数
        /// <summary>
        /// 将时间转换成毫秒数
        /// </summary>
        /// <param name="time">时间</param>
        /// <returns>毫秒数</returns>
        public static long GetTimeMilliseconds(DateTime time)
        {
            return (long)time.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
        }

        #endregion
    }

    public class ClientInfo
    {
        public byte[] buffer;

        public string NickName { get; set; }

        public EndPoint Id { get; set; }

        public IntPtr handle { get; set; }

        public string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(NickName))
                {
                    return NickName;
                }
                else
                {
                    return string.Format("{0}#{1}", Id, handle);
                }
            }
        }

        public bool IsHandShaked { get; set; }

        public  string Bar { get; set; }
    }
    public class SocketMessage
    {
        public string RoomID { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// 房间信息
    /// </summary>
    public class RoomInfo
    {
        public RoomInfo()
        {
            UserList = new Dictionary<Socket, ClientInfo>();
        }
        public string ID { get; set; }
        public string Name { get; set; }
        public Dictionary<Socket, ClientInfo> UserList { get; set; }
    }
}
