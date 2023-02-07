using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using WotGenC;

namespace GénérateurWot
{
    public partial class AllTanks : Window
    {
        public static readonly RoutedCommand ViewStats = new RoutedCommand("Stats", typeof(Button));

        public static readonly DependencyProperty PlayerProperty = DependencyProperty.Register(
            "Player", typeof(Player), typeof(AllTanks), new PropertyMetadata(default(Player)));
        
        public ICollectionView TanksCollectionView { get; private set; }

        public Player Player
        {
            get => (Player)GetValue(PlayerProperty);
            set => SetValue(PlayerProperty, value);
        }
        
        public AllTanks()
        {
            InitializeComponent();
            TanksCollectionView = CollectionViewSource.GetDefaultView(Player.Tanks);
        }

        public AllTanks(Player p)
        {
            
            Player = p;
            DataContext = p;
            TanksCollectionView = CollectionViewSource.GetDefaultView(Player.Tanks);
            InitializeComponent();
            //TiersSortList.ItemsSource = TiersSorting;
        }
        
        void Filter_Changed(object sender, RoutedEventArgs e)
        {
            var checkedTanks = new List<Tank>();

            foreach (object checkBox in Checkboxes.Children)
            {
                if (!(checkBox is CheckBox c)) continue;
                if((string)c.Content == "All") continue;
                if (!c.IsChecked.HasValue || !c.IsChecked.Value) continue;
                Enum.TryParse(typeof(Tier), (string)c.Content, true, out var t);
                checkedTanks.AddRange(Player.Tanks.Where(x => x.Te == (Tier)t));
            }

            if(CheckboxesTypes != null)
            {
                foreach (UIElement checkBox in CheckboxesTypes.Children)
                {
                    if (!(checkBox is CheckBox c)) continue;
                    if((string)c.Content == "All") continue;
                    if (!c.IsChecked.HasValue || c.IsChecked.Value) continue;
                    Enum.TryParse(typeof(TankType), (string)c.Content, true, out var t);
                    checkedTanks.RemoveAll(x => x.Type == (TankType)t);
                }
            }

            TanksCollectionView.Filter =
                tank => checkedTanks.Contains(tank as Tank);
            
            if (checkedTanks.Count == 0)
            {
                NoResults.Visibility = Visibility.Visible;
                ListOfTanks.Visibility = Visibility.Collapsed;
            }
            else
            {
                NoResults.Visibility = Visibility.Collapsed;
                ListOfTanks.Visibility = Visibility.Visible;
            }
        }

        private void Stats_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((Tank)e.Parameter)?.Nom != "Unknown";
        }

        private void Stats_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var window = new StatsWindow(Player, (Tank)e.Parameter);
            if (!window.IsVisible)
            {
                new Thread(() => WaitUntilActivation(e.OriginalSource as Button, "Couldn't open stats")).Start();
            }
        }
        
        private void WaitUntilActivation(Button button, string text)
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
        
        private void All_OnChecked(object sender, RoutedEventArgs e)
        {
            if (!(sender is CheckBox box)) return;
            if (box.IsChecked == null) return;

            foreach (UIElement element in Checkboxes.Children)
            {
                if (element is CheckBox c)
                {
                    c.IsChecked = box.IsChecked;
                }
            }
        }

        private void All_types_changed(object sender, RoutedEventArgs e)
        {
            if (!(sender is CheckBox box)) return;
            if (box.IsChecked == null) return;

            foreach (UIElement element in CheckboxesTypes.Children)
            {
                if (element is CheckBox c)
                {
                    c.IsChecked = box.IsChecked;
                }
            }
        }
    }
}