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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Data.SQLite;
using System.Data.Common;

namespace SenseCamBrowser1
{
    class User_Object
    {
        public static int OVERALL_userID = int.Parse(ConfigurationManager.AppSettings["userID"].ToString()); //this value is updated by User_Management_Window.xaml
        public static string OVERALL_USER_NAME = "Aiden"; //this value is updated by User_Management_Window.xaml


        public int userID { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string name { get; set; }

        public User_Object(int userID, string username, string password, string name)
        {
            this.userID = userID;
            this.username = username;
            this.password = password;
            this.name = name;
        } //close constructor()...




        public static List<User_Object> get_list_of_users_in_database()
        {
            //this method calls the relevant database stored procedure to retrieve a list of events

            List<User_Object> list_of_users = new List<User_Object>();
            SQLiteConnection con = new SQLiteConnection(global::SenseCamBrowser1.Properties.Settings.Default.DBConnectionString);
            SQLiteCommand selectCmd = new SQLiteCommand(Database_Versioning.text_for_stored_procedures.spGet_List_Of_Users(), con);            
            con.Open();            
            SQLiteDataReader read_events = selectCmd.ExecuteReader();

            int userID;
            string username, password, name; //values that just allow me to store individual database values before storing them in an object of type Event_Rep

            while (read_events.Read())
            {
                userID = int.Parse(read_events[0].ToString());
                username = read_events[1].ToString();
                password = read_events[2].ToString();
                name = read_events[3].ToString();

                list_of_users.Add(new User_Object(userID, username, password, name));
            } //end while (read_chunk_ids.Read())		
            con.Close();

            return list_of_users;
        } //end method get_list_of_users_in_database()




        /// <summary>
        /// this method is responsible for inserting a new user's information into the database...
        /// </summary>
        /// <param name="usr_name"></param>
        public static int insert_new_user_into_database_and_get_id(string usr_name)
        {
            int new_userID = -1, new_eventID = -1;

            //this method calls the relevant database stored procedure to insert a new user and then return the ID of this newly added user...
            SQLiteConnection con = new SQLiteConnection(global::SenseCamBrowser1.Properties.Settings.Default.DBConnectionString);
            SQLiteCommand selectCmd = new SQLiteCommand(Database_Versioning.text_for_stored_procedures.spInsert_New_User_Into_Database_and_Return_ID(usr_name), con);
            con.Open();

            //insert into Users table and get userID
            try { new_userID = int.Parse(selectCmd.ExecuteScalar().ToString()); }
            catch (Exception excep) { }

            //insert dummy event into All_Events table and get event id
            selectCmd = new SQLiteCommand(Database_Versioning.text_for_stored_procedures.spCreate_new_event_and_return_its_ID(new_userID), con);
            try { new_eventID = int.Parse(selectCmd.ExecuteScalar().ToString()); }
            catch (Exception excep) { }

            //insert dummy image into All_Images table
            selectCmd = new SQLiteCommand(Database_Versioning.text_for_stored_procedures.spCreate_dummy_image(new_userID, new_eventID), con);
            try { selectCmd.ExecuteNonQuery(); }
            catch (Exception excep) { }

            con.Close();

            return new_userID;
        } //end method insert_new_user_into_database_and_get_id()


        /// <summary>
        /// this method is used to automatically detect the likely desired destination of the SenseCam images, i.e. where the most recent images have been uploaded to...
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public static string get_likely_PC_destination_root(int userID, string user_name)
        {
            string last_keyframe_path = "";
            //this method calls a database stored procedure to return the number of images in an event
            SQLiteConnection con = new SQLiteConnection(global::SenseCamBrowser1.Properties.Settings.Default.DBConnectionString);
            SQLiteCommand selectCmd = new SQLiteCommand(Database_Versioning.text_for_stored_procedures.spGet_Last_Keyframe_Path(userID), con);
            con.Open();
            try
            {
                last_keyframe_path = selectCmd.ExecuteScalar().ToString(); //gets the path of the most recently uploaded event...
            }
            catch (Exception excep) { }
            con.Close();

            //now we examine the last_keyframe_path (if one exists)
            if (!last_keyframe_path.Equals(""))
            {
                string[] components = last_keyframe_path.Split(new string[] { @"\", "/" }, StringSplitOptions.RemoveEmptyEntries); //in case there are any web url images in there, we change the "\", "/" slashes
                last_keyframe_path = ""; //let's reset this string...
                for (int c = 0; c < components.Length - 2; c++) //so we include all elements except the last 2, which is the image name, and the direct folder holding those images
                    last_keyframe_path += components[c] + @"\"; //this is our likely SenseCam root directory...
            } //close if (!last_keyframe_path.Equals(""))...


            //now check to make sure we've got a valid path
            if (last_keyframe_path.Equals(""))
                last_keyframe_path = get_likely_PC_destination_root_based_on_another_user(userID, user_name); //else (when it's first time to upload for this user) try getting an estimated path, based on another user...

            return last_keyframe_path;
        }
        

        /// <summary>
        /// this method is used to automatically detect the likely desired destination of the SenseCam images (for a new user uploading images for the 1st time)
        /// i.e. where the most recent images have been uploaded to...
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        private static string get_likely_PC_destination_root_based_on_another_user(int userID_of_first_time_user, string user_name)
        {
            int userID_of_user_with_most_recent_data = -1;
            SQLiteConnection con = new SQLiteConnection(global::SenseCamBrowser1.Properties.Settings.Default.DBConnectionString);
            SQLiteCommand selectCmd = new SQLiteCommand(Database_Versioning.text_for_stored_procedures.spGet_user_id_of_Most_Recent_Data_Upload(), con);
            con.Open();
            try
            {
                userID_of_user_with_most_recent_data = int.Parse(selectCmd.ExecuteScalar().ToString()); //gets the userID of the user who has the most data (it's likely all other users info will be stored near the same folder location as them)...
            }
            catch (Exception excep) { }
            con.Close();

            string last_keyframe_path = "";
            //this method calls a database stored procedure to return the number of images in an event
            selectCmd = new SQLiteCommand(Database_Versioning.text_for_stored_procedures.spGet_Last_Keyframe_Path(userID_of_user_with_most_recent_data), con);
            con.Open();
            try
            {
                last_keyframe_path = selectCmd.ExecuteScalar().ToString(); //gets the path of the most recently uploaded event...
            }
            catch (Exception excep) { }
            con.Close();

            //now we examine the last_keyframe_path (if one exists)
            if (!last_keyframe_path.Equals(""))
            {
                string[] components = last_keyframe_path.Split(new string[] { @"\", "/" }, StringSplitOptions.RemoveEmptyEntries); //in case there are any web url images in there, we change the "\", "/" slashes
                last_keyframe_path = ""; //let's reset this string...
                for (int c = 0; c < components.Length - 3; c++) //so we include all elements except the last 2, which is the image name, and the direct folder holding those images
                    last_keyframe_path += components[c] + @"\"; //this is our likely SenseCam root directory...
            } //close if (!last_keyframe_path.Equals(""))...

            return last_keyframe_path + user_name + @"\";
        }



    } //close class User_Object...



} //close namespace...
