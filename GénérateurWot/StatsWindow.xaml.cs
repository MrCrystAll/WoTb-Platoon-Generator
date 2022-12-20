using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using GénérateurWot.Annotations;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json.Linq;
using WotGenC;

namespace GénérateurWot
{
    public sealed partial class StatsWindow : Window, INotifyPropertyChanged
    {
        public Player CurrentPlayer { get; }

        public Dictionary<Tank, uint> ListTanksKilled { get; } = new Dictionary<Tank, uint>();

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
                    Debug.WriteLine(tanks[i].Nom);
                    ListTanksKilled.Add(tanks[i], (uint)frags[ids[i]]);
                }

                var list = ListTanksKilled.ToList();
                list.Sort((x,y) => -x.Value.CompareTo(y.Value));
                ListTanksKilled = list.ToDictionary(x => x.Key, x => x.Value);
                
                OnPropertyChanged(nameof(ListTanksKilled));
                KilledTanks.ItemsSource = ListTanksKilled;

                Stats s = new Stats(
                    spotted: (float)playerTank["all"]["spotted"],
                    hits: (float)playerTank["all"]["hits"],
                    frags: (float)playerTank["all"]["frags"],
                    numberOfBattles: (float)playerTank["all"]["battles"],
                    wins: (float)playerTank["all"]["wins"],
                    losses: (float)playerTank["all"]["losses"],
                    maxXp1B: (float)playerTank["all"]["max_xp"],
                    totalDmgDlt: (float)playerTank["all"]["damage_dealt"],
                    totalDmgRecvd: (float)playerTank["all"]["damage_received"],
                    maxFrags1B: (float)playerTank["all"]["max_frags"],
                    totalShots: (float)playerTank["all"]["shots"],
                    xp: (float)playerTank["all"]["xp"],
                    winAndSurvived: (float)playerTank["all"]["win_and_survived"],
                    survivedBattles: (float)playerTank["all"]["survived_battles"],
                    droppedCapturePoints: (float)playerTank["all"]["dropped_capture_points"],
                    killedTanks: ListTanksKilled
                );

                //Misc infos
                Spotted.Text += NumberFormatter.Format_To_Space(s.Spotted);
                Hits.Text += NumberFormatter.Format_To_Space(s.Hits);
                Frags.Text += NumberFormatter.Format_To_Space(s.Frags);
                LightKillRate.Text += $"{Math.Round(s.LightKillRate * 100,2).ToString(CultureInfo.CurrentCulture)}% ({s.NbLightsKilled})";
                MedKillRate.Text += $"{Math.Round(s.MediumKillRate * 100,2).ToString(CultureInfo.CurrentCulture)}% ({s.NbMedsKilled})";
                HeavyKillRate.Text += $"{Math.Round(s.HeavyKillRate * 100,2).ToString(CultureInfo.CurrentCulture)}% ({s.NbHeaviesKilled})";
                TdKillRate.Text += $"{Math.Round(s.TdKillRate * 100,2).ToString(CultureInfo.CurrentCulture)}% ({s.NbTdsKilled})";

                NumberOfBattles.Text += NumberFormatter.Format_To_Space(s.NumberOfBattles);
                Wins.Text += NumberFormatter.Format_To_Space(s.Wins);
                Losses.Text += NumberFormatter.Format_To_Space(s.Losses);
                Draws.Text += NumberFormatter.Format_To_Space(s.Draws);

                float winrate = s.WinRate * 100;

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

                WinRate.Text += $"{Math.Round(winrate,2)}%";

                MaxXPOneBattle.Text += NumberFormatter.Format_To_Space(s.MaxXp1B);
                TotalDamageDealt.Text += NumberFormatter.Format_To_Space(s.TotalDmgDlt);
                TotalDamageReceived.Text += NumberFormatter.Format_To_Space(s.TotalDmgRecvd);

                MaximumFragsOneBattle.Text += s.MaxFrags1B.ToString(CultureInfo.CurrentCulture);
                ShotsTotal.Text += NumberFormatter.Format_To_Space(s.TotalShots);
                xp.Text += NumberFormatter.Format_To_Space(s.Xp);

                float survivalWinRate = (float)Math.Round(s.SurvivalWinRate * 100,2);
                float survivalRate = (float)Math.Round(s.SurvivalRate * 100,2);

                SurvivalWinRate.Text += survivalWinRate.ToString("F2") + "%";
                SurvivalRate.Text += survivalRate.ToString("F2") + "%";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}