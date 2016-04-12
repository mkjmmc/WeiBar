using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Concurrent;
using System.Diagnostics;
using CommonLIB;
namespace CommonLIB.Network.TCP.Server
{
    public class SocketModel
    {
        public Socket socket;
        public SocketAsyncEventArgs readEvent;
        public SocketAsyncEventArgs writeEvent;
        public List<byte[]> writeDataBuf;
        //public ReadDataModel readDataBuf;
        public bool writeType;
    }
    public class TcpServer
    {
        int connectErrorNum;
        Socket socket;
        SocketAsyncEventArgs connectEvent;
        public Stack<byte[]> bufDataPool = new Stack<byte[]>(100000);
        public SocketAsyncEventArgsPool readEventPool;
        public SocketAsyncEventArgsPool writeEventPool;
        public Dictionary<Socket, SocketModel> socketDic;
        //Dictionary<Socket, SocketAsyncEventArgs> writeEvent;
        //Dictionary<Socket, ReadDataModel> readDataBuf;
        //Dictionary<Socket, bool> writeType;
        //object writeLock = new object();
        //object readLock = new object();
        //object connectLock = new object();
        //static Dictionary<Socket, long> messageTime;
        //Thread messageThread;
        SocketTcpServerEvent SocketTcpServerEvent;
        byte[] headByte = new byte[] { 0xaa, 0xbb, 0xcc, 0xdd, 0x00, 0x00, 0x00, 0x00 };
        PerformanceCounter[] counters = new PerformanceCounter[System.Environment.ProcessorCount];
        float processorNum = 0.0f;
        int threadNum = 0;
        int threadNowNum = 0;
        public void Init(string IP,int Port, SocketTcpServerEvent SocketTcpServerEvent, int MessageNum, int MessageTime)
        {
            for (int i = 0; i < counters.Length; i++)
            {
                counters[i] = new PerformanceCounter("Processor", "% Processor Time", i.ToString());
            }
            ThreadPool.GetMinThreads(out threadNum, out threadNum);
            connectErrorNum = 0;
            this.SocketTcpServerEvent = SocketTcpServerEvent;
            readEventPool = new SocketAsyncEventArgsPool(100000);
            //readDataBuf = new Dictionary<Socket, ReadDataModel>();
            writeEventPool = new SocketAsyncEventArgsPool(100000);
            socketDic = new Dictionary<Socket, SocketModel>();
            //writeEvent = new Dictionary<Socket, SocketAsyncEventArgs>();
            //writeDataBuf = new Dictionary<Socket, List<byte[]>>();
            //writeType = new Dictionary<Socket, bool>();
            //messageThread = new Thread(new ThreadStart(message_Run));
            //messageThread.Start();
            //messageTime = new Dictionary<Socket, long>();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //socket.SendBufferSize = 102400000;
            //socket.ReceiveBufferSize =102400000;
            socket.NoDelay = false;
            uint dummy = 0;
            byte[] inOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
            BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes((uint)60000).CopyTo(inOptionValues, Marshal.SizeOf(dummy));
            BitConverter.GetBytes((uint)6000).CopyTo(inOptionValues, Marshal.SizeOf(dummy) * 2);
            socket.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
            socket.Bind(new IPEndPoint(IPAddress.Parse(IP), Port));
            socket.Listen(100);
            connectEvent = new SocketAsyncEventArgs();
            connectEvent.DisconnectReuseSocket = false;
            connectEvent.Completed +=connectEvent_Completed;
            socket.AcceptAsync(connectEvent);

        }
        int GetProcessor()
        {
            processorNum = 0;
            for (int i = 0; i < counters.Length; i++)
            {
                processorNum += counters[i].NextValue();
            }
            return (int)(processorNum / counters.Length);
        }
        public byte[] getBufData()
        {
            lock (bufDataPool)
            {
                if (bufDataPool.Count == 0)
                {
                    return null;
                }
                return bufDataPool.Pop();
            }
        }
        public void setBufData(byte[] data)
        {
            lock (bufDataPool)
            {
                if (data != null)
                {
                    bufDataPool.Push(data);
                }
            }
        }
        //void message_Run()
        //{
        //    while (true)
        //    {
        //        lock (messagePool)
        //        {

        //            foreach (var _v in messagePool)
        //            {
        //                if (_v.Value.Count > messageNum)
        //                {
        //                    _v.Key.Close();
        //                    continue;
        //                }
        //                if (_v.Value.Count > 0)
        //                {
        //                    try
        //                    {
        //                        //if (GetProcessor() <= 80)
        //                        //{
        //                        //threadNowNum++;
        //                        ThreadPool.QueueUserWorkItem(ReadOK, new object[] { _v.Key, _v.Value[0] });
        //                        _v.Value.RemoveAt(0);
        //                        //}

        //                    }
        //                    catch { }
        //                }
        //            }

        //        }
        //        Thread.Sleep(messageTime);
        //    }
        //}
        void connectEvent_Completed(object sender, SocketAsyncEventArgs e)
        {
            lock (socketDic)
            {
                SocketModel socketModel = new SocketModel();
                try
                {

                    socketModel.socket = e.AcceptSocket;
                    //_socket.SendBufferSize = 65535;
                    //_socket.ReceiveBufferSize = 65535;
                    socketModel.socket.NoDelay = true;
                    socketModel.readEvent = readEventPool.Pop();
                    if (socketModel.readEvent == null)
                    {
                        socketModel.readEvent = new SocketAsyncEventArgs();
                        byte[] _buf = getBufData();
                        if (_buf == null)
                        {
                            _buf = new byte[4096];
                        }
                        socketModel.readEvent.DisconnectReuseSocket = false;
                        socketModel.readEvent.SetBuffer(_buf, 0, _buf.Length);
                        socketModel.readEvent.Completed += readEvent_Completed;
                    }
                    socketModel.readEvent.UserToken = socketModel.socket;
                    socketModel.writeEvent = writeEventPool.Pop();
                    if (socketModel.writeEvent == null)
                    {
                        socketModel.writeEvent = new SocketAsyncEventArgs();
                        socketModel.writeEvent.DisconnectReuseSocket = false;
                        socketModel.writeEvent.Completed +=writeEvent_Completed;
                    }
                    socketModel.writeEvent.UserToken = socketModel.socket;
                    socketModel.writeType = false;
                    socketModel.writeDataBuf = new List<byte[]>();
                    //socketModel.readDataBuf = new ReadDataModel();
                    // messageTime.Add(_socket, 0L);
                    connectErrorNum = 0;
                    ThreadPool.QueueUserWorkItem(ConnectOK, new object[] { socketModel.socket });
                    e.AcceptSocket = null;
                    socketDic.Add(socketModel.socket, socketModel);
                    bool isReceiveOK=socketModel.socket.ReceiveAsync(socketModel.readEvent);
                    if (!isReceiveOK)
                    {
                        readEvent_Completed(null, socketModel.readEvent);
                    }
                    bool isOK=socket.AcceptAsync(e);
                    if (!isOK)
                    {
                        connectEvent_Completed(socket, e);
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        StreamWriter _sw = new StreamWriter(@"c:\SocketError.txt", true);
                        _sw.WriteLine("-------------------" + DateTime.Now.ToString() + "-----------------------");
                        _sw.WriteLine(ex.ToString());
                        _sw.Close();
                        if (socketModel.readEvent != null)
                        {
                            //_readEventx.AcceptSocket = null;
                            readEventPool.Push(socketModel.readEvent);
                        }
                        if (socketModel.writeEvent != null)
                        {
                            writeEventPool.Push(socketModel.writeEvent);
                        }
                        if (socketModel.socket != null)
                        {
                            DisposeSocket(socketModel.socket);
                        }

                    }
                    catch { }
                }
            }
        }

        void writeEvent_Completed(object sender, SocketAsyncEventArgs e)
        {

            SocketModel socketModel;
            lock (socketDic)
            {
                Socket _socket = e.UserToken as Socket;
                if (_socket == null || !socketDic.ContainsKey(_socket) || e.SocketError != SocketError.Success)
                {
                    return;
                }
                else
                {
                    socketModel = socketDic[_socket];
                }
                try
                {

                    if (socketModel.writeDataBuf.Count() > 0)
                    {
                        byte[] _data = socketModel.writeDataBuf[0];
                        socketModel.writeDataBuf.RemoveAt(0);
                        e.SetBuffer(_data, 0, _data.Length);
                        bool isOK=socketModel.socket.SendAsync(e);
                        if (!isOK)
                        {
                            writeEvent_Completed(null, e);
                        }
                    }
                    else
                    {
                        socketModel.writeType = false;
                    }
                }
                catch
                {
                    socketModel.writeType = false;
                }
            }
            
        }

        void readEvent_Completed(object sender, SocketAsyncEventArgs e)
        {
            SocketModel socketModel;
            lock (socketDic)
            {
                socketModel = socketDic[e.UserToken as Socket];
                try
                {
                    if (e.SocketError == SocketError.Success && e.BytesTransferred != 0)
                    {
                        byte[] _readData = new byte[e.BytesTransferred];
                        Array.Copy(e.Buffer, 0, _readData, 0, e.BytesTransferred);
                        //ReadDataModel _readDataModel = socketModel.readDataBuf;
                        ThreadPool.QueueUserWorkItem(ReadOK, new object[] { socketModel.socket, _readData });
                        //while (true)
                        //{
                        //    //byte[] _data = _readDataModel.Process(_readData);
                        //    //if (_readDataModel.readType == 0 && _readDataModel.dataOK != null)
                        //    //{
                        //    if (_readData.Length > 0)
                        //    {
                        //        ThreadPool.QueueUserWorkItem(ReadOK, new object[] { socketModel.socket, _readData });
                        //    }
                        //    else
                        //    {
                        //        break;
                        //    }
                        //    //}
                        //    //if (_data != null)
                        //    //{
                        //    //    _readData = _data;
                        //    //}
                        //    //else
                        //    //{
                        //    //    break;
                        //    //}
                        //}
                        bool isOK=socketModel.socket.ReceiveAsync(e);
                        if (!isOK)
                        {
                            readEvent_Completed(null, e);
                        }
                    }
                    else
                    {
                        long _ip = 0;
                        try
                        {
                            _ip = (socketModel.socket.RemoteEndPoint as IPEndPoint).Address.Address;
                        }
                        catch { }
                        ThreadPool.QueueUserWorkItem(Error, new object[] { socketModel.socket, _ip });
                        DisposeSocket(socketModel.socket);
                    }
                }
                catch
                {
                    long _ip = 0;
                    try
                    {
                        _ip = (socketModel.socket.RemoteEndPoint as IPEndPoint).Address.Address;
                    }
                    catch { }
                    ThreadPool.QueueUserWorkItem(Error, new object[] { socketModel.socket, _ip });
                    DisposeSocket(socketModel.socket);
                }
            }
        }
        public void SendData(Socket Socket, byte[] Data)
        {
            SendData(0, Socket, Data);
        }
        public void SendData(byte Type, Socket Socket, byte[] Data)
        {
            lock (socketDic)
            {
                if (!socketDic.ContainsKey(Socket))
                {
                    return;
                }
                SocketModel socketModel = socketDic[Socket];
                try
                {
                    byte[] _sendData = new byte[Data.Length + 9];
                    _sendData[8] = Type;
                    Array.Copy(Data, 0, _sendData, 9, Data.Length);
                    Array.Copy(headByte, 0, _sendData, 0, 8);
                    Array.Copy(BitConverter.GetBytes(Data.Length + 1), 0, _sendData, 4, 4);
                    int _dataLength = _sendData.Length;
                    if (_dataLength <= 65535)
                    {
                        socketModel.writeDataBuf.Add(_sendData);
                    }
                    else
                    {
                        int _readNum = 0;
                        while (true)
                        {
                            if (_dataLength - _readNum <= 65535)
                            {
                                byte[] _temp = new byte[_dataLength - _readNum];
                                Array.Copy(_sendData, _readNum, _temp, 0, _dataLength - _readNum);
                                socketModel.writeDataBuf.Add(_temp);
                                break;
                            }
                            else
                            {
                                byte[] _temp = new byte[65535];
                                Array.Copy(_sendData, _readNum, _temp, 0, 65535);
                                socketModel.writeDataBuf.Add(_temp);
                                _readNum += 65535;
                            }
                        }
                    }
                    if (!socketModel.writeType)
                    {
                        socketModel.writeType = true;
                        byte[] _sendTempData = socketModel.writeDataBuf[0];
                        socketModel.writeDataBuf.RemoveAt(0);
                        SocketAsyncEventArgs _socketAsyncEventArgs = socketModel.writeEvent;
                        _socketAsyncEventArgs.SetBuffer(_sendTempData, 0, _sendTempData.Length);
                        bool isOK=Socket.SendAsync(_socketAsyncEventArgs);
                        if (!isOK)
                        {
                            writeEvent_Completed(null, _socketAsyncEventArgs);
                        }
                    }
                }
                catch { }
            }
        }
        public void DisposeMainSocket()
        {
            socket.Close();
        }
        void DisposeSocket(Socket Socket)
        {
            lock (socketDic)
            {
                try
                {
                    Socket.Shutdown(SocketShutdown.Both);
                }
                catch { }
                if (socketDic.ContainsKey(Socket))
                {
                    SocketModel socketModel = socketDic[Socket];
                    setBufData(socketModel.readEvent.Buffer);
                    socketModel.readEvent.Completed -= readEvent_Completed;
                    socketModel.writeEvent.Completed -= writeEvent_Completed;
                    socketModel.readEvent.Dispose();
                    socketModel.writeEvent.Dispose();
                    //readEventPool.Push(socketModel.readEvent);
                    //writeEventPool.Push(socketModel.writeEvent);
                    socketDic.Remove(Socket);
                }
                Socket.Close();
                Socket.Dispose();
            }
            
        }
        void ReadOK(object o)
        {
            //try
            //{
            Socket _socket = (Socket)((object[])o)[0];
            byte[] _data = (byte[])((object[])o)[1];
            byte _id = _data[0];
            //byte[] _temp = new byte[_data.Length - 1];
            //Array.Copy(_data, 1, _temp, 0, _data.Length - 1);
            SocketTcpServerEvent(new SocketTcpServerBase(this, _socket, SocketTcpServer.接收成功, _id), _data);
            threadNowNum--;
            //}
            //catch { }
        }
        void Error(object o)
        {
            Socket _socket = (Socket)((object[])o)[0];
            long _ip = (long)((object[])o)[1];
            SocketTcpServerEvent(new SocketTcpServerBase(this, _socket, SocketTcpServer.断开连接, 0), BitConverter.GetBytes(_ip));
        }
        void ConnectOK(object o)
        {
            Socket _socket = (Socket)((object[])o)[0];
            SocketTcpServerEvent(new SocketTcpServerBase(this, _socket, SocketTcpServer.连接成功, 0), null);
        }
    }

    public class SocketAsyncEventArgsPool
    {
        Stack<SocketAsyncEventArgs> m_pool;

        // Initializes the object pool to the specified size
        //
        // The "capacity" parameter is the maximum number of 
        // SocketAsyncEventArgs objects the pool can hold
        public void Clear()
        {
            lock (m_pool)
            {
                m_pool.Clear();
            }
        }
        public SocketAsyncEventArgsPool(int capacity)
        {
            m_pool = new Stack<SocketAsyncEventArgs>(capacity);
        }

        // Add a SocketAsyncEventArg instance to the pool
        //
        //The "item" parameter is the SocketAsyncEventArgs instance 
        // to add to the pool
        public void Push(SocketAsyncEventArgs item)
        {
            if (item == null) { return; }
            lock (m_pool)
            {
                m_pool.Push(item);
            }
        }

        // Removes a SocketAsyncEventArgs instance from the pool
        // and returns the object removed from the pool
        public SocketAsyncEventArgs Pop()
        {
            lock (m_pool)
            {
                if (Count == 0)
                {
                    return null;
                }
                return m_pool.Pop();
            }
        }

        // The number of SocketAsyncEventArgs instances in the pool
        public int Count
        {
            get { return m_pool.Count; }
        }
    }
    //public class ReadDataModel
    //{
    //    public int readType = 0;//0.无 1.头不完整 2.包不完整 
    //    int headNum = 0;
    //    int dataAllNum = 0;
    //    int dataNowNum = 0;
    //    byte[] data;
    //    public byte[] dataOK;
    //    public byte[] Process(byte[] Data)
    //    {
    //        if (readType == 0 || readType == 1)
    //        {
    //            if (readType == 0)
    //            {
    //                data = new byte[8];
    //                headNum = 0;
    //            }
    //            if (Data.Length < 8 - headNum)
    //            {
    //                readType = 1;
    //                Array.Copy(Data, 0, data, headNum, Data.Length);
    //                headNum += Data.Length;
    //                return null;
    //            }
    //            if (Data.Length == 8 - headNum)
    //            {
    //                readType = 2;
    //                Array.Copy(Data, 0, data, headNum, Data.Length);
    //                return null;
    //            }
    //            if (Data.Length > 8 - headNum)
    //            {
    //                byte[] _data = new byte[Data.Length - (8 - headNum)];
    //                readType = 2;
    //                Array.Copy(Data, 0, data, headNum, 8 - headNum);
    //                Array.Copy(Data, 8, _data, 0, Data.Length - (8 - headNum));
    //                return _data;
    //            }
    //        }
    //        else if (readType == 2)
    //        {
    //            if (data.Length == 8)
    //            {
    //                if (data[0] == 0xaa && data[1] == 0xbb && data[2] == 0xcc && data[3] == 0xdd)
    //                {
    //                    dataAllNum = BitConverter.ToInt32(data, 4);
    //                    if (dataAllNum > 10240000)
    //                    {
    //                        readType = 0;
    //                        headNum = 0;
    //                        dataAllNum = 0;
    //                        dataNowNum = 0;
    //                        dataOK = null;
    //                        return null;
    //                    }
    //                    data = new byte[dataAllNum];
    //                    dataNowNum = 0;

    //                }
    //                else
    //                {
    //                    readType = 0;
    //                    headNum = 0;
    //                    dataAllNum = 0;
    //                    dataNowNum = 0;
    //                    dataOK = null;
    //                    return null;
    //                }

    //            }
    //            if (dataAllNum - dataNowNum == Data.Length)
    //            {
    //                Array.Copy(Data, 0, data, dataNowNum, Data.Length);
    //                readType = 0;
    //                dataOK = data;
    //                return null;
    //            }
    //            if (dataAllNum - dataNowNum > Data.Length)
    //            {
    //                Array.Copy(Data, 0, data, dataNowNum, Data.Length);
    //                dataNowNum += Data.Length;
    //                return null;
    //            }
    //            if (dataAllNum - dataNowNum < Data.Length)
    //            {
    //                Array.Copy(Data, 0, data, dataNowNum, dataAllNum - dataNowNum);
    //                readType = 0;
    //                dataOK = data;
    //                byte[] _data = new byte[Data.Length - (dataAllNum - dataNowNum)];
    //                Array.Copy(Data, dataAllNum - dataNowNum, _data, 0, Data.Length - (dataAllNum - dataNowNum));
    //                return _data;
    //            }
    //        }
    //        return null;

    //    }
    //}
}
