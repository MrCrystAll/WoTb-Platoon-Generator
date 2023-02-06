using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using WotGenC;
using WotGenC.Challenges;
using WotGenC.Database;
using WotGenC.Missions;

namespace GénérateurWot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : INotifyPropertyChanged
    {
        private bool _suppPenalty;

        public bool SuppPenalty
        {
            get => _suppPenalty;
            set
            {
                _suppPenalty = value;
                OnPropertyChanged(nameof(SuppPenalty));
            }
        }

        private bool _sameTankTwice;

        public bool SameTankTwice
        {
            get => _sameTankTwice;
            set
            {
                _sameTankTwice = value;
                OnPropertyChanged(nameof(SameTankTwice));
            }
        }

        private bool _isMission;
        public bool IsMission
        {
            get => _isMission;
            set
            {
                _isMission = value;
                OnPropertyChanged(nameof(IsMission));
            } 
        }

        private GameMode _currentMode;

        public List<GameMode> Modes { get; } = Enum.GetValues(typeof(GameMode)).Cast<GameMode>().ToList();

        public GameMode CurrentMode
        {
            get => _currentMode;
            set
            {
                _currentMode = value;
                OnPropertyChanged(nameof(CurrentMode));
                Settings.GameMode = CurrentMode;
            }
        }

        private Tier _currentTier;

        public Tier CurrentTier
        {
            get => _currentTier;
            set
            {
                _currentTier = value;
                OnPropertyChanged(nameof(CurrentTier));
                Tier.Text = "Tier " + _currentTier.ToString("G");
            }
        }

        private bool LockedTier { get; set; }
        private bool _challengesOnOff = true;

        public bool ChallengesOnOff
        {
            get => _challengesOnOff;
            set
            {
                _challengesOnOff = value;
                OnPropertyChanged(nameof(ChallengesOnOff));
            }
        }

        private List<PlayerStuff> PlayerStuffs { get; set; } = new List<PlayerStuff>();

        public static List<Challenge> Challenges { get; set; } = new List<Challenge>();
        public List<Player> Players { get; set; } = new List<Player>();
        public MainWindow()
        {
            Debug.WriteLine("Creating players...");
            Players.Add(new Player("nath231", "524090414", "8e5547dd4e3e5b2de0fd45d831a0b951ce163c3b"));
            Players.Add(new Player("Mathieu1er", "528189939", "bd7c03ccc6e1aed03f2f82e25aede880aeb114ce"));

            DbPlayer player = new DbPlayer(Players[0]);
            player.WriteToDb();
            player.Player = Players[1];
            player.WriteToDb();
            
            Debug.WriteLine("Initializing components");
            InitializeComponent();
            
            
            Debug.WriteLine("Assigning players");
            J1.Joueur = Players[0];
            J2.Joueur = Players[1];

            J1.FirstParent = this;
            J2.FirstParent = this;

            Settings.Players = Players;

            PlayerStuffs.Add(J1);
            PlayerStuffs.Add(J2);
            Loader.LoadBackupsFor(Players);
            CurrentMode = GameMode.Random;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            #region Reset des timers

            foreach (var player in Players)
            {
                player.Timers.Clear();
            }

            foreach (var playerStuff in PlayerStuffs)
            {
                playerStuff.Timers.Visibility = Visibility.Collapsed;
                PauseRestart.Visibility = Visibility.Collapsed;
            }

            #endregion
            

            #region Choix du char

            Random r = new Random();

            int ti = r.Next(0, 4);
            if (!LockedTier)
            {
                switch (ti)
                {
                    case 0:
                        foreach (var player in Players)
                        {
                            var list = Tri(WotGenC.Tier.VII, player.Tanks);
                            var current = player.Current;
                            player.Current = TankPicker.PickARandomTank(!(list.Count <= 1) && SameTankTwice ? list.Where(x => x != current).ToList() : list);
                        }

                        CurrentTier = WotGenC.Tier.VII;
                        break;
                    case 1:
                        foreach (var player in Players)
                        {
                            var list = Tri(WotGenC.Tier.VIII, player.Tanks);
                            var current = player.Current;
                            player.Current = TankPicker.PickARandomTank(!(list.Count <= 1) && SameTankTwice ? list.Where(x => x != current).ToList() : list);
                        }

                        CurrentTier = WotGenC.Tier.VIII;
                        break;
                    case 2:
                        foreach (var player in Players)
                        {
                            var list = Tri(WotGenC.Tier.IX, player.Tanks);
                            var current = player.Current;
                            player.Current = TankPicker.PickARandomTank(!(list.Count <= 1) && SameTankTwice ? list.Where(x => x != current).ToList() : list);
                        }

                        CurrentTier = WotGenC.Tier.IX;
                        break;
                    case 3:
                        foreach (var player in Players)
                        {
                            var list = Tri(WotGenC.Tier.X, player.Tanks);
                            var current = player.Current;
                            player.Current = TankPicker.PickARandomTank(!(list.Count <= 1) && SameTankTwice ? list.Where(x => x != current).ToList() : list);
                        }

                        CurrentTier = WotGenC.Tier.X;
                        Settings.Tier = CurrentTier;
                        break;
                }
            }
            else
            {
                foreach (var player in Players)
                {
                    var list = Tri(CurrentTier, player.Tanks);
                    var current = player.Current;
                    player.Current = TankPicker.PickARandomTank(!(list.Count <= 1) && SameTankTwice ? list.Where(x => x != current).ToList() : list);
                }
            }

            #endregion

            #region Choix des challenges et des joueur

            switch (ChallengesOnOff)
            {
                case true:
                    ChallengeSetter setter = new ChallengeSetter();
                    setter.WillThereBeAChallenge(Players, Challenges);

                    //Challenge.Text = $"Challenge : {Challenges[T].Intitule} pour {Players[p].Pseudo}";
                    List<PlayerStuff> stuffs = PlayerStuffs.Where(x => x.Joueur.Challenge is ChallengeImmobility)
                        .ToList();
                    if (stuffs.Count != 0)
                    {
                        StartTimers.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        StartTimers.Visibility = Visibility.Collapsed;
                        PauseRestart.Visibility = Visibility.Collapsed;
                        break;
                    }

                    foreach (var playerStuff in stuffs)
                    {
                        int tps = r.Next(10, 30);
                        playerStuff.CreateTimers(false, tps);

                        playerStuff.Timers.Visibility = Visibility.Visible;

                        //Challenge.Text +=
                        //$". Il ne bouge pas pendant {tps} secondes, il s'arrête à {6 - Players[p].Timers.Timers[0].Minutes}:{60 - Players[p].Timers.Timers[0].Seconds:D2}, {6 - Players[p].Timers.Timers[1].Minutes}:{60 - Players[p].Timers.Timers[1].Seconds:D2}, {6 - Players[p].Timers.Timers[2].Minutes}:{60 - Players[p].Timers.Timers[2].Seconds:D2}\n";
                    }

                    break;
                case false:
                    foreach (var player in Players)
                    {
                        player.Challenge = null;
                    }

                    break;
            }

            #endregion

            Tier.Visibility = Visibility.Visible;
            //Tanks.Visibility = Visibility.Visible;

            #region Reset des missions

            foreach (var player in Players)
            {
                player.Mission = new NoMission();
            }

            #endregion

            #region Choix des missions

            double v = r.NextDouble();
            if (v < 0.9 && v > 0.3)
            {
                MissionSetter setter = new MissionSetter();

                if(IsMission) setter.WillThereBeAMission(Players);
            }

            #endregion

            #region Mise à jour des UserControls

            foreach (var playerStuff in PlayerStuffs)
            {
                playerStuff.ModifySelf();
            }

            #endregion
            
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Pas de pseudo-réaliste en réaliste
            Challenges.Add(new ChallengePseudoRealiste(GameMode.Random, GameMode.Gravity, GameMode.MadGames));
            
            //Standard
            Challenges.Add(new ChallengeMarcheArriere(GameMode.Random,GameMode.Réaliste, GameMode.Gravity, GameMode.MadGames));
            Challenges.Add(new ChallengeUnShotSurDeux(GameMode.Random,GameMode.Réaliste, GameMode.Gravity, GameMode.MadGames, GameMode.Burning));
            Challenges.Add(new ChallengeReverseBinding(GameMode.Random,GameMode.Réaliste, GameMode.Gravity, GameMode.MadGames, GameMode.Burning));
            Challenges.Add(new ChallengeFullZoom(GameMode.Random,GameMode.Réaliste, GameMode.Gravity, GameMode.MadGames));
            Challenges.Add(new ChallengeNoZoom(GameMode.Random,GameMode.Réaliste, GameMode.Gravity, GameMode.MadGames));
            Challenges.Add(new ChallengeImmobility(GameMode.Random,GameMode.Réaliste, GameMode.Gravity, GameMode.MadGames));
            Challenges.Add(new ChallengeTourelleBloquee(GameMode.Random,GameMode.Réaliste, GameMode.Gravity, GameMode.MadGames));
            
            //Powers related
            Challenges.Add(new ChallengeActivationPouvoir(GameMode.MadGames, GameMode.BigBoss, GameMode.Uprising));
            Challenges.Add(new ChallengeInterdictionPouvoir(GameMode.MadGames, GameMode.BigBoss, GameMode.Uprising));
        }

        private List<Tank> Tri(Tier t, IEnumerable<Tank> list) => list.Where(tank => tank.Te == t).ToList();

        private void Tier_Change_Click(object sender, RoutedEventArgs e)
        {
            if (!LockedTier)
            {
                Lock.Content = "Unlock tier";
                LockedTier = true;
                return;
            }

            Lock.Content = "Lock tier";
            LockedTier = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Start_Timers_OnClick(object sender, RoutedEventArgs e)
        {
            StartTimers.Visibility = Visibility.Collapsed;
            PauseRestart.Visibility = Visibility.Visible;
            foreach (var player in PlayerStuffs)
            {
                player.Joueur.Timers.StartAll();
            }
        }

        private void Pause_OnClick(object sender, RoutedEventArgs e)
        {
            Paused = !Paused;
            Pause.Content = Paused ? "Unpause" : "Pause";
            foreach (var playerStuff in PlayerStuffs)
            {
                playerStuff.Joueur.Timers.SwitchPauseUnpauseAll();
            }
        }

        private bool Paused { get; set; }

        private void Restart_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var playerStuff in PlayerStuffs)
            {
                playerStuff.Joueur.Timers.RestartAll();
            }
        }

        private void Parameters_OnClick(object sender, RoutedEventArgs e)
        {
            Parameters.IsEnabled = false;
            var param =  new Parameters(this);
            param.Closed += (o, args) => Parameters.IsEnabled = true;
            param.Show();
        }
    }
}