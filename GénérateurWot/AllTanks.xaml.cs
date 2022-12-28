using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WotGenC;

namespace GénérateurWot
{
    public partial class AllTanks : Window
    {
        public static readonly RoutedCommand ViewStats = new RoutedCommand("Stats", typeof(Button));
        
        private readonly bool[] _tiersSorting = new bool[6];
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
            InitializeComponent();
        }

        public AllTanks(Player p)
        {
            Player = p;
            DataContext = p;
            InitializeComponent();
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            switch ((sender as CheckBox)!.Content)
            {
                case "V":
                    _tiersSorting[0] = !_tiersSorting[0];
                    break;
                case "VI":
                    _tiersSorting[1] = !_tiersSorting[1];
                    break;
                case "VII":
                    _tiersSorting[2] = !_tiersSorting[2];
                    break;
                case "VIII":
                    _tiersSorting[3] = !_tiersSorting[3];
                    break;
                case "IX":
                    _tiersSorting[4] = !_tiersSorting[4];
                    break;
                case "X":
                    _tiersSorting[5] = !_tiersSorting[5];
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
            }

            List<Tank> sortedTanks = new List<Tank>();

            foreach (var playerTank in Player.Tanks)
            {
                if ((playerTank.Te != Tier.V || !_tiersSorting[0]) &&
                    (playerTank.Te != Tier.VI || !_tiersSorting[1]) &&
                    (playerTank.Te != Tier.VII || !_tiersSorting[2]) &&
                    (playerTank.Te != Tier.VIII || !_tiersSorting[3]) &&
                    (playerTank.Te != Tier.IX || !_tiersSorting[4]) &&
                    (playerTank.Te != Tier.X || !_tiersSorting[5]) ||  
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
    }
}