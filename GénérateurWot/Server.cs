using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GénérateurWot.Annotations;
using GénérateurWot.Packets;
using SimpleTCP;
using ThreadPool = Windows.System.Threading.ThreadPool;

namespace GénérateurWot
{
    public delegate void PacketEventHandler(object sender, PacketEvents e);
    public delegate void PersonalPacketEventHandler(object sender, PersonalPacketEvents e);
    
    public class SimpleServer
    {
        public IPAddress Address { get; private set; }
        public int Port { get; private set; }

        public IPEndPoint EndPoint { get; private set; }
        public Socket Socket { get; private set; }

        public bool IsRunning { get; private set; }
        public List<SimpleClient> Connections { get; private set; }

        private Task _receivingTask;

        
        public event PacketEventHandler OnConnectionAccepted;
        public event PacketEventHandler OnConnectionRemoved;
        public event PacketEventHandler OnPacketReceived;
        public event PacketEventHandler OnPacketSent;

        public event PersonalPacketEventHandler OnPersonalPacketSent;
        public event PersonalPacketEventHandler OnPersonalPacketReceived;

        public SimpleServer(IPAddress address, int port)
        {
            Address = address;
            Port = port;

            EndPoint = new IPEndPoint(address, port);
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket.ReceiveTimeout = 5000;
            Connections = new List<SimpleClient>();
        }

        public bool Open()
        {
            Socket.Bind(EndPoint);
            Socket.Listen(10);
            return true;
        }

        public async Task<bool> Start()
        {
            _receivingTask = Task.Run(MonitorStreams);
            IsRunning = true;
            await Listen();
            await _receivingTask;
            Socket.Close();
            return true;
        }

        public bool Close()
        {
            IsRunning = false;
            Connections.Clear();
            return true;
        }

        private async Task<bool> Listen()
        {
            while (IsRunning)
            {
                if (!Socket.Poll(100000, SelectMode.SelectRead)) continue;
                
                var newConnection = Socket.Accept();
                if (newConnection != null)
                {
                    var client = new SimpleClient();
                    var newGuid = await client.CreateGuid(newConnection);
                    await client.SendMessage(newGuid);
                    Connections.Add(client);
                    var e = BuildEvent(client, null, string.Empty);
                    OnConnectionAccepted?.Invoke(this, e);
                }
            }

            return true;
        }

        private PacketEvents BuildEvent(SimpleClient sender, SimpleClient receiver, object package)
        {
            return new PacketEvents
            {
                Sender = sender,
                Packet = package,
                Receiver = receiver
            };
        }
        
        private PersonalPacketEvents BuildEvent(SimpleClient sender, SimpleClient receiver, PersonalPacket package)
        {
            return new PersonalPacketEvents
            {
                Sender = sender,
                Packet = package,
                Receiver = receiver
            };
        }

        private void MonitorStreams()
        {
            while (IsRunning)
            {
                foreach (var simpleClient in Connections.ToList())
                {
                    if (!simpleClient.IsConnected)
                    {
                        var e5 = BuildEvent(simpleClient, null, string.Empty);
                        Connections.Remove(simpleClient);
                        OnConnectionRemoved?.Invoke(this, e5);
                        continue;
                    }

                    if (simpleClient.Socket.Available == 0) continue;
                    
                    var readObject = ReadObject(simpleClient.Socket);
                    var e1 = BuildEvent(simpleClient, null, readObject);
                    OnPacketReceived?.Invoke(this, e1);

                    if (readObject is PingPacket ping)
                    {
                        simpleClient.SendObject(ping).Wait();
                        continue;
                    }

                    if (readObject is PersonalPacket pp)
                    {
                        var destination = Connections.FirstOrDefault(c => c.ClientId.ToString() == pp.GuidId);
                        var e4 = BuildEvent(simpleClient, destination, pp);
                        OnPersonalPacketReceived?.Invoke(this, e4);

                        if (destination != null)
                        {
                            destination.SendObject(pp).Wait();
                            var e2 = BuildEvent(simpleClient, destination, pp);
                            OnPersonalPacketSent?.Invoke(this, e2);
                        }
                    }
                    else
                    {
                        foreach (var client in Connections.ToList())
                        {
                            client.SendObject(readObject).Wait();
                            var e3 = BuildEvent(simpleClient, client, readObject);
                            OnPacketSent?.Invoke(this,e3);
                        }
                    }
                }
            }
        }

        public void SendObjectToClients(object package)
        {
            foreach (var simpleClient in Connections.ToList())
            {
                simpleClient.SendObject(package).Wait();
                var e3 = BuildEvent(simpleClient, simpleClient, package);
                OnPacketSent?.Invoke(this, e3);
            }
        }

        private object ReadObject(Socket simpleClientSocket)
        {
            byte[] data = new byte[simpleClientSocket.ReceiveBufferSize];

            using Stream s = new NetworkStream(simpleClientSocket);
            s.Read(data, 0, data.Length);
            var memory = new MemoryStream(data);
            memory.Position = 0;

            var formatter = new BinaryFormatter();
            return formatter.Deserialize(memory);
        }
    }

    public class AppServer : BaseViewModel
    {
        private string _externalAddress;
        public string ExternalAddress
        {
            get { return _externalAddress; }
            set { OnPropertyChanged(ref _externalAddress, value); }
        }

        private string _port = "8000";
        public string Port
        {
            get { return _port; }
            set { OnPropertyChanged(ref _port, value); }
        }

        private string _status = "Idle";
        public string Status
        {
            get { return _status; }
            set { OnPropertyChanged(ref _status, value); }
        }

        private int _clientsConnected;
        public int ClientsConnected
        {
            get { return _clientsConnected; }
            set { OnPropertyChanged(ref _clientsConnected, value); }
        }

        public ObservableCollection<string> Outputs { get; set; }
        public ObservableCollection<string> Usernames = new ObservableCollection<string>();

        public ICommand RunCommand { get; set; }
        public ICommand StopCommand { get; set; }

        private SimpleServer _server;
        private bool _isRunning;

        private Task _updateTask;
        private Task _listenTask;
        
        public AppServer()
        {
            Outputs = new ObservableCollection<string>();

            RunCommand = new AsyncCommand(Run);
            StopCommand = new AsyncCommand(Stop);
        }

        private async Task Run()
        {
            Status = "Connection...";
            await SetupServer();
            _server.Open();
            _listenTask = Task.Run(_server.Start);
            _updateTask = Task.Run(Update);

            _isRunning = true;
        }

        private async Task Stop()
        {
            ExternalAddress = string.Empty;
            _isRunning = false;
            ClientsConnected = 0;
            _server.Close();

            await _listenTask;
            await _updateTask;
            Status = "Stopped";
        }

        private void Update()
        {
            while (_isRunning)
            {
                Thread.Sleep(5);
                if (!_server.IsRunning)
                {
                    Task.Run(Stop);
                    return;
                }

                ClientsConnected = _server.Connections.Count;
                Status = "Running";
            }
        }

        private async Task SetupServer()
        {
            Status = "Validating socket...";
            
            var isValidPort = int.TryParse(Port, out int socketPort);

            if (!isValidPort)
            {
                DisplayError("Port number not valid");
                return;
            }

            Status = "Obtaining IP...";
            await Task.Run(GetExternalIp);
            Status = "Setting up server...";
            _server = new SimpleServer(IPAddress.Any, socketPort);

            Status = "Setting up events...";
            _server.OnConnectionAccepted += ServerConnectionAccepted;
            _server.OnConnectionRemoved += ServerConnectionRemoved;
            _server.OnPacketReceived += ServerPacketReceived;
            _server.OnPacketSent += ServerPacketSent;
            _server.OnPersonalPacketSent += ServerPersonalPacketSent;
            _server.OnPersonalPacketReceived += ServerPersonalPacketReceived;
        }

        private void ServerPersonalPacketReceived(object sender, PersonalPacketEvents e)
        {
            if (e.Packet.Package is UserConnectionPacket ucp)
            {
                var notification = "Yeeeeeeeeeeeeeeeeeeeeeeeeeeee";
                if (Usernames.Contains(ucp.UserGuid))
                    Usernames.Remove(ucp.UserGuid);
                else
                    Usernames.Add(ucp.UserGuid);

                ucp.Users = Usernames.ToArray();
                Task.Run(() => _server.SendObjectToClients(ucp)).Wait();
                Thread.Sleep(500);
                Task.Run(() => _server.SendObjectToClients(notification)).Wait();
            }
            Debug.WriteLine("Gottem");
        }

        private void ServerPersonalPacketSent(object sender, PersonalPacketEvents e)
        {
        }

        private void ServerPacketSent(object sender, PacketEvents e)
        {
        }

        private void ServerPacketReceived(object sender, PacketEvents e)
        {
        }

        private void ServerConnectionRemoved(object sender, PacketEvents e)
        {
            if (!Usernames.Contains(e.Sender.ClientId.ToString())) return;
            
           
            var notification = "Byyyyyyyyyyyeee";
            var userPacket = new UserConnectionPacket
            {
                UserGuid = e.Sender.ClientId.ToString(),
                IsJoining = false
            };

            if (Usernames.Contains(userPacket.UserGuid))
                Usernames.Remove(userPacket.UserGuid);

            userPacket.Users = Usernames.ToArray();

            if (_server.Connections.Count != 0)
            {
                Task.Run(() => _server.SendObjectToClients(userPacket)).Wait();
                Task.Run(() => _server.SendObjectToClients(notification)).Wait();
            }
            
            Debug.WriteLine("Client disconnected : " + e.Sender.Socket.RemoteEndPoint);
        }

        private void ServerConnectionAccepted(object sender, PacketEvents e)
        {
            Debug.WriteLine("Client connected : " + e.Sender.Socket.RemoteEndPoint);
        }

        private void GetExternalIp()
        {
            try
            {
                string externalIP;
                externalIP = (new WebClient()).DownloadString("http://checkip.dyndns.org/");
                externalIP = (new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")).Matches(externalIP)[0].ToString();
                ExternalAddress = externalIP;
            }
            catch
            {
                ExternalAddress = "Error receiving IP address";
            }
        }

        private void DisplayError(string portValueIsNotValid)
        {
            Debug.WriteLine(portValueIsNotValid);
        }
    }
}