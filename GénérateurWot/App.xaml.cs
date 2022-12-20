using System;
using System.Windows;

namespace GénérateurWot
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnExit(object sender, ExitEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
