using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WotGenC;

namespace GénérateurWot
{
    public partial class AllTanks : Window
    {
        public static readonly RoutedCommand ViewStats = new RoutedCommand("Stats", typeof(Button));
        
        public ObservableCollection<bool> TiersSorting { get; set; } = new ObservableCollection<bool>();
        private readonly bool[] _typesSorting = new bool[4];
        
        public static readonly DependencyProperty PlayerProperty = DependencyProperty.Register(
            "Player", typeof(Player), typeof(AllTanks), new PropertyMetadata(default(Player)));

        public Player Player
        {
            get => (Player)GetValue(PlayerProperty);
            set => SetValue(PlayerProperty, value);
        }
        
        public AllTanks()
        {
            for (int i = 0; i < 6; i++)
            {
                TiersSorting.Add(false);
            }
            InitializeComponent();
        }

        public AllTanks(Player p)
        {
            for (int i = 0; i < 6; i++)
            {
                TiersSorting.Add(false);
            }
            Player = p;
            DataContext = p;
            InitializeComponent();
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            /*switch ((sender as CheckBox)!.Content)
            {
                case "V":
                    TiersSorting[0] = !TiersSorting[0];
                    break;
                case "VI":
                    TiersSorting[1] = !TiersSorting[1];
                    break;
                case "VII":
                    TiersSorting[2] = !TiersSorting[2];
                    break;
                case "VIII":
                    TiersSorting[3] = !TiersSorting[3];
                    break;
                case "IX":
                    TiersSorting[4] = !TiersSorting[4];
                    break;
                case "X":
                    TiersSorting[5] = !TiersSorting[5];
                    break;
                case "Light":
                    _typesSorting[0] = !_typesSorting[0];
                    break;
                case "Medium":
                    _typesSorting[1] = !_typesSorting[1];
                    break;
                case "Heavy":
                    _typesSorting[2] = !_typesSorting[2];
                    break;
                case "TD":
                    _typesSorting[3] = !_typesSorting[3];
                    break;
            }*/
            
            All.IsChecked = !TiersSorting.Any(x => false);

            List<Tank> sortedTanks = new List<Tank>();

            foreach (var playerTank in Player.Tanks)
            {
                if ((playerTank.Te != Tier.V || !TiersSorting[0]) &&
                    (playerTank.Te != Tier.VI || !TiersSorting[1]) &&
                    (playerTank.Te != Tier.VII || !TiersSorting[2]) &&
                    (playerTank.Te != Tier.VIII || !TiersSorting[3]) &&
                    (playerTank.Te != Tier.IX || !TiersSorting[4]) &&
                    (playerTank.Te != Tier.X || !TiersSorting[5]) ||  
                    (playerTank.Type != TankType.LIGHT || !_typesSorting[0]) &&
                    (playerTank.Type != TankType.MEDIUM || !_typesSorting[1]) &&
                    (playerTank.Type != TankType.HEAVY || !_typesSorting[2]) &&
                    (playerTank.Type != TankType.TD || !_typesSorting[3])) continue;
                sortedTanks.Add(playerTank);
                Debug.WriteLine("Adding " + playerTank.Nom);
            }

            if (sortedTanks.Count == 0)
            {
                NoResults.Visibility = Visibility.Visible;
                ListOfTanks.Visibility = Visibility.Collapsed;
            }
            else
            {
                NoResults.Visibility = Visibility.Collapsed;
                ListOfTanks.Visibility = Visibility.Visible;
            }
            
            sortedTanks.Sort();

            ListOfTanks.ItemsSource = sortedTanks;
        }

        private void Stats_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((Tank)e.Parameter)?.Nom != "Unknown";
        }

        private void Stats_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            new StatsWindow(Player, (Tank)e.Parameter).Show();
        }
        
        private void All_OnChecked(object sender, RoutedEventArgs e)
        {
            if (!(sender is CheckBox box)) return;
            if (box.IsChecked == null) return;
            
            for (int i = 0; i < TiersSorting.Count; i++)
            {
                TiersSorting[i] = box.IsChecked.Value;
            }
        }
    }
}