/*
Copyright (c) 2010, CLARITY: Centre for Sensor Web Technologies, DCU (Dublin City University)
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of CLARITY: Centre for Sensor Web Technologies, DCU (Dublin City University) nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
*/

using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Data.Common;


namespace SenseCamBrowser1
{


    public class Event_Rep
    {
        public static string DEFAULT_IMAGE_CAPTION_TEXT = "     Free text area"; //this property is used to display default text if there is no comment associated with the event
        public static string DEFAULT_EVENT_KEYFRAME_BORDER_COLOUR = "#24000000"; //this property is used to display the default border colour for event keyframes on the main screen...



        #region Event_Rep properties which are associated with each and every event retrieved from the database

            public int event_id { get; set; }
            public DateTime start_time { get; set; }
            public DateTime end_time { get; set; }
            public string comment { get; set; }
            public string str_time { get; set; }
            public string str_keyframe_path { get; set; }            
            public ImageSource keyframe_path { get; set; }
            public string short_comment { get; set; }
            public string border_colour { get; set; }
            public string event_length { get; set; }

        #endregion Event_Rep properties which are associated with each and every event retrieved from the database




        /// <summary>
        /// constructor which initiates each and every new event loaded from the database...
        /// </summary>
        /// <param name="event_id"></param>
        /// <param name="start_time"></param>
        /// <param name="end_time"></param>
        /// <param name="keyframe_path"></param>
        /// <param name="comment"></param>
        public Event_Rep(int event_id, DateTime start_time, DateTime end_time, string keyframe_path, string comment)
        {
            this.event_id = event_id;
            this.start_time = start_time;
            this.end_time = end_time;
            this.comment = comment;
            this.str_keyframe_path = keyframe_path;
            this.keyframe_path = Image_Rep.get_image_source(keyframe_path, true);
            this.border_colour = DEFAULT_EVENT_KEYFRAME_BORDER_COLOUR;           

            //here we format the time string for proper display on the interface
            if (start_time.Hour < 12)
                this.str_time = start_time.ToShortTimeString() + " am";
            else this.str_time = start_time.ToShortTimeString() + " pm";

            update_event_short_comment(); //so part of the comment can be displayed on the main interface...
            
            TimeSpan ts_duration = this.end_time - this.start_time;
            this.event_length = Daily_Annotation_Summary.format_total_seconds_to_mins_and_seconds((int)ts_duration.TotalSeconds);
        } //end constructor for Event_Rep()




        /// <summary>
        /// This method attempts to release the memory resources used by the bitmap image class
        /// </summary>
        /// <param name="list_of_events"></param>
        public static void release_imagesource_resources_from_all_Simple_Event_Rep_items(List<Event_Rep> list_of_events)
        {
            //todo we're not releasing memory of the bitmap image correctly so this part of the application isn't working right

            //firstly make sure the image source is released ok...
            foreach (Event_Rep individual_event in list_of_events)
                individual_event.keyframe_path = null; //and hopefully this will release the system resources...

            //and now clear all the items...
            list_of_events.Clear();
        } //close method release_imagesource_resources_from_all_Simple_Image_Rep_items()...



        /// <summary>
        /// this method formats the comment string for proper display on the interface, where the interface calls the "short_comment" string...
        /// </summary>
        public void update_event_short_comment()
        {
            //below we format the comment string for proper display on the interface...
            if (!comment.Equals(""))
            {
                if (comment.Length < 15)
                    this.short_comment = comment + "...";
                else this.short_comment = comment.Substring(0, 15) + "...";
            } //close if (!comment.Equals(""))...
            else this.short_comment = "";
        } //close method update_event_short_comment()...



        public TimeSpan get_event_duration()
        {
            return end_time - start_time;
        } //close method get_event_duration()...
        



        #region retrieving event information from the database

            public static List<Event_Rep> get_list_of_day_events(int user_id, DateTime day)
            {
                //this method calls a database stored procedure to retrieve the list of events that happened in the morning of a given day
                return get_list_of_events_from_query("spGet_All_Events_In_Day", user_id, day);
            } //end method get_list_of_day_events


            public static List<Event_Rep> get_list_of_day_morning_events(int user_id, DateTime day)
            {
                //this method calls a database stored procedure to retrieve the list of events that happened in the morning of a given day
                return get_list_of_events_from_query("spGet_Morning_Events", user_id, day); //todo update for sqlite
            } //end method get_list_of_day_morning_events




            public static List<Event_Rep> get_list_of_day_afternoon_events(int user_id, DateTime day)
            {
                //this method calls a database stored procedure to retrieve the list of events that happened in the morning of a given day
                return get_list_of_events_from_query("spGet_Afternoon_Events", user_id, day); //todo update for sqlite
            } //end method get_list_of_day_morning_events




            public static List<Event_Rep> get_list_of_day_evening_events(int user_id, DateTime day)
            {
                //this method calls a database stored procedure to retrieve the list of events that happened in the morning of a given day
                return get_list_of_events_from_query("spGet_Evening_Events", user_id, day); //todo update for sqlite
            } //end method get_list_of_day_morning_events



            /// <summary>
            /// this method retrieves events from the database, depending on the stored procedure name
            /// </summary>
            /// <param name="stored_procedure_name"></param>
            /// <param name="user_id"></param>
            /// <param name="day"></param>
            /// <returns></returns>
            private static List<Event_Rep> get_list_of_events_from_query(string stored_procedure_name, int user_id, DateTime day)
            {
                //this method calls the relevant database stored procedure to retrieve a list of events
                List<Event_Rep> list_of_events = new List<Event_Rep>();

                SQLiteConnection con = new SQLiteConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);
                SQLiteCommand command = new SQLiteCommand(con);
                con.Open();
                command.CommandText = Database_Versioning.text_for_stored_procedures.spGet_All_Events_In_Day(user_id, day);
                SQLiteDataReader read_events = command.ExecuteReader();

                int event_id;
                DateTime start_time, end_time;
                string keyframe_path, comment; //values that just allow me to store individual database values before storing them in an object of type Event_Rep

                while (read_events.Read())
                {
                    try { event_id = int.Parse(read_events[0].ToString()); }
                    catch (Exception excep) { event_id = -1; }
                    start_time = (DateTime)read_events[1];
                    end_time = (DateTime)read_events[2];
                    keyframe_path = read_events[3].ToString();
                    comment = read_events[4].ToString();

                    list_of_events.Add(new Event_Rep(event_id, start_time, end_time, keyframe_path, comment));
                } //end while (read_chunk_ids.Read())
                con.Close();

                return list_of_events;
            } //end method get_list_of_events_from_query()




            /// <summary>
            /// this method is responsible for updating any comments associated with this event...
            /// </summary>
            /// <param name="user_id"></param>
            /// <param name="event_id"></param>
            /// <param name="comment"></param>
            public static void update_event_comment(int user_id, int event_id, string comment)
            {
                //THIS METHOD IS RESPONSIBLE FOR UPDATING AN EVENT'S COMMENT FIELD
                SQLiteConnection con = new SQLiteConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);
                SQLiteCommand selectCmd = new SQLiteCommand(Database_Versioning.text_for_stored_procedures.spUpdateEventComment(user_id, event_id, comment), con);
                con.Open();
                selectCmd.ExecuteNonQuery();
                con.Close();
            } //end method update_event_comment()




            /// <summary>
            /// this method is responsible for updating any comments associated with this event...
            /// </summary>
            /// <param name="user_id"></param>
            /// <param name="event_id"></param>
            /// <param name="comment"></param>
            public static void update_event_as_being_viewed_another_time(int user_id, int event_id)
            {
                //THIS METHOD IS RESPONSIBLE FOR UPDATING AN EVENT'S COMMENT FIELD
                SQLiteConnection con = new SQLiteConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);
                SQLiteCommand selectCmd = new SQLiteCommand(Database_Versioning.text_for_stored_procedures.feb10_spUpdateEvent_Number_Times_Viewed(user_id, event_id), con);
                con.Open();
                selectCmd.ExecuteNonQuery();
                con.Close();
            } //end method update_event_comment()




            /// <summary>
            /// this method updates the keyframe in an event
            /// </summary>
            /// <param name="user_id"></param>
            /// <param name="event_id"></param>
            /// <param name="new_keyframe_path"></param>
            public static void update_event_keyframe_path(int user_id, int event_id, string new_keyframe_path)
            {
                //this method calls a database stored procedure to update the keyframe path of the event in question
                SQLiteConnection con = new SQLiteConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);
                SQLiteCommand selectCmd = new SQLiteCommand(Database_Versioning.text_for_stored_procedures.spUpdate_Event_Keyframe_Path(user_id,event_id,new_keyframe_path), con);
                con.Open();
                selectCmd.ExecuteNonQuery();
                con.Close();
            } //end method update_event_keyframe_path()




            /// <summary>
            /// this method deletes an event from the database
            /// </summary>
            /// <param name="user_id"></param>
            /// <param name="event_id"></param>
            public static void delete_event_from_database(int user_id, int event_id)
            {
                //this method calls a database stored procedure to delete this event from the database
                SQLiteConnection con = new SQLiteConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);
                SQLiteCommand selectCmd = new SQLiteCommand(Database_Versioning.text_for_stored_procedures.spDelete_Event(user_id,event_id), con);
                con.Open();
                selectCmd.ExecuteNonQuery();
                con.Close();
            } //end method delete_event_from_database()




            /// <summary>
            /// This method sends all the images in an event before a certain time, to the previous event (if there is no previous event in the day, it'll create a new one)
            /// </summary>
            /// <param name="user_id"></param>
            /// <param name="event_id_of_source_images"></param>
            /// <param name="time_of_end_image"></param>
            public static void send_images_to_previous_event(int user_id, int event_id_of_source_images, DateTime time_of_end_image)
            {
                //step 1, identify the ID of the previous event...
                //step 2, update the All_Images table, to change the given images in the event_id_source_of_new_images to the ID of their new event
                //step 3, update the start/end time of the two events in question...
                //step 4, update the keyframe path of the events in question...
                
                //all the above steps are covered by the stored procedure below...
                SQLiteConnection con = new SQLiteConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);
                SQLiteCommand selectCmd = new SQLiteCommand(Database_Versioning.text_for_stored_procedures.Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT(),con);//todo write SQL syntax! "Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT", con);
                con.Open();
                selectCmd.ExecuteNonQuery();
                con.Close();
            } //close method send_images_to_previous_event()...


            /// <summary>
            /// This method sends all the images in an event after a certain time, to the next event (if there is no next event in the day, it'll create a new one)
            /// </summary>
            /// <param name="user_id"></param>
            /// <param name="event_id_of_source_images"></param>
            /// <param name="time_of_start_image"></param>
            public static void send_images_to_next_event(int user_id, int event_id_of_source_images, DateTime time_of_start_image)
            {
                //step 1, identify the ID of the previous event...
                //step 2, update the All_Images table, to change the given images in the event_id_source_of_new_images to the ID of their new event
                //step 3, update the start/end time of the two events in question...
                //step 4, update the keyframe path of the events in question...

                //all the above steps are covered by the stored procedure below...
                SQLiteConnection con = new SQLiteConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);
                SQLiteCommand selectCmd = new SQLiteCommand(Database_Versioning.text_for_stored_procedures.Oct10_ADD_NEW_MERGED_IMAGES_TO_NEXT_EVENT(),con);//todo write SQL syntax! "Oct10_ADD_NEW_MERGED_IMAGES_TO_NEXT_EVENT", con);
                con.Open();
                selectCmd.ExecuteNonQuery();
                con.Close();
            } //close method send_images_to_next_event()...


            /// <summary>
            /// This method an event into two, with the new event starting with the image at the supplied parameter time
            /// </summary>
            /// <param name="user_id"></param>
            /// <param name="event_id_of_source_images"></param>
            /// <param name="time_of_start_image"></param>
            public static int split_event_into_two(int user_id, int event_id_of_source_images, DateTime time_of_start_image)
            {
                int new_event_id = -1; //anja updates

                //step 1, identify the ID of the previous event...
                //step 2, update the All_Images table, to change the given images in the event_id_source_of_new_images to the ID of their new event
                //step 3, update the start/end time of the two events in question...
                //step 4, update the keyframe path of the events in question...

                //all the above steps are covered by the stored procedure below...
                SQLiteConnection con = new SQLiteConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);
                SQLiteCommand selectCmd = new SQLiteCommand(Database_Versioning.text_for_stored_procedures.Jan11_SPLIT_EVENT_INTO_TWO(),con);//todo write stored procedure code! "Jan11_SPLIT_EVENT_INTO_TWO", con); //anja updates, this stored procedure is now updated to return the ID of the newly created event...                
                con.Open();
                try
                {
                    new_event_id = int.Parse(selectCmd.ExecuteScalar().ToString()); //Anja updates used to be executenonquery()...
                }
                catch (Exception excep) { selectCmd.ExecuteNonQuery(); new_event_id = event_id_of_source_images; } //Anja updates i.e. the code that used to be here before try...catch }
                con.Close();

                return new_event_id; //anja updates...
            } //close method split_event_into_two()...




        #endregion end of region retrieving event information from the database


             


    } //end class...

} //end namespace...
