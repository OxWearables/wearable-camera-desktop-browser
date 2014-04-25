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
        public static string DefaultImageCaption = "     Free text annotation";
        public static string DefaultKeyframeBorderColour = "#24000000";
        private static string DbString = global::SenseCamBrowser1.Properties.Settings.Default.DBConnectionString;

        //Properties associated with every event retrieved from database.
        public int event_id{ get; set; }
        public DateTime start_time { get; set; }
        public DateTime end_time { get; set; }
        public string comment { get; set; }
        public string str_time { get; set; }
        public string str_keyframe_path { get; set; }            
        public ImageSource keyframe_path { get; set; }
        public string short_comment { get; set; }
        public string border_colour { get; set; }
        public string event_length { get; set; }
        
        public Event_Rep(
            int eventID,
            DateTime startTime,
            DateTime endTime,
            string keyframePath,
            string comment)
        {
            this.event_id = eventID;
            this.start_time = startTime;
            this.end_time = endTime;
            this.comment = comment;
            update_event_short_comment(); //todo
            this.str_keyframe_path = keyframePath;
            this.keyframe_path = Image_Rep.get_image_source(keyframePath, true);
            this.border_colour = DefaultKeyframeBorderColour;
           
            //format time string for UI
            if (start_time.Hour < 12)
            {
                this.str_time = start_time.ToShortTimeString() + " am";
            }
            else
            {
                this.str_time = start_time.ToShortTimeString() + " pm";
            }
            TimeSpan ts_duration = endTime - startTime;
            this.event_length = Daily_Annotation_Summary.format_total_seconds_to_mins_and_seconds((int)ts_duration.TotalSeconds);
        }


        /// <summary>
        /// this method formats the comment string for proper display on the interface, where the interface calls the "short_comment" string...
        /// </summary>
        public void update_event_short_comment()
        {
            //below we format the comment string for proper display on the interface...
            if (!comment.Equals(""))
            {
            if (comment.Length < 15)
            {
                this.short_comment = comment + "...";
            }
            else
            {
                this.short_comment = comment.Substring(0, 15) + "...";
            }
            } //close if (!comment.Equals(""))...
            else this.short_comment = "";
        } //close method update_event_short_comment()...

        //todo
        public TimeSpan get_event_duration()
        {
            return end_time - start_time;
        }
        

        public static List<Event_Rep> GetDayEvents(int userID, DateTime day)
        {
            //Variables to store database values before converting to Event_Rep object.
            int eventID;
            DateTime startTime, endTime;
            string keyframePath, comment;
            List<Event_Rep> eventList = new List<Event_Rep>();      

            //Connect to database and retrieve all events in day.
            string query = Database_Versioning.text_for_stored_procedures.spGet_All_Events_In_Day(userID, day);
            SQLiteConnection con = new SQLiteConnection(DbString);
            SQLiteCommand command = new SQLiteCommand(query,con);
            con.Open();      
            SQLiteDataReader read_events = command.ExecuteReader();
            while (read_events.Read())
            {
                try
                {
                    eventID = int.Parse(read_events[0].ToString());
                }
                catch (Exception excep)
                {
                    eventID = -1;
                }
                startTime = DateTime.Parse(read_events[1].ToString());
                endTime = DateTime.Parse(read_events[2].ToString());
                keyframePath = read_events[3].ToString();
                comment = read_events[4].ToString();

                eventList.Add(new Event_Rep(eventID,
                    startTime,
                    endTime,
                    keyframePath,
                    comment));
            }
            con.Close();
            return eventList;
        }


        public static void UpdateComment(int userID, int eventID, string comment)
        {
            string query = Database_Versioning.text_for_stored_procedures.spUpdateEventComment(userID, eventID, comment);
            SQLiteConnection con = new SQLiteConnection(DbString);
            SQLiteCommand selectCmd = new SQLiteCommand(query, con);
            con.Open();
            selectCmd.ExecuteNonQuery();
            con.Close();
        }


        public static void UpdateTimesViewed(int userID, int eventID)
        {
            //Update DB with how many times this particular event has been viewed.
            //This may be helpful for memory researchers interested in visual exposure.
            string query = Database_Versioning.text_for_stored_procedures.spUpdateEvent_Number_Times_Viewed(userID, eventID);
            SQLiteConnection con = new SQLiteConnection(global::SenseCamBrowser1.Properties.Settings.Default.DBConnectionString);
            SQLiteCommand selectCmd = new SQLiteCommand(query, con);
            con.Open();
            selectCmd.ExecuteNonQuery();
            con.Close();
        }


        public static void UpdateKeyframe(int userID, int eventID, string keyframePath)
        {
            string query = Database_Versioning.text_for_stored_procedures.spUpdate_Event_Keyframe_Path(userID, eventID, keyframePath);
            SQLiteConnection con = new SQLiteConnection(DbString);
            SQLiteCommand selectCmd = new SQLiteCommand(query, con);
            con.Open();
            selectCmd.ExecuteNonQuery();
            con.Close();
        }


        public static void DeleteEvent(int userID, int eventID)
        {
            string query = Database_Versioning.text_for_stored_procedures.spDelete_Event(userID,eventID);
            SQLiteConnection con = new SQLiteConnection(DbString);
            SQLiteCommand selectCmd = new SQLiteCommand(query, con);
            con.Open();
            selectCmd.ExecuteNonQuery();
            con.Close();
        }


        public static void MoveImagesToPreviousEvent(
            int userID,
            int sourceEventID,
            DateTime targetEndTime)
        {
            //This method sends all the images in an event before a certain time, to
            //the previous event in the database.
            //If there is no previous event in the day, a new one is created.

            SQLiteConnection con = new SQLiteConnection(DbString);
            SQLiteCommand command = new SQLiteCommand(con);
            con.Open();

            //Find the day of the source event.      
            command.CommandText = Database_Versioning.text_for_stored_procedures.spGet_day_of_source_event(
            userID,
            sourceEventID);
            DateTime sourceEventDay = DateTime.Parse(command.ExecuteScalar().ToString());

            //Find the ID of the previous event to append images to.
            command.CommandText = Database_Versioning.text_for_stored_procedures.spGet_id_of_event_before_ID_and_time(
            userID,
            sourceEventID,
            targetEndTime,
            sourceEventDay);
            int newEventID = -1;
            try
            {
                newEventID = int.Parse(command.ExecuteScalar().ToString());
            }
            catch (Exception excep)
            {

            }

            //If no previous event exists, we create a new event.
            if (newEventID < 0)
            {
                command.CommandText = Database_Versioning.text_for_stored_procedures.spCreate_new_event_and_return_its_ID(
                    userID,
                    sourceEventDay);
                newEventID = int.Parse(command.ExecuteScalar().ToString());
            }

            //Update Image and Sensor ID tables in DB with new event IDs.
            command.CommandText = Database_Versioning.text_for_stored_procedures.spUpdate_image_sensors_tables_with_new_event_id_before_target_time(
                userID,
                newEventID,
                sourceEventID,
                targetEndTime);
            command.ExecuteNonQuery();

            //Update information (start/end time + keyframe) of new/previous event.
            UpdateDBEventInfo(userID, newEventID);

            //Update information of old/source event.
            UpdateDBEventInfo(userID, sourceEventID);
            con.Close();
        }


        public static void MoveImagesToNextEvent(
            int userID,
            int sourceEventID,
            DateTime targetStartTime)
        {
            //This method sends all the images in an event after a certain time,
            //to the next event in the database.
            //If there is no next event in the day, a new one is created.
            SQLiteConnection con = new SQLiteConnection(DbString);
            SQLiteCommand command = new SQLiteCommand(con);
            con.Open();

            //Find the day of the source event.
            command.CommandText = Database_Versioning.text_for_stored_procedures.spGet_day_of_source_event(
                userID,
                sourceEventID);
            DateTime sourceEventDay = DateTime.Parse(command.ExecuteScalar().ToString());

            //Find ID of the next event to append images to.
            command.CommandText = Database_Versioning.text_for_stored_procedures.spGet_id_of_event_after_ID_and_time(
                userID,
                sourceEventID,
                targetStartTime,
                sourceEventDay);
            int newEventID = -1;
            try { newEventID = int.Parse(command.ExecuteScalar().ToString()); }
            catch (Exception excep) { }

            //If no next event exists, we create a new event.
            if (newEventID < 0)
            {
                command.CommandText = Database_Versioning.text_for_stored_procedures.spCreate_new_event_and_return_its_ID(
                    userID,
                    sourceEventDay);
                newEventID = int.Parse(command.ExecuteScalar().ToString());
            }

            //Update Image and Sensor ID tables in DB with new event IDs.
            command.CommandText = Database_Versioning.text_for_stored_procedures.spUpdate_image_sensors_tables_with_new_event_id_after_target_time(
                userID,
                newEventID,
                sourceEventID,
                targetStartTime);
            command.ExecuteNonQuery();

            //Update information (start/end time + keyframe) of new/next event.
            UpdateDBEventInfo(userID, newEventID);

            //Update information of old/source event.
            UpdateDBEventInfo(userID, sourceEventID);
            con.Close();
        }


        public static int SplitEvent(
            int userID,
            int sourceEventID,
            DateTime targetStartTime)
        {            
            //This method splits an event into two separate events.
            //The new event starts at targetStartTime.
            SQLiteConnection con = new SQLiteConnection(DbString);
            SQLiteCommand command = new SQLiteCommand(con);
            con.Open();

            //Find the day of the source event.
            command.CommandText = Database_Versioning.text_for_stored_procedures.spGet_day_of_source_event(
                userID,
                sourceEventID);
            DateTime sourceEventDay = DateTime.Parse(command.ExecuteScalar().ToString());

            //Create a new event where the split images will be sent to.
            command.CommandText = Database_Versioning.text_for_stored_procedures.spCreate_new_event_and_return_its_ID(
                userID,
                sourceEventDay);
            int newEventID = int.Parse(command.ExecuteScalar().ToString());

            //Update Image and Sensor ID tables in DB with new event IDs.
            command.CommandText = Database_Versioning.text_for_stored_procedures.spUpdate_image_sensors_tables_with_new_event_id_after_target_time(
                userID,
                newEventID,
                sourceEventID,
                targetStartTime);
            command.ExecuteNonQuery();

            //Update information (start/end time + keyframe) of new/next event.
            UpdateDBEventInfo(userID, newEventID);

            //Update information of old/source event.
            UpdateDBEventInfo(userID, sourceEventID);

            con.Close();                                
            return newEventID;
        } //close method split_event_into_two()...


        public static void UpdateDBEventInfo(int userID, int eventID)
        {
            //Update event info in database (start time, end time, keyframe path).
            //This method assumes images/sensors tables have just been updated.
            SQLiteConnection con = new SQLiteConnection(DbString);
            SQLiteCommand command = new SQLiteCommand(con);
            con.Open();

            //Check if there any images remain in the event.
            command.CommandText = Database_Versioning.text_for_stored_procedures.spGet_Num_Images_In_Event(
                userID,
                eventID);
            int numEventImages = int.Parse(command.ExecuteScalar().ToString());

            //Delete the event if no images remain, otherwise update event info.
            if (numEventImages == 0)
            {
                command.CommandText = Database_Versioning.text_for_stored_procedures.spDelete_Event(
                    userID,
                    eventID);
                command.ExecuteNonQuery();
            }
            else
            {
                //Find event start/end time info from All_Images table.
                command.CommandText = Database_Versioning.text_for_stored_procedures.spGet_start_end_time_of_event(
                    userID,
                    eventID);
                SQLiteDataReader readTimes = command.ExecuteReader();
                readTimes.Read();
                DateTime startTime = DateTime.Parse(readTimes[0].ToString());
                DateTime endTime = DateTime.Parse(readTimes[1].ToString());
                readTimes.Close();

                //Set target keyframe time as mid-point between event start/end.
                TimeSpan eventLength = endTime - startTime; //seconds
                DateTime targetTime = startTime.AddSeconds(eventLength.TotalSeconds / 2);

                //Try to set new keyframe path as random image near target time.
                int KeyframeSearchWindow = 2; //minutes
                command.CommandText = Database_Versioning.text_for_stored_procedures.spSelect_random_image_from_event_around_target_window(
                    userID,
                    eventID,
                    targetTime,
                    KeyframeSearchWindow);
                string newKeyframe = "";
                try { newKeyframe = command.ExecuteScalar().ToString(); }
                catch (Exception excep) { }

                //If no image exists near the target time, then we just select
                //any random image from the event.
                if (newKeyframe.Equals(""))
                {
                    command.CommandText = Database_Versioning.text_for_stored_procedures.spSelect_any_random_image_from_event(
                        userID,
                        eventID);
                    newKeyframe = command.ExecuteScalar().ToString();
                }

                //Update All_Events table in DB with start/end time + keyframe.
                command.CommandText = Database_Versioning.text_for_stored_procedures.spUpdate_Event_time_keyframe_info(
                    userID,
                    eventID,
                    startTime,
                    endTime,
                    newKeyframe);
                command.ExecuteNonQuery();
            }
            con.Close();
        }
            
    }
}