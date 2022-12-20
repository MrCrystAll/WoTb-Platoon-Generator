using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
            MaxXp1B,
            TotalDmgDlt,
            TotalDmgRecvd,
            MaxFrags1B,
            TotalShots,
            Xp,
            WinAndSurvived,
            SurvivedBattles,
            DroppedCapturePoints;

        public float Draws => NumberOfBattles - (Wins + Losses);
        public float WinRate => Wins / (Wins + Losses);
        public float AvgSpotsPerGame => Spotted / NumberOfBattles;
        public float AvgHitsPerGame => Hits / NumberOfBattles;
        public float AvgFragsPerGame => Frags / NumberOfBattles;
        public float SurvivalRate => SurvivedBattles / NumberOfBattles;
        public float SurvivalWinRate => WinAndSurvived / NumberOfBattles;

        public Dictionary<Tank, uint> KilledTanks;

        public float NbLightsKilled => KilledTanks.Where(x => x.Key.Type == TankType.LIGHT).Sum(x => x.Value);
        public float NbMedsKilled => KilledTanks.Where(x => x.Key.Type == TankType.MEDIUM).Sum(x => x.Value);
        public float NbHeaviesKilled => KilledTanks.Where(x => x.Key.Type == TankType.HEAVY).Sum(x => x.Value);
        public float NbTdsKilled => KilledTanks.Where(x => x.Key.Type == TankType.TD).Sum(x => x.Value);

        public float LightKillRate => NbLightsKilled / KilledTanks.Sum(x => x.Value);

        public float MediumKillRate => NbMedsKilled / KilledTanks.Sum(x => x.Value);
        
        public float HeavyKillRate => NbHeaviesKilled / KilledTanks.Sum(x => x.Value);
        
        public float TdKillRate => NbTdsKilled / KilledTanks.Sum(x => x.Value);

        public Stats(float spotted, float hits, float frags, float numberOfBattles, float wins, 
            float losses, float maxXp1B, float totalDmgDlt, float totalDmgRecvd, 
            float maxFrags1B, float totalShots, float xp, float winAndSurvived,  
            float survivedBattles, float droppedCapturePoints, Dictionary<Tank, uint> killedTanks)
        {
            Spotted = spotted;
            Hits = hits;
            Frags = frags;
            NumberOfBattles = numberOfBattles;
            Wins = wins;
            Losses = losses;
            MaxXp1B = maxXp1B;
            TotalDmgDlt = totalDmgDlt;
            TotalDmgRecvd = totalDmgRecvd;
            MaxFrags1B = maxFrags1B;
            TotalShots = totalShots;
            Xp = xp;
            WinAndSurvived = winAndSurvived;
            SurvivedBattles = survivedBattles;
            DroppedCapturePoints = droppedCapturePoints;
            KilledTanks = killedTanks;
        }
    }
}