using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using WotGenC;

namespace GénérateurWot
{
    public partial class AccessTokenInsertion : Window
    {

        public Player Player;

        private string _playerFile;

        public AccessTokenInsertion()
        {
            InitializeComponent();
        }

        public AccessTokenInsertion(Player player)
        {
            Player = player;
            InitializeComponent();
            _playerFile = player.Id + "_save.xml";
        }

        private void AccessTokenConfirmation(object sender, RoutedEventArgs e)
        {
            Player.Token = AccessToken.Text.Trim();
            DataContractSerializer serializer = new DataContractSerializer(Player.GetType());
            FileStream fs = new FileStream(_playerFile, FileMode.OpenOrCreate);
            serializer.WriteObject(fs, Player);
            fs.Close();
            Close();
        }
    }
}