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
using System.Data.SQLite;
using System.Data.Common;

namespace SenseCamBrowser1
{
    class Record_User_Interactions
    {
        /// <summary>
        /// this method is responsible for recording a user interaction in the database any time it's called (automatically logs the user and time information)
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="interaction_time"></param>
        /// <param name="uixaml_element"></param>
        /// <param name="optional_parameters"></param>
        public static void log_interaction_to_database(string uixaml_element, string optional_parameters)
        {
            //let's supress the length of the string/comment, so as not to through a "sql varchar too big"
            int max_length = 250;
            if (optional_parameters.Length > max_length)
                optional_parameters = optional_parameters.Substring(optional_parameters.Length - max_length - 1, max_length);

            SQLiteConnection con = new SQLiteConnection(global::SenseCamBrowser1.Properties.Settings.Default.DBConnectionString);
            SQLiteCommand selectCmd = new SQLiteCommand(Database_Versioning.text_for_stored_procedures.spLog_User_Interaction(Window1.OVERALL_userID,DateTime.Now,uixaml_element,optional_parameters), con);
            con.Open();
            selectCmd.ExecuteNonQuery();
            con.Close();
        } //end method log_interaction_to_database()


        /// <summary>
        /// overload method for log_interaction_to_database
        /// </summary>
        /// <param name="uixaml_element"></param>
        public static void log_interaction_to_database(string uixaml_element)
        {
            log_interaction_to_database(uixaml_element, "");
        } //close method log_interaction_to_database()...


    } //end class ...
} //close namespace...
