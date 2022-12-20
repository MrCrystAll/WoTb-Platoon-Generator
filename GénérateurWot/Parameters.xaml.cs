using System.Windows;
using System.Windows.Controls;
using WotGenC;

namespace GénérateurWot
{
    public partial class Parameters : Window
    {
        public Parameters()
        {
            InitializeComponent();
        }

        public Parameters(MainWindow mainWindow)
        {
            InitializeComponent();
            DataContext = mainWindow;
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((MainWindow)DataContext).CurrentMode = (GameMode)((ComboBox)sender).SelectedValue;
        }
    }
}