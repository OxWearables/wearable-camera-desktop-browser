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
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Windows.Controls;
using System.Configuration;


using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Data.SQLite;
using System.Data.Common;


namespace SenseCamBrowser1
{


    public class calendar_control:IValueConverter
    {
       

        /// <summary>
        /// This method connects to the database and retrieves the list of days that are available to be displayed to the user...
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public static DateTime[] get_list_of_available_days_for_user(int userID)
        {
            //THIS METHOD SHALL RETURN ALL THE DAYS DATA THAT A USER HAS IMAGES IN
            //THIS WILL BE THEN USED IN THE CALENDAR ON THE PAGE TO SHOW THE USER WHAT
            //DAYS THEY CAN CLICK ON, BUT THIS METHOD FOCUSES ON JUST RETRIEVING THAT DATA FROM THE DATABASE

            List<string> list_of_days = new List<string>();

            //spGet_Event_Image_Paths
            SQLiteConnection con = new SQLiteConnection(global::SenseCamBrowser1.Properties.Settings.Default.DBConnectionString);
            SQLiteCommand selectCmd = new SQLiteCommand(Database_Versioning.text_for_stored_procedures.spGet_List_Of_All_Days_For_User(userID), con);
            con.Open();
            SQLiteDataReader day_reader = selectCmd.ExecuteReader();

            while (day_reader.Read())
            {
                list_of_days.Add(day_reader[0].ToString());
            } //end while (path_reader.Read())
            con.Close();

            if (list_of_days.Count == 0)
                return null; //i.e. there are no days available for this user

            DateTime[] array_of_available_days = new DateTime[list_of_days.Count];
            for (int c = 0; c < array_of_available_days.Length; c++)
                array_of_available_days[c] = DateTime.Parse(list_of_days[c]);

            return array_of_available_days;
        } //end method get_list_of_available_days_for_user()





        #region code to display the available dates on the calendar
        //this code was inspired from MS documentation -> http://msdn.microsoft.com/en-us/magazine/dd882520.aspx

        static Dictionary<DateTime, string> dict = new Dictionary<DateTime, string>();
        public calendar_control() { }
        static calendar_control()
        {
            DateTime[] tmp = get_list_of_available_days_for_user(Window1.OVERALL_userID);
            foreach (DateTime dtime in tmp)
                dict.Add(new DateTime(dtime.Year, dtime.Month, dtime.Day), "");            
        } //close static constructor method calendar_control()...


        /// <summary>
        /// This method is responsible for updating the available dates of data, as displayed on the calendar (useful to be called after new data is uploaded for user...)
        /// </summary>
        public static void update_days_on_calendar()
        {
            //this method was written by Aiden Doherty (Oxford University and Dublin City University)

            dict.Clear(); //clear out the old dictionary firstly            

            //then get the available days of data for this user...
            DateTime[] tmp = get_list_of_available_days_for_user(Window1.OVERALL_userID);

            //and finally update the dictionary with new information on the available days to click...
            foreach (DateTime dtime in tmp)
                dict.Add(new DateTime(dtime.Year, dtime.Month, dtime.Day), ""); 
        } //close method update_days_on_calendar()...
        

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text;
            if (!dict.TryGetValue((DateTime)value, out text))
                text = null;
            return text;
        } //close method Convert()...
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        } //close method ConvertBack()...

        #endregion code to display the available dates on the calendar

    } //end class calendar_control...

} //end namespace...

