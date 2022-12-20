using System.Collections.Generic;
using System.Linq;

namespace WotGenC.Challenges
{
    public abstract class Challenge
    {
        public string Intitule { get; set; }

        public List<GameMode> AvailablesModes;

        protected Challenge(params GameMode[] modes)
        {
            Intitule = "Intitulé";
            AvailablesModes = modes.ToList();
        }
    }
}