using System.Collections.Generic;

namespace WotGenC
{
    public struct Stats
    {
        public float 
            Spotted, 
            Hits, 
            Frags, 
            NumberOfBattles, 
            Wins, 
            Losses, 
            Draws, 
            WinRate, 
            MaxXp1B, 
            TotalDmgDlt, 
            TotalDmgRecvd,
            MaxFrags1B,
            TotalShots,
            Xp,
            SurvWinRate,
            SurvRate;

        public Dictionary<Tank, uint> KilledTanks;
    }
}