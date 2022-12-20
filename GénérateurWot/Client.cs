using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using GénérateurWot.Annotations;
using GénérateurWot.Packets;
using SimpleTCP;
using WotGenC;

namespace GénérateurWot
{

    public class SimpleClient
    {
        public Guid ClientId { get; private set; }
        public Socket Socket { get; private set; }
        public IPEndPoint EndPoint { get; private set; }
        public IPAddress Address { get; private set; }
        public bool IsConnected { get; private set; }
        public bool IsGuidAssigned { get; set; }

        public int ReceiveBufferSize
        {
            get { return Socket.ReceiveBufferSize; }
            set { Socket.ReceiveBufferSize = value; }
        }

        public int SendBufferSize
        {
            get { return Socket.SendBufferSize; }
            set { Socket.SendBufferSize = value; }
        }

        public SimpleClient(string address, int port)
        {
            IPAddress ipAddress;
            var validIp = IPAddress.TryParse(address, out ipAddress);

            if (!validIp)
            {
                ipAddress = Dns.GetHostAddresses(address)[0];
            }

            Address = ipAddress;
            EndPoint = new IPEndPoint(ipAddress, port);
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ReceiveBufferSize = 8000;
            SendBufferSize = 8000;
        }
        
        public SimpleClient(){}

        public async Task<bool> Connect()
        {
            var result = await Task.Run(TryConnect);
            string guid = string.Empty;

            try
            {
                if (result)
                {
                    guid = ReceiveGuid();
                    ClientId = Guid.Parse(guid);
                    IsGuidAssigned = true;
                    return true;
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("Connect : " + e.Message);
            }

            return false;
        }

        private string ReceiveGuid()
        {
            try
            {
                using Stream s = new NetworkStream(Socket);
                var reader = new StreamReader(s);
                s.ReadTimeout = 5000;

                return reader.ReadLine();
            }
            catch (IOException e)
            {
                Console.WriteLine("ReceiveGuid : " + e.Message);
                return null;
            }
        }

        private bool TryConnect()
        {
            try
            {
                Socket.Connect(EndPoint);
                return true;
            }
            catch
            {
                Console.WriteLine("Connection failed");
                return false;
            }
        }

        public async Task<string> CreateGuid(Socket newConnection)
        {
            return await Task.Run(() => TryCreateGuid(newConnection));
        }

        private string TryCreateGuid(Socket newConnection)
        {
            Socket = newConnection;
            var endPoint = (IPEndPoint)Socket.LocalEndPoint;
            EndPoint = endPoint;
            
            ClientId = Guid.NewGuid();
            return ClientId.ToString();
        }

        public async Task<bool> SendMessage(string message)
        {
            return await Task.Run(() => TrySendMessage(message));
        }

        private bool TrySendMessage(string message)
        {
            try
            {
                using Stream s = new NetworkStream(Socket);
                StreamWriter writer = new StreamWriter(s);
                writer.AutoFlush = true;
                writer.WriteLine(message);
                return true;
            }
            catch (IOException e)
            {
                Console.WriteLine("TrySendMessage : " + e.Message);
                return false;
            }
        }

        public async Task<bool> SendObject(object obj)
        {
            return await Task.Run(() => TrySendObject(obj));
        }

        private bool TrySendObject(object obj)
        {
            try
            {
                using Stream s = new NetworkStream(Socket);
                var memory = new MemoryStream();
                var formatter = new BinaryFormatter();
                formatter.Serialize(memory, obj);
                var newObj = memory.ToArray();

                s.Write(newObj, 0, newObj.Length);
                return true;
            }
            catch (IOException e)
            {
                Console.WriteLine("TrySendObject : " + e.Message);
                return false;
            }
        }

        public async Task<object> ReceiveObject()
        {
            return await Task.Run(() => TryReceiveObject());
        }

        private object TryReceiveObject()
        {
            if (Socket.Available == 0) return null;

            try
            {
                byte[] data = new byte[Socket.ReceiveBufferSize];
                using Stream s = new NetworkStream(Socket);
                s.Read(data, 0, data.Length);
                var memory = new MemoryStream();
                memory.Position = 0;

                var formatter = new BinaryFormatter();
                return formatter.Deserialize(memory);
            }
            catch (Exception e)
            {
                Console.WriteLine("TryReceiveObjects : " + e.Message);
                return null;
            }
        }

        public bool IsSocketConnected()
        {
            try
            {
                bool part1 = Socket.Poll(5000, SelectMode.SelectRead);
                bool part2 = Socket.Available == 0;
                return part1 && part2;
            }
            catch(ObjectDisposedException e)
            {
                Console.WriteLine("IsSocketConnected " + e.Message);
                return false;
            }
        }

        public async Task<bool> PingConnection()
        {
            try
            {
                return await SendObject(new PingPacket());
            }
            catch (ObjectDisposedException e)
            {
                Console.WriteLine("PingConnection " + e.Message);
                return false;
            }
        }

        public void Disconnect()
        {
            Socket.Close();
        }
    }
    
    public class Client : BaseViewModel
    {
        private string _username;
        public string Username
        {
            get { return _username; }
            set { OnPropertyChanged(ref _username, value); }
        }

        private string _address;
        public string Address
        {
            get { return _address; }
            set { OnPropertyChanged(ref _address, value); }
        }

        private string _port = "8000";
        public string Port
        {
            get { return _port; }
            set { OnPropertyChanged(ref _port, value); }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set { OnPropertyChanged(ref _message, value); }
        }

        private string _colorCode;
        public string ColorCode
        {
            get { return _colorCode; }
            set { OnPropertyChanged(ref _colorCode, value); }
        }

        public Session Session { get; set; }

        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }
        public ICommand SendCommand { get; set; }

        public Client()
        {
            ConnectCommand = new AsyncCommand(Connect, CanConnect);
            DisconnectCommand = new AsyncCommand(Disconnect, CanDisconnect);
            SendCommand = new AsyncCommand(Send, CanSend);
        }

        private bool CanSend() => true;

        private async Task Send()
        {
            if(Session is null)
                Debug.WriteLine("Ah yes, send something to null");

            await Session.Send();
        }

        private bool CanDisconnect() => Session.IsRunning;

        private async Task Disconnect()
        {
            if(Session is null)
                Debug.WriteLine("So you plan to disconnect from what ?");

            await Session.Disconnect();
        }

        private bool CanConnect() => !Session.IsRunning;

        private async Task Connect()
        {
            bool isValidPort = int.TryParse(Port, out int socketPort);
            if (!isValidPort)
            {
                Debug.WriteLine("Wrong socket port number");
                return;
            }

            if (string.IsNullOrWhiteSpace(Address))
            {
                Debug.WriteLine("Wrong address format");
                return;
            }

            if (string.IsNullOrWhiteSpace(Username))
            {
                Debug.WriteLine("Username can't be empty");
            }

            await Task.Run(() => Session.Connect(Address, socketPort));
        }
    }
}