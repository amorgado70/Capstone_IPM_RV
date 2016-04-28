using System;
using System.Text;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace IPMRVPark.Services
{
    public class _reserve_selection
    {
        public enum _type { reservation, selection };
        public long id;
        public _type type;
        public bool removed;
        public DateTime lastUpdate;
        public string lastUpdateString;
    }


    public class MySqlReader
    {
        static private string ConnectionString = "server=mysql6.loosefoot.com;Database=rvpark;Uid=conestoga;Pwd=Eh6teiva;";

        public static void GetSiteUpdate(DateTime lastUpdate, out List<_reserve_selection> updates)
        {
            updates = new List<_reserve_selection>();

           
            StringBuilder sCommand = new StringBuilder("Select idRVSite, cast( coalesce(isCancelled,0,1) as UNSIGNED) as cancel, timeStamp From reservationitem Where timeStamp > '");
            //StringBuilder sCommand = new StringBuilder("Select idRVSite, cast( isCancelled as UNSIGNED) as cancel, timeStamp From reservationitem Where timeStamp > '");
            sCommand.Append(lastUpdate.ToString("yyyy-MM-dd HH:mm:ss"));
            sCommand.Append("'");


            //StringBuilder sCommand2 = new StringBuilder("Select idRVSite, cast( coalesce(isSiteChecked,0,1) as UNSIGNED) as cancel, timeStamp From selecteditem Where timeStamp > '");
            StringBuilder sCommand2 = new StringBuilder("Select idRVSite, cast( isSiteChecked as UNSIGNED) as cancel, timeStamp From selecteditem Where timeStamp > '");
            sCommand2.Append(lastUpdate.ToString("yyyy-MM-dd HH:mm:ss"));
            sCommand2.Append("'");

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
                            string ln = string.Format(" {0}, {1}, {2} ", rdr[0], rdr[1], rdr[2]);

                            _reserve_selection r = new _reserve_selection();
                            long.TryParse( rdr[0].ToString(), out r.id );
                            r.type = _reserve_selection._type.reservation;
                            int cancel;
                            int.TryParse(rdr[1].ToString(), out cancel);
                            r.removed = cancel == 1;

                            r.lastUpdateString = rdr[2].ToString();
                            DateTime.TryParse(r.lastUpdateString, out r.lastUpdate);
                            updates.Add(r);
                        }
                        rdr.Close();
                    }
                    mConnection.Close();

                    mConnection.Open();
                    using (MySqlCommand myCmd = new MySqlCommand(sCommand2.ToString(), mConnection))
                    {
                        MySqlDataReader rdr = myCmd.ExecuteReader();

                        while (rdr.Read())
                        {
                            string ln = string.Format(" {0}, {1}, {2} ", rdr[0], rdr[1], rdr[2]);

                            _reserve_selection r = new _reserve_selection();
                            long.TryParse(rdr[0].ToString(), out r.id);
                            r.type = _reserve_selection._type.selection;
                            int cancel;
                            int.TryParse(rdr[1].ToString(), out cancel);
                            r.removed = cancel == 0;

                            r.lastUpdateString = rdr[2].ToString();
                            DateTime.TryParse(r.lastUpdateString, out r.lastUpdate);
                            updates.Add(r);
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
