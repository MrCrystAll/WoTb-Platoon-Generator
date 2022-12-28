using System.Data;
using System.Data.SqlClient;

namespace WotGenC.Database
{
    public class DbPlayer
    {
        public Player Player;

        public DbPlayer(Player player)
        {
            Player = player;
        }

        public bool WriteToDb()
        {
            SqlConnection cnn = new SqlConnection(Settings.ConnectionString);
            cnn.Open();
            
            if (cnn.State != ConnectionState.Open) return false;

            SqlCommand queryCheck = new SqlCommand($"SELECT * FROM Player WHERE ID = {Player.Id}", cnn);

            var reader = queryCheck.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Close();
                cnn.Close();
                return false;
            }
            
            reader.Close();
            
            SqlCommand command = new SqlCommand($"INSERT INTO Player VALUES ({int.Parse(Player.Id)},'{Player.Pseudo}','{Player.Token}')", cnn);

            command.ExecuteNonQuery();
            
            cnn.Close();
            return true;
        }
    }
}