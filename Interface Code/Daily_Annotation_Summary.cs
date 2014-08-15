using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.SQLite;
using System.Data.Common;
using System.IO;

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


        public static void writeAllAnnotationsToCsv(int userID, string csvFile)
        {
            //write a participant's annotations for a given day to csv output file
            string header = "participant,startTime,endTime,source,annotation";
            TextWriter fWriter = new StreamWriter(csvFile);
            string query =
                    Database_Versioning.text_for_stored_procedures.spGetAnnotationSummary(
                    userID);
            SQLiteConnection con = new SQLiteConnection(DbString);
            SQLiteCommand selectCmd = new SQLiteCommand(query, con);
            con.Open();
            SQLiteDataReader dbAnnotates = selectCmd.ExecuteReader();
            fWriter.WriteLine(header);
            DateTime previousEndTime = new DateTime();
            int counter = 0;
            while (dbAnnotates.Read())
            {
                string participant = dbAnnotates[0].ToString();
                DateTime startTime = DateTime.Parse(dbAnnotates[1].ToString());
                DateTime endTime = DateTime.Parse(dbAnnotates[2].ToString());
                string annotation = dbAnnotates[3].ToString();

                //highlight (potential) nonWear gaps between annotations
                if (counter == 0) {
                    previousEndTime = startTime;
                }
                TimeSpan gapToPreviousAnnotation = startTime - previousEndTime;
                if (gapToPreviousAnnotation.TotalMinutes >
                        Upload_Images_and_Segment_into_Events.Upload_Manipulated_Sensor_Data.MAXIMUM_NUMBER_MINUTES_BETWEEN_IMAGES_ALLOWED_TO_STAY_IN_THE_SAME_EVENT
                    )
                {
                    //write nonwear line
                    fWriter.WriteLine(participant + ","
                        + previousEndTime.AddSeconds(1) + ","
                        + startTime.AddSeconds(-1) + ",nonWear, <unknown>");
                }
                previousEndTime = endTime;
                counter++;

                //write current event annotation to file
                fWriter.WriteLine(participant + "," + startTime + "," + endTime
                            + ",images," + annotation.Replace(",","-"));
            }
            con.Close();
            fWriter.Close();
        }

    }

}
