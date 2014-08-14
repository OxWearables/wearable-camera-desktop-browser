using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseCamBrowser1.Database_Versioning
{
    class text_for_stored_procedures
    {

        #region get/set user details

        public static string spGet_List_Of_Users()
        {
            string end_string = "";
            end_string += "\n" + "select [user_id], username, [password], [name]";
            end_string += "\n" + "from Users";
            end_string += "\n" + "order by [name];";
            return end_string;
        } //close method spGet_List_Of_Users()...


        public static string spGet_user_id_of_Most_Recent_Data_Upload()
        {
            string end_string = "";
            end_string += "\n" + "select [user_id]";
            end_string += "\n" + "from All_Events";
            end_string += "\n" + "group by [user_id]";
            end_string += "\n" + "order by max([day]) desc";
            end_string += "\n" + "LIMIT 1;";
            return end_string;
        } //close method spGet_user_id_of_Most_Recent_Data_Upload()...

        public static string spInsert_New_User_Into_Database_and_Return_ID(string new_user_name)
        {
            string end_string = "";
            end_string += "\n" + "insert into Users (username,password,name) values ('" + new_user_name + "','" + new_user_name + "','" + new_user_name + "');";
            end_string += "\n" + "";
            end_string += "\n" + "select max([user_id]) from Users;";
            return end_string;
        } //close method spInsert_New_User_Into_Database_and_Return_ID()...


        public static string spCreate_dummy_image(int user_id, int event_id)
        {
            string end_string = "";
            end_string += "\n" + "insert into All_Images (user_id,event_id,image_path,image_time) values (" + user_id + "," + event_id + ",'','1991-01-23 08:02:00');";
            return end_string;
        } //close method spCreate_dummy_image()...
        
        #endregion get/set user details




        #region update event details

        public static string spCreate_new_event_and_return_its_ID(int user_id)
        {
            DateTime default_day = new DateTime(1999, 1, 1);
            return spCreate_new_event_and_return_its_ID(user_id, default_day);
        } //close method spCreate_new_event_and_return_its_ID()...


        public static string spCreate_new_event_and_return_its_ID(int user_id, DateTime day_of_source_event)
        {
            string end_string = "INSERT INTO All_Events(user_id,day,start_time,end_time,keyframe_path,number_times_viewed) VALUES (" + user_id + ", " + convert_datetime_to_sql_string(day_of_source_event) + ", " + convert_datetime_to_sql_string(day_of_source_event) + ", " + convert_datetime_to_sql_string(day_of_source_event) + ", '', 0);";
            end_string += "\n" + "SELECT MAX(event_id) FROM All_Events WHERE [user_id]=" + user_id + ";";
            return end_string;
        } //close method spCreate_new_event_and_return_its_ID()...


        public static string spDelete_Event(int user_id, int event_id)
        {
            string end_string = "DELETE";
            end_string += "\n" + "FROM All_Events";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND [event_id] = " + event_id + ";";
            return end_string;
        } //close method spDelete_Event()...


        public static string spDelete_Image_From_Event(int user_id, int event_id, DateTime image_time)
        {
            string end_string = "DELETE";
            end_string += "\n" + "FROM All_Images";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND [event_id] = " + event_id;
            end_string += "\n" + "AND image_time = " + convert_datetime_to_sql_string(image_time) + ";";
            return end_string;
        } //close method spDelete_Image_From_Event()...


        public static string spUpdateEventComment(int user_id, int event_id, string comment)
        {
            string end_string = "UPDATE All_Events";
            end_string += "\n" + "SET comment = '" + comment + "'";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND [event_id] = " + event_id + ";";
            return end_string;
        } //close method spUpdateEventComment()...
        

        public static string spUpdate_Event_Keyframe_Path(int user_id, int event_id, string keyframe_path)
        {
            string end_string = "UPDATE All_Events";
            end_string += "\n" + "SET keyframe_path = '" + keyframe_path + "'";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND [event_id] = " + event_id + ";";
            return end_string;
        } //end method spUpdate_Event_Keyframe_Path()...


        public static string spUpdate_Event_time_keyframe_info(int user_id, int event_id, DateTime start_time, DateTime end_time, string new_keyframe_path)
        {
            string end_string = "UPDATE All_Events";
            end_string += "\n" + "SET start_time = " + convert_datetime_to_sql_string(start_time) + ",";
            end_string += "\n" + "end_time = " + convert_datetime_to_sql_string(end_time) + ",";
            end_string += "\n" + "keyframe_path = '" + new_keyframe_path + "'";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND event_id = " + event_id + ";";
            return end_string;
        } //close method spUpdate_Event_time_keyframe_info()...
        

        public static string spUpdateEvent_Number_Times_Viewed(int user_id, int event_id)
        {
            string end_string = "";
            end_string += "\n" + "UPDATE All_Events";
            end_string += "\n" + "SET number_times_viewed = number_times_viewed + 1";
            end_string += "\n" + "WHERE [user_id]= " + user_id;
            end_string += "\n" + "and event_id = " + event_id + ";";
            return end_string;
        } //close method spUpdateEvent_Number_Times_Viewed()...


        public static string spUpdate_image_sensors_tables_with_new_event_id_after_target_time(int user_id, int new_event_id, int source_event_id, DateTime target_start_time)
        {
            string end_string = "UPDATE All_Images";
            end_string += "\n" + "SET event_id = " + new_event_id;
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND event_id = " + source_event_id;
            end_string += "\n" + "AND image_time >= " + convert_datetime_to_sql_string(target_start_time) + ";";
            end_string += "\n" + "";
            end_string += "\n" + "UPDATE Sensor_Readings";
            end_string += "\n" + "SET event_id = " + new_event_id;
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND event_id = " + source_event_id;
            end_string += "\n" + "AND sample_time >= " + convert_datetime_to_sql_string(target_start_time) + ";";
            return end_string;
        } //close method spUpdate_image_sensors_tables_with_new_event_id_after_target_time()...


        public static string spUpdate_image_sensors_tables_with_new_event_id_before_target_time(int user_id, int new_event_id, int source_event_id, DateTime target_end_time)
        {
            string end_string = "UPDATE All_Images";
            end_string += "\n" + "SET event_id = " + new_event_id;
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND event_id = " + source_event_id;
            end_string += "\n" + "AND image_time <= " + convert_datetime_to_sql_string(target_end_time) + ";";
            end_string += "\n" + "";
            end_string += "\n" + "UPDATE Sensor_Readings";
            end_string += "\n" + "SET event_id = " + new_event_id;
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND event_id = " + source_event_id;
            end_string += "\n" + "AND sample_time <= " + convert_datetime_to_sql_string(target_end_time) + ";";
            return end_string;
        } //close method spUpdate_image_sensors_tables_with_new_event_id_after_target_time()...


        public static string spUpdate_newly_uploaded_images_and_sensor_readings_with_relevant_event_id(int user_id, int most_recent_event_id)
        {
            string end_string = "";
            end_string += "\n" + "UPDATE All_Images";
            end_string += "\n" + "SET event_id=";
            end_string += "\n" + "(";
            end_string += "\n" + "SELECT event_id";
            end_string += "\n" + "FROM All_Events";
            end_string += "\n" + "WHERE event_id > " + most_recent_event_id;
            end_string += "\n" + "AND [user_id] = " + user_id;
            end_string += "\n" + "AND start_time <= All_Images.image_time AND end_time >= All_Images.image_time";
            end_string += "\n" + "LIMIT 1";
            end_string += "\n" + ")";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND event_id IS NULL;";
            end_string += "\n" + "";
            end_string += "\n" + "UPDATE Sensor_Readings";
            end_string += "\n" + "SET event_id=";
            end_string += "\n" + "(";
            end_string += "\n" + "SELECT MAX(event_id)";
            end_string += "\n" + "FROM All_Events";
            end_string += "\n" + "WHERE event_id > " + most_recent_event_id;
            end_string += "\n" + "AND [user_id] = " + user_id;
            //end_string += "\n" + "AND DATEADD(MINUTE,-1,start_time) <= Sensor_Readings.sample_time AND DATEADD(MINUTE,1,end_time) >= Sensor_Readings.sample_time";
            end_string += "\n" + "AND DATETIME(start_time,'-1 minute') <= Sensor_Readings.sample_time AND DATETIME(end_time,'+1 minute') >= Sensor_Readings.sample_time";
            end_string += "\n" + ")";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND event_id IS NULL;";
            return end_string;
        } //close method spUpdate_newly_uploaded_images_and_sensor_readings_with_relevant_event_id()....


        public static string spUpdate_newly_uploaded_images_tidy_up_spurious_events(int user_id)
        {
            string end_string = "";
            end_string += "\n" + "DELETE FROM All_Events";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "and start_time>end_time;";
            end_string += "\n" + "";
            end_string += "\n" + "";
            end_string += "\n" + "UPDATE All_Images";
            end_string += "\n" + "SET event_id = (SELECT MAX(event_id) FROM All_Events WHERE [user_id] = " + user_id + ")";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND event_id IS NULL;";
            return end_string;
        } //close method spUpdate_newly_uploaded_images_tidy_up_spurious_events()....
        
        #endregion update event details




        #region get event details

        public static string spGet_most_recent_event_id_for_user(int user_id)
        {
            string end_string = "";
            end_string += "\n" + "SELECT CASE WHEN MAX(event_id) IS NOT NULL THEN MAX(event_id)-1 ELSE -1 END "; //WILL JUST GO BACK 1 JUST TO BE ON THE SAFE SIDE SO THAT I DON'T MISS ASSIGNING THE EVENT TO ANY IMAGE
            end_string += "\n" + "FROM All_Images where [user_id] = " + user_id + ";";
            return end_string;
        } //close method spGet_most_recent_event_id_for_user()....


        public static string spGet_All_Events_In_Day(int user_id, DateTime day)
        {
            string end_string = "SELECT event_id, start_time, end_time, keyframe_path, comment";
            end_string += "\n" + "FROM All_Events";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND day >=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day)) + "";
            end_string += "\n" + "AND day <=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day, 23, 59, 59)) + "";
            //end_string += "\n" + "AND DATEPART(YEAR, [day]) = DATEPART(YEAR, " + convert_datetime_to_sql_string(day) + ")";
            //end_string += "\n" + "AND DATEPART(DAYOFYEAR, [day]) = DATEPART(DAYOFYEAR, " + convert_datetime_to_sql_string(day) + ")";
            end_string += "\n" + "ORDER BY start_time;";
            return end_string;
        } //close method spGet_All_Events_In_Day()...


        public static string spGet_Last_Keyframe_Path(int user_id)
        {
            string end_string = "SELECT keyframe_path";
            end_string += "\n" + "FROM All_Events";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "ORDER BY [day] DESC";
            end_string += "\n" + "LIMIT 1;";
            return end_string;
        } //close method spGet_Last_Keyframe_Path()


        public static string spGet_day_of_source_event(int user_id, int event_id)
        {
            string end_string = "SELECT [day] FROM All_Events WHERE [user_id]=" + user_id + " AND event_id=" + event_id + ";";
            return end_string;
        } //close method spGet_day_of_source_event()...


        public static string spGet_start_end_time_of_event(int user_id, int event_id)
        {
            string end_string = "SELECT MIN(image_time) as start_time, MAX(image_time) as end_time";
            end_string += "\n" + "FROM All_Images";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND event_id = " + event_id + ";";
            return end_string;
        } //close method spGet_start_end_time_of_event()...


        public static string spGet_Num_Images_In_Event(int user_id, int event_id)
        {
            string end_string = "SELECT COUNT(image_id)";
            end_string += "\n" + "FROM All_Images";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND [event_id] = " + event_id + ";";
            return end_string;
        } //close method spGet_Num_Images_In_Event()...


        public static string spGet_Paths_Of_All_Images_In_Event(int user_id, int event_id)
        {
            string end_string = "SELECT image_time, image_path";
            end_string += "\n" + "FROM All_Images";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND [event_id] = " + event_id;
            end_string += "\n" + "ORDER BY image_time;";
            return end_string;
        } //close method spGet_Paths_Of_All_Images_In_Events()...
        

        public static string spSelect_random_image_from_event_around_target_window(int user_id, int event_id, DateTime target_time, int search_window_minutes)
        {
            string end_string = "SELECT image_path";
            end_string += "\n" + "FROM All_Images";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND event_id = " + event_id;
            end_string += "\n" + "AND image_time >= " + convert_datetime_to_sql_string(target_time.AddMinutes(-search_window_minutes));
            end_string += "\n" + "AND image_time <= " + convert_datetime_to_sql_string(target_time.AddMinutes(search_window_minutes));
            end_string += "\n" + "ORDER BY RANDOM()";
            end_string += "\n" + "LIMIT 1;";
            return end_string;
        } //close method spSelect_random_image_from_event_around_target_window()...


        public static string spSelect_any_random_image_from_event(int user_id, int event_id)
        {
            string end_string = "SELECT image_path";
            end_string += "\n" + "FROM All_Images";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND event_id = " + event_id;
            end_string += "\n" + "ORDER BY RANDOM()";
            end_string += "\n" + "LIMIT 1;";
            return end_string;
        } //close method spSelect_any_random_image_from_event()...


        public static string spGet_id_of_event_before_ID_and_time(int user_id, int source_event_id, DateTime target_end_time, DateTime source_day)
        {
            string end_string = "SELECT event_id";
            end_string += "\n" + "FROM All_Events";
            end_string += "\n" + "WHERE [user_id]=" + user_id;
            end_string += "\n" + "AND event_id!=" + source_event_id;
            end_string += "\n" + "AND start_time >= " + convert_datetime_to_sql_string(target_end_time.AddHours(-6));
            end_string += "\n" + "AND start_time < " + convert_datetime_to_sql_string(target_end_time);
            end_string += "\n" + "AND [day] = " + convert_datetime_to_sql_string(source_day);
            end_string += "\n" + "ORDER BY start_time DESC";
            end_string += "\n" + "LIMIT 1 ;";
            return end_string;
        } //close method spGet_id_of_event_before_ID_and_time()...


        public static string spGet_id_of_event_after_ID_and_time(int user_id, int source_event_id, DateTime target_start_time, DateTime source_day)
        {
            string end_string = "SELECT event_id";
            end_string += "\n" + "FROM All_Events";
            end_string += "\n" + "WHERE [user_id]=" + user_id;
            end_string += "\n" + "AND event_id!=" + source_event_id;
            end_string += "\n" + "AND start_time > " + convert_datetime_to_sql_string(target_start_time);
            end_string += "\n" + "AND start_time <= " + convert_datetime_to_sql_string(target_start_time.AddHours(6));
            end_string += "\n" + "AND [day] = " + convert_datetime_to_sql_string(source_day);
            end_string += "\n" + "ORDER BY start_time";
            end_string += "\n" + "LIMIT 1 ;";
            return end_string;
        } //close method spGet_id_of_event_after_ID_and_time()...

        #endregion get event details




        #region get day details

        public static string spGet_List_Of_All_Days_For_User(int user_id)
        {
            string end_string = "SELECT MIN([day])";
            end_string += "\n" + "FROM All_Events";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "GROUP BY strftime('%Y-%m-%d',day)";
            end_string += "\n" + "ORDER BY MIN([day]) DESC;";
            return end_string;
        } //close method spGet_List_Of_All_Days_For_User()...


        public static string spGet_Day_Start_and_end_times(int user_id, DateTime day)
        {
            string end_string = "SELECT MIN(start_time) AS start_time, MAX(end_time) AS end_time";
            end_string += "\n" + "FROM All_Events";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND day >=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day));
            end_string += "\n" + "AND day <=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day, 23, 59, 59)) + ";";
            //end_string += "\n" + "AND DATEPART(YEAR, [day]) = DATEPART(YEAR, " + convert_datetime_to_sql_string(day) + ")";
            //end_string += "\n" + "AND DATEPART(DAYOFYEAR, [day]) = DATEPART(DAYOFYEAR, " + convert_datetime_to_sql_string(day) + ")";
            return end_string;
        } //close method spGet_Day_Start_and_end_times()...


        public static string spGet_Num_Images_In_Day(int user_id, DateTime day)
        {
            string end_string = "SELECT COUNT(*)";
            end_string += "\n" + "";
            end_string += "\n" + "FROM All_Images";
            end_string += "\n" + "";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND event_id in";
            end_string += "\n" + "(";
            end_string += "\n" + "SELECT event_id";
            end_string += "\n" + "FROM All_Events";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND day >=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day));
            end_string += "\n" + "AND day <=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day, 23, 59, 59)) + "";
            //end_string += "\n" + "AND DATEPART(YEAR, [day]) = DATEPART(YEAR, " + convert_datetime_to_sql_string(day) + ")";
            //end_string += "\n" + "AND DATEPART(DAYOFYEAR, [day]) = DATEPART(DAYOFYEAR, " + convert_datetime_to_sql_string(day) + ")";
            end_string += "\n" + ");";
            return end_string;
        } //close method spGet_Num_Images_In_Day()...

        #endregion get user and day details




        #region get event annotations

        public static string spGet_list_of_annotation_types()
        {
            string end_string = "";
            end_string += "\n" + "SELECT annotation_id, annotation_type, [description]";
            end_string += "\n" + "FROM Annotation_Types";
            end_string += "\n" + "order by annotation_type;";
            return end_string;
        } //close method spGet_list_of_annotation_types()...

        public static string spGet_event_annotations(int user_id, int event_id)
        {
            string end_string = "";
            end_string += "\n" + "SELECT annotation_name";
            end_string += "\n" + "FROM SC_Browser_User_Annotations AS annotations";
            end_string += "\n" + "WHERE annotations.[user_id]=" + user_id;
            end_string += "\n" + "AND annotations.event_id=" + event_id + ";";
            return end_string;
        } //close method spGet_event_annotations()...
        

        public static string spGet_annotated_events_in_day(int user_id, DateTime day)
        {
            //string end_string = "SELECT annotations.event_id, annotations.annotation_name,DATEDIFF(SECOND, All_Events.start_time, All_Events.end_time) AS duration_in_seconds";
            string end_string = "SELECT annotations.event_id, annotations.annotation_name, strftime('%s',All_Events.end_time) - strftime('%s',All_Events.start_time) AS duration_in_seconds";
            end_string += "\n" + "FROM SC_Browser_User_Annotations AS annotations";
            end_string += "\n" + "";
            end_string += "\n" + "INNER JOIN All_Events";
            end_string += "\n" + "ON annotations.event_id = All_Events.[event_id]";
            end_string += "\n" + "";
            end_string += "\n" + "WHERE annotations.[user_id]=" + user_id;
            end_string += "\n" + "AND day >=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day)) + "";
            end_string += "\n" + "AND day <=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day, 23, 59, 59)) + "";
            //end_string += "\n" + "AND DATEPART(YEAR, All_Events.[day]) = DATEPART(YEAR, " + convert_datetime_to_sql_string(day) + ")";
            //end_string += "\n" + "AND DATEPART(DAYOFYEAR, All_Events.[day]) = DATEPART(DAYOFYEAR, " + convert_datetime_to_sql_string(day) + ")";
            end_string += "\n" + "";
            end_string += "\n" + "ORDER BY All_Events.start_time;";
            return end_string;
        } //close method spGet_annotated_events_in_day()...


        public static string spGet_event_ids_in_day_for_specific_activity(int user_id, DateTime day, string annotation_type)
        {
            string end_string = "";
            end_string += "\n" + "SELECT annotations.event_id";
            end_string += "\n" + "FROM SC_Browser_User_Annotations AS annotations";
            end_string += "\n" + "INNER JOIN All_Events";
            end_string += "\n" + "ON annotations.event_id = All_Events.event_id";
            end_string += "\n" + "";
            end_string += "\n" + "WHERE annotations.[user_id]=" + user_id;
            end_string += "\n" + "  AND day >=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day)) + "";
            end_string += "\n" + "  AND day <=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day, 23, 59, 59)) + "";
            //end_string += "\n" + "AND DATEPART(YEAR, All_Events.[day]) = DATEPART(YEAR," + convert_datetime_to_sql_string(day) + ")";
            //end_string += "\n" + "AND DATEPART(DAYOFYEAR, All_Events.[day]) = DATEPART(DAYOFYEAR," + convert_datetime_to_sql_string(day) + ")";
            end_string += "\n" + "AND annotations.annotation_name='" + annotation_type + "'";
            end_string += "\n" + "";
            end_string += "\n" + "ORDER BY annotations.event_id;";
            return end_string;
        } //close method spGet_event_ids_in_day_for_specific_activity()...


        public static string spGet_daily_activity_summary_from_annotations(int user_id, DateTime day)
        {
            string end_string = "";
            end_string += "\n" + "SELECT annotation_name, sum(duration_in_seconds) as total_time_spent_at_activity";
            end_string += "\n" + "";
            end_string += "\n" + "FROM (";
            end_string += "\n" + "  SELECT annotations.annotation_name, ";
            //end_string += "\n" + "  DATEDIFF(SECOND, All_Events.start_time, All_Events.end_time) AS duration_in_seconds";
            end_string += "\n" + "  strftime('%s', All_Events.end_time) - strftime('%s', All_Events.start_time) AS duration_in_seconds";
            end_string += "\n" + "";
            end_string += "\n" + "  FROM SC_Browser_User_Annotations AS annotations";
            end_string += "\n" + "      INNER JOIN All_Events";
            end_string += "\n" + "      ON annotations.event_id = All_Events.[event_id]";
            end_string += "\n" + "";
            end_string += "\n" + "  WHERE annotations.[user_id]=" + user_id;
            end_string += "\n" + "      AND day >=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day)) + "";
            end_string += "\n" + "      AND day <=" + convert_datetime_to_sql_string(new DateTime(day.Year, day.Month, day.Day, 23, 59, 59)) + "";
            //end_string += "\n" + "  AND DATEPART(YEAR, All_Events.[day]) = DATEPART(YEAR," + convert_datetime_to_sql_string(day) + ")";
            //end_string += "\n" + "  AND DATEPART(DAYOFYEAR, All_Events.[day]) = DATEPART(DAYOFYEAR," + convert_datetime_to_sql_string(day) + ")";
            end_string += "\n" + "";
            end_string += "\n" + "  ) AS inner_table";
            end_string += "\n" + "";
            end_string += "\n" + "GROUP BY annotation_name";
            end_string += "\n" + "ORDER BY annotation_name;";
            return end_string;
        } //close method spGet_daily_activity_summary_from_annotations()...


        public static string spGetAnnotationSummary(int userId)
        {
            string endString = "SELECT u.name, evnt.start_time, evnt.end_time, coding.annotation_name";
            endString += "\n" + "FROM SC_Browser_User_Annotations AS coding";
            endString += "\n" + "INNER JOIN Users AS u";
            endString += "\n" + "  ON coding.[user_id] = u.[user_id]";
            endString += "\n" + "  INNER JOIN All_Events AS evnt";
            endString += "\n" + "      ON coding.[user_id] = evnt.[user_id] AND coding.event_id = evnt.event_id";
            endString += "\n" + "WHERE evnt.[user_id]=" + userId;
            endString += "\n" + "ORDER BY u.[user_id], evnt.start_time";            
            return endString;
        }

        #endregion get event annotations




        #region add/remove event annotations

        public static string spAdd_event_annotation(int user_id, int event_id, string event_annotation_name)
        {
            string end_string = "";
            end_string += "\n" + "INSERT INTO SC_Browser_User_Annotations";
            end_string += "\n" + "VALUES (" + user_id + "," + event_id + "," + convert_datetime_to_sql_string(DateTime.Now) + ",'" + event_annotation_name + "');";
            return end_string;
        } //close method spAdd_event_annotation()...

        public static string spClear_event_annotations(int user_id, int event_id)
        {
            string end_string = "";
            end_string += "\n" + "DELETE FROM SC_Browser_User_Annotations";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND [event_id] = " + event_id + ";";
            return end_string;
        } //close method spClear_event_annotations()...


        public static string spClear_event_annotations(int user_id, int event_id, string individual_annotation_text)
        {
            string end_string = "";
            end_string += "\n" + "DELETE FROM SC_Browser_User_Annotations";
            end_string += "\n" + "WHERE [user_id] = " + user_id;
            end_string += "\n" + "AND [event_id] = " + event_id;
            end_string += "\n" + "AND [annotation_name] = '" + individual_annotation_text + "';";
            return end_string;
        } //close method spClear_event_annotations()...

        #endregion add/remove event annotations




        #region add/remove annotation types

        public static string spAdd_annotation_type(string annotation_type_name)
        {
            string end_string = "";
            end_string += "\n" + "DELETE FROM Annotation_Types";
            end_string += "\n" + "WHERE annotation_type = '" + annotation_type_name + "';";
            end_string += "\n" + "INSERT INTO Annotation_Types (annotation_type,description) VALUES('" + annotation_type_name + "','" + annotation_type_name + "');";
            return end_string;
        } //close method spAdd_annotation_type()...

        public static string spRemove_annotation_type(string annotation_type_name)
        {
            string end_string = "";
            end_string += "\n" + "DELETE FROM Annotation_Types";
            end_string += "\n" + "WHERE annotation_type = '" + annotation_type_name + "';";;
            return end_string;
        } //close method spRemove_annotation_type()...


        public static string spRemove_all_annotation_types()
        {
            string end_string = "";
            end_string += "\n" + "DELETE FROM Annotation_Types;";
            return end_string;
        } //close spRemove_all_annotation_types()...

        #endregion add/remove annotation types




        public static string spLog_User_Interaction(int user_id, DateTime interaction_time, string uixaml_element, string comma_seperated_parameters)
        {
            string end_string = "INSERT INTO User_Interaction_Log";
            end_string += "\n" + "VALUES(" + user_id + ", " + convert_datetime_to_sql_string(interaction_time) + ", '" + uixaml_element + "', '" + comma_seperated_parameters + "');";
            return end_string;
        } //close method spLog_User_Interaction()...


        private static string convert_datetime_to_sql_string(DateTime time)
        {
            string month, day, hour, minute, second;
            if (time.Month < 10)
                month = "0" + time.Month;
            else month = time.Month.ToString();

            if (time.Day < 10)
                day = "0" + time.Day;
            else day = time.Day.ToString();

            if (time.Hour < 10)
                hour = "0" + time.Hour;
            else hour = time.Hour.ToString();

            if (time.Minute < 10)
                minute = "0" + time.Minute;
            else minute = time.Minute.ToString();

            if (time.Second < 10)
                second = "0" + time.Second;
            else second = time.Second.ToString();

            return "'" + time.Year + "-" + month + "-" + day + " " + hour + ":" + minute + ":" + second + "'";// +"." + time.Millisecond + "'";
        } //close method convert_datetime_to_sql_string()...

    } //close class text_for_stored_procedures...
} //close namespace...