using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;

namespace IPMRVPark.Services
{
    public class MySqlBulkInsert
    {
        static private string ConnectionString = "server=mysql6.loosefoot.com;Database=rvpark;Uid=conestoga;Pwd=Eh6teiva;";

        public static void InsertStyleUrl(Polygons poly, long eventId)
        {
            StringBuilder sCommand = new StringBuilder("INSERT INTO styleurl( styleUrl, backgroundColor, idIPMEvent, createDate, lastUpdate ) Values");
            List<string> Rows = new List<string>();

            for (int i = 0; i < poly.Styles.Count; i++)
            {
                Rows.Add(string.Format("('{0}','{1}',{2}, curdate(), curdate() )",
                    poly.Styles[i].styleUrl, poly.Styles[i].poly_color, eventId));
            }

            sCommand.Append(string.Join(",", Rows));
            sCommand.Append(";");
            ExecuteQuery(sCommand);
        }

        public static void InsertSiteType(Polygons poly, long eventId)
        {
            StringBuilder sCommand = new StringBuilder("INSERT INTO sitetype( idIPMEvent, idSiteSize, idService, createDate, lastUpdate, idStyleUrl ) Values");
                List<string> Rows = new List<string>();

            for (int i = 0; i < poly.Types.Count; i++)
            {
                Rows.Add(string.Format("({0},{1},{2}, current_time(), current_time(), {3} )",
                    poly.Types[i].eventId, poly.Types[i].sizeId, poly.Types[i].serviceId, poly.Types[i].styleId));
            }

            sCommand.Append(string.Join(",", Rows));
            sCommand.Append(";");
            ExecuteQuery(sCommand);
        }

        public static void InsertPlaceInMap(Polygons poly, long eventId )
        {
            StringBuilder sCommand = new StringBuilder("INSERT INTO placeinmap( idIPMEvent, tag, site, idSiteType, createDate, lastUpdate, isRVSite ) Values");
            List<string> Rows = new List<string>();

            for (int i = 0; i < poly.Sites.Count; i++)
            {
                Rows.Add(string.Format("({0},'{1}','{2}',{3}, current_time(), current_time(), 1 )", 
                    poly.Sites[i].eventId, poly.Sites[i].tag, poly.Sites[i].name, poly.Sites[i].typeId ));
            }

            sCommand.Append(string.Join(",", Rows));
            sCommand.Append(";");
            ExecuteQuery(sCommand);
        }

        public static void UpdateIsRVSite_PlaceInMap(Polygons poly)
        {
            StringBuilder sCommand = new StringBuilder("Update placeinmap set isRVSite = 0 Where ID in (");
            List<string> Rows = new List<string>();

            for (int i = 0; i < poly.Sites.Count; i++)
            {
                if( !poly.Sites[i].isSite )
                    Rows.Add(string.Format("{0}",poly.Sites[i].id));
            }

            sCommand.Append(string.Join(",", Rows));
            sCommand.Append(");");
            ExecuteQuery(sCommand);
        }

        public static void InsertCoordinates(Polygons poly)
        {
            StringBuilder sCommand = new StringBuilder("INSERT INTO coordinates ( idIPMEvent, idPlaceInMap, seqCoordinate, longitude, latitude, createDate, lastUpdate ) VALUES ");
            List<string> Rows = new List<string>();

            for (int i = 0; i < poly.Coords.Count; i++)
            {
                Rows.Add(string.Format("({0},{1},{2},'{3}','{4}', current_time(), current_time() )", poly.Coords[i].eventId,
                (int)poly.Coords[i].placeId, poly.Coords[i].seqCoordinate, poly.Coords[i].y, poly.Coords[i].x));
            }

            sCommand.Append(string.Join(",", Rows));
            sCommand.Append(";");
            ExecuteQuery(sCommand);
        }

        private static void ExecuteQuery(StringBuilder sCommand)
        {
            using (MySqlConnection mConnection = new MySqlConnection(ConnectionString))
            {
                mConnection.Open();
                using (MySqlCommand myCmd = new MySqlCommand(sCommand.ToString(), mConnection))
                {
                    myCmd.CommandType = CommandType.Text;
                    myCmd.ExecuteNonQuery();
                }
            }
        }
    }
}
