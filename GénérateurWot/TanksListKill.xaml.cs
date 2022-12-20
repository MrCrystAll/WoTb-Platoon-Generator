using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WotGenC;

namespace GénérateurWot
{
    public partial class TanksListKill : UserControl
    {
        public static readonly DependencyProperty TankProperty = DependencyProperty.Register(
            "Tank", typeof(Tank), typeof(TanksListKill), new PropertyMetadata(new Tank("0", "Unknown", Tier.I, "None", TankType.HEAVY)));

        public Tank Tank
        {
            get => (Tank)GetValue(TankProperty);
            set => SetValue(TankProperty, value);
        }

        public static readonly DependencyProperty NbKilledProperty = DependencyProperty.Register(
            "NbKilled", typeof(int), typeof(TanksListKill), new PropertyMetadata(0));

        public int NbKilled
        {
            get => (int)GetValue(NbKilledProperty);
            set => SetValue(NbKilledProperty, value);
        }
        public TanksListKill()
        {
            InitializeComponent();
        }
    }
}