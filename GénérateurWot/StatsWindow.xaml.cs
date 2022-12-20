using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using GénérateurWot.Annotations;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json.Linq;
using WotGenC;

namespace GénérateurWot
{
    public partial class StatsWindow : Window, INotifyPropertyChanged
    {
        public Player CurrentPlayer { get; }

        public Dictionary<Tank, int> ListTanksKilled { get; } = new Dictionary<Tank, int>();

        public StatsWindow()
        {
            InitializeComponent();
        }
        public StatsWindow(Player joueur)
        {
            CurrentPlayer = joueur;
            DataContext = CurrentPlayer;
            InitializeComponent();

            KilledTanks.ItemsSource = ListTanksKilled;

            Stream stream = Requester.RequestTankStats(CurrentPlayer.Current, CurrentPlayer);
            State.Visibility = Visibility.Collapsed;

            using (StreamReader reader = new StreamReader(stream))
            {
                string text = reader.ReadToEnd();

                JObject json = JObject.Parse(text);

                JToken playerTank = json["data"][CurrentPlayer.Id][0];

                //Misc infos
                Spotted.Text += NumberFormatter.Format_To_Underscore((float)playerTank["all"]["spotted"]);
                Hits.Text += NumberFormatter.Format_To_Underscore((float)playerTank["all"]["hits"]);
                Frags.Text += NumberFormatter.Format_To_Underscore((float)playerTank["all"]["frags"]);

                float numberOfBattles = (float)playerTank["all"]["battles"];
                float wins = (float)playerTank["all"]["wins"];
                float loss = (float)playerTank["all"]["losses"];

                float draw = numberOfBattles - (wins + loss);

                NumberOfBattles.Text += NumberFormatter.Format_To_Underscore(numberOfBattles);
                Wins.Text += NumberFormatter.Format_To_Underscore(wins);
                Losses.Text += NumberFormatter.Format_To_Underscore(loss);
                Draws.Text += NumberFormatter.Format_To_Underscore(draw);

                float winrate = (float)Math.Round(wins * 100 / numberOfBattles,2);

                if (winrate < 50)
                {
                    WinRate.Foreground = new SolidColorBrush(Colors.White);
                }
                else if (winrate > 50 && winrate < 60)
                {
                    WinRate.Foreground = new SolidColorBrush(Colors.LightGreen);
                }
                else if (winrate > 60 && winrate < 70)
                {
                    WinRate.Foreground = new SolidColorBrush(Colors.RoyalBlue);
                }
                else
                {
                    WinRate.Foreground = new SolidColorBrush(Colors.MediumPurple);
                }

                WinRate.Text += $"{winrate}%";

                MaxXPOneBattle.Text += NumberFormatter.Format_To_Underscore((float)playerTank["all"]["max_xp"]);
                TotalDamageDealt.Text += NumberFormatter.Format_To_Underscore((float)playerTank["all"]["damage_dealt"]);
                TotalDamageReceived.Text += NumberFormatter.Format_To_Underscore((float)playerTank["all"]["damage_received"]);

                MaximumFragsOneBattle.Text += (string)playerTank["all"]["max_frags"];
                ShotsTotal.Text += NumberFormatter.Format_To_Underscore((float)playerTank["all"]["shots"]);
                xp.Text += NumberFormatter.Format_To_Underscore((float)playerTank["all"]["xp"]);

                float survivalWinRate = (float)Math.Round((float)playerTank["all"]["win_and_survived"] / wins * 100,2);
                float survivalRate = (float)Math.Round((float)playerTank["all"]["survived_battles"] / numberOfBattles * 100,2);

                SurvivalWinRate.Text += survivalWinRate.ToString("F2") + "%";
                SurvivalRate.Text += survivalRate.ToString("F2") + "%";
                
                JObject frags = (JObject)playerTank["frags"];
                using var enumerator = frags.GetEnumerator();
                enumerator.MoveNext();

                string[] ids = new string[frags.Count];
                
                for (int i = 0; i < frags.Count; i++)
                {
                    ids[i] = enumerator.Current.Key;
                    enumerator.MoveNext();
                }

                var tanks = Loader.FindTanksById(ids);

                for (int i = 0; i < frags.Count; i++)
                {
                    ListTanksKilled.Add(tanks[i], (int)frags[ids[i]]);
                }

                
                OnPropertyChanged(nameof(ListTanksKilled));
                KilledTanks.ItemsSource = ListTanksKilled;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}