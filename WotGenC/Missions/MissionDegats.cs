using System;
using System.Collections.Generic;
using System.Text;

namespace WotGenC.Missions
{
    public class MissionDegats : Mission
    {
        public int Degats { get; set; }
        public int Kills { get; set; }

        public MissionDegats(int degats, int kill)
        {
            Degats = degats;
            Kills = kill;

            Intitule = $"Faire {Degats}HP de dégats ou {Kills}kill(s)";
        }
    }
}
