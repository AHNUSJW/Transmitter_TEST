using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;

//Lumi 20240111

//网络通讯
namespace Model
{
    public class TCPServer
    {
        #region 定义变量

        private bool isOpen;                                                            //是否正在侦听
        private static CancellationTokenSource cts = new CancellationTokenSource();     //用于关闭侦听
        private ConcurrentQueue<TCPMessage> rxBuff = new ConcurrentQueue<TCPMessage>(); //接收缓冲区

        private Socket serverSocket;                                        //服务端socket
        //连接过的客户端列表 IPAddress:客户端IP地址 Socket:客户端的socket
        //一个ip地址对应一个客户端
        private static ConcurrentDictionary<IPAddress, Client> myClients = new ConcurrentDictionary<IPAddress, Client>();

        public int MessagesToRead { get { return rxBuff.Count; } }          //获取接收缓冲区中数据的消息数
        public bool IsOpen { get => isOpen && (!cts.IsCancellationRequested); }
        public static Dictionary<IPAddress, Client> MyClients
        {
            get
            {
                return myClients?.ToDictionary(x => x.Key, x => x.Value);
            }
        }
        public Socket ServerSocket { get => serverSocket; set => serverSocket = value; }

        #endregion

        //打开侦听
        public bool Listen(string ip, int port)
        {
            if (isOpen && (!cts.IsCancellationRequested))
            {
                return true;
            }
            else
            {
                try
                {
                    serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);   //ipv4  TCP
                    serverSocket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));                                 //绑定ip地址和端口
                    serverSocket.Listen(20);
                    serverSocket.IOControl(IOControlCode.KeepAliveValues, GetKeepAliveConfig(1, 1000, 100), null);//配置keep alive，监测网络异常断线
                    isOpen = true;
                    cts = new CancellationTokenSource();                                                          //重置cts.IsCancellationRequested
                    Task.Factory.StartNew(() =>
                    {
                        serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), serverSocket);                //连接回调
                    }, cts.Token);

                    return true;
                }
                catch
                {
                    //绑定的端口号可能和电脑已有的有冲突
                }

                return false;
            }
        }

        //停止侦听
        public void Stop()
        {
            try
            {
                if (!isOpen) return;
                cts.Cancel();
                isOpen = false;

                foreach (var client in myClients.Values)
                {
                    if (client.IsConnected)
                    {
                        client.ClientSocket?.Shutdown(SocketShutdown.Both);
                        client.ClientSocket?.Close();
                    }
                }
                myClients.Clear();
            }
            catch
            {
            }
            finally
            {
                serverSocket?.Close();
                serverSocket = null;

                cts.Dispose();  //防内存泄漏
            }
        }

        //清除串口任务
        public void DiscardBuffer()
        {
            try
            {
                if (isOpen && (!cts.IsCancellationRequested))
                {
                    List<Socket> readList = new List<Socket>
                    {
                        serverSocket
                    };
                    Socket.Select(readList, null, null, 0); //检测可读状态
                    if (readList.Count > 0) //如果有可读的 socket
                    {
                        int bytesToRead = serverSocket.Available;
                        byte[] buffer = new byte[bytesToRead];
                        serverSocket.Receive(buffer, bytesToRead, SocketFlags.None); //读取并清空接收缓冲区
                    }
                }

                while (!rxBuff.IsEmpty)  //清空缓冲区
                {
                    rxBuff.TryDequeue(out _);
                }
            }
            catch
            {
            }
        }

        //从接收缓冲区读一条消息
        public TCPMessage ReadMessage()
        {
            if (rxBuff.TryDequeue(out TCPMessage message))
            {
                return message;
            }
            else
            {
                return new TCPMessage(null);
            }
        }

        //发送
        public int Send(TCPMessage msgToSend)
        {
            //发送时，TCPMessage的workSocket必须赋值为目标客户端的socket
            try
            {
                if (msgToSend.MsgSocket != null)
                {
                    IPEndPoint ep = (IPEndPoint)msgToSend.MsgSocket.RemoteEndPoint;
                    IPAddress ip = ep.Address;
                    if (myClients.ContainsKey(ip))    //当前客户端已连接
                    {
                        byte[] buffer = msgToSend.Data;
                        int size = msgToSend.DataLen;
                        SocketFlags socketFlags = 0;
                        return myClients[ip].ClientSocket.Send(buffer, 0, size, socketFlags);
                    }
                    else
                    {
                        return -1;
                    }
                }
                else
                {
                    return -1;
                }
            }
            catch
            {
            }
            return -1;
        }

        //连接回调，用于处理客户端连接请求
        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                if ((!isOpen) || cts.IsCancellationRequested) return;
                Socket listener = (Socket)ar.AsyncState;  //获取异步操作的原始Socket
                if (listener == null) return;

                Socket handler = listener.EndAccept(ar);  //获取新连接的socket
                if (handler == null || !handler.Connected) return;

                IPEndPoint ep = (IPEndPoint)handler.RemoteEndPoint; //获取客户端的IP地址和端口号
                IPAddress ip = ep.Address;
                Client newClient = new Client(handler);
                if (myClients.TryGetValue(ip, out Client existingClient))
                {
                    existingClient.ClientSocket.Close();
                    myClients[ip] = newClient;
                }
                else
                {
                    myClients.TryAdd(ip, newClient);
                }

                RefreshClients();                         //更新当前连接的clients事件

                StateObject state = new StateObject();    //创建StateObject容器，用于保存与客户端通信的状态信息
                state.workSocket = handler;
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(RevCallback), state);
                listener.BeginAccept(new AsyncCallback(AcceptCallback), serverSocket); //获取新连接的客户端socket
            }
            catch
            {
            }
        }

        //接收回调
        public void RevCallback(IAsyncResult ar)
        {
            if ((!isOpen) || cts.IsCancellationRequested) return;

            // 获取客户请求的socket
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            var remoteEndPoint = handler.RemoteEndPoint;

            if (handler == null || !handler.Connected)
            {
                closeConnection(handler);              //关闭连接
            }
            else
            {
                try
                {
                    int len = handler.EndReceive(ar);   //完成一次连接, 数据存储在state.buffer, len为读取的长度
                    if (len > 0)
                    {
                        TCPMessage message = new TCPMessage(handler)
                        {
                            Data = state.buffer.Take(len).ToArray(),        //获取数据
                            DataLen = len
                        };
                        rxBuff.Enqueue(message);                            //存入缓存

                        ReceiveData();                                      //接收事件

                        // 发送数据byteData，回调函数SendCallback。容器handler
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(RevCallback), state);
                    }
                    else
                    {
                        //断线
                        updateClientState(remoteEndPoint, false);            //更新客户端连接状态
                    }
                }
                catch (Exception ex) when (ex is ObjectDisposedException || ex is SocketException)
                {
                    //断线
                    closeConnection(handler);          //关闭连接
                }
            }
        }

        //断开异常连接
        private void closeConnection(Socket _socket)
        {
            if (_socket == null || !_socket.Connected)
            {
                return;
            }

            var remoteEndPoint = _socket.RemoteEndPoint;

            //关闭连接
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();

            //更新客户端连接状态
            updateClientState(remoteEndPoint, false);
        }

        //更新客户端连接状态
        private void updateClientState(EndPoint endPoint, bool state)
        {
            IPAddress ip = ((IPEndPoint)endPoint).Address;
            if (myClients.ContainsKey(ip))
            {
                myClients[ip].IsConnected = state;
                RefreshClients(); //更新当前连接的clients事件
            }
        }

        //依据ip地址返回对应的客户端socket
        public static Socket FindSocketByIP(string ipAddr)
        {
            KeyValuePair<IPAddress, Client> foundItem = myClients.FirstOrDefault(kv => kv.Key.ToString().Contains(ipAddr));

            if (foundItem.Key != null && foundItem.Value != null)
            {
                return foundItem.Value.ClientSocket;
            }
            else
            {
                return null;
            }
        }

        //依据ip地址返回对应的客户端端口号
        public static int FindPortByIP(string ipAddr)
        {
            KeyValuePair<IPAddress, Client> foundItem = myClients.FirstOrDefault(kv => kv.Key.ToString().Contains(ipAddr));

            if (foundItem.Key != null && foundItem.Value != null)
            {
                return foundItem.Value.ClientPort;
            }
            else
            {
                return -1;
            }
        }

        //依据ip地址返回对应的客户端链接状态
        public static bool FindClientStateByIP(string ipAddr)
        {
            KeyValuePair<IPAddress, Client> foundItem = myClients.FirstOrDefault(kv => kv.Key.ToString().Contains(ipAddr));

            if (foundItem.Key != null && foundItem.Value != null)
            {
                return foundItem.Value.IsConnected;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 获取KeepAlive的控制参数
        /// </summary>
        /// <param name="onOff">是否开启KeepAlive</param>
        /// <param name="keepAliveTime">当开启KeepAlive后，经过多长时间开始侦测，单位ms</param>
        /// <param name="keepAliveInterval">侦测间隔时间，单位ms</param>
        /// <returns></returns>
        private static byte[] GetKeepAliveConfig(int onOff, int keepAliveTime, int keepAliveInterval)
        {
            byte[] buffer = new byte[12];
            BitConverter.GetBytes(onOff).CopyTo(buffer, 0);
            BitConverter.GetBytes(keepAliveTime).CopyTo(buffer, 4);
            BitConverter.GetBytes(keepAliveInterval).CopyTo(buffer, 8);
            return buffer;
        }

        #region 接收事件

        private EventHandler<NetDataReceiveEventArgs> dataReceivedHandler; //委托字段

        public event EventHandler<NetDataReceiveEventArgs> DataReceived    //接收event
        {
            add
            {
                if (dataReceivedHandler == null)
                {
                    dataReceivedHandler = value;
                }
                else
                {
                    //DataReceived只能订阅一个事件
                    //throw new InvalidOperationException("DataReceived event 只能订阅一个事件.");
                }
            }
            remove
            {
                dataReceivedHandler = null;
            }
        }

        //接收数据
        private void ReceiveData()
        {
            //接收到数据时触发
            OnDataReceived(new NetDataReceiveEventArgs());
        }

        //封装接收事件
        protected virtual void OnDataReceived(NetDataReceiveEventArgs e)
        {
            dataReceivedHandler?.Invoke(this, e);
        }

        #endregion

        #region clients更新事件

        public static event EventHandler<NetClientsRefreshEventArgs> ClientsRefreshed;    //接收event

        //接收事件
        public void RefreshClients()
        {
            //连接的client的连接状态发生改变时触发
            ClientsRefresh(new NetClientsRefreshEventArgs());
        }

        //封装接收事件
        protected virtual void ClientsRefresh(NetClientsRefreshEventArgs e)
        {
            ClientsRefreshed?.Invoke(this, e);
        }

        #endregion

        //用于存储客户端的socket和缓存
        private class StateObject
        {
            //容器的结构类型为：Code
            //容器至少为一个socket类型。
            //客户端socket
            public Socket workSocket = null;
            //buffer大小
            public const int BufferSize = 1024;
            //接收缓存
            public byte[] buffer = new byte[BufferSize];
            //接收字符串
            public StringBuilder sb = new StringBuilder();
        }
    }

    //TCP消息
    //存储数据到缓存时，需要记录IP地址和socket
    //否则多设备且多个设备ID(e_addr)相同时
    //会难以区分接收到的是哪个设备的数据
    public class TCPMessage
    {
        //buffer大小
        private const int BufferSize = 1024;
        //socket
        private Socket msgSocket = null;
        //Endpoint
        private EndPoint msgEndPoint = null;
        //数据长度
        private int dataLen = 0;
        //数据
        private byte[] data = new byte[BufferSize];
        //客户端ip
        private IPAddress msgIP;
        //客户端port
        private int msgPort;
        //消息时间
        private DateTime msgTime;

        public Socket MsgSocket { get => msgSocket; set => msgSocket = value; }
        public EndPoint MsgEndPoint { get => msgEndPoint; set => msgEndPoint = value; }
        public int DataLen { get => dataLen; set => dataLen = value; }
        public byte[] Data { get => data; set => data = value; }
        public IPAddress MsgIP { get => msgIP; set => msgIP = value; }
        public int MsgPort { get => msgPort; set => msgPort = value; }
        public DateTime MsgTime { get => msgTime; set => msgTime = value; }

        public TCPMessage(Socket workSocket)
        {
            this.msgSocket = workSocket;
            this.msgEndPoint = workSocket?.RemoteEndPoint;
            if (msgEndPoint != null && workSocket.Connected)
            {
                this.msgIP = ((IPEndPoint)msgEndPoint).Address;
                this.MsgPort = ((IPEndPoint)msgEndPoint).Port;
            }
            this.MsgTime = DateTime.Now;
        }
    }

    //Modbus-TCP客户端
    public class Client
    {
        private Socket clientSocket;    //客户端socket
        private EndPoint clientEndPoint;//客户端ip和端口号
        private IPAddress clientIP;     //客户端ip
        private int clientPort;         //客户端port
        private DateTime connectedTime; //tcp连接时间
        private bool isConnected;       //是否已连接

        public Socket ClientSocket { get => clientSocket; }
        public DateTime ConnectedTime { get => connectedTime; }
        public EndPoint ClientEndPoint { get => clientEndPoint; }
        public IPAddress ClientIP { get => clientIP; }
        public int ClientPort { get => clientPort; }
        public bool IsConnected { get => isConnected; set => isConnected = value; }

        public Client(Socket clientSocket)
        {
            this.clientSocket = clientSocket;
            this.clientEndPoint = clientSocket?.RemoteEndPoint;
            if (clientEndPoint != null)
            {
                this.clientIP = ((IPEndPoint)clientEndPoint).Address;
                this.clientPort = ((IPEndPoint)clientEndPoint).Port;
            }
            this.connectedTime = DateTime.Now;
            this.IsConnected = true;
        }
    }

    public class NetDataReceiveEventArgs : EventArgs { }     //接收事件
    public class NetClientsRefreshEventArgs : EventArgs { }  //客户端连接数量改变事件
}
