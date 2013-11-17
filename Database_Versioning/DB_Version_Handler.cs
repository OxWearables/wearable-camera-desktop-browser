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
using System.Data.SqlClient;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Configuration;

namespace SenseCamBrowser1.Database_Versioning
{
    class DB_Version_Handler
    {
        public static string DATABASE_NAME = "Oxford_CLARITY_SenseCam_May_2011";
        


        /// <summary>
        /// this method is responsible for the first time install of the required database on the person's machine...
        /// </summary>
        /// <returns>the new connection string for the database on this machine...</returns>
        public static string install_DCU_SenseCam_database()
        {
            string final_connection_string_of_installed_version = "";

            //let's just check to see if DCU_SenseCam is already installed anyways, if not...

            //step 1 check if full version is already on the machine
            string sql_full_instance_name = "(local)";
            bool is_full_version_already_installed = check_sql_server_db_is_already_installed_on_machine(sql_full_instance_name);

            if (is_full_version_already_installed)
            {
                final_connection_string_of_installed_version = install_dcu_sensecam_database_on_machine(sql_full_instance_name);
            } //close if (is_full_version_already_installed)...
            else //let's check the express version....
            {

                //step 2 check if express version is already on the machine
                string sql_express_instance_name = @".\SQLExpress";
                bool is_express_version_already_installed = check_sql_server_db_is_already_installed_on_machine(sql_express_instance_name);
                

                //firstly install sql express on the machine...
                if (!is_express_version_already_installed)
                    install_sql_express_on_machine();

                //and then install the dcu sensecam database...
                final_connection_string_of_installed_version = install_dcu_sensecam_database_on_machine(sql_express_instance_name);

            } //close else ... if (is_full_version_already_installed)...


            return final_connection_string_of_installed_version;
           
        } //close method deploy_DCU_SenseCam_with_SqlExpress()...


        public static string install_DCU_SenseCam_localDB_database()
        {
            string target_localDB = @"Data Source=(localdb)\v11.0;Integrated Security=True;AttachDBFileName=|DataDirectory|\SenseCam_Doherty_6Feb2013.mdf;";

            
            bool is_express_version_already_installed = check_sql_server_localDB_is_already_installed_on_machine(target_localDB);

            //firstly install sql express on the machine...
            //if (!is_express_version_already_installed)
              //  install_sql_express_on_machine();

            //and then install the dcu sensecam database...
            Initiate_DCU_SenseCam_Database.setup_DCU_SenseCam_localDB_database(@"Data Source=(localdb)\v11.0;Integrated Security=True;AttachDBFileName=SenseCam_Doherty_6Feb2013.mdf;");

            return target_localDB;
                        
        } //close method deploy_DCU_SenseCam_with_SqlExpress()...







        /// <summary>
        /// the purpose of this method is to update the configuration file so that it reflects the latest version of the database that we 
        /// connect to...
        /// </summary>
        /// <param name="db_already_installed"></param>
        /// <param name="latest_db_version"></param>
        /// <param name="db_connection_string"></param>
        public static void update_config_file(int db_already_installed, string db_connection_string)
        {
            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            foreach (XmlElement element in xmlDoc.DocumentElement)
            {
                if (element.Name.Equals("appSettings"))
                {
                    foreach (XmlNode node in element.ChildNodes)
                    {
                        if (node.Attributes[0].Value.Equals("db_already_installed"))
                        {
                            node.Attributes[1].Value = db_already_installed.ToString();
                        } //close if (node.Attributes[0].Value.Equals("db_already_installed"))...                        
                    } //close foreach (XmlNode node in element.ChildNodes)...
                } //close if (element.Name.Equals("appSettings"))...
                else if (element.Name.Equals("connectionStrings"))
                {
                    foreach (XmlNode node in element.ChildNodes)
                    {
                        if (node.Attributes[0].Value.Equals("SenseCamBrowser1.Properties.Settings.DCU_SenseCamConnectionString"))
                        {
                            node.Attributes[1].Value = db_connection_string;
                        } //close if (node.Attributes[0].Value.Equals("db_already_installed"))...                        
                    } //close foreach (XmlNode node in element.ChildNodes)...
                } //close if (element.Name.Equals("appSettings"))...

            } //close foreach (XmlElement element in xmlDoc.DocumentElement)...

            xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);


            //and now update the configuration manager, so rest of application reads the updated values...
            ConfigurationManager.RefreshSection("appSettings");
            ConfigurationManager.RefreshSection("connectionStrings");
        } //close method update_config_file()...





        /// <summary>
        /// this method is responsible for checking if a version of sql server is already on the machine (i.e. check for [master] db)
        /// </summary>
        /// <param name="db_data_source"></param>
        /// <returns></returns>
        private static bool check_sql_server_db_is_already_installed_on_machine(string db_data_source)
        {
            try
            {
                SqlConnection con = new SqlConnection(@"Data Source=" + db_data_source + ";Initial Catalog=master;Integrated Security=True; Connection Timeout=10");
                con.Open(); //let's open the database connection
                con.Close();
                return true;
            } //try to connect to master database...
            catch (Exception error_connecting_to_express_master_db)
            {
                return false;
            } //close catch (Exception error_connecting_to_express_master_db) ... for -> try to connect to master database...
        } //close method check_sql_server_is_already_installed_on_machine()...


        /// <summary>
        /// this method is responsible for checking if a version of sql server is already on the machine (i.e. check for [master] db)
        /// </summary>
        /// <param name="db_data_source"></param>
        /// <returns></returns>
        private static bool check_sql_server_localDB_is_already_installed_on_machine(string db_data_source)
        {
            try
            {
                SqlConnection con = new SqlConnection(db_data_source + " Connection Timeout=10");
                con.Open(); //let's open the database connection
                con.Close();
                return true;
            } //try to connect to master database...
            catch (Exception error_connecting_to_express_master_db)
            {
                return false;
            } //close catch (Exception error_connecting_to_express_master_db) ... for -> try to connect to master database...
        } //close method check_sql_server_is_already_installed_on_machine()...



        /// <summary>
        /// this method installs the DCU_SenseCam database on a given instance of SQL Server...
        /// </summary>
        /// <param name="db_data_source"></param>
        private static string install_dcu_sensecam_database_on_machine(string db_data_source)
        {
            try
            {
                SqlConnection con = new SqlConnection(@"Data Source=" + db_data_source + ";Initial Catalog=" + DATABASE_NAME + ";Integrated Security = True; Connection Timeout=10");
                con.Open(); //let's open the database connection
                con.Close();
                write_output(DATABASE_NAME+" database already installed!!");
            } //try to connect to DCU_SenseCam db...
            catch (Exception error_connecting_to_DCU_SenseCam)
            {

                write_output("No instance of "+DATABASE_NAME+" detected ... now setting up new "+DATABASE_NAME+" database ...");
                Initiate_DCU_SenseCam_Database.setup_DCU_SenseCam_database(db_data_source);
                write_output(DATABASE_NAME+" database now set up!!");

            } //end catch (Exception error_connecting_to_DCU_SenseCam) ... for -> try to connect to DCU_SenseCam db... 

            return @"Data Source=" + db_data_source + ";Initial Catalog=" + DATABASE_NAME + ";Integrated Security = True;";
        } //close method install_dcu_sensecam_database_on_machine()...




        /// <summary>
        /// this method installs SQL Express on the individual's computer...
        /// </summary>
        private static void install_sql_express_on_machine()
        {
            try
            {
                SqlConnection con = new SqlConnection(@"Data Source=.\SQLExpress;Initial Catalog="+DATABASE_NAME+";Integrated Security=True; Connection Timeout=10");
                con.Open(); //let's open the database connection
                con.Close();
                write_output(DATABASE_NAME+" database already installed!!");
            } //try to connect to DCU_SenseCam db...
            catch (Exception error_connecting_to_DCU_SenseCam)
            {

                write_output("No instance of SQL Server Express found on local server\n\n");
                write_output("starting to install SQL Server Express....");

                string file_path = "";
                if (File.Exists("SQLEXPR32.exe"))
                    file_path = "SQLEXPR32.exe";

                char double_quote = '"';
                //sqlexpr32.exe /qb INSTANCENAME="SQLExpress" INSTALLSQLDIR="C:\Program Files" INSTALLSQLSHAREDDIR="C:\Program Files" ADDLOCAL=SQL_Engine SECURITYMODE=SQL SAPWD=c1l2a3r4i5t6y7 DISABLENETWORKPROTOCOL=0 SQLCOLLATION="SQL_Latin1_General_Cp1_CI_AS" ERRORREPORTING=1
                Process pro_install_sql_server = new Process();
                pro_install_sql_server.StartInfo.FileName = file_path;
                pro_install_sql_server.StartInfo.Arguments = "/qb INSTANCENAME=" + double_quote + "SQLExpress" + double_quote + " INSTALLSQLDIR=" + double_quote + @"C:\Program Files" + double_quote + " INSTALLSQLSHAREDDIR=" + double_quote + @"C:\Program Files" + double_quote + " ADDLOCAL=SQL_Engine SECURITYMODE=SQL SAPWD=c1l2a3r4i5t6y7 DISABLENETWORKPROTOCOL=0 SQLCOLLATION=" + double_quote + "SQL_Latin1_General_Cp1_CI_AS" + double_quote + " ERRORREPORTING=1";
                pro_install_sql_server.Start();
                pro_install_sql_server.WaitForExit();

            } //end catch (Exception error_connecting_to_DCU_SenseCam) ... for -> try to connect to DCU_SenseCam db... 
        } //close method install_dcu_sensecam_database_on_machine()...




        /// <summary>
        /// this method is used to help us display messages to the user...
        /// </summary>
        /// <param name="message"></param>
        private static void write_output(string message)
        {
            //currently don't give them feedback, but this can be incorporated in...
        } //close method write_output()...


    } //end class DB_Version_Handler..

} //end namespace...
