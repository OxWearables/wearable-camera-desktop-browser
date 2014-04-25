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
        
        public Daily_Annotation_Summary(string annType, int numSeconds)
        {
            this.annType = annType;
            this.dayNumSeconds = numSeconds;
            this.dayDurationStr = format_total_seconds_to_mins_and_seconds(numSeconds);
        } //close constructor...


        /// <summary>
        /// this method converts total number of seconds into minutes and seconds...
        /// </summary>
        /// <param name="total_number_of_seconds"></param>
        /// <returns></returns>
        public static string format_total_seconds_to_mins_and_seconds(int total_number_of_seconds)
        {
            int num_minutes = total_number_of_seconds / 60;

            return num_minutes + "m " + (total_number_of_seconds - (num_minutes * 60)) + "s";
        } //close method format_total_seconds_to_mins_and_seconds()...





        /// <summary>
        /// this method returns a list of all the activities annotated for in the given day, plus their duration...
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        public static List<Daily_Annotation_Summary> get_daily_activity_summary_from_annotations(int userID, DateTime day)
        {
            //this method calls the relevant database stored procedure to retrieve a list of annotations already associated with this event...
            List<Daily_Annotation_Summary> list_of_annotations = new List<Daily_Annotation_Summary>();
            SQLiteConnection con = new SQLiteConnection(global::SenseCamBrowser1.Properties.Settings.Default.DBConnectionString);
            SQLiteCommand selectCmd = new SQLiteCommand(Database_Versioning.text_for_stored_procedures.spGet_daily_activity_summary_from_annotations(userID, day), con);
            
            string annotation_type;
            int activity_total_number_of_seconds;

            //then open the db connection, connect to the stored procedure and return the list of results...
            con.Open();
            SQLiteDataReader read_events = selectCmd.ExecuteReader();
            while (read_events.Read())
            {
                annotation_type = read_events[0].ToString();
                activity_total_number_of_seconds = int.Parse(read_events[1].ToString());

                list_of_annotations.Add(new Daily_Annotation_Summary(annotation_type, activity_total_number_of_seconds));
            } //end while (read_chunk_ids.Read())...
            con.Close();

            return list_of_annotations; //and finally return the annotations for this event...
        } //close method get_daily_activity_summary_from_annotations()...

        


        /// <summary>
        /// this method retrieves a list of events in a day that belong to a certain activity/annotation type/class
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="day"></param>
        /// <param name="annotation_type"></param>
        /// <returns></returns>
        public static List<int> get_list_of_eventIDs_to_highlight_for_annotation_type(int userID, DateTime day, string annotation_type)
        {
            //this method calls the relevant database stored procedure to retrieve a list of annotations already associated with this event...
            List<int> list_of_eventIDs = new List<int>();
            SQLiteConnection con = new SQLiteConnection(global::SenseCamBrowser1.Properties.Settings.Default.DBConnectionString);
            SQLiteCommand selectCmd = new SQLiteCommand(Database_Versioning.text_for_stored_procedures.spGet_event_ids_in_day_for_specific_activity(userID, day, annotation_type), con);
            
            //then open the db connection, connect to the stored procedure and return the list of results...
            con.Open();
            SQLiteDataReader read_events = selectCmd.ExecuteReader();
            while (read_events.Read())
            {
                list_of_eventIDs.Add(int.Parse(read_events[0].ToString()));                
            } //end while (read_chunk_ids.Read())...
            con.Close();

            return list_of_eventIDs; //and finally return the event ids which belong to this type of activity...
        } //close method get_daily_activity_summary_from_annotations()...




    } //close class Daily_Annotation_Summary...

} //close namespace...
