using System;
using System.Collections.Generic;
using System.Text;

using System.Linq;

namespace WotGenC.Missions
{
    public class MissionProtect : Mission
    {
        public Player Protected { get; set; }
        public Player Protector { get; set; }

        public MissionProtect(Player @protected ,Player protector)
        {
            Protected = @protected;
            Protector = protector;
        }

        public MissionProtect(Player protector)
        {
            Protector = protector;
            Player[] list = new Player[5];
            Settings.Players.CopyTo(list);
            List<Player> players = list.ToList();
            players.RemoveAll(x => x is null || x == Protector);

            Random random = new Random();

            int @protected = random.Next(players.Count);
            Protected = players[@protected];
            
            Intitule = $"Empêcher {Protected.Pseudo} de mourir";
        }
    }
}
