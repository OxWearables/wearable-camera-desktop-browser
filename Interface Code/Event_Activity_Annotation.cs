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

        public int eventIndex { get; set; }
        public int eventID { get; set; }
        public string annotation { get; set; }
        public int durationSeconds { get; set; }
        public string durationStr { get; set; }


        public Event_Activity_Annotation(int event_index, int eventID, string annotation, int duration_in_seconds)
        {
            this.eventIndex = event_index;
            this.eventID = eventID;
            this.annotation = annotation;
            this.durationSeconds = duration_in_seconds;
            this.durationStr = Daily_Annotation_Summary.format_total_seconds_to_mins_and_seconds(duration_in_seconds);
        } //close constructor method Event_Activity_Annotation()...



        /// <summary>
        /// this method returns a list of annotated events for a given user and day (helps us highlight them on the interface)...
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        public static List<Event_Activity_Annotation> get_list_of_annotated_events_in_day(int userID, DateTime day)
        {
            //this method calls the relevant database stored procedure to retrieve a list of annotationed events for this day...
            List<Event_Activity_Annotation> list_of_annotated_events = new List<Event_Activity_Annotation>();

            SQLiteConnection con = new SQLiteConnection(global::SenseCamBrowser1.Properties.Settings.Default.DBConnectionString);
            SQLiteCommand selectCmd = new SQLiteCommand(Database_Versioning.text_for_stored_procedures.spGet_annotated_events_in_day(userID,day), con);
            
            int event_index_counter = 1;
            int eventID;
            string annotation_type;
            int activity_total_number_of_seconds;

            //then open the db connection, connect to the stored procedure and return the list of results...
            con.Open();
            SQLiteDataReader read_events = selectCmd.ExecuteReader();
            while (read_events.Read())
            {
                eventID = int.Parse(read_events[0].ToString());
                annotation_type = read_events[1].ToString();
                activity_total_number_of_seconds = int.Parse(read_events[2].ToString());

                list_of_annotated_events.Add(new Event_Activity_Annotation(event_index_counter, eventID, annotation_type, activity_total_number_of_seconds));
                event_index_counter++;
            } //end while (read_chunk_ids.Read())...
            con.Close();

            return list_of_annotated_events; //and finally return the list of annotated events...
        } //close method get_list_of_annotated_events_in_day()...




    } //close class...

} //close namespace...
