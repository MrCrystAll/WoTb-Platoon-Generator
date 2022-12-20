using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using WotGenC.Challenges;
using WotGenC.Missions;

namespace WotGenC
{
    public class Player : INotifyPropertyChanged
    {
        public string Token { get; set; }
        public string Pseudo { get; set; }
        
        public string Id { get; set; }
        public AllTimers Timers { get; set; } = new AllTimers();
        public ListOfTanks Tanks { get; set; } = new ListOfTanks();

        private int _cpt;
        private int _cptSupp;

        private Tank _current;

        public Dictionary<Tank, Stats> Stats = new Dictionary<Tank, Stats>();

        public Tank Current
        {
            get => _current;
            set
            {
                _current = value;
                _cpt = _penalite is null ? 0 : _cpt + 1;
                _cptSupp = _penaliteSupp is null ? 0 : _cptSupp + 1;
                if (_cpt == 2)
                {
                    Penalite = null;
                    _cpt = 0;
                }

                if (_cptSupp == 2)
                {
                    PenaliteSupp = null;
                    _cptSupp = 0;
                }
                OnPropertyChanged(nameof(Current));
            }
        }

        private Mission _mission = new NoMission();
        private Challenge _challenge;
        private Challenge _penalite;
        private Challenge _penaliteSupp;

        public Challenge Penalite
        {
            get => _penalite;
            set
            {
                _penalite = value;
                _cpt = 0;
                OnPropertyChanged(nameof(Penalite));
            }
        }

        public Challenge PenaliteSupp
        {
            get => _penaliteSupp;
            set
            {
                _penaliteSupp = value;
                _cptSupp = 0;
                OnPropertyChanged(nameof(PenaliteSupp));
            }
        }

        public Mission Mission
        {
            get => _mission;
            set => _mission = value ?? new NoMission();
        }

        public Challenge Challenge
        {
            get => _challenge;
            set
            {
                _challenge = value;
                OnPropertyChanged(nameof(Challenge));
            }
        }

        public Player(string pseudo, string id, string token)
        {
            Pseudo = pseudo;
            Id = id;
            Token = token;

            foreach (Tank t in Stats.Keys)
            {
                Stats[t] = new Stats();
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
