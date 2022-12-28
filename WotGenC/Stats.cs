using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace WotGenC
{
    public struct Stats
    {
        public int PlayerId, TankId;

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
        public float AvgDmgDltPerGame => TotalDmgDlt / NumberOfBattles;
        public float AvgDmgRecvdPerGame => TotalDmgRecvd / NumberOfBattles;
        public float RatioDmgDltRecvd => AvgDmgDltPerGame / AvgDmgRecvdPerGame;
        public float AvgXpPerGame => Xp / NumberOfBattles;
        public float AvgShotsPerGame => TotalShots / NumberOfBattles;

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
        public float Precision => Hits / TotalShots;
        public float Destroyed => NumberOfBattles - SurvivedBattles;
        public float HitLethality => Frags / Hits;
        public float SurvivabilityRate => Destroyed / SurvivedBattles;

        public double ProbabilityOfHitsEqualOrMoreThan(ulong nbHits)
        {
            double sum = 0;

            for (ulong i = 0; i <= nbHits; i++)
            {
                sum += ProbabilityOfHits(i);
                if (sum > 1) return 1;
            }

            return ProbabilityOfHits(nbHits) + (1 - sum);
        }

        public double ProbabilityOfSurvivalEqualOrMoreThan(ulong nbbattles)
        {
            double sum = 0;
            for (ulong i = 0; i < nbbattles; i++)
            {
                sum += ProbabiltyOfSurvival(i);
            }

            return ProbabiltyOfSurvival(nbbattles) + (1 - sum);
        }

        public double ProbabilityOfHits(ulong nbHits) => Math.Pow(AvgShotsPerGame, nbHits) / Factorial(nbHits) * Math.Exp(-AvgShotsPerGame);

        public double ProbabiltyOfSurvival(ulong nbBattles) => Math.Pow(SurvivalRate, nbBattles) * Math.Pow(1 - SurvivalRate, 1);

        private ulong Factorial(ulong n)
        {
            ulong fact = 1;
            for (ulong i = 1; i <= n; i++)
            {
                fact *= i;
            }

            return fact;
        }

        public Stats(int playerId, int tankId, float spotted, float hits, float frags, float numberOfBattles,
            float wins,
            float losses, float maxXp1B, float totalDmgDlt, float totalDmgRecvd,
            float maxFrags1B, float totalShots, float xp, float winAndSurvived,
            float survivedBattles, float droppedCapturePoints, Dictionary<Tank, uint> killedTanks)
        {
            PlayerId = playerId;
            TankId = tankId;
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

        public override string ToString()
        {
            var message = $"======== Stats for {PlayerId} with the tank n°{TankId} ==========\n" +
                          $"-------Raw battle data------\n" +
                          $"Enemy spotted :                       {Spotted}\n" +
                          $"Number of hits :                      {Hits}\n" +
                          $"Number of frags :                     {Frags}\n" +
                          $"-------Battle statistics----\n" +
                          $"Number of battles :                   {NumberOfBattles}\n" +
                          $"Number of wins :                      {Wins}\n" +
                          $"Number of losses :                    {Losses}\n" +
                          $"Number of survived battles :          {SurvivedBattles}\n" +
                          $"Number of wins and survived battles : {WinAndSurvived}\n" +
                          $"-------Battle maximums------\n" +
                          $"Maximum frags in one battle :         {MaxFrags1B}\n" +
                          $"Maximum XP in one battle :            {MaxXp1B}\n" +
                          $"-------Battle totals--------\n" +
                          $"Total damage dealt :                  {TotalDmgDlt}\n" +
                          $"Total damage received :               {TotalDmgRecvd}\n" +
                          $"Total number of shots :               {TotalShots}\n" +
                          $"Total XP : {Xp}\n" +
                          $"Total dropped capture points :        {DroppedCapturePoints}\n" +
                          $"\n" +
                          $"=======Calculated results========\n" +
                          $"-------Battle averages------\n" +
                          $"Average spots per battle :            {AvgSpotsPerGame}\n" +
                          $"Average frags per battle :            {AvgFragsPerGame}\n" +
                          $"Average shots per battle :            {AvgShotsPerGame}\n" +
                          $"Average hits per battle :             {AvgHitsPerGame}\n" +
                          $"Average XP per battle :               {AvgXpPerGame}\n" +
                          $"Average damage dealt per battle :     {AvgDmgDltPerGame}\n" +
                          $"Average damage received per battle :  {AvgDmgRecvdPerGame}\n" +
                          $"------Battle rates---------\n" +
                          $"Rate damage dealt/received :          {RatioDmgDltRecvd}\n" +
                          $"Rate light tanks killed :             {LightKillRate}\n" +
                          $"Rate med tanks killed :               {MediumKillRate}\n" +
                          $"Rate heavy tanks killed :             {HeavyKillRate}\n" +
                          $"Rate TD tanks killed :                {TdKillRate}\n" +
                          $"Rate of survivability :               {SurvivabilityRate}\n" +
                          $"------Battle probabilities-\n" +
                          $"Probability to win the next game :                 {Math.Round(WinRate * 100, 2)}%\n" +
                          $"Probability to hit a target :                      {Math.Round(Precision * 100, 2)}%\n" +
                          $"Probability to kill a target :                     {Math.Round(HitLethality * 100, 2)}%\n" +
                          $"Probability to hit at least 5 shots in one game :  {Math.Round(ProbabilityOfHitsEqualOrMoreThan(5) * 100, 2)}%\n" +
                          $"Probability to survive 5 or more battle in a row : {Math.Round(ProbabilityOfSurvivalEqualOrMoreThan(5) * 100, 2)}%\n" +
                          $"\n" +
                          $"--------Tanks killed (sorted by number of kills)---------\n";
            foreach (var killedTank in KilledTanks)
            {
                message += $"{killedTank.Key.Nom} => Killed {killedTank.Value} time";
                message += killedTank.Value > 1 ? "s\n" : "\n";
            }

            return message;
        }
    }
}