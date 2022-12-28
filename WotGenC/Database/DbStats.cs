using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace WotGenC.Database
{
    public class DbStats
    {
        public Stats Stats;

        public DbStats(Stats stats)
        {
            Stats = stats;
        }

        public bool WriteToDb()
        {
            SqlConnection cnn = new SqlConnection(Settings.ConnectionString);
            cnn.Open();

            if (cnn.State != ConnectionState.Open) return false;

            SqlCommand command = new SqlCommand(
                $"INSERT INTO Stats (IDp, IDt, Spotted, Hits, Frags, NumberOfBattles,Wins ,Losses, MaxXp1B, TotalDmgDlt, TotalDmgRecvd, MaxFrags1B, TotalShots, Xp, WinAndSurvived, SurvivedBattles, DroppedCapturePoints, Date) VALUES ({Stats.PlayerId},'{Stats.TankId}', '{Stats.Spotted}', '{Stats.Hits}', '{Stats.Frags}', '{Stats.NumberOfBattles}', '{Stats.Wins}', '{Stats.Losses}', '{Stats.MaxXp1B}', '{Stats.TotalDmgDlt}', '{Stats.TotalDmgRecvd}', '{Stats.MaxFrags1B}', '{Stats.TotalShots}', '{Stats.Xp}', '{Stats.WinAndSurvived}', '{Stats.SurvivedBattles}','{Stats.DroppedCapturePoints}', '{new SqlDateTime(DateTime.Now)}')", cnn);

            command.ExecuteNonQuery();

            SqlCommand GetStatsKey =
                new SqlCommand(
                    $"SELECT Ids FROM Stats WHERE Date = (SELECT MAX(Date) FROM Stats WHERE IDp = {Stats.PlayerId} AND IDt = {Stats.TankId})", cnn);

            var reader = GetStatsKey.ExecuteReader();
            reader.Read();

            int ids = (int)reader[0];
            reader.Close();
            

            foreach (var statsKilledTank in Stats.KilledTanks)
            {
                if(statsKilledTank.Key.Nom == "Unknown") continue;
                command = new SqlCommand(
                    $"INSERT INTO KilledTanks(Ids, Idt, NbKills) VALUES ({ids},{int.Parse(statsKilledTank.Key.Id)},{statsKilledTank.Value})", cnn);
                command.ExecuteNonQuery();
            }
            cnn.Close();

            return true;
        }

        public static Stats? GetFromDb(int playerId, int tankId)
        {
            SqlConnection cnn = new SqlConnection(Settings.ConnectionString);
            cnn.Open();

            SqlCommand command =
                new SqlCommand($"SELECT * FROM Stats WHERE IDp = '{playerId}' AND IDt = '{tankId}'", cnn);

            var reader = command.ExecuteReader();
            if (!reader.HasRows) return null;
            reader.Read();
            return new Stats(
                playerId: (int)reader[0],
                tankId: (int)reader[1],
                spotted: (int)reader[2],
                hits: (int)reader[3],
                frags: (int)reader[4],
                numberOfBattles: (int)reader[5],
                wins: (int)reader[6],
                losses: (int)reader[7],
                maxFrags1B: (int)reader[11],
                maxXp1B: (int)reader[8],
                totalDmgDlt: (int)reader[9],
                totalDmgRecvd: (int)reader[10],
                totalShots: (int)reader[12],
                xp: (float)(double)reader[13],
                winAndSurvived: (int)reader[14],
                survivedBattles: (int)reader[15],
                droppedCapturePoints: (int)reader[16],
                killedTanks: new Dictionary<Tank, uint>()
                );

        }
    }
}