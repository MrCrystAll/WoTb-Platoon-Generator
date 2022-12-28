using System.Data;
using System.Data.SqlClient;

namespace WotGenC.Database
{
    public class DbTank
    {
        public Tank Tank;

        public DbTank(Tank tank)
        {
            Tank = tank;
        }

        public bool WriteToDb()
        {
            SqlConnection cnn = new SqlConnection(Settings.ConnectionString);
            cnn.Open();
            
            if (cnn.State != ConnectionState.Open) return false;

            string newNom = Tank.Nom;

            if (Tank.Nom.Contains("'"))
            {
                newNom = Tank.Nom.Replace("'", "''");
            }

            SqlCommand command = new SqlCommand($"INSERT INTO Tank VALUES ({Tank.Id}, '{newNom}','{(int)Tank.Te}')", cnn);
            
            SqlCommand queryCheck = new SqlCommand($"SELECT * FROM Tank WHERE IDt = {Tank.Id}", cnn);

            var reader = queryCheck.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Close();
                cnn.Close();
                return false;
            }

            reader.Close();
            command.ExecuteNonQuery();
            cnn.Close();
            return true;
        }
    }
}