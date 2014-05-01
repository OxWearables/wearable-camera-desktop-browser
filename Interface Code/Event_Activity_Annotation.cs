using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.SQLite;
using System.Data.Common;

namespace SenseCamBrowser1.Interface_Code
{
    class Event_Activity_Annotation
    {
        private static string DbString = 
            global::SenseCamBrowser1.Properties.Settings.Default.DBConnectionString;

        public int eventIndex { get; set; }
        public int eventID { get; set; }
        public string annotation { get; set; }
        public int durationSeconds { get; set; }
        public string durationStr { get; set; }

        public Event_Activity_Annotation(
            int eventIndex,
            int eventID,
            string annotation,
            int durationSec)
        {
            this.eventIndex = eventIndex;
            this.eventID = eventID;
            this.annotation = annotation;
            this.durationSeconds = durationSec;
            this.durationStr = 
                Daily_Annotation_Summary.format_total_seconds_to_mins_and_seconds(
                durationSec);
        }


        public static List<Event_Activity_Annotation> getAnnotatedEventsDay(
            int userID,
            DateTime day)
        {
            //Get list of annotated events for this day...
            List<Event_Activity_Annotation> annotationList 
                = new List<Event_Activity_Annotation>();
            
            //annotation properties
            int ixCounter = 1;
            int eventID;
            string annType;
            int durationSec;

            string query = 
                Database_Versioning.text_for_stored_procedures.spGet_annotated_events_in_day(
                userID,
                day);
            SQLiteConnection con = new SQLiteConnection(DbString);
            SQLiteCommand selectCmd = new SQLiteCommand(query, con);
            con.Open();
            SQLiteDataReader readAnnotations = selectCmd.ExecuteReader();
            while (readAnnotations.Read())
            {
                eventID = int.Parse(readAnnotations[0].ToString());
                annType = readAnnotations[1].ToString();
                durationSec = int.Parse(readAnnotations[2].ToString());

                annotationList.Add(new Event_Activity_Annotation(
                    ixCounter,
                    eventID,
                    annType,
                    durationSec));
                ixCounter++;
            }
            con.Close();
            return annotationList;
        }

    }
}
