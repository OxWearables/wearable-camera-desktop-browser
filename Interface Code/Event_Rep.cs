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
                SQLiteCommand selectCmd = new SQLiteCommand(Database_Versioning.text_for_stored_procedures.spUpdateEvent_Number_Times_Viewed(user_id, event_id), con);
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
                SQLiteConnection con = new SQLiteConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);
                SQLiteCommand command = new SQLiteCommand(con);
                con.Open();

                //firstly get the day of the source event...
                command.CommandText = Database_Versioning.text_for_stored_procedures.spGet_day_of_source_event(user_id, event_id_of_source_images);
                DateTime day_of_source_event = DateTime.Parse(command.ExecuteScalar().ToString());

                //then get the id of the previous event (which to append images to)
                command.CommandText = Database_Versioning.text_for_stored_procedures.spGet_id_of_event_before_ID_and_time(user_id, event_id_of_source_images, time_of_end_image, day_of_source_event);
                int new_event_id = -1;
                try { new_event_id = int.Parse(command.ExecuteScalar().ToString()); } catch (Exception excep) { }

                //if no previous event exists, we create a new event
                if (new_event_id < 0)
                {
                    //then create and get the id of a new event where the split images will be sent to...
                    command.CommandText = Database_Versioning.text_for_stored_procedures.spCreate_new_event_and_return_its_ID(user_id, day_of_source_event);
                    new_event_id = int.Parse(command.ExecuteScalar().ToString());
                } //close if (new_event_id < 0)...

                //then update image and sensor tables transferring relevant images from original event id to new/previous event id...
                command.CommandText = Database_Versioning.text_for_stored_procedures.spUpdate_image_sensors_tables_with_new_event_id_before_target_time(user_id, new_event_id, event_id_of_source_images, time_of_end_image);
                command.ExecuteNonQuery();

                //update the event information (start time, end time, keyframe path) of the newly created event...
                update_event_information_in_database(user_id, new_event_id);

                //now update the event information of what is left of the old event
                update_event_information_in_database(user_id, event_id_of_source_images);

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
                SQLiteConnection con = new SQLiteConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);
                SQLiteCommand command = new SQLiteCommand(con);
                con.Open();

                //firstly get the day of the source event...
                command.CommandText = Database_Versioning.text_for_stored_procedures.spGet_day_of_source_event(user_id, event_id_of_source_images);
                DateTime day_of_source_event = DateTime.Parse(command.ExecuteScalar().ToString());

                //then get the id of the next event (which to append images to)
                command.CommandText = Database_Versioning.text_for_stored_procedures.spGet_id_of_event_after_ID_and_time(user_id, event_id_of_source_images, time_of_start_image, day_of_source_event);
                int new_event_id = -1;
                try { new_event_id = int.Parse(command.ExecuteScalar().ToString()); }
                catch (Exception excep) { }

                //if no next event exists, we create a new event
                if (new_event_id < 0)
                {
                    //then create and get the id of a new event where the split images will be sent to...
                    command.CommandText = Database_Versioning.text_for_stored_procedures.spCreate_new_event_and_return_its_ID(user_id, day_of_source_event);
                    new_event_id = int.Parse(command.ExecuteScalar().ToString());
                } //close if (new_event_id < 0)...

                //then update image and sensor tables transferring relevant images from original event id to new/previous event id...
                command.CommandText = Database_Versioning.text_for_stored_procedures.spUpdate_image_sensors_tables_with_new_event_id_after_target_time(user_id, new_event_id, event_id_of_source_images, time_of_start_image);
                command.ExecuteNonQuery();

                //update the event information (start time, end time, keyframe path) of the newly created event...
                update_event_information_in_database(user_id, new_event_id);

                //now update the event information of what is left of the old event
                update_event_information_in_database(user_id, event_id_of_source_images);

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
                SQLiteConnection con = new SQLiteConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);
                SQLiteCommand command = new SQLiteCommand(con);
                con.Open();

                //firstly get the day of the source event...
                command.CommandText = Database_Versioning.text_for_stored_procedures.spGet_day_of_source_event(user_id, event_id_of_source_images);
                DateTime day_of_source_event = DateTime.Parse(command.ExecuteScalar().ToString());

                //then create and get the id of a new event where the split images will be sent to...
                command.CommandText = Database_Versioning.text_for_stored_procedures.spCreate_new_event_and_return_its_ID(user_id, day_of_source_event);
                int new_event_id = int.Parse(command.ExecuteScalar().ToString());

                //then update image and sensor tables with the new event id...
                command.CommandText = Database_Versioning.text_for_stored_procedures.spUpdate_image_sensors_tables_with_new_event_id_after_target_time(user_id, new_event_id, event_id_of_source_images, time_of_start_image);
                command.ExecuteNonQuery();

                //update the event information (start time, end time, keyframe path) of the newly created event...
                update_event_information_in_database(user_id, new_event_id);                

                //now update the event information of what is left of the old event
                update_event_information_in_database(user_id, event_id_of_source_images);

                con.Close();                                
                return new_event_id;
            } //close method split_event_into_two()...


            public static void update_event_information_in_database(int user_id, int event_id)
            {
                //update the event information (start time, end time, keyframe path) of any event (presuming the images/sensors tables have just been updated for some reason)...

                SQLiteConnection con = new SQLiteConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);
                SQLiteCommand command = new SQLiteCommand(con);
                con.Open();

                //firstly check if there are now any images left in the event...
                command.CommandText = Database_Versioning.text_for_stored_procedures.spGet_Num_Images_In_Event(user_id, event_id);
                int num_images_in_event = int.Parse(command.ExecuteScalar().ToString());

                //if no images are left in the event then we will delete this event
                if (num_images_in_event == 0)
                {
                    command.CommandText = Database_Versioning.text_for_stored_procedures.spDelete_Event(user_id, event_id);
                    command.ExecuteNonQuery();
                } //close if (num_images_in_event == 0)...
                else
                {
                    //if images are present in the event, then let's update it's start/end time and keyframe information

                    //firstly get event start/end time info (from All_Images table)...
                    command.CommandText = Database_Versioning.text_for_stored_procedures.spGet_start_end_time_of_event(user_id, event_id);
                    SQLiteDataReader read_events = command.ExecuteReader();
                    read_events.Read();
                    DateTime start_time = DateTime.Parse(read_events[0].ToString());
                    DateTime end_time = DateTime.Parse(read_events[1].ToString());
                    read_events.Close();

                    //then get a target keyframe time (i.e. exactly between start/end times)
                    TimeSpan event_length_seconds = end_time - start_time;
                    DateTime target_time = start_time.AddSeconds(event_length_seconds.TotalSeconds / 2);

                    //then select a random image around the target time to be the new keyframe path
                    int ALLOWABLE_TIME_WINDOW_FOR_KEYFRAME_AROUND_TARGET_TIME_IN_MINUTES = 2;
                    command.CommandText = Database_Versioning.text_for_stored_procedures.spSelect_random_image_from_event_around_target_window(user_id, event_id, target_time, ALLOWABLE_TIME_WINDOW_FOR_KEYFRAME_AROUND_TARGET_TIME_IN_MINUTES);
                    string new_keyframe_path = "";
                    try { new_keyframe_path = command.ExecuteScalar().ToString(); }
                    catch (Exception excep) { }

                    //if no image is found around the target time, then just resort to selecting any random image from the event...
                    if (new_keyframe_path.Equals(""))
                    {
                        command.CommandText = Database_Versioning.text_for_stored_procedures.spSelect_any_random_image_from_event(user_id, event_id);
                        new_keyframe_path = command.ExecuteScalar().ToString();
                    }

                    //finally update the event keyframe path in the All_Events table...
                    command.CommandText = Database_Versioning.text_for_stored_procedures.spUpdate_Event_time_keyframe_info(user_id, event_id, start_time, end_time, new_keyframe_path);
                    command.ExecuteNonQuery();
                } //close else ... if (num_images_in_event == 0)
                con.Close();
            } //close method update_event_information_in_database()...




        #endregion end of region retrieving event information from the database


             


    } //end class...

} //end namespace...
