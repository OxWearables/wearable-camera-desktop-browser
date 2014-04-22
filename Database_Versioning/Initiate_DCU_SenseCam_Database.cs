/*
Copyright (c) 2010, CLARITY: Centre for Sensor Web Technologies, DCU (Dublin City University)
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of CLARITY: Centre for Sensor Web Technologies, DCU (Dublin City University) nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
*/

using System.Data;
using System.Data.SqlClient;
using System;

namespace SenseCamBrowser1.Database_Versioning
{
    class Initiate_DCU_SenseCam_Database
    {

        #region installing_sensecam_db_for_first_time

        public static void setup_DCU_SenseCam_database(string db_data_source)
        {
            string master_db = "master";
            string new_db_name = DB_Version_Handler.DATABASE_NAME;
            
            string master_db_connection_string = @"Data Source=" + db_data_source + ";Initial Catalog=" + master_db + ";Integrated Security=True";


            create_new_database(new_db_name, master_db_connection_string);

            //let's take a small break to give SQL server sometime to get the database created...
            System.Threading.Thread.Sleep(15000);

            string new_db_connection_string = @"Data Source=" + db_data_source + ";Initial Catalog=" + new_db_name + ";Integrated Security=True";


            add_DCU_SenseCam_tables_to_database(new_db_name, new_db_connection_string);


            //now we populate the database with some data - via "insert into" statements!!!
            add_DCU_SenseCam_data_to_tables(new_db_name, new_db_connection_string);

        } //close method setup_DCU_SenseCam_database()...



        public static void setup_DCU_SenseCam_localDB_database(string localDB_connection_string)
        {
            add_DCU_SenseCam_tables_to_database("", localDB_connection_string);
            
            //now we populate the database with some data - via "insert into" statements!!!
            add_DCU_SenseCam_data_to_tables("", localDB_connection_string);
        } //close method setup_DCU_SenseCam_localDB_database()...



        /// <summary>
        /// this method determines the sql text to register a new database with the [master] database
        /// </summary>
        /// <param name="new_db_name"></param>
        /// <returns></returns>
        private static void create_new_database(string new_db_name, string db_con_string)
        {
            SqlConnection con = new SqlConnection(db_con_string);
            con.Open(); //let's open the database connection

            string sql_text="";

            sql_text += "\n" + "USE [master]";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            sql_text += "\n" + "/****** Object:  Database [" + new_db_name + "]    Script Date: 04/15/2009 11:01:14 ******/";
            sql_text += "\n" + "CREATE DATABASE " + new_db_name;
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            
            sql_text += "\n" + "ALTER DATABASE [" + new_db_name + "] SET ANSI_NULL_DEFAULT OFF ";
            //sql_text += "\n" + "GO";

            sql_text += "\n" + "ALTER DATABASE [" + new_db_name + "] SET ANSI_NULLS OFF ";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            sql_text += "\n" + "ALTER DATABASE [" + new_db_name + "] SET ANSI_PADDING OFF ";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            sql_text += "\n" + "ALTER DATABASE [" + new_db_name + "] SET ANSI_WARNINGS OFF ";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            sql_text += "\n" + "ALTER DATABASE [" + new_db_name + "] SET ARITHABORT OFF ";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            sql_text += "\n" + "ALTER DATABASE [" + new_db_name + "] SET AUTO_CLOSE OFF ";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            sql_text += "\n" + "ALTER DATABASE [" + new_db_name + "] SET AUTO_CREATE_STATISTICS ON ";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            sql_text += "\n" + "ALTER DATABASE [" + new_db_name + "] SET AUTO_SHRINK OFF ";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            sql_text += "\n" + "ALTER DATABASE [" + new_db_name + "] SET AUTO_UPDATE_STATISTICS ON ";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            sql_text += "\n" + "ALTER DATABASE [" + new_db_name + "] SET CURSOR_CLOSE_ON_COMMIT OFF ";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            sql_text += "\n" + "ALTER DATABASE [" + new_db_name + "] SET CURSOR_DEFAULT  GLOBAL ";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            sql_text += "\n" + "ALTER DATABASE [" + new_db_name + "] SET CONCAT_NULL_YIELDS_NULL OFF ";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            sql_text += "\n" + "ALTER DATABASE [" + new_db_name + "] SET NUMERIC_ROUNDABORT OFF ";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            sql_text += "\n" + "ALTER DATABASE [" + new_db_name + "] SET QUOTED_IDENTIFIER OFF ";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            sql_text += "\n" + "ALTER DATABASE [" + new_db_name + "] SET RECURSIVE_TRIGGERS OFF ";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            sql_text += "\n" + "ALTER DATABASE [" + new_db_name + "] SET DISABLE_BROKER ";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            sql_text += "\n" + "ALTER DATABASE [" + new_db_name + "] SET AUTO_UPDATE_STATISTICS_ASYNC OFF ";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            sql_text += "\n" + "ALTER DATABASE [" + new_db_name + "] SET DATE_CORRELATION_OPTIMIZATION OFF ";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            sql_text += "\n" + "ALTER DATABASE [" + new_db_name + "] SET TRUSTWORTHY OFF ";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            sql_text += "\n" + "ALTER DATABASE [" + new_db_name + "] SET ALLOW_SNAPSHOT_ISOLATION OFF ";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            sql_text += "\n" + "ALTER DATABASE [" + new_db_name + "] SET PARAMETERIZATION SIMPLE ";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            sql_text += "\n" + "ALTER DATABASE [" + new_db_name + "] SET READ_COMMITTED_SNAPSHOT OFF ";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            //ad 23/06/09 sql_text += "\n" + "ALTER DATABASE [" + new_db_name + "] SET HONOR_BROKER_PRIORITY OFF ";
            //ad 23/06/09 execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            sql_text += "\n" + "ALTER DATABASE [" + new_db_name + "] SET  READ_WRITE ";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            sql_text += "\n" + "ALTER DATABASE [" + new_db_name + "] SET RECOVERY FULL ";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            sql_text += "\n" + "ALTER DATABASE [" + new_db_name + "] SET  MULTI_USER ";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            sql_text += "\n" + "ALTER DATABASE [" + new_db_name + "] SET PAGE_VERIFY CHECKSUM  ";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            sql_text += "\n" + "ALTER DATABASE [" + new_db_name + "] SET DB_CHAINING OFF ";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            //sql_text += "\n" + "CREATE LOGIN [" + account + "] WITH PASSWORD=N'" + password + "', DEFAULT_DATABASE=[" + new_db_name + "], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF";
            //execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";


            //let's finally close the connection!
            con.Close();
        } //close method get_sql_text_to_create_database()...


        /// <summary>
        /// This method determines the sql text to set up the DCU_SenseCam tables, users, and indices on the database
        /// </summary>
        /// <param name="new_db_name"></param>
        /// <returns></returns>
        private static void add_DCU_SenseCam_tables_to_database(string new_db_name, string db_con_string)
        {
            SqlConnection con = new SqlConnection(db_con_string);
            con.Open(); //let's open the database connection

            string sql_text="";

            //sql_text += "\n" + "/****** Object:  User [sl_testing]    Script Date: 04/17/2009 10:22:12 ******/";
            //sql_text += "\n" + "CREATE USER [" + account + "] FOR LOGIN [" + account + "];";
            //sql_text += "\n" + "EXEC sp_addrolemember N'db_owner', N'" + account + "'";
            //execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            //ad 23/06/09 sql_text += "\n" + "/****** Object:  Schema [sl_testing]    Script Date: 04/17/2009 10:22:10 ******/";
            //ad 23/06/09 sql_text += "\n" + "CREATE SCHEMA [sl_testing] AUTHORIZATION [sl_testing]";
            //ad 23/06/09 execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";


            sql_text += "\n" + "/****** Object:  Table [dbo].[All_Events]    Script Date: 05/13/2011 10:28:17 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            sql_text += "\n" + "SET ANSI_PADDING ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            

            sql_text += "\n" + "CREATE TABLE [dbo].[All_Events](";
            sql_text += "\n" + "[event_id] [int] IDENTITY(1,1) NOT NULL,";
            sql_text += "\n" + "[user_id] [int] NOT NULL,";
            sql_text += "\n" + "[day] [datetime] NOT NULL,";
            sql_text += "\n" + "[start_time] [datetime] NOT NULL,";
            sql_text += "\n" + "[end_time] [datetime] NOT NULL,";
            sql_text += "\n" + "[keyframe_path] [varchar](256) NOT NULL,";
            sql_text += "\n" + "[comment] [text] NULL,";
            sql_text += "\n" + "[number_times_viewed] [int] NULL CONSTRAINT [DF_All_Events_Num_Times_Viewed]  DEFAULT ((0))";
            sql_text += "\n" + ") ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";            
            sql_text += "\n" + "SET ANSI_PADDING OFF";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";            
            sql_text += "\n" + "CREATE CLUSTERED INDEX [IX_All_Events] ON [dbo].[All_Events] ";
            sql_text += "\n" + "(";
            sql_text += "\n" + "[day] ASC";
            sql_text += "\n" + ")WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]";            
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";            
            sql_text += "\n" + "CREATE NONCLUSTERED INDEX [IX_All_Events_1] ON [dbo].[All_Events] ";
            sql_text += "\n" + "(";
            sql_text += "\n" + "[event_id] ASC,";
            sql_text += "\n" + "[start_time] ASC";
            sql_text += "\n" + ")WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  Table [dbo].[All_Images]    Script Date: 05/13/2011 10:28:18 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET ANSI_PADDING ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "CREATE TABLE [dbo].[All_Images](";
            sql_text += "\n" + "[image_id] [int] IDENTITY(1,1) NOT NULL,";
            sql_text += "\n" + "[user_id] [int] NULL,";
            sql_text += "\n" + "[event_id] [int] NULL,";
            sql_text += "\n" + "[image_path] [varchar](200) NULL,";
            sql_text += "\n" + "[image_time] [datetime] NULL";
            sql_text += "\n" + ") ON [PRIMARY]";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET ANSI_PADDING OFF";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "CREATE CLUSTERED INDEX [IX_All_Images] ON [dbo].[All_Images] ";
            sql_text += "\n" + "(";
            sql_text += "\n" + "[event_id] ASC";
            sql_text += "\n" + ")WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  Table [dbo].[Users]    Script Date: 05/13/2011 10:28:27 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET ANSI_PADDING ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "CREATE TABLE [dbo].[Users](";
            sql_text += "\n" + "[user_id] [int] IDENTITY(1,1) NOT NULL,";
            sql_text += "\n" + "[username] [varchar](50) NULL,";
            sql_text += "\n" + "[password] [varchar](50) NULL,";
            sql_text += "\n" + "[name] [varchar](50) NULL,";
            sql_text += "\n" + "CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ";
            sql_text += "\n" + "(";
            sql_text += "\n" + "[user_id] ASC";
            sql_text += "\n" + ")WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]";
            sql_text += "\n" + ") ON [PRIMARY]";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET ANSI_PADDING OFF";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
                       
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[NOV10_CLEAR_EVENT_ANNOTATIONS]    Script Date: 05/13/2011 10:28:10 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- Create date: <Create Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "create PROCEDURE [dbo].[NOV10_CLEAR_EVENT_ANNOTATIONS] ";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT,";
            sql_text += "\n" + "@EVENT_ID AS INT";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- Insert statements for procedure here";
            sql_text += "\n" + "DELETE FROM SC_Browser_User_Annotations";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND [event_id] = @EVENT_ID";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  Table [dbo].[SC_Browser_User_Annotations]    Script Date: 05/13/2011 10:28:20 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET ANSI_PADDING ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "CREATE TABLE [dbo].[SC_Browser_User_Annotations](";
            sql_text += "\n" + "[user_id] [int] NOT NULL,";
            sql_text += "\n" + "[event_id] [int] NOT NULL,";
            sql_text += "\n" + "[timestamp] [datetime] NOT NULL,";
            sql_text += "\n" + "[annotation_name] [varchar](100) NULL,";
            sql_text += "\n" + "CONSTRAINT [PK_SC_Browser_User_Annotations] PRIMARY KEY CLUSTERED ";
            sql_text += "\n" + "(";
            sql_text += "\n" + "[user_id] ASC,";
            sql_text += "\n" + "[event_id] ASC,";
            sql_text += "\n" + "[timestamp] ASC";
            sql_text += "\n" + ")WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]";
            sql_text += "\n" + ") ON [PRIMARY]";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET ANSI_PADDING OFF";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  Table [dbo].[User_Interaction_Log]    Script Date: 05/13/2011 10:28:25 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET ANSI_PADDING ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "CREATE TABLE [dbo].[User_Interaction_Log](";
            sql_text += "\n" + "[user_id] [int] NULL,";
            sql_text += "\n" + "[interaction_time] [datetime] NULL,";
            sql_text += "\n" + "[xaml_ui_element] [varchar](80) NULL,";
            sql_text += "\n" + "[comma_seperated_parameters] [varchar](256) NULL";
            sql_text += "\n" + ") ON [PRIMARY]";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET ANSI_PADDING OFF";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  Table [dbo].[Sensor_Readings]    Script Date: 05/13/2011 10:28:24 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET ANSI_PADDING ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "CREATE TABLE [dbo].[Sensor_Readings](";
            sql_text += "\n" + "[user_id] [int] NULL,";
            sql_text += "\n" + "[event_id] [int] NULL,";
            sql_text += "\n" + "[sample_time] [datetime] NULL,";
            sql_text += "\n" + "[acc_x] [decimal](10, 4) NULL,";
            sql_text += "\n" + "[acc_y] [decimal](10, 4) NULL,";
            sql_text += "\n" + "[acc_z] [decimal](10, 4) NULL,";
            sql_text += "\n" + "[white_val] [int] NULL,";
            sql_text += "\n" + "[battery] [int] NULL,";
            sql_text += "\n" + "[temperature] [decimal](10, 4) NULL,";
            sql_text += "\n" + "[pir] [int] NULL,";
            sql_text += "\n" + "[trigger_code] [nchar](1) NULL,";
            sql_text += "\n" + "[image_name] [varchar](50) NULL,";
            sql_text += "\n" + "[mag_x] [int] NULL,";
            sql_text += "\n" + "[mag_y] [int] NULL,";
            sql_text += "\n" + "[mag_z] [int] NULL";
            sql_text += "\n" + ") ON [PRIMARY]";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET ANSI_PADDING OFF";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "CREATE CLUSTERED INDEX [IX_Sensor_Readings] ON [dbo].[Sensor_Readings] ";
            sql_text += "\n" + "(";
            sql_text += "\n" + "[user_id] ASC,";
            sql_text += "\n" + "[event_id] ASC,";
            sql_text += "\n" + "[sample_time] ASC";
            sql_text += "\n" + ")WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  Table [dbo].[Annotation_Types]    Script Date: 05/13/2011 10:28:19 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET ANSI_PADDING ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "CREATE TABLE [dbo].[Annotation_Types](";
            sql_text += "\n" + "[annotation_id] [int] IDENTITY(1,1) NOT NULL,";
            sql_text += "\n" + "[annotation_type] [varchar](100) NULL,";
            sql_text += "\n" + "[description] [varchar](150) NULL,";
            sql_text += "\n" + "CONSTRAINT [PK_Annotation_Types] PRIMARY KEY CLUSTERED ";
            sql_text += "\n" + "(";
            sql_text += "\n" + "[annotation_id] ASC";
            sql_text += "\n" + ")WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]";
            sql_text += "\n" + ") ON [PRIMARY]";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET ANSI_PADDING OFF";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "CREATE UNIQUE NONCLUSTERED INDEX [IX_Annotation_Types_1] ON [dbo].[Annotation_Types] ";
            sql_text += "\n" + "(";
            sql_text += "\n" + "[annotation_type] ASC";
            sql_text += "\n" + ")WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[spGet_Specific_Event]    Script Date: 05/13/2011 10:28:14 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- alter date: <alter Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[spGet_Specific_Event] ";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT,";
            sql_text += "\n" + "@EVENT_ID AS INT";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- Insert statements for procedure here";
            sql_text += "\n" + "SELECT event_id,";
            sql_text += "\n" + "start_time,";
            sql_text += "\n" + "end_time,";
            sql_text += "\n" + "keyframe_path,";
            sql_text += "\n" + "comment";
            sql_text += "\n" + "";
            sql_text += "\n" + "FROM All_Events";
            sql_text += "\n" + "";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND [event_id] = @EVENT_ID";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[NOV10_GET_DAILY_ACTIVITY_SUMMARY_FROM_ANNOTATIONS]    Script Date: 05/13/2011 10:28:11 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- Create date: <Create Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[NOV10_GET_DAILY_ACTIVITY_SUMMARY_FROM_ANNOTATIONS] ";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT,";
            sql_text += "\n" + "@DAY AS DATETIME";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- Insert statements for procedure here";
            sql_text += "\n" + "";
            sql_text += "\n" + "SELECT annotation_name, sum(duration_in_seconds) as total_time_spent_at_activity";
            sql_text += "\n" + "";
            sql_text += "\n" + "FROM";
            sql_text += "\n" + "(";
            sql_text += "\n" + "SELECT annotations.annotation_name, ";
            sql_text += "\n" + "DATEDIFF(SECOND, All_Events.start_time, All_Events.end_time) AS duration_in_seconds";
            sql_text += "\n" + "";
            sql_text += "\n" + "FROM SC_Browser_User_Annotations AS annotations";
            sql_text += "\n" + "";
            sql_text += "\n" + "INNER JOIN All_Events";
            sql_text += "\n" + "ON annotations.event_id = All_Events.[event_id]";
            sql_text += "\n" + "";
            sql_text += "\n" + "WHERE annotations.[user_id]=@USER_ID";
            sql_text += "\n" + "AND DATEPART(YEAR, All_Events.[day]) = DATEPART(YEAR, @DAY)";
            sql_text += "\n" + "AND DATEPART(DAYOFYEAR, All_Events.[day]) = DATEPART(DAYOFYEAR, @DAY)";
            sql_text += "\n" + ") AS inner_table";
            sql_text += "\n" + "";
            sql_text += "\n" + "GROUP BY annotation_name";
            sql_text += "\n" + "";
            sql_text += "\n" + "ORDER BY annotation_name";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[JAN11_GET_ANNOTATED_EVENTS_IN_DAY]    Script Date: 05/13/2011 10:28:10 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- Create date: <Create Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[JAN11_GET_ANNOTATED_EVENTS_IN_DAY] ";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT,";
            sql_text += "\n" + "@DAY AS DATETIME";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- Insert statements for procedure here";
            sql_text += "\n" + "";
            sql_text += "\n" + "SELECT annotations.event_id, annotations.annotation_name,";
            sql_text += "\n" + "DATEDIFF(SECOND, All_Events.start_time, All_Events.end_time) AS duration_in_seconds";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "FROM SC_Browser_User_Annotations AS annotations";
            sql_text += "\n" + "";
            sql_text += "\n" + "INNER JOIN All_Events";
            sql_text += "\n" + "ON annotations.event_id = All_Events.[event_id]";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "WHERE annotations.[user_id]=@USER_ID";
            sql_text += "\n" + "AND DATEPART(YEAR, All_Events.[day]) = DATEPART(YEAR, @DAY)";
            sql_text += "\n" + "AND DATEPART(DAYOFYEAR, All_Events.[day]) = DATEPART(DAYOFYEAR, @DAY)";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "ORDER BY All_Events.start_time";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[NOV10_GET_EVENTS_IDS_IN_DAY_FOR_GIVEN_ACTIVITY]    Script Date: 05/13/2011 10:28:11 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- Create date: <Create Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[NOV10_GET_EVENTS_IDS_IN_DAY_FOR_GIVEN_ACTIVITY] ";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT,";
            sql_text += "\n" + "@DAY AS DATETIME,";
            sql_text += "\n" + "@ANNOTATION_TYPE AS VARCHAR(100)";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- Insert statements for procedure here";
            sql_text += "\n" + "";
            sql_text += "\n" + "SELECT annotations.event_id";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "FROM SC_Browser_User_Annotations AS annotations";
            sql_text += "\n" + "INNER JOIN All_Events";
            sql_text += "\n" + "ON annotations.event_id = All_Events.event_id";
            sql_text += "\n" + "";
            sql_text += "\n" + "WHERE annotations.[user_id]=@USER_ID";
            sql_text += "\n" + "AND DATEPART(YEAR, All_Events.[day]) = DATEPART(YEAR, @DAY)";
            sql_text += "\n" + "AND DATEPART(DAYOFYEAR, All_Events.[day]) = DATEPART(DAYOFYEAR, @DAY)";
            sql_text += "\n" + "AND annotations.annotation_name=@ANNOTATION_TYPE";
            sql_text += "\n" + "";
            sql_text += "\n" + "ORDER BY annotations.event_id";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[spUpdate_Images_With_Event_ID]    Script Date: 05/13/2011 10:28:14 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- alter date: 15/10/07";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[spUpdate_Images_With_Event_ID] ";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "DECLARE @MOST_RECENT_EVENT_ID AS INT";
            sql_text += "\n" + "SET @MOST_RECENT_EVENT_ID = ( (SELECT CASE WHEN MAX(event_id) IS NOT NULL THEN MAX(event_id) ELSE 0 END FROM All_Images where [user_id] = @USER_ID) - 1 ) --WILL JUST GO BACK 1 JUST TO BE ON THE SAFE SIDE SO THAT I DON'T MISS ASSIGNING THE EVENT TO ANY IMAGE";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- Insert statements for procedure here";
            sql_text += "\n" + "UPDATE All_Images";
            sql_text += "\n" + "";
            sql_text += "\n" + "SET event_id =";
            sql_text += "\n" + "(";
            sql_text += "\n" + "SELECT top 1 event_id";
            sql_text += "\n" + "FROM All_Events";
            sql_text += "\n" + "WHERE event_id > @MOST_RECENT_EVENT_ID";
            sql_text += "\n" + "AND [user_id] = @USER_ID";
            sql_text += "\n" + "AND start_time <= All_Images.image_time AND end_time >= All_Images.image_time";
            sql_text += "\n" + ")";
            sql_text += "\n" + "";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id IS NULL;";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "--NEXT WE'LL ATTEMPT TO IDENTIFY THE EVENTS THAT EACH OF THE SENSOR READINGS BELONG TO...";
            sql_text += "\n" + "--THIS FOLLOWS THE SAME PROCESS AS THE IMAGES ABOVE, EXCEPT FOR ONE SUBTLE DIFFERENCE...";
            sql_text += "\n" + "--WHERE THE ALL_EVENTS TABLE TAKES THE START/END EVENT TIMES FROM THE ALL_IMAGES TABLE RATHER THAN SENSOR_READINGS";
            sql_text += "\n" + "--AS THE SENSOR READINGS ARE MORE FINE GRAINED THAN THE IMAGE TIMES, THIS MEANS THERE'LL BE A FEW SENSOR VALUES MISSED OUT ON (BETWEEN EVENTS)";
            sql_text += "\n" + "--THEREFORE WE LOOK FOR EVENTS THAT START +- 1 MINUTE OF THE DETECTED SENSOR TIME";
            sql_text += "\n" + "UPDATE Sensor_Readings";
            sql_text += "\n" + "SET event_id =";
            sql_text += "\n" + "(";
            sql_text += "\n" + "SELECT MAX(event_id) --IN CASE 2 EVENTS SATISFY THE CONDITION, WE'LL TAKE THE LATTER ONE OF THE TWO (I.E. THE LARGEST ID NUMBER ASSIGNED BY THE DB)";
            sql_text += "\n" + "FROM All_Events";
            sql_text += "\n" + "WHERE event_id > @MOST_RECENT_EVENT_ID";
            sql_text += "\n" + "AND [user_id] = @USER_ID";
            sql_text += "\n" + "AND DATEADD(MINUTE,-1,start_time) <= Sensor_Readings.sample_time AND DATEADD(MINUTE,1,end_time) >= Sensor_Readings.sample_time -- HERE'S WHERE WE USE ADD +-1 MINUTE, TO DEAL WITH THE FACT THAT SENSOR_READINGS ARE MORE FINE GRAINED THAN THE START/END TIME DEFINED IN THE ALL_EVENTS TABLE...";
            sql_text += "\n" + ")";
            sql_text += "\n" + "";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id IS NULL;";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- LASTLY, IF THERE'S BEEN ANY UPLOADING PROBLEM WHERE THE CAMERA WAS ACCIDENTALLY";
            sql_text += "\n" + "-- RESET ... I.E. WHERE NEW IMAGES APPEAR TO BELONG TO THE YEAR 2000";
            sql_text += "\n" + "-- THEN WHAT GENERALLY SEEMS TO HAPPEN IS THAT ONE EVENT HAS A NORMAL START TIME";
            sql_text += "\n" + "-- BUT AN END TIME SOMETIME IN THE YEAR 2000 ... SO WHAT WE NEED TO DO";
            sql_text += "\n" + "-- IS DELETE THAT PARTICULAR EVENT AS IT HAS NO IMAGES";
            sql_text += "\n" + "DELETE FROM All_Events";
            sql_text += "\n" + "WHERE [user_id]=@USER_ID";
            sql_text += "\n" + "and start_time>end_time;";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "--then IF ANY EVENTS ARE MISSED for ANY REASON (GENERALLY WHEN A CHUNK IS MINISCULE I.E. LESS THAN the TEXTTILING BOUNDARY WINDOW SIZE)";
            sql_text += "\n" + "--WE'LL GIVE ANY EVENTS TO BE IN THE SAME EVENT AS THE VERY LAST EVENT_ID";
            sql_text += "\n" + "";
            sql_text += "\n" + "--UPDATE All_Images";
            sql_text += "\n" + "--SET event_id = (SELECT MAX(event_id) FROM All_Events WHERE [user_id] = @USER_ID)";
            sql_text += "\n" + "--WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "--AND event_id IS NULL;";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[JAN_11_GET_ACC_SENSOR_VALUES]    Script Date: 05/13/2011 10:28:09 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- Create date: <Create Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[JAN_11_GET_ACC_SENSOR_VALUES]";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT,";
            sql_text += "\n" + "@DAY AS DATETIME";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- Insert statements for procedure here";
            sql_text += "\n" + "";
            sql_text += "\n" + "SELECT event_id,";
            sql_text += "\n" + "sample_time,";
            sql_text += "\n" + "acc_x,";
            sql_text += "\n" + "acc_y,";
            sql_text += "\n" + "acc_z";
            sql_text += "\n" + "";
            sql_text += "\n" + "FROM Sensor_Readings";
            sql_text += "\n" + "";
            sql_text += "\n" + "WHERE [user_id]=@USER_ID";
            sql_text += "\n" + "AND event_id IN (SELECT event_id";
            sql_text += "\n" + "FROM All_Events";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND DATEPART(YEAR, [day]) = DATEPART(YEAR, @DAY)";
            sql_text += "\n" + "AND DATEPART(DAYOFYEAR, [day]) = DATEPART(DAYOFYEAR, @DAY)";
            sql_text += "\n" + ")";
            sql_text += "\n" + "";
            sql_text += "\n" + "ORDER BY sample_time";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[spUpdate_Event_Keyframe_Path]    Script Date: 05/13/2011 10:28:14 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- alter date: <alter Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "create PROCEDURE [dbo].[spUpdate_Event_Keyframe_Path]";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT,";
            sql_text += "\n" + "@EVENT_ID AS INT,";
            sql_text += "\n" + "@KEYFRAME_PATH AS VARCHAR(256)";
            sql_text += "\n" + "";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- Insert statements for procedure here";
            sql_text += "\n" + "UPDATE All_Events";
            sql_text += "\n" + "";
            sql_text += "\n" + "SET keyframe_path = @KEYFRAME_PATH";
            sql_text += "\n" + "";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND [event_id] = @EVENT_ID";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[spDelete_Event]    Script Date: 05/13/2011 10:28:12 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- alter date: <alter Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "create PROCEDURE [dbo].[spDelete_Event]";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT,";
            sql_text += "\n" + "@EVENT_ID AS INT";
            sql_text += "\n" + "";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- Insert statements for procedure here";
            sql_text += "\n" + "DELETE";
            sql_text += "\n" + "";
            sql_text += "\n" + "FROM All_Events";
            sql_text += "\n" + "";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND [event_id] = @EVENT_ID";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[spGet_Afternoon_Events]    Script Date: 05/13/2011 10:28:12 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- alter date: <alter Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[spGet_Afternoon_Events] ";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT,";
            sql_text += "\n" + "@DAY AS DATETIME";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- Insert statements for procedure here";
            sql_text += "\n" + "SELECT event_id,";
            sql_text += "\n" + "start_time,";
            sql_text += "\n" + "end_time,";
            sql_text += "\n" + "keyframe_path,";
            sql_text += "\n" + "comment";
            sql_text += "\n" + "";
            sql_text += "\n" + "FROM All_Events";
            sql_text += "\n" + "";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND DATEPART(YEAR, [day]) = DATEPART(YEAR, @DAY)";
            sql_text += "\n" + "AND DATEPART(DAYOFYEAR, [day]) = DATEPART(DAYOFYEAR, @DAY)";
            sql_text += "\n" + "AND DATEPART(HOUR, start_time) >= 12";
            sql_text += "\n" + "AND DATEPART(HOUR, start_time) <= 17";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[spGet_All_Events_In_Day]    Script Date: 05/13/2011 10:28:12 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- alter date: <alter Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[spGet_All_Events_In_Day] ";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT,";
            sql_text += "\n" + "@DAY AS DATETIME";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- Insert statements for procedure here";
            sql_text += "\n" + "SELECT event_id,";
            sql_text += "\n" + "start_time,";
            sql_text += "\n" + "end_time,";
            sql_text += "\n" + "keyframe_path,";
            sql_text += "\n" + "comment";
            sql_text += "\n" + "";
            sql_text += "\n" + "FROM All_Events";
            sql_text += "\n" + "";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND DATEPART(YEAR, [day]) = DATEPART(YEAR, @DAY)";
            sql_text += "\n" + "AND DATEPART(DAYOFYEAR, [day]) = DATEPART(DAYOFYEAR, @DAY)";
            sql_text += "\n" + "";
            sql_text += "\n" + "order by start_time ";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[spGet_Day_Start_and_End_Times]    Script Date: 05/13/2011 10:28:12 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- alter date: <alter Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "create PROCEDURE [dbo].[spGet_Day_Start_and_End_Times] ";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT,";
            sql_text += "\n" + "@DAY AS DATETIME";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- Insert statements for procedure here";
            sql_text += "\n" + "SELECT min(start_time) as start_time, max(end_time) as end_time";
            sql_text += "\n" + "";
            sql_text += "\n" + "FROM All_Events";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND DATEPART(YEAR, [day]) = DATEPART(YEAR, @DAY)";
            sql_text += "\n" + "AND DATEPART(DAYOFYEAR, [day]) = DATEPART(DAYOFYEAR, @DAY)";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[spGet_Evening_Events]    Script Date: 05/13/2011 10:28:13 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- alter date: <alter Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[spGet_Evening_Events] ";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT,";
            sql_text += "\n" + "@DAY AS DATETIME";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- Insert statements for procedure here";
            sql_text += "\n" + "SELECT event_id,";
            sql_text += "\n" + "start_time,";
            sql_text += "\n" + "end_time,";
            sql_text += "\n" + "keyframe_path,";
            sql_text += "\n" + "comment";
            sql_text += "\n" + "";
            sql_text += "\n" + "FROM All_Events";
            sql_text += "\n" + "";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND DATEPART(YEAR, [day]) = DATEPART(YEAR, @DAY)";
            sql_text += "\n" + "AND DATEPART(DAYOFYEAR, [day]) = DATEPART(DAYOFYEAR, @DAY)";
            sql_text += "\n" + "AND DATEPART(HOUR, start_time) > 17";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[spGet_Last_Keyframe_Path]    Script Date: 05/13/2011 10:28:13 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[spGet_Last_Keyframe_Path] ";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- Insert statements for procedure here";
            sql_text += "\n" + "select top 1 keyframe_path";
            sql_text += "\n" + "from All_Events";
            sql_text += "\n" + "where [user_id] = @USER_ID";
            sql_text += "\n" + "order by [day] desc";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[spGet_List_Of_All_Days_For_User]    Script Date: 05/13/2011 10:28:13 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- alter date: <alter Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[spGet_List_Of_All_Days_For_User] ";
            sql_text += "\n" + "@USER_ID AS INT";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "SELECT MIN([day])";
            sql_text += "\n" + "FROM All_Events";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "GROUP BY DATEPART(YEAR, [day]), DATEPART(DAYOFYEAR, [day])";
            sql_text += "\n" + "ORDER BY MIN([day]) desc";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[spGet_Morning_Events]    Script Date: 05/13/2011 10:28:13 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- alter date: <alter Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[spGet_Morning_Events] ";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT,";
            sql_text += "\n" + "@DAY AS DATETIME";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- Insert statements for procedure here";
            sql_text += "\n" + "SELECT event_id,";
            sql_text += "\n" + "start_time,";
            sql_text += "\n" + "end_time,";
            sql_text += "\n" + "keyframe_path,";
            sql_text += "\n" + "comment";
            sql_text += "\n" + "";
            sql_text += "\n" + "FROM All_Events";
            sql_text += "\n" + "";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND DATEPART(YEAR, [day]) = DATEPART(YEAR, @DAY)";
            sql_text += "\n" + "AND DATEPART(DAYOFYEAR, [day]) = DATEPART(DAYOFYEAR, @DAY)";
            sql_text += "\n" + "AND DATEPART(HOUR, start_time) < 12";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[spGet_Num_Images_In_Day]    Script Date: 05/13/2011 10:28:13 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- alter date: <alter Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[spGet_Num_Images_In_Day] ";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT,";
            sql_text += "\n" + "@DAY AS DATETIME";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- Insert statements for procedure here";
            sql_text += "\n" + "SELECT COUNT(*)";
            sql_text += "\n" + "";
            sql_text += "\n" + "FROM All_Images";
            sql_text += "\n" + "";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id in";
            sql_text += "\n" + "(";
            sql_text += "\n" + "SELECT event_id";
            sql_text += "\n" + "FROM All_Events";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND DATEPART(YEAR, [day]) = DATEPART(YEAR, @DAY)";
            sql_text += "\n" + "AND DATEPART(DAYOFYEAR, [day]) = DATEPART(DAYOFYEAR, @DAY)";
            sql_text += "\n" + ")";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[spUpdateEventComment]    Script Date: 05/13/2011 10:28:15 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- alter date: <alter Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[spUpdateEventComment] ";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT,";
            sql_text += "\n" + "@EVENT_ID AS INT,";
            sql_text += "\n" + "@COMMENT AS TEXT";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- Insert statements for procedure here";
            sql_text += "\n" + "UPDATE All_Events";
            sql_text += "\n" + "";
            sql_text += "\n" + "SET comment = @COMMENT";
            sql_text += "\n" + "";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND [event_id] = @EVENT_ID";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[feb_10_spGet_UserID_of_Most_Recent_Data_Upload]    Script Date: 05/13/2011 10:28:09 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- Create date: <Create Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[feb_10_spGet_UserID_of_Most_Recent_Data_Upload]";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "select top 1 [user_id]";
            sql_text += "\n" + "from All_Events";
            sql_text += "\n" + "group by [user_id]";
            sql_text += "\n" + "order by max([day]) desc";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[feb_10_spInsert_New_User_Into_Database_and_Return_ID]    Script Date: 05/13/2011 10:28:09 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- Create date: <Create Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[feb_10_spInsert_New_User_Into_Database_and_Return_ID]";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@NEW_USER_NAME AS VARCHAR(50)";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- Insert statements for procedure here";
            sql_text += "\n" + "insert into Users values (@NEW_USER_NAME, @NEW_USER_NAME, @NEW_USER_NAME);";
            sql_text += "\n" + "";
            sql_text += "\n" + "--then using that userid, we insert a dummy event into the All_Events table";
            sql_text += "\n" + "declare @tmp_uid as int";
            sql_text += "\n" + "set @tmp_uid = (select max([user_id]) from Users)";
            sql_text += "\n" + "insert into All_Events values (@tmp_uid,'2001-01-23 08:00:00','2001-01-23 07:00:00', '2009-01-23 08:00:00', '2009-01-23 08:05:00', '','test event',0);";
            sql_text += "\n" + "";
            sql_text += "\n" + "--then using the userid and the newly created Event_ID, we then insert a dummy image in the All_Images table…";
            sql_text += "\n" + "declare @tmp_eid as int";
            sql_text += "\n" + "set @tmp_eid = (select max(event_id) from All_Events where [user_id]=@tmp_uid)";
            sql_text += "\n" + "insert into All_Images values (@tmp_uid, @tmp_eid,'','2001-01-23 08:02:00');";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "--and FINALLY RETURN the ID for THIS NEW USER…";
            sql_text += "\n" + "select top 1 [user_id] from Users where [name]=@NEW_USER_NAME;";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[feb10_spUpdateEvent_Number_Times_Viewed]    Script Date: 05/13/2011 10:28:09 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- Create date: <Create Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[feb10_spUpdateEvent_Number_Times_Viewed]";
            sql_text += "\n" + "@USER_ID AS INT,";
            sql_text += "\n" + "@EVENT_ID AS INT";
            sql_text += "\n" + "";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "--let's now update the number of times that this event has been viewed";
            sql_text += "\n" + "UPDATE All_Events";
            sql_text += "\n" + "SET number_times_viewed = number_times_viewed + 1";
            sql_text += "\n" + "WHERE [user_id]= @USER_ID";
            sql_text += "\n" + "and event_id = @EVENT_ID;";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[Oct10_UPDATE_EVENT_KEYFRAME_IMAGE]    Script Date: 05/13/2011 10:28:12 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- Create date: <Create Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[Oct10_UPDATE_EVENT_KEYFRAME_IMAGE] ";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT,";
            sql_text += "\n" + "@EVENT_ID AS INT";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "--STEP 1. FIRSTLY LET'S DETERMINE THE START/END TIMES OF THE EVENT...";
            sql_text += "\n" + "DECLARE @NEW_START_TIME AS DATETIME";
            sql_text += "\n" + "DECLARE @NEW_END_TIME AS DATETIME ";
            sql_text += "\n" + "";
            sql_text += "\n" + "--firstly the event which has the images added to it...";
            sql_text += "\n" + "SELECT @NEW_START_TIME = start_time, @NEW_END_TIME = end_time";
            sql_text += "\n" + "FROM All_Events";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id = @EVENT_ID";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "--STEP 2. FROM THE START/END TIMES, LET'S SET A TARGET (MIDDLE) TIME FROM WHICH TO SELECT THE KEYFRAME...";
            sql_text += "\n" + "DECLARE @NEW_KEYFRAME_TARGET_TIME AS DATETIME";
            sql_text += "\n" + "SET @NEW_KEYFRAME_TARGET_TIME = DATEADD(MINUTE, DATEDIFF(MINUTE,@NEW_START_TIME, @NEW_END_TIME)/2, @NEW_START_TIME)";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- also let's declare an allowable bit of time leeway from which to select the keyframe image...";
            sql_text += "\n" + "DECLARE @ALLOWABLE_TIME_WINDOW_FOR_KEYFRAME_AROUND_TARGET_TIME_IN_MINUTES AS INT";
            sql_text += "\n" + "SET @ALLOWABLE_TIME_WINDOW_FOR_KEYFRAME_AROUND_TARGET_TIME_IN_MINUTES=1";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- STEP 3. CONSIDERING OUR TARGET_TIME, AND THE AMOUND OF LEEWAY, LET'S SELECT A RANDOM IMAGE FROM AROUND THIS TIME TO BE OUR KEYFRAME PATH";
            sql_text += "\n" + "DECLARE @NEW_KEYFRAME_PATH AS VARCHAR(255)";
            sql_text += "\n" + "SET @NEW_KEYFRAME_PATH = ";
            sql_text += "\n" + "(";
            sql_text += "\n" + "SELECT TOP 1 image_path";
            sql_text += "\n" + "FROM All_Images";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id = @EVENT_ID";
            sql_text += "\n" + "AND DATEDIFF(MINUTE, image_time, @NEW_KEYFRAME_TARGET_TIME) > -@ALLOWABLE_TIME_WINDOW_FOR_KEYFRAME_AROUND_TARGET_TIME_IN_MINUTES";
            sql_text += "\n" + "AND DATEDIFF(MINUTE, image_time, @NEW_KEYFRAME_TARGET_TIME) < @ALLOWABLE_TIME_WINDOW_FOR_KEYFRAME_AROUND_TARGET_TIME_IN_MINUTES";
            sql_text += "\n" + "ORDER BY NEWID()";
            sql_text += "\n" + ")";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- STEP 4. JUST IN CASE, STEP 3 DIDN'T PRODUCE A KEYFRAME IMAGE (e.g. no images taken around that time window in the middle as privacy button was hit on the camera)";
            sql_text += "\n" + "--";
            sql_text += "\n" + "IF @NEW_KEYFRAME_PATH IS NULL";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "SET @NEW_KEYFRAME_PATH = ";
            sql_text += "\n" + "(";
            sql_text += "\n" + "SELECT TOP 1 image_path";
            sql_text += "\n" + "FROM All_Images";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id = @EVENT_ID";
            sql_text += "\n" + "ORDER BY NEWID()";
            sql_text += "\n" + ")";
            sql_text += "\n" + "END";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- STEP 5. AND LASTLY LET'S UPDATE THE All_Events TABLE WITH THE NEW KEYFRAME PATH INFORMATION!";
            sql_text += "\n" + "UPDATE All_Events";
            sql_text += "\n" + "SET keyframe_path = @NEW_KEYFRAME_PATH";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id = @EVENT_ID";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[JAN_11_GET_IMAGE_IN_DAY_NEAREST_TARGET_TIME]    Script Date: 05/13/2011 10:28:09 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- Create date: <Create Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[JAN_11_GET_IMAGE_IN_DAY_NEAREST_TARGET_TIME]";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT,";
            sql_text += "\n" + "@DAY AS DATETIME, --NEED DIFFERENT VARIABLES FOR DAY & TARGET TIME, JUST IN CASE FOR CASES OF DATA AROUND MIDNIGHT...";
            sql_text += "\n" + "@TARGET_TIME AS DATETIME,";
            sql_text += "\n" + "@SEARCH_WINDOW_IN_MINUTES AS INT";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- Insert statements for procedure here";
            sql_text += "\n" + "SELECT TOP 1 image_path";
            sql_text += "\n" + "";
            sql_text += "\n" + "FROM All_Images";
            sql_text += "\n" + "";
            sql_text += "\n" + "WHERE [user_id]=@USER_ID";
            sql_text += "\n" + "AND event_id IN (SELECT event_id";
            sql_text += "\n" + "FROM All_Events";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND DATEPART(YEAR, [day]) = DATEPART(YEAR, @DAY)";
            sql_text += "\n" + "AND DATEPART(DAYOFYEAR, [day]) = DATEPART(DAYOFYEAR, @DAY)";
            sql_text += "\n" + "--NEED DIFFERENT VARIABLES FOR @DAY & @TARGET TIME, JUST IN CASE FOR CASES OF DATA AROUND MIDNIGHT...";
            sql_text += "\n" + "--THE REASON FOR THIS IS THAT IF THE @TARGET_TIME IS AFTER MIDNIGHT,";
            sql_text += "\n" + "--BUT ALL THE EVENTS WILL HAVE A @DAY TIME OF BEFORE MIDNIGHT, SO CAN'T CARRY OUT THE SEARCH ABOVE";
            sql_text += "\n" + "--ALSO CAN'T USE START/END TIMES AS THEY COULD BE EITHER SIDE OF MIDNIGHT...";
            sql_text += "\n" + ")";
            sql_text += "\n" + "AND image_time >= DATEADD(MINUTE,-@SEARCH_WINDOW_IN_MINUTES,@TARGET_TIME)";
            sql_text += "\n" + "AND image_time <= DATEADD(MINUTE,@SEARCH_WINDOW_IN_MINUTES,@TARGET_TIME)";
            sql_text += "\n" + "";
            sql_text += "\n" + "ORDER BY ABS(DATEDIFF(SECOND,image_time,@TARGET_TIME))";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[JAN_11_GET_IMAGE_IN_EVENT_NEAREST_TARGET_TIME]    Script Date: 05/13/2011 10:28:10 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- Create date: <Create Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[JAN_11_GET_IMAGE_IN_EVENT_NEAREST_TARGET_TIME]";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT,";
            sql_text += "\n" + "@EVENT_ID AS INT,";
            sql_text += "\n" + "@TARGET_TIME AS DATETIME,";
            sql_text += "\n" + "@SEARCH_WINDOW_IN_MINUTES AS INT";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- Insert statements for procedure here";
            sql_text += "\n" + "SELECT TOP 1 image_path";
            sql_text += "\n" + "";
            sql_text += "\n" + "FROM All_Images";
            sql_text += "\n" + "";
            sql_text += "\n" + "WHERE [user_id]=@USER_ID";
            sql_text += "\n" + "AND event_id = @EVENT_ID";
            sql_text += "\n" + "AND image_time >= DATEADD(MINUTE,-@SEARCH_WINDOW_IN_MINUTES,@TARGET_TIME)";
            sql_text += "\n" + "AND image_time <= DATEADD(MINUTE,@SEARCH_WINDOW_IN_MINUTES,@TARGET_TIME)";
            sql_text += "\n" + "";
            sql_text += "\n" + "ORDER BY ABS(DATEDIFF(SECOND,image_time,@TARGET_TIME))";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[spGet_Num_Images_In_Event]    Script Date: 05/13/2011 10:28:13 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- alter date: <alter Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[spGet_Num_Images_In_Event] ";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT,";
            sql_text += "\n" + "@EVENT_ID AS INT";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- Insert statements for procedure here";
            sql_text += "\n" + "SELECT COUNT(image_id)";
            sql_text += "\n" + "";
            sql_text += "\n" + "FROM All_Images";
            sql_text += "\n" + "";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id = @EVENT_ID";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[spGet_Paths_Of_All_Images_In_Events]    Script Date: 05/13/2011 10:28:14 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- alter date: <alter Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[spGet_Paths_Of_All_Images_In_Events] ";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT,";
            sql_text += "\n" + "@EVENT_ID AS INT";
            sql_text += "\n" + "";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- Insert statements for procedure here";
            sql_text += "\n" + "SELECT image_path, image_time";
            sql_text += "\n" + "";
            sql_text += "\n" + "FROM All_Images";
            sql_text += "\n" + "";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND [event_id] = @EVENT_ID";
            sql_text += "\n" + "";
            sql_text += "\n" + "ORDER BY image_time";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[spDelete_Image_From_Event]    Script Date: 05/13/2011 10:28:12 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- alter date: <alter Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "create PROCEDURE [dbo].[spDelete_Image_From_Event]";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT,";
            sql_text += "\n" + "@EVENT_ID AS INT,";
            sql_text += "\n" + "@IMAGE_TIME AS DATETIME";
            sql_text += "\n" + "";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- Insert statements for procedure here";
            sql_text += "\n" + "DELETE";
            sql_text += "\n" + "";
            sql_text += "\n" + "FROM All_Images";
            sql_text += "\n" + "";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND [event_id] = @EVENT_ID";
            sql_text += "\n" + "AND image_time = @IMAGE_TIME";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[feb_10_spGet_List_Of_Users]    Script Date: 05/13/2011 10:28:08 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- Create date: <Create Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[feb_10_spGet_List_Of_Users]";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "select [user_id], username, [password], [name]";
            sql_text += "\n" + "from Users";
            sql_text += "\n" + "order by [name]";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[spValidate_User]    Script Date: 05/13/2011 10:28:15 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- alter date: <alter Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[spValidate_User] ";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USERNAME AS VARCHAR(10),";
            sql_text += "\n" + "@PASSWORD AS VARCHAR(10)";
            sql_text += "\n" + "";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- Insert statements for procedure here";
            sql_text += "\n" + "SELECT [user_id]";
            sql_text += "\n" + "";
            sql_text += "\n" + "FROM Users";
            sql_text += "\n" + "";
            sql_text += "\n" + "WHERE username = @USERNAME";
            sql_text += "\n" + "AND password = @PASSWORD";
            sql_text += "\n" + "END;";
            sql_text += "\n" + "";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[NOV10_ADD_EVENT_ANNOTATION]    Script Date: 05/13/2011 10:28:10 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- Create date: <Create Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[NOV10_ADD_EVENT_ANNOTATION] ";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT,";
            sql_text += "\n" + "@EVENT_ID AS INT,";
            sql_text += "\n" + "@EVENT_ANNOTATION_NAME AS VARCHAR(100)";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- Insert statements for procedure here";
            sql_text += "\n" + "INSERT INTO SC_Browser_User_Annotations";
            sql_text += "\n" + "VALUES (@USER_ID, @EVENT_ID, GETDATE(), @EVENT_ANNOTATION_NAME)";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[NOV10_GET_ANNOTATIONS_FOR_EVENT]    Script Date: 05/13/2011 10:28:10 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- Create date: <Create Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[NOV10_GET_ANNOTATIONS_FOR_EVENT] ";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT,";
            sql_text += "\n" + "@EVENT_ID AS INT";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- Insert statements for procedure here";
            sql_text += "\n" + "SELECT annotation_name";
            sql_text += "\n" + "";
            sql_text += "\n" + "FROM SC_Browser_User_Annotations AS annotations";
            sql_text += "\n" + "";
            sql_text += "\n" + "WHERE annotations.[user_id]=@USER_ID";
            sql_text += "\n" + "AND annotations.event_id=@EVENT_ID";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[spLog_User_Interaction]    Script Date: 05/13/2011 10:28:14 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- Create date: <Create Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[spLog_User_Interaction]";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT,";
            sql_text += "\n" + "@INTERACTION_TIME AS DATETIME,";
            sql_text += "\n" + "@UIXAML_ELEMENT AS VARCHAR(80),";
            sql_text += "\n" + "@COMMA_SEPERATED_PARAMETERS AS VARCHAR(255)";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- Insert statements for procedure here";
            sql_text += "\n" + "INSERT INTO User_Interaction_Log";
            sql_text += "\n" + "VALUES(@USER_ID, @INTERACTION_TIME, @UIXAML_ELEMENT, @COMMA_SEPERATED_PARAMETERS)";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[NOV10_GET_LIST_OF_ANNOTATION_CLASSES]    Script Date: 05/13/2011 10:28:11 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- Create date: <Create Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[NOV10_GET_LIST_OF_ANNOTATION_CLASSES] ";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- Insert statements for procedure here";
            sql_text += "\n" + "SELECT annotation_id, annotation_type, [description]";
            sql_text += "\n" + "FROM Annotation_Types";
            sql_text += "\n" + "order by annotation_type";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[APR11_REMOVE_ANNOTATION_TYPE]    Script Date: 05/13/2011 10:28:08 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- Create date: <Create Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[APR11_REMOVE_ANNOTATION_TYPE] ";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@ANNOTATION_TYPE_NAME AS VARCHAR(50)";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "--now delete the relevant entry";
            sql_text += "\n" + "DELETE FROM Annotation_Types";
            sql_text += "\n" + "WHERE annotation_type = @ANNOTATION_TYPE_NAME";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[APR11_REMOVE_ALL_ANNOTATION_TYPES]    Script Date: 05/13/2011 10:28:08 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- Create date: <Create Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[APR11_REMOVE_ALL_ANNOTATION_TYPES] ";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "--now delete all entries in the annotation types table";
            sql_text += "\n" + "DELETE FROM Annotation_Types";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[Oct10_ADD_NEW_MERGED_IMAGES_TO_NEXT_EVENT]    Script Date: 05/13/2011 10:28:11 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- Create date: <Create Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[Oct10_ADD_NEW_MERGED_IMAGES_TO_NEXT_EVENT] ";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT,";
            sql_text += "\n" + "@EVENT_ID_OF_NEW_SOURCE_IMAGES AS INT,";
            sql_text += "\n" + "@TIME_OF_START_IMAGE AS DATETIME";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "DECLARE @DAY_OF_SOURCE_EVENT AS DATETIME";
            sql_text += "\n" + "SET @DAY_OF_SOURCE_EVENT = (SELECT [day] FROM All_Events WHERE [user_id]=@USER_ID AND event_id=@EVENT_ID_OF_NEW_SOURCE_IMAGES);";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- step 1, identify the ID of the next event...";
            sql_text += "\n" + "DECLARE @EVENT_ID_TO_APPEND_IMAGES_TO AS INT";
            sql_text += "\n" + "SET @EVENT_ID_TO_APPEND_IMAGES_TO = (SELECT TOP 1 event_id";
            sql_text += "\n" + "FROM All_Events";
            sql_text += "\n" + "WHERE [user_id]=@USER_ID";
            sql_text += "\n" + "AND event_id!=@EVENT_ID_OF_NEW_SOURCE_IMAGES";
            sql_text += "\n" + "AND start_time > @TIME_OF_START_IMAGE";
            sql_text += "\n" + "AND start_time <= DATEADD(HOUR,6,@TIME_OF_START_IMAGE)";
            sql_text += "\n" + "AND [day] = @DAY_OF_SOURCE_EVENT";
            sql_text += "\n" + "ORDER BY start_time --order ascending, i.e. select the next event (that's our target)";
            sql_text += "\n" + ");";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- STEP 2, CHECK TO SEE IF THERE'S NO NEXT EVENT ... IN THIS CASE WE'LL HAVE TO ADD IN A NEW ONE...";
            sql_text += "\n" + "IF @EVENT_ID_TO_APPEND_IMAGES_TO IS NULL";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "--AND LET'S INSERT A NEW EVENT IN TO OUR EVENTS TABLE... ";
            sql_text += "\n" + "INSERT INTO All_Events VALUES (@USER_ID, @DAY_OF_SOURCE_EVENT, @DAY_OF_SOURCE_EVENT, @DAY_OF_SOURCE_EVENT, @DAY_OF_SOURCE_EVENT, '', NULL, 0)";
            sql_text += "\n" + "";
            sql_text += "\n" + "--AND NOW OUR NEW EVENT TO APPEND THINGS TO, WILL BE THIS EVENT HERE...";
            sql_text += "\n" + "SET @EVENT_ID_TO_APPEND_IMAGES_TO = (SELECT MAX(Event_ID) FROM All_Events WHERE [user_id]=@USER_ID);";
            sql_text += "\n" + "END";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "--step 2, update the All_Images table, to change the given images in the event_id_source_of_new_images to the ID of their new event";
            sql_text += "\n" + "UPDATE All_Images";
            sql_text += "\n" + "SET event_id = @EVENT_ID_TO_APPEND_IMAGES_TO";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id = @EVENT_ID_OF_NEW_SOURCE_IMAGES";
            sql_text += "\n" + "AND image_time >= @TIME_OF_START_IMAGE --i.e. all images including and after this image time...";
            sql_text += "\n" + "";
            sql_text += "\n" + "--ALSO UPDATE THE SENSOR_READINGS TABLE...";
            sql_text += "\n" + "UPDATE Sensor_Readings";
            sql_text += "\n" + "SET event_id = @EVENT_ID_TO_APPEND_IMAGES_TO";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id = @EVENT_ID_OF_NEW_SOURCE_IMAGES";
            sql_text += "\n" + "AND sample_time >= @TIME_OF_START_IMAGE --i.e. all images including and after this image time...";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- step 3, update the start/end time, plus the keyframe path, of the two events in question ...";
            sql_text += "\n" + "DECLARE @NEW_START_TIME AS DATETIME";
            sql_text += "\n" + "DECLARE @NEW_END_TIME AS DATETIME ";
            sql_text += "\n" + "";
            sql_text += "\n" + "--firstly the event which has the images added to it...";
            sql_text += "\n" + "SELECT @NEW_START_TIME = MIN(image_time), @NEW_END_TIME = MAX(image_time)";
            sql_text += "\n" + "FROM All_Images";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id = @EVENT_ID_TO_APPEND_IMAGES_TO";
            sql_text += "\n" + "";
            sql_text += "\n" + "UPDATE All_Events";
            sql_text += "\n" + "SET start_time = @NEW_START_TIME, end_time = @NEW_END_TIME";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id = @EVENT_ID_TO_APPEND_IMAGES_TO;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- and let's update the keyframe image for the event by calling this stored procedure";
            sql_text += "\n" + "EXEC Oct10_UPDATE_EVENT_KEYFRAME_IMAGE @USER_ID, @EVENT_ID_TO_APPEND_IMAGES_TO;";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "--and secondly the event which has the images removed from it...";
            sql_text += "\n" + "SELECT @NEW_START_TIME = MIN(image_time), @NEW_END_TIME = MAX(image_time)";
            sql_text += "\n" + "FROM All_Images";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id = @EVENT_ID_OF_NEW_SOURCE_IMAGES";
            sql_text += "\n" + "";
            sql_text += "\n" + "--if the start_time is null, then that means that there's no images left in the source event";
            sql_text += "\n" + "-- therefore we'll delete it";
            sql_text += "\n" + "IF @NEW_START_TIME IS NULL";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "DELETE FROM All_Events";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id = @EVENT_ID_OF_NEW_SOURCE_IMAGES;";
            sql_text += "\n" + "END --CLOSE IF @NEW_START_TIME IS NULL...";
            sql_text += "\n" + "";
            sql_text += "\n" + "ELSE -- however for most scenarios it's more likely that there'll still be images left";
            sql_text += "\n" + "--therefore we'll update the start/end times, plus the keyframe path too";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "UPDATE All_Events";
            sql_text += "\n" + "SET start_time = @NEW_START_TIME, end_time = @NEW_END_TIME";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id = @EVENT_ID_OF_NEW_SOURCE_IMAGES;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- and let's update the keyframe image for the event by calling this stored procedure";
            sql_text += "\n" + "EXEC Oct10_UPDATE_EVENT_KEYFRAME_IMAGE @USER_ID, @EVENT_ID_OF_NEW_SOURCE_IMAGES;";
            sql_text += "\n" + "END --CLOSE ELSE ... IF @NEW_START_TIME IS NULL";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT]    Script Date: 05/13/2011 10:28:11 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- Create date: <Create Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[Oct10_ADD_NEW_MERGED_IMAGES_TO_PREVIOUS_EVENT] ";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT,";
            sql_text += "\n" + "@EVENT_ID_OF_NEW_SOURCE_IMAGES AS INT,";
            sql_text += "\n" + "@TIME_OF_END_IMAGE AS DATETIME";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "DECLARE @DAY_OF_SOURCE_EVENT AS DATETIME";
            sql_text += "\n" + "SET @DAY_OF_SOURCE_EVENT = (SELECT [day] FROM All_Events WHERE [user_id]=@USER_ID AND event_id=@EVENT_ID_OF_NEW_SOURCE_IMAGES);";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- step 1, identify the ID of the previous event...";
            sql_text += "\n" + "DECLARE @EVENT_ID_TO_APPEND_IMAGES_TO AS INT";
            sql_text += "\n" + "SET @EVENT_ID_TO_APPEND_IMAGES_TO = (SELECT TOP 1 event_id";
            sql_text += "\n" + "FROM All_Events";
            sql_text += "\n" + "WHERE [user_id]=@USER_ID";
            sql_text += "\n" + "AND event_id!=@EVENT_ID_OF_NEW_SOURCE_IMAGES --ad update on 25/01/10";
            sql_text += "\n" + "AND start_time >= DATEADD(HOUR,-6,@TIME_OF_END_IMAGE)";
            sql_text += "\n" + "AND start_time < @TIME_OF_END_IMAGE";
            sql_text += "\n" + "AND [day] = @DAY_OF_SOURCE_EVENT";
            sql_text += "\n" + "ORDER BY start_time DESC";
            sql_text += "\n" + ");";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- STEP 2, CHECK TO SEE IF THERE'S NO PREVIOUS EVENT ... IN THIS CASE WE'LL HAVE TO ADD IN A NEW ONE...";
            sql_text += "\n" + "IF @EVENT_ID_TO_APPEND_IMAGES_TO IS NULL";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "--AND LET'S INSERT A NEW EVENT IN TO OUR EVENTS TABLE... ";
            sql_text += "\n" + "INSERT INTO All_Events VALUES (@USER_ID, @DAY_OF_SOURCE_EVENT, @DAY_OF_SOURCE_EVENT, @DAY_OF_SOURCE_EVENT, @DAY_OF_SOURCE_EVENT, '', NULL, 0)";
            sql_text += "\n" + "";
            sql_text += "\n" + "--AND NOW OUR NEW EVENT TO APPEND THINGS TO, WILL BE THIS EVENT HERE...";
            sql_text += "\n" + "SET @EVENT_ID_TO_APPEND_IMAGES_TO = (SELECT MAX(Event_ID) FROM All_Events WHERE [user_id]=@USER_ID);";
            sql_text += "\n" + "END";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "--step 3, update the All_Images table, to change the given images in the event_id_source_of_new_images to the ID of their new event";
            sql_text += "\n" + "UPDATE All_Images";
            sql_text += "\n" + "SET event_id = @EVENT_ID_TO_APPEND_IMAGES_TO";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id = @EVENT_ID_OF_NEW_SOURCE_IMAGES";
            sql_text += "\n" + "AND image_time <= @TIME_OF_END_IMAGE";
            sql_text += "\n" + "";
            sql_text += "\n" + "UPDATE Sensor_Readings";
            sql_text += "\n" + "SET event_id = @EVENT_ID_TO_APPEND_IMAGES_TO";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id = @EVENT_ID_OF_NEW_SOURCE_IMAGES";
            sql_text += "\n" + "AND sample_time <= @TIME_OF_END_IMAGE";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- step 4, update the start/end time, plus the keyframe path, of the two events in question ...";
            sql_text += "\n" + "DECLARE @NEW_START_TIME AS DATETIME";
            sql_text += "\n" + "DECLARE @NEW_END_TIME AS DATETIME ";
            sql_text += "\n" + "";
            sql_text += "\n" + "--firstly the event which has the images added to it...";
            sql_text += "\n" + "SELECT @NEW_START_TIME = MIN(image_time), @NEW_END_TIME = MAX(image_time)";
            sql_text += "\n" + "FROM All_Images";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id = @EVENT_ID_TO_APPEND_IMAGES_TO";
            sql_text += "\n" + "";
            sql_text += "\n" + "UPDATE All_Events";
            sql_text += "\n" + "SET start_time = @NEW_START_TIME, end_time = @NEW_END_TIME";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id = @EVENT_ID_TO_APPEND_IMAGES_TO;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- and let's update the keyframe image for the event by calling this stored procedure";
            sql_text += "\n" + "EXEC Oct10_UPDATE_EVENT_KEYFRAME_IMAGE @USER_ID, @EVENT_ID_TO_APPEND_IMAGES_TO;";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "--and secondly the event which has the images removed from it...";
            sql_text += "\n" + "SELECT @NEW_START_TIME = MIN(image_time), @NEW_END_TIME = MAX(image_time)";
            sql_text += "\n" + "FROM All_Images";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id = @EVENT_ID_OF_NEW_SOURCE_IMAGES";
            sql_text += "\n" + "";
            sql_text += "\n" + "--if the start_time is null, then that means that there's no images left in the source event";
            sql_text += "\n" + "-- therefore we'll delete it";
            sql_text += "\n" + "IF @NEW_START_TIME IS NULL";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "DELETE FROM All_Events";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id = @EVENT_ID_OF_NEW_SOURCE_IMAGES;";
            sql_text += "\n" + "END --CLOSE IF @NEW_START_TIME IS NULL...";
            sql_text += "\n" + "";
            sql_text += "\n" + "ELSE -- however for most scenarios it's more likely that there'll still be images left";
            sql_text += "\n" + "--therefore we'll update the start/end times, plus the keyframe path too";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "UPDATE All_Events";
            sql_text += "\n" + "SET start_time = @NEW_START_TIME, end_time = @NEW_END_TIME";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id = @EVENT_ID_OF_NEW_SOURCE_IMAGES;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- and let's update the keyframe image for the event by calling this stored procedure";
            sql_text += "\n" + "EXEC Oct10_UPDATE_EVENT_KEYFRAME_IMAGE @USER_ID, @EVENT_ID_OF_NEW_SOURCE_IMAGES;";
            sql_text += "\n" + "END --CLOSE ELSE ... IF @NEW_START_TIME IS NULL";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[Jan11_SPLIT_EVENT_INTO_TWO]    Script Date: 05/13/2011 10:28:10 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- Create date: <Create Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[Jan11_SPLIT_EVENT_INTO_TWO] ";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@USER_ID AS INT,";
            sql_text += "\n" + "@EVENT_ID_OF_SOURCE_IMAGES AS INT,";
            sql_text += "\n" + "@TIME_OF_START_IMAGE AS DATETIME";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "--VERY FIRST OF ALL, LET'S MAKE SURE THAT THE USER HASN'T SELECTED THE VERY FIRST IMAGE OF THE EVENT,";
            sql_text += "\n" + "--AND THEN TRIED A SPLIT (AS WE TAKE THE IMAGE SELECTED AND ALL THE ONES AFTER THAT TO BE THE NEW EVENT)";
            sql_text += "\n" + "-- IN THIS CASE THERE'S NO POINT IN TRYING TO SPILT THE EVENT, AS THERE'S NOTHING TO SPLIT...";
            sql_text += "\n" + "DECLARE @EVENT_START_TIME AS DATETIME";
            sql_text += "\n" + "SET @EVENT_START_TIME = (SELECT start_time FROM All_Events WHERE [user_id]=@USER_ID AND event_id=@EVENT_ID_OF_SOURCE_IMAGES)";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- NOW DOING CHECK TO MAKE SURE THAT IT ISN'T THE VERY START IMAGE IN THE EVENT THAT WE'RE USING...";
            sql_text += "\n" + "IF(@TIME_OF_START_IMAGE > @EVENT_START_TIME AND @EVENT_START_TIME IS NOT NULL)";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "DECLARE @DAY_OF_SOURCE_EVENT AS DATETIME";
            sql_text += "\n" + "SET @DAY_OF_SOURCE_EVENT = (SELECT [day] FROM All_Events WHERE [user_id]=@USER_ID AND event_id=@EVENT_ID_OF_SOURCE_IMAGES);";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- step 1, identify the ID of the new event...";
            sql_text += "\n" + "DECLARE @EVENT_ID_TO_APPEND_IMAGES_TO AS INT";
            sql_text += "\n" + "";
            sql_text += "\n" + "--AND LET'S INSERT A NEW EVENT IN TO OUR EVENTS TABLE... ";
            sql_text += "\n" + "INSERT INTO All_Events VALUES (@USER_ID, @DAY_OF_SOURCE_EVENT, @DAY_OF_SOURCE_EVENT, @DAY_OF_SOURCE_EVENT, @DAY_OF_SOURCE_EVENT, '', NULL, 0)";
            sql_text += "\n" + "";
            sql_text += "\n" + "--AND NOW OUR NEW EVENT TO APPEND THE END IMAGES TO, WILL BE THIS EVENT HERE...";
            sql_text += "\n" + "SET @EVENT_ID_TO_APPEND_IMAGES_TO = (SELECT MAX(Event_ID) FROM All_Events WHERE [user_id]=@USER_ID AND [day]=@DAY_OF_SOURCE_EVENT);";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "--step 2, update the All_Images table, to change the given images in the event_id_source_of_new_images to the ID of their new event";
            sql_text += "\n" + "UPDATE All_Images";
            sql_text += "\n" + "SET event_id = @EVENT_ID_TO_APPEND_IMAGES_TO";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id = @EVENT_ID_OF_SOURCE_IMAGES";
            sql_text += "\n" + "AND image_time >= @TIME_OF_START_IMAGE --i.e. all images including and after this image time...";
            sql_text += "\n" + "";
            sql_text += "\n" + "--ALSO UPDATE THE SENSOR_READINGS TABLE...";
            sql_text += "\n" + "UPDATE Sensor_Readings";
            sql_text += "\n" + "SET event_id = @EVENT_ID_TO_APPEND_IMAGES_TO";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id = @EVENT_ID_OF_SOURCE_IMAGES";
            sql_text += "\n" + "AND sample_time >= @TIME_OF_START_IMAGE --i.e. all images including and after this image time...";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- step 3, update the start/end time, plus the keyframe path, of the two events in question ...";
            sql_text += "\n" + "DECLARE @NEW_START_TIME AS DATETIME";
            sql_text += "\n" + "DECLARE @NEW_END_TIME AS DATETIME ";
            sql_text += "\n" + "";
            sql_text += "\n" + "--firstly the event which has the images added to it...";
            sql_text += "\n" + "SELECT @NEW_START_TIME = MIN(image_time), @NEW_END_TIME = MAX(image_time)";
            sql_text += "\n" + "FROM All_Images";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id = @EVENT_ID_TO_APPEND_IMAGES_TO";
            sql_text += "\n" + "";
            sql_text += "\n" + "UPDATE All_Events";
            sql_text += "\n" + "SET start_time = @NEW_START_TIME, end_time = @NEW_END_TIME";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id = @EVENT_ID_TO_APPEND_IMAGES_TO;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- and let's update the keyframe image for the event by calling this stored procedure";
            sql_text += "\n" + "EXEC Oct10_UPDATE_EVENT_KEYFRAME_IMAGE @USER_ID, @EVENT_ID_TO_APPEND_IMAGES_TO;";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "--and secondly the event which has the images removed from it...";
            sql_text += "\n" + "SELECT @NEW_START_TIME = MIN(image_time), @NEW_END_TIME = MAX(image_time)";
            sql_text += "\n" + "FROM All_Images";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id = @EVENT_ID_OF_SOURCE_IMAGES";
            sql_text += "\n" + "";
            sql_text += "\n" + "--if the start_time is null, then that means that there's no images left in the source event";
            sql_text += "\n" + "-- therefore we'll delete it";
            sql_text += "\n" + "IF @NEW_START_TIME IS NULL";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "DELETE FROM All_Events";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id = @EVENT_ID_OF_SOURCE_IMAGES;";
            sql_text += "\n" + "END --CLOSE IF @NEW_START_TIME IS NULL...";
            sql_text += "\n" + "";
            sql_text += "\n" + "ELSE -- however for most scenarios it's more likely that there'll still be images left";
            sql_text += "\n" + "--therefore we'll update the start/end times, plus the keyframe path too";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "UPDATE All_Events";
            sql_text += "\n" + "SET start_time = @NEW_START_TIME, end_time = @NEW_END_TIME";
            sql_text += "\n" + "WHERE [user_id] = @USER_ID";
            sql_text += "\n" + "AND event_id = @EVENT_ID_OF_SOURCE_IMAGES;";
            sql_text += "\n" + "";
            sql_text += "\n" + "-- and let's update the keyframe image for the event by calling this stored procedure";
            sql_text += "\n" + "EXEC Oct10_UPDATE_EVENT_KEYFRAME_IMAGE @USER_ID, @EVENT_ID_OF_SOURCE_IMAGES;";
            sql_text += "\n" + "END --CLOSE ELSE ... IF @NEW_START_TIME IS NULL";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "END --CLOSE ... IF(@TIME_OF_START_IMAGE > @EVENT_START_TIME AND @EVENT_START_TIME IS NOT NULL)";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            
            
            sql_text += "\n" + "/****** Object:  StoredProcedure [dbo].[APR11_ADD_ANNOTATION_TYPE]    Script Date: 05/13/2011 10:28:08 ******/";
            sql_text += "\n" + "SET ANSI_NULLS ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "SET QUOTED_IDENTIFIER ON";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "-- Author:";
            sql_text += "\n" + "-- Create date: <Create Date,,>";
            sql_text += "\n" + "-- Description:";
            sql_text += "\n" + "-- =============================================";
            sql_text += "\n" + "CREATE PROCEDURE [dbo].[APR11_ADD_ANNOTATION_TYPE] ";
            sql_text += "\n" + "-- Add the parameters for the stored procedure here";
            sql_text += "\n" + "@ANNOTATION_TYPE_NAME AS VARCHAR(100)";
            sql_text += "\n" + "AS";
            sql_text += "\n" + "BEGIN";
            sql_text += "\n" + "-- SET NOCOUNT ON added to prevent extra result sets from";
            sql_text += "\n" + "-- interfering with SELECT statements.";
            sql_text += "\n" + "SET NOCOUNT ON;";
            sql_text += "\n" + "";
            sql_text += "\n" + "--firstly make sure it doesn't already exist in the database";
            sql_text += "\n" + "--so to achieve that, we just try to delete any entry that may already exist";
            sql_text += "\n" + "EXEC APR11_REMOVE_ANNOTATION_TYPE @ANNOTATION_TYPE_NAME;";
            sql_text += "\n" + "";
            sql_text += "\n" + "--and now we can add in our new entry safe in the knowledge there'll be no duplicate entry after this";
            sql_text += "\n" + "INSERT INTO Annotation_Types VALUES(@ANNOTATION_TYPE_NAME, @ANNOTATION_TYPE_NAME)";
            sql_text += "\n" + "";
            sql_text += "\n" + "END";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";





            //let's finally close the connection!
            con.Close();
        } //close method get_sql_text_to_add_DCU_SenseCam_tables_to_database()...






        /// <summary>
        /// This method initialises the DCU_SenseCam tables with some sample data
        /// </summary>
        /// <param name="new_db_name"></param>
        /// <returns></returns>
        private static void add_DCU_SenseCam_data_to_tables(string new_db_name, string db_con_string)
        {
            SqlConnection con = new SqlConnection(db_con_string);
            con.Open(); //let's open the database connection
            
            string sql_text = "exec feb_10_spInsert_New_User_Into_Database_and_Return_ID 'Aiden_Doherty'";
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            sql_text += "\n" + get_sql_insert_text_for_annotation_type("cycling");
            sql_text += "\n" + get_sql_insert_text_for_annotation_type("driving");
            sql_text += "\n" + get_sql_insert_text_for_annotation_type("walking");
            sql_text += "\n" + get_sql_insert_text_for_annotation_type("train");
            sql_text += "\n" + get_sql_insert_text_for_annotation_type("bus");
            sql_text += "\n" + get_sql_insert_text_for_annotation_type("shopping");
            sql_text += "\n" + get_sql_insert_text_for_annotation_type("standing");
            sql_text += "\n" + get_sql_insert_text_for_annotation_type("underground");
            execute_sql_text_on_database(sql_text, con); sql_text = ""; //sql_text += "\n" + "GO";GO";

            //let's finally close the connection!
            con.Close();
        } //close method get_sql_text_to_add_DCU_SenseCam_tables_to_database()...


        private static string get_sql_insert_text_for_annotation_type(string annotation_text)
        {
            return "insert into Annotation_Types values ('"+annotation_text+"', '"+annotation_text+"')";
        } //close method get_sql_insert_text_for_annotation_type()...






        //need another method to get the sql text to populate the database with some sample data (possibly call an XML file, we'll see)


        //another method then to actually make execute the sql text on the database...
        private static void execute_sql_text_on_database(string sql_text, SqlConnection con)
        {
            SqlCommand sql_command = new SqlCommand(sql_text, con);
            sql_command.CommandTimeout = 120;
            sql_command.CommandType = CommandType.Text;
            try
            {
                sql_command.ExecuteNonQuery();                
            } //close try...
            catch (Exception excep) { System.Console.Write(excep.ToString()); } //the procedure or bit of code must already be on the db, so we can't update it...
            System.Console.Write(".");
        } //close method execute_sql_text_on_database()...




        #endregion installing_sensecam_db_for_first_time

        


    } //end class...
} //end namespace...
