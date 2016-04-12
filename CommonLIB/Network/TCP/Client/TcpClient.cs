using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Runtime.InteropServices;

namespace CommonLIB.Network.TCP.Client
{
    public class TcpClient
    {
        Socket socket;
        string IP;
        int Port;
        SocketAsyncEventArgs connectEvent;
        SocketAsyncEventArgs readEvent;
        SocketAsyncEventArgs writeEvent;
        SocketTcpClientEvent socketTcpClientEvent;
        ReadDataModel readDataModel;
        List<byte[]> writeDataBuf=new List<byte[]>();
        public int isConnect;
        bool isSend;
        static byte[] headByte = new byte[] { 0xaa, 0xbb, 0xcc, 0xdd, 0x00, 0x00, 0x00, 0x00 };
        public TcpClient(SocketTcpClientEvent socketTcpClientEvent)
        {
            this.socketTcpClientEvent = socketTcpClientEvent;
           
            connectEvent = new SocketAsyncEventArgs();
            readEvent = new SocketAsyncEventArgs();
            writeEvent = new SocketAsyncEventArgs();
            connectEvent.Completed += new EventHandler<SocketAsyncEventArgs>(connectEvent_Completed);
            readEvent.Completed += new EventHandler<SocketAsyncEventArgs>(readEvent_Completed);
            readEvent.SetBuffer(new byte[4096], 0, 4096);
            writeEvent.Completed += new EventHandler<SocketAsyncEventArgs>(writeEvent_Completed);
        }
        void CreateSocket()
        {
            if (socket == null)
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.SendBufferSize = 64 * 1024;
                socket.ReceiveBufferSize = 64 * 1024;
                socket.NoDelay = true;
                uint dummy = 0;
                byte[] inOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
                BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);
                BitConverter.GetBytes((uint)1000).CopyTo(inOptionValues, Marshal.SizeOf(dummy));
                BitConverter.GetBytes((uint)1000).CopyTo(inOptionValues, Marshal.SizeOf(dummy) * 2);
                socket.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
            }
        }
        void writeEvent_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                lock (writeDataBuf)
                {
                    if (writeDataBuf.Count > 0)
                    {
                        byte[] _data = writeDataBuf[0];
                        writeDataBuf.RemoveAt(0);
                        e.SetBuffer(_data, 0, _data.Length);
                        bool isOK=socket.SendAsync(e);
                        if (!isOK)
                        {
                            writeEvent_Completed(null, e);
                        }
                    }
                    else
                    {
                        isSend = false;
                    }
                }
            }
        }

        void readEvent_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success && e.BytesTransferred != 0)
            {
                byte[] _readData = new byte[e.BytesTransferred];
                Array.Copy(e.Buffer, 0, _readData, 0, e.BytesTransferred);
                while (true)
                {
                    byte[] _data = readDataModel.Process(_readData);
                    if (readDataModel.readType == 0 && readDataModel.dataOK != null)
                    {
                        ThreadPool.QueueUserWorkItem(ReadOK, new object[] { this, readDataModel.dataOK });
                    }
                    if (_data != null)
                    {
                        _readData = _data;
                    }
                    else
                    {
                        break;
                    }
                }
                bool isOK=socket.ReceiveAsync(e);
                if (!isOK)
                {
                    readEvent_Completed(null, e);
                }
            }
            else
            {
                isConnect = 0;
                DisposeSocket();
                ThreadPool.QueueUserWorkItem(Error, new object[] { this });
            }
        }

        void connectEvent_Completed(object sender, SocketAsyncEventArgs e)
        {

            if (e.SocketError == SocketError.Success)
            {
                isConnect = 2;
                isSend = false;
                readDataModel = new ReadDataModel();
                lock (writeDataBuf)
                {
                    writeDataBuf = new List<byte[]>();
                }
                ThreadPool.QueueUserWorkItem(ConnectOK, new object[] { this });
                bool isOK = socket.ReceiveAsync(readEvent);
                if (!isOK)
                {
                    readEvent_Completed(null, readEvent);
                }
                return;
            }
            isConnect = 0;
            //ThreadPool.QueueUserWorkItem(Error, new object[] { this });
        }

        object closeLock = new object();
        public void DisposeSocket()
        {
            lock (closeLock)
            {
                writeEvent.Dispose();
                readEvent.Dispose();
                if (socket != null)
                {
                    socket.Close();
                    socket.Dispose();
                    socket = null;
                }
            }
        }
        public void Connect(string IP, int Port)
        {
            if (isConnect == 0 || socket == null || (isConnect == 2 && socket != null && socket.Connected == false))
            {
                this.IP = IP;
                this.Port = Port;
                isConnect = 1;
                CreateSocket();
                connectEvent.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), Port);
                bool isOK=socket.ConnectAsync(connectEvent);
                if (!isOK)
                {
                    connectEvent_Completed(null, connectEvent);
                }
            }
        }
        public void Connect()
        {
            if (isConnect == 0 || socket == null || (isConnect==2 && socket!=null && socket.Connected == false))
            {
                isConnect = 1;
                CreateSocket();
                connectEvent.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), Port);
                bool isOK = socket.ConnectAsync(connectEvent);
                if (!isOK)
                {
                    connectEvent_Completed(null, connectEvent);
                }
            }
        }
        public void SendData(byte[] Data)
        {
            Data = AddHead(Data);
            if (isConnect != 2)
            {
                return;
            }
            lock (writeEvent)
            {
                lock (writeDataBuf)
                {
                    byte[] _sendData = new byte[Data.Length + 8];
                    Array.Copy(headByte, 0, _sendData, 0, 8);
                    Array.Copy(BitConverter.GetBytes(Data.Length), 0, _sendData, 4, 4);
                    Array.Copy(Data, 0, _sendData, 8, Data.Length);
                    writeDataBuf.Add(_sendData);
                    if (!isSend)
                    {
                        isSend = true;
                        writeDataBuf.RemoveAt(0);
                        writeEvent.SetBuffer(_sendData, 0, _sendData.Length);
                        bool isOK=socket.SendAsync(writeEvent);
                        if (!isOK)
                        {
                            writeEvent_Completed(null, writeEvent);
                        }
                    }
                }
            }
        }
        byte[] AddHead(byte[] Data)
        {
            var _temp = new byte[Data.Length + 1];
            _temp[0] = 0;
            Array.Copy(Data, 0, _temp, 1, Data.Length);
            return _temp;
        }
        byte[] RemoveHead(byte[] Data)
        {
            var _temp = new byte[Data.Length - 1];
            Array.Copy(Data, 1, _temp, 0, _temp.Length);
            return _temp;
        }
        void ReadOK(object o)
        {
            TcpClient _socket = (TcpClient)((object[])o)[0];
            byte[] _data = (byte[])((object[])o)[1];
            socketTcpClientEvent(_socket, SocketTcpClient.接收成功, RemoveHead(_data));
        }
        void Error(object o)
        {
            TcpClient _socket = (TcpClient)((object[])o)[0];
            socketTcpClientEvent(_socket, SocketTcpClient.断开连接, null);
        }
        void ConnectOK(object o)
        {
            TcpClient _socket = (TcpClient)((object[])o)[0];
            socketTcpClientEvent(_socket, SocketTcpClient.连接成功, null);
        }
        class ReadDataModel
        {
            public int readType = 0;//0.无 1.头不完整 2.包不完整 
            int headNum = 0;
            int dataAllNum = 0;
            int dataNowNum = 0;
            byte[] data;
            public byte[] dataOK;
            public byte[] Process(byte[] Data)
            {
                if (readType == 0 || readType == 1)
                {
                    if (readType == 0)
                    {
                        data = new byte[8];
                        headNum = 0;
                    }
                    if (Data.Length < 8 - headNum)
                    {
                        readType = 1;
                        Array.Copy(Data, 0, data, headNum, Data.Length);
                        headNum += Data.Length;
                        return null;
                    }
                    if (Data.Length == 8 - headNum)
                    {
                        readType = 2;
                        Array.Copy(Data, 0, data, headNum, Data.Length);
                        return null;
                    }
                    if (Data.Length > 8 - headNum)
                    {
                        byte[] _data = new byte[Data.Length - (8 - headNum)];
                        readType = 2;
                        Array.Copy(Data, 0, data, headNum, 8 - headNum);
                        Array.Copy(Data, 8, _data, 0, Data.Length - (8 - headNum));
                        return _data;
                    }
                }
                else if (readType == 2)
                {
                    if (data.Length == 8)
                    {
                        if (data[0] == 0xaa && data[1] == 0xbb && data[2] == 0xcc && data[3] == 0xdd)
                        {
                            dataAllNum = BitConverter.ToInt32(data, 4);
                            if (dataAllNum > 10240000)
                            {
                                readType = 0;
                                headNum = 0;
                                dataAllNum = 0;
                                dataNowNum = 0;
                                dataOK = null;
                                return null;
                            }
                            data = new byte[dataAllNum];
                            dataNowNum = 0;

                        }
                        else
                        {
                            readType = 0;
                            headNum = 0;
                            dataAllNum = 0;
                            dataNowNum = 0;
                            dataOK = null;
                            return null;
                        }

                    }
                    if (dataAllNum - dataNowNum == Data.Length)
                    {
                        Array.Copy(Data, 0, data, dataNowNum, Data.Length);
                        readType = 0;
                        dataOK = data;
                        return null;
                    }
                    if (dataAllNum - dataNowNum > Data.Length)
                    {
                        Array.Copy(Data, 0, data, dataNowNum, Data.Length);
                        dataNowNum += Data.Length;
                        return null;
                    }
                    if (dataAllNum - dataNowNum < Data.Length)
                    {
                        Array.Copy(Data, 0, data, dataNowNum, dataAllNum - dataNowNum);
                        readType = 0;
                        dataOK = data;
                        byte[] _data = new byte[Data.Length - (dataAllNum - dataNowNum)];
                        Array.Copy(Data, dataAllNum - dataNowNum, _data, 0, Data.Length - (dataAllNum - dataNowNum));
                        return _data;
                    }
                }
                return null;

            }
        }
    }
}
