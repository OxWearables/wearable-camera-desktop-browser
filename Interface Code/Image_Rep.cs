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
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Data.Common;

namespace SenseCamBrowser1
{
    public class Image_Rep
    {
        public static List<Image_Rep> list_of_images_for_viewer_to_show;


        #region properties which are associated with each and every image retrieved from the database

        public DateTime image_time { get; set; }
        public string short_image_time { get; set; }
        public string image_path { get; set; }
        public ImageSource scaled_image_src { get; set; }
        public int array_position_in_event { get; set; }

        #endregion properties which are associated with each and every image retrieved from the database




        public Image_Rep(DateTime img_time, string image_path, int array_position_in_event)
        {
            this.image_time = img_time;
            this.image_path = image_path;
            this.array_position_in_event = array_position_in_event;
            this.scaled_image_src = Image_Rep.get_image_source(image_path, true);

            this.short_image_time = image_time.ToLongTimeString();
        } //close method Simple_Image_Rep()...


        public Image_Rep(DateTime img_time, ImageSource image_src, int array_position_in_event)
        {
            this.image_time = img_time;
            this.array_position_in_event = array_position_in_event;
            this.scaled_image_src = image_src;            
        } //close method Simple_Image_Rep()...



        #region bitmap image properties

        /// <summary>
        /// this method is used to the unscaled image source, i.e. the full image will be loaded to memory
        /// </summary>
        /// <returns></returns>
        public ImageSource image_source()
        {
            return get_image_source(image_path, false);
        } //close image_source()...




        /// <summary>
        /// this method is used to load a bitmap image
        /// </summary>
        /// <param name="image_path"></param>
        /// <param name="scaled">if we want to load a smaller image in memory, we set this parameter to true</param>
        /// <returns></returns>
        public static BitmapImage get_image_source(string image_path, bool scaled)
        {
            BitmapImage tmp_bitmap = new BitmapImage();
            try
            {
                tmp_bitmap.BeginInit();

                if (scaled)
                    tmp_bitmap.DecodePixelWidth = 120;
                //else tmp_bitmap.DecodePixelWidth = 480; //else I'll make a "full size" image, to 75% size (i.e. 480 pixels wide as opposed to 640)

                tmp_bitmap.CacheOption = BitmapCacheOption.OnLoad;
                tmp_bitmap.UriSource = new Uri(image_path, UriKind.RelativeOrAbsolute);
                
                tmp_bitmap.EndInit();
                tmp_bitmap.Freeze();
            }
            catch (Exception excep)
            {
                //if there's some error with this image, we'll just show another default image instead...
                tmp_bitmap = new BitmapImage(new Uri("Image-Unavailable.gif", UriKind.RelativeOrAbsolute));                
            } //end try ... catch
            tmp_bitmap.Freeze(); //this allows images to be loaded in a background thread, and then passed on to the UI thread ... important to give an interactive feel ...
                        
            return tmp_bitmap;
        } //close method get_image_source()...




        /// <summary>
        /// here we ATTEMPT to release the images from memory
        /// </summary>
        /// <param name="list_of_images"></param>
        public static void release_imagesource_resources_from_all_Simple_Image_Rep_items(List<Image_Rep> list_of_images)
        {
            //todo we're not releasing memory of the bitmap image correctly so this part of the application isn't working right
            //todo gotta design a Virtualizing Wrap Panel to display the images properly ... see C:\software development\Code Directory\Little Apps\image scroller\virtuallist1
            
            //firstly make sure the image source is released ok...
            foreach (Image_Rep individual_image in list_of_images)
                individual_image.scaled_image_src = null; //and hopefully this will release the system resources...
            
            
            //and now clear all the items...
            list_of_images.Clear();
        } //close method release_imagesource_resources_from_all_Simple_Image_Rep_items()...

        #endregion bitmap image properties







        #region get information on the images in an event/day from the database

        /// <summary>
        /// here we get the number of images in a given day...
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="day_in_question"></param>
        /// <returns></returns>
        public static int get_number_of_images_in_day(int user_id, DateTime day_in_question)
        {
            int num_images_in_day = 0;

            //this method calls a database stored procedure and returns the paths of all the images in this event
            SqlConnection con = new SqlConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);
            SqlCommand selectCmd = new SqlCommand("spGet_Num_Images_In_Day", con);
            selectCmd.CommandType = CommandType.StoredProcedure;
            selectCmd.Parameters.Add("@USER_ID", SqlDbType.Int).Value = user_id;
            selectCmd.Parameters.Add("@DAY", SqlDbType.DateTime).Value = day_in_question;
            con.Open();
            try
            {
                num_images_in_day = int.Parse(selectCmd.ExecuteScalar().ToString());
            }
            catch (Exception excep) { }
            con.Close();

            return num_images_in_day;
        } //end method get_number_of_images_in_day()  




        /// <summary>
        /// this method retrieves all the images in an event (from the DB)
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="event_id"></param>
        /// <returns></returns>
        public static List<Image_Rep> get_all_images_in_event(int user_id, int event_id)
        {

            //this method calls a database stored procedure and returns the paths of all the images in this event
            SqlConnection con = new SqlConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);
            SqlCommand selectCmd = new SqlCommand("spGet_Paths_Of_All_Images_In_Events", con);
            selectCmd.CommandType = CommandType.StoredProcedure;
            selectCmd.Parameters.Add("@USER_ID", SqlDbType.Int).Value = user_id;
            selectCmd.Parameters.Add("@EVENT_ID", SqlDbType.Int).Value = event_id;
            con.Open();
            SqlDataReader read_events = selectCmd.ExecuteReader();

            List<Image_Rep> list_of_images = new List<Image_Rep>();
            int counter = 0;
            while (read_events.Read())
            {
                //if (counter % 5 == 0)
                list_of_images.Add(new Image_Rep((DateTime)read_events[1], read_events[0].ToString(), counter));

                counter++;
            } //end while (read_chunk_ids.Read())		
            con.Close();

            return list_of_images;
        } //end method get_paths_of_all_images_in_event()




        /// <summary>
        /// this method gets the number of images in an event...
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="event_id"></param>
        /// <returns></returns>
        private static int get_number_of_images_in_event(int user_id, int event_id)
        {
            int num_images = 0;
            //this method calls a database stored procedure to return the number of images in an event
            SqlConnection con = new SqlConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);
            SqlCommand selectCmd = new SqlCommand("spGet_Num_Images_In_Event", con);
            selectCmd.CommandType = CommandType.StoredProcedure;
            selectCmd.Parameters.Add("@USER_ID", SqlDbType.Int).Value = user_id;
            selectCmd.Parameters.Add("@EVENT_ID", SqlDbType.Int).Value = event_id;
            con.Open();
            try
            {
                num_images = int.Parse(selectCmd.ExecuteScalar().ToString());
            }
            catch (Exception excep)
            {
                num_images = 2;
            }
            con.Close();

            return num_images;
        } //end method get_number_of_images_in_event()




        /// <summary>
        /// this method gets the start and end times of images captured in any given day, note it's a void method and updates two parameters by reference
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="day_in_question"></param>
        /// <param name="day_start_time">updated by reference</param>
        /// <param name="day_end_time">updated by reference</param>
        public static void get_start_and_end_time_of_images_in_day(int user_id, DateTime day_in_question, ref DateTime day_start_time, ref DateTime day_end_time)
        {
            //this method calls the relevant database stored procedure to retrieve a list of events
            List<Event_Rep> list_of_events = new List<Event_Rep>();

            SQLiteConnection con = new SQLiteConnection(@"Data Source=C:\software development\APIs downloaded\Databases\sql lite\aiden_test.db;Pooling=true;FailIfMissing=false;Version=3");
            SQLiteCommand command = new SQLiteCommand(con);
            con.Open();
            command.CommandText = Database_Versioning.text_for_stored_procedures.spGet_Day_Start_and_End_Times(user_id, day_in_question);
            SQLiteDataReader read_start_and_end_time_row = command.ExecuteReader();
            /*
            //this method calls a database stored procedure and returns the start and end time of all the images in this day
            SqlConnection con = new SqlConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);
            SqlCommand selectCmd = new SqlCommand("spGet_Day_Start_and_End_Times", con);
            selectCmd.CommandType = CommandType.StoredProcedure;
            selectCmd.Parameters.Add("@USER_ID", SqlDbType.Int).Value = user_id;
            selectCmd.Parameters.Add("@DAY", SqlDbType.DateTime).Value = day_in_question;
            con.Open();
            SqlDataReader read_start_and_end_time_row = selectCmd.ExecuteReader();
            */ 
            read_start_and_end_time_row.Read();

            string ha, ha1;
            //and here we update the parameters passed in by reference, meaning they'll be updated in the interface class (if called by that class), which means we don't need to pass out any values (hence it's a void method, rather than a DateTime method)
            try
            {
                ha = read_start_and_end_time_row[0].ToString();
                ha1 = read_start_and_end_time_row[1].ToString();
                day_start_time = (DateTime)read_start_and_end_time_row[0];
                day_end_time = (DateTime)read_start_and_end_time_row[1];
            }
            catch (Exception excep) { }

            con.Close(); //and close our connection
        } //end method get_number_of_images_in_day()  




        /// <summary>
        /// this method deletes a given image from a given event
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="event_id"></param>
        /// <param name="image_time"></param>
        public static void delete_image_from_event(int user_id, int event_id, DateTime image_time, string image_path)
        {
            //this method calls a database stored procedure and deletes the given image reference in the database
            SqlConnection con = new SqlConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);
            SqlCommand selectCmd = new SqlCommand("spDelete_Image_From_Event", con);
            selectCmd.CommandType = CommandType.StoredProcedure;
            selectCmd.Parameters.Add("@USER_ID", SqlDbType.Int).Value = user_id;
            selectCmd.Parameters.Add("@EVENT_ID", SqlDbType.Int).Value = event_id;
            selectCmd.Parameters.Add("@IMAGE_TIME", SqlDbType.DateTime).Value = image_time;
            con.Open();
            selectCmd.ExecuteNonQuery();
            con.Close();

            //now we also delete the actual image
            System.IO.File.Delete(image_path);
        } //end method delete_image_from_event()

        #endregion get information on the images in an event




        public static void sort_list_of_images_by_id(List<Image_Rep> image_list)
        {
            int i;
            int j;
            Image_Rep temp = new Image_Rep(DateTime.Now,"",-99);

            for (i = (image_list.Count - 1); i >= 0; i--)
            {
                for (j = 1; j <= i; j++)
                {
                    if (image_list[j - 1].array_position_in_event > image_list[j].array_position_in_event)
                    {
                        temp.array_position_in_event = image_list[j - 1].array_position_in_event;
                        temp.image_path = image_list[j - 1].image_path;
                        temp.image_time = image_list[j - 1].image_time;
                        temp.scaled_image_src = image_list[j - 1].scaled_image_src;
                        temp.short_image_time = image_list[j - 1].short_image_time;

                        image_list[j - 1].array_position_in_event = image_list[j].array_position_in_event;
                        image_list[j - 1].image_path = image_list[j].image_path;
                        image_list[j - 1].image_time = image_list[j].image_time;
                        image_list[j - 1].scaled_image_src = image_list[j].scaled_image_src;
                        image_list[j - 1].short_image_time = image_list[j].short_image_time;

                        image_list[j].array_position_in_event = temp.array_position_in_event;
                        image_list[j].image_path = temp.image_path;
                        image_list[j].image_time = temp.image_time;
                        image_list[j].scaled_image_src = temp.scaled_image_src;
                        image_list[j].short_image_time = temp.short_image_time;
                    } //end if (value_list[j - 1].array_position_in_event > value_list[j].array_position_in_event)
                } //end for (j = 1; j <= i; j++)
            } //end for (i = (value_list.Length - 1); i >= 0; i--)
        } //close method sort_list_of_images_by_id()...


    } //end class...
} //end namespace...
