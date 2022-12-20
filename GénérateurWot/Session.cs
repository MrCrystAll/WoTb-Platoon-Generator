using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GénérateurWot.Packets;
using WotGenC;
using ThreadPool = Windows.System.Threading.ThreadPool;

namespace GénérateurWot
{
    public class Session : BaseViewModel
    {
        public ObservableCollection<Player> Players { get; set; }

        private string _status;

        public string Status
        {
            get => _status;
            set => OnPropertyChanged(ref _status, value);
        }

        private bool _isRunning;
        private SimpleClient _client;
        private Task<bool> _listenTask;

        public bool IsRunning
        {
            get => _isRunning;
            set => OnPropertyChanged(ref _isRunning, value);
        }

        public SimpleClient Client
        {
            get => _client;
            set => OnPropertyChanged(ref _client, value);
        }

        private Task _updateTask;
        private Task _connectionTask;

        private DateTime _pingSent, _pingLastSent;
        private bool _pinged;

        public Session()
        {
            Players = new ObservableCollection<Player>();
        }

        public async Task Connect(string address, int port)
        {
            Status = "Connecting...";
            if (SetupClient(address, port))
            {
                var packet = await GetNewConnectionPacket();
                await InitializeConnection(packet);
            }
        }

        private async Task InitializeConnection(PersonalPacket packet)
        {
            _pinged = false;
            if (IsRunning)
            {
                _updateTask = Task.Run(Update);
                await _client.SendObject(packet);
                _connectionTask = Task.Run(MonitorConnection);
                Status = "Connected";
            }
            else
            {
                Status = "Connection failed";
                await Disconnect();
            }
        }

        private async Task<PersonalPacket> GetNewConnectionPacket()
        {
            _listenTask = Task.Run(Client.Connect);
            IsRunning = await _listenTask;

            var notifyServer = new UserConnectionPacket
            {
                IsJoining = true,
                UserGuid = _client.ClientId.ToString()
            };

            return new PersonalPacket
            {
                GuidId = _client.ClientId.ToString(),
                Package = notifyServer
            };
        }

        private bool SetupClient(string address, int port)
        {
            Client = new SimpleClient(address, port);
            return true;
        }

        public async Task Disconnect()
        {
            if (IsRunning)
            {
                IsRunning = false;
                await _connectionTask;
                await _updateTask;
                
                _client.Disconnect();
            }

            Status = "Disconnected";
            //TODO: Quit the session
        }
        
        public async Task Send(){ }

        private async void Update()
        {
            while (IsRunning)
            {
                Thread.Sleep(1);
                var received = await MonitorData();

                if (received)
                {
                    Debug.WriteLine("Received");
                }
            }
        }

        private async Task<bool> MonitorData()
        {
            var newObject = await _client.ReceiveObject();
            
            //TODO: Show changes here
            return false;
        }

        private async Task MonitorConnection()
        {
            _pingSent = DateTime.Now;
            _pingLastSent = DateTime.Now;

            while (IsRunning)
            {
                Thread.Sleep(1);
                var timePassed = _pingSent.TimeOfDay - _pingLastSent.TimeOfDay;
                if (timePassed > TimeSpan.FromSeconds(5))
                {
                    if (_pinged) continue;
                    var result = await _client.PingConnection();
                    _pinged = true;
                        
                    Thread.Sleep(5000);

                    if (_pinged)
                    {
                        Task.Run(Disconnect);
                    }
                }
                else
                {
                    _pingSent = DateTime.Now;
                }
            }
        }
    }
}