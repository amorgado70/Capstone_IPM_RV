using System;
using System.Text;
using MySql.Data.MySqlClient;

namespace IPMRVPark.Services
{
    public class MySqlReader
    {
        static private string ConnectionString = "server=mysql6.loosefoot.com;Database=rvpark;Uid=conestoga;Pwd=Eh6teiva;";

        public static void GetSiteUpdate(DateTime lastUpdate)
        {
            StringBuilder sCommand = new StringBuilder("Select idRVSite, ISNULL(isCancelled) as cancel, timeStamp From reservationitem Where timeStamp > '");
            sCommand.Append(lastUpdate.ToString("yyyy-MM-dd HH:mm:ss"));
            sCommand.Append("'");

            try
            {
                using (MySqlConnection mConnection = new MySqlConnection(ConnectionString))
                {
                    mConnection.Open();
                    using (MySqlCommand myCmd = new MySqlCommand(sCommand.ToString(), mConnection))
                    {
                        MySqlDataReader rdr = myCmd.ExecuteReader();

                        while (rdr.Read())
                        {
                            Console.WriteLine(rdr[0] + "," + rdr[1] + "," + rdr[2]);
                            string ln = string.Format(" {0}, {1}, {2} ", rdr[0], rdr[1], rdr[2]);
                            Console.WriteLine(ln);
                        }
                        rdr.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}