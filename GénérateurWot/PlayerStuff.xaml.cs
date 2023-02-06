using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WotGenC;
using WotGenC.Challenges;
using WotGenC.Missions;
using DependencyProperty = System.Windows.DependencyProperty;
using PropertyMetadata = System.Windows.PropertyMetadata;
using RoutedEventArgs = System.Windows.RoutedEventArgs;
using Timer = WotGenC.Timer;
using Visibility = System.Windows.Visibility;

namespace GénérateurWot
{
    public partial class PlayerStuff
    {
        public MainWindow FirstParent;
        public Player Joueur
        {
            get => (Player)GetValue(JoueurProperty);
            set
            {
                SetValue(JoueurProperty, value);
                Joueur.Mission.PropertyChanged += OnMissionChange;
                Joueur.PropertyChanged += OnPlayerChanged;
                Pseudo.Text = Joueur.Pseudo;
                Joueur.Timers.OnAddTimer += OnAddTimer;
            }
        }


        private void OnPlayerChanged(object sender, PropertyChangedEventArgs e)
        {
            Stats.Visibility = Visibility.Visible;
            
            if(Joueur.Current is null)
            {
                Tank.Text = string.Empty;
                TankPicture.Source = null;
            }
            else
            {
                Tank.Text = Joueur.Current.Nom;
                TankPicture.Source = new BitmapImage(new Uri(Joueur.Current.Image, UriKind.Absolute));
            }

            Challenge.Text = Joueur.Challenge is null 
                ? string.Empty 
                : Joueur.Challenge.Intitule;
            
            Penalite.Text = Joueur.Penalite is null 
                ? string.Empty 
                : "Pénalité : " + Joueur.Penalite.Intitule;
            
            PenaliteSupp.Text = Joueur.PenaliteSupp is null
                ? string.Empty
                : "Pénalité supplémentaire : " + Joueur.PenaliteSupp.Intitule;
        }

        // Using a DependencyProperty as the backing store for Joueur.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty JoueurProperty =
            DependencyProperty.Register("Joueur", typeof(Player), typeof(PlayerStuff), new PropertyMetadata(null));


        public PlayerStuff()
        {
            InitializeComponent();
        }

        private void OnMissionChange(object sender, PropertyChangedEventArgs e)
        {
            if (Joueur.Mission is NoMission)
            {
                Valid.Visibility = Visibility.Collapsed;
                Bconfirm.Visibility = Visibility.Collapsed;
            }
            else
            {
                Valid.Visibility = Visibility.Visible;
                Bconfirm.Visibility = Visibility.Visible;
            }
        }

        public void ModifySelf()
        {
            BrushConverter conv = new BrushConverter();

            Timers.Items.Refresh();

            Miss.Text = Joueur.Mission.Intitule;
            Valid.Visibility = Joueur.Mission.GetType() != typeof(NoMission) ? Visibility.Visible : Visibility.Collapsed;
            Bconfirm.Visibility = Joueur.Mission.GetType() != typeof(NoMission) ? Visibility.Visible : Visibility.Collapsed;
        }

        // private void MissionOk_OnChecked(object sender, RoutedEventArgs e)
        // {
        //     throw new NotImplementedException();
        // }
        //
        // private void MissionFailed_OnChecked(object sender, RoutedEventArgs e)
        // {
        //     throw new NotImplementedException();
        // }

        private void Confirmation_Click(object sender, RoutedEventArgs e)
        {
            Valid.Visibility = Visibility.Collapsed;
            Bconfirm.Visibility = Visibility.Collapsed;

            switch (Validé.IsChecked)
            {
                case true:
                    Miss.Text = "Mission accomplie";
                    if (FirstParent.SuppPenalty && Joueur.Challenge != null && Joueur.Penalite != null)
                    {
                        Random r = new Random();
                        int y = r.Next(0, MainWindow.Challenges.Count);
                        while (MainWindow.Challenges[y].GetType() == Joueur.Challenge.GetType() ||
                               MainWindow.Challenges[y].GetType() == Joueur.Penalite.GetType())
                        {
                            y = r.Next(0, MainWindow.Challenges.Count);
                        }

                        Settings.Players.Where(x => x != Joueur).ToList()[0].PenaliteSupp =
                            MainWindow.Challenges[y];
                    }
                    Joueur.Penalite = null;
                    break;
                case false:
                {
                    Valid.Visibility = Visibility.Collapsed;
                    Bconfirm.Visibility = Visibility.Collapsed;
                    Random r = new Random();

                    int y = r.Next(0, MainWindow.Challenges.Count);
                    Joueur.Penalite = FirstParent.ChallengesOnOff ? MainWindow.Challenges[y] : null;
                    Miss.Text = FirstParent.ChallengesOnOff ? $"Mission échouée : {MainWindow.Challenges[y].Intitule}" : "Mission échouée";

                    if (FirstParent.ChallengesOnOff)
                    {
                        //Si jamais on a déja un challenge interdiction de zoomer
                        if (!(Joueur.Challenge is null) && Joueur.Challenge.GetType() == typeof(ChallengeNoZoom))
                        {
                            //On interdit le challenge full zoom
                            while (MainWindow.Challenges[y].GetType() == typeof(ChallengeFullZoom))
                            {
                                y = r.Next(0, MainWindow.Challenges.Count);
                            }
                            Joueur.Penalite = MainWindow.Challenges[y];
                        }
                    
                        //Interdiction d'avoir le challenge d'immobilité en pénalité de mission
                        while (MainWindow.Challenges[y].GetType() == typeof(ChallengeImmobility) && !(Joueur.Challenge is null) && Joueur.Challenge.GetType() == MainWindow.Challenges[y].GetType())
                        {
                            y = r.Next(0, MainWindow.Challenges.Count);
                        }
                        Joueur.Penalite = MainWindow.Challenges[y];
                    }
                    break;
                }
            }
            
            Validé.IsChecked = false;
            Raté.IsChecked = false;
        }

        public void CreateTimers(bool mission, int tps)
        {
            if (!(Joueur.Challenge is ChallengeImmobility))
            {
                Joueur.Timers.Clear();
                Timers.Items.Refresh();
                Timers.Visibility = Visibility.Collapsed;
                return;
            }

            Timers.Visibility = Visibility.Visible;
            Random r = new Random();

            const int ecart = 20;
            const int startTime = 400;

            int t3 = r.Next(startTime - tps, startTime);
            int t2 = r.Next(t3 - tps - (int)(ecart * 1.5), t3 - tps - ecart);
            int t1 = r.Next(t2 - tps - (int)(ecart * 1.5), t2 - tps - ecart);

            Joueur.Timers.AddTimer(new Timer((int)(7 - Math.Round((double)t3 / 60, MidpointRounding.ToPositiveInfinity)), 60 - t3 % 60, Joueur, tps)).AddTimer(new Timer((int)(7 - Math.Round((double)t2 / 60, MidpointRounding.ToPositiveInfinity)), 60 - t2 % 60, Joueur, tps)).AddTimer(new Timer((int)(7 - Math.Round((double)t1 / 60, MidpointRounding.ToPositiveInfinity)), 60 - t1 % 60, Joueur, tps));
            
            if (mission)
            {
                Miss.Text += $". Il ne bouge pas pendant {tps} secondes, il s'arrête à {t3 / 60}:{t3 % 60}, {t2 / 60}:{t2 % 60}, {t1 / 60}:{t1 % 60}";
            }
        }

        private void Timers_Updated(object sender, DataTransferEventArgs e)
        {
            Timers.ItemsSource ??= Joueur.Timers.Timers;
            Timers.Items.Refresh();
        }
        
        private void OnAddTimer(object sender, EventArgs e)
        {
            Timers_Updated(sender, null);
        }

        private void Stats_OnClick(object sender, RoutedEventArgs e)
        {
            var window = new StatsWindow(Joueur, Joueur.Current);
            if (!window.IsVisible)
            {
                new Thread(() => WaitUntilActivationFailed(e.OriginalSource as Button, "Can't see stats")).Start();
            }
            
        }

        private void Actualize_OnClick(object sender, RoutedEventArgs e)
        {
            Stream stream = Requester.RequestTanks(Joueur.Id, Joueur.Token);
            if (Loader.LoadTanks(Joueur.Id, Joueur.Tanks, stream))
            {
                new Thread(WaitUntilActivation).Start();
            }
            else
            {
                new Thread(() => WaitUntilActivationFailed(e.OriginalSource as Button, "Request failed")).Start();
            }
            
        }

        private void WaitUntilActivationFailed(Button button, string text)
        {
            string originalText = "";
            Dispatcher.Invoke(() => originalText = (string)button.Content);
            Dispatcher.Invoke(() => button.IsEnabled = false);
            
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            while (stopwatch.Elapsed.Seconds < 5)
            {
                Dispatcher.Invoke(() => button.Content = text);
            }

            stopwatch.Stop();
            
            Dispatcher.Invoke(() => button.IsEnabled = true);
            Dispatcher.Invoke(() => button.Content = originalText);
        }

        private void WaitUntilActivation()
        {
            string originalText = "";
            Dispatcher.Invoke(() => originalText = (string)Actualize.Content);
            Dispatcher.Invoke(() => Actualize.IsEnabled = false);
            
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            while (stopwatch.Elapsed.Seconds < 5)
            {
                Dispatcher.Invoke(() => Actualize.Content = $"{5 - stopwatch.Elapsed.Seconds}s");
            }

            stopwatch.Stop();
            
            Dispatcher.Invoke(() => Actualize.IsEnabled = true);
            Dispatcher.Invoke(() => Actualize.Content = originalText);
            
        }

        private void AllTanks_OnClick(object sender, RoutedEventArgs e)
        {
            
            var allTanks = new AllTanks(Joueur);
            allTanks.Show();
        }
    }
}