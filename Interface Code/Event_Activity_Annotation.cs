using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace SenseCamBrowser1.Interface_Code
{
    class Event_Activity_Annotation
    {

        public int event_index { get; set; }
        public int event_id { get; set; }
        public string annotation { get; set; }
        public int duration_in_seconds { get; set; }
        public string duration_string { get; set; }


        public Event_Activity_Annotation(int event_index, int event_id, string annotation, int duration_in_seconds)
        {
            this.event_index = event_index;
            this.event_id = event_id;
            this.annotation = annotation;
            this.duration_in_seconds = duration_in_seconds;
            this.duration_string = Daily_Annotation_Summary.format_total_seconds_to_mins_and_seconds(duration_in_seconds);
        } //close constructor method Event_Activity_Annotation()...



        /// <summary>
        /// this method returns a list of annotated events for a given user and day (helps us highlight them on the interface)...
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        public static List<Event_Activity_Annotation> get_list_of_annotated_events_in_day(int user_id, DateTime day)
        {
            //this method calls the relevant database stored procedure to retrieve a list of annotationed events for this day...
            List<Event_Activity_Annotation> list_of_annotated_events = new List<Event_Activity_Annotation>();

            SqlConnection con = new SqlConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);
            SqlCommand selectCmd = new SqlCommand("JAN11_GET_ANNOTATED_EVENTS_IN_DAY", con);
            selectCmd.CommandType = CommandType.StoredProcedure;
            selectCmd.Parameters.Add("@USER_ID", SqlDbType.Int).Value = user_id;
            selectCmd.Parameters.Add("@DAY", SqlDbType.DateTime).Value = day;

            int event_index_counter = 1;
            int event_id;
            string annotation_type;
            int activity_total_number_of_seconds;

            //then open the db connection, connect to the stored procedure and return the list of results...
            con.Open();
            SqlDataReader read_events = selectCmd.ExecuteReader();
            while (read_events.Read())
            {
                event_id = int.Parse(read_events[0].ToString());
                annotation_type = read_events[1].ToString();
                activity_total_number_of_seconds = int.Parse(read_events[2].ToString());

                list_of_annotated_events.Add(new Event_Activity_Annotation(event_index_counter, event_id, annotation_type, activity_total_number_of_seconds));
                event_index_counter++;
            } //end while (read_chunk_ids.Read())...
            con.Close();

            return list_of_annotated_events; //and finally return the list of annotated events...
        } //close method get_list_of_annotated_events_in_day()...




    } //close class...

} //close namespace...
