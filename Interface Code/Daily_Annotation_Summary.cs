using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.SQLite;
using System.Data.Common;

namespace SenseCamBrowser1
{
    class Daily_Annotation_Summary
    {

        public string annType { get; set; }
        public string dayDurationStr { get; set; }
        public int dayNumSeconds { get; set; }
        private static string DbString =
            global::SenseCamBrowser1.Properties.Settings.Default.DBConnectionString;
        
        public Daily_Annotation_Summary(string annType, int numSeconds)
        {
            this.annType = annType;
            this.dayNumSeconds = numSeconds;
            this.dayDurationStr = SecondsToString(numSeconds);
        }


        public static string SecondsToString(int totalSeconds)
        {
            int numMins = totalSeconds / 60;
            return numMins + "m " + (totalSeconds - (numMins * 60)) + "s";
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        public static List<Daily_Annotation_Summary> getDayAnnotationSummary(
            int userID,
            DateTime day)
        {
            //Returns a list of all activities (+ duration) annotated in a day            
            List<Daily_Annotation_Summary> annList = new List<Daily_Annotation_Summary>();

            //annotation properties
            string annType;
            int DurationSec;

            string query = 
                Database_Versioning.text_for_stored_procedures.spGet_daily_activity_summary_from_annotations(
                userID,
                day);
            SQLiteConnection con = new SQLiteConnection(DbString);
            SQLiteCommand selectCmd = new SQLiteCommand(query, con);
            con.Open();
            SQLiteDataReader readAnnotations = selectCmd.ExecuteReader();
            while (readAnnotations.Read())
            {
                annType = readAnnotations[0].ToString();
                DurationSec = int.Parse(readAnnotations[1].ToString());
                annList.Add(new Daily_Annotation_Summary(annType, DurationSec));
            }
            con.Close();
            return annList;
        }

        
        public static List<int> ActivityEventIds(
            int userID,
            DateTime day,
            string annotation_type)
        {
            //Get list of events in a day belonging to activity/annotation
            List<int> idList = new List<int>();
            string query = 
                Database_Versioning.text_for_stored_procedures.spGet_event_ids_in_day_for_specific_activity(
                userID,
                day,
                annotation_type);
            SQLiteConnection con = new SQLiteConnection(DbString);
            SQLiteCommand selectCmd = new SQLiteCommand(query, con);
            con.Open();
            SQLiteDataReader readEvents = selectCmd.ExecuteReader();
            while (readEvents.Read())
            {
                idList.Add(int.Parse(readEvents[0].ToString()));                
            }
            con.Close();
            return idList;
        }

    }

}
