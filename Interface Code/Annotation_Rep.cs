using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.SQLite;
using System.Data.Common;

namespace SenseCamBrowser1
{

    public class Annotation_Rep
    {

        public int annotation_id { get; set; }
        public string annotation_type { get; set; }
        public string annotation_description { get; set; }


        public Annotation_Rep(int annotation_id, string annotation_type, string annotation_description)
        {
            this.annotation_id = annotation_id;
            this.annotation_type = annotation_type;
            this.annotation_description = annotation_description;
        } //close constructor...




        /// <summary>
        /// this method get the types of annotations available
        /// </summary>
        /// <param name="class_type">e.g. 1 = for active travel annotations...</param>
        /// <returns></returns>
        public static AnnotationTreeViewModel get_list_of_annotation_types()
        {
            //this method calls the relevant database stored procedure to retrieve a list of annotation classes...
            List<string> list_of_annotation_classes = new List<string>();
            SQLiteConnection con = new SQLiteConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);
            SQLiteCommand selectCmd = new SQLiteCommand(Database_Versioning.text_for_stored_procedures.NOV10_GET_LIST_OF_ANNOTATION_CLASSES(), con);
            
            int annotation_id;
            string annotation_type, annotation_description;

            //then open the db connection, connect to the stored procedure and return the list of results...
            con.Open();
            SQLiteDataReader read_events = selectCmd.ExecuteReader();
            while (read_events.Read())
            {
                annotation_id = int.Parse(read_events[0].ToString());
                annotation_type = read_events[1].ToString();
                annotation_description = read_events[2].ToString();

                list_of_annotation_classes.Add(annotation_type);// (new Annotation_Rep(annotation_id, annotation_type, annotation_description));
            } //end while (read_chunk_ids.Read())...
            con.Close();

            //return list_of_annotation_classes; //and finally return the possible list of annotation classes...


            Annotation_Rep_Tree_Data hierarchy_of_annotation_types = Annotation_Rep_Tree_Data.convert_delimited_string_collection_to_tree_hierarchy(list_of_annotation_classes);
            // credit note: ... http://weblogs.asp.net/psheriff/archive/2012/07/23/wpf-tree-view-with-multiple-levels.aspx was very helpful for a syntax template...
            return new AnnotationTreeViewModel(hierarchy_of_annotation_types);
        } //close method get_list_of_annotation_types()...


        /// <summary>
        /// This method gets the prior annotations associated with an event...
        /// </summary>
        /// <param name="class_type"></param>
        /// <param name="user_id"></param>
        /// <param name="event_id"></param>
        /// <returns></returns>
        public static List<string> get_event_prior_annotations(int user_id, int event_id)
        {
            //this method calls the relevant database stored procedure to retrieve a list of annotations already associated with this event...
            List<string> list_of_annotations = new List<string>();
            SQLiteConnection con = new SQLiteConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);
            SQLiteCommand selectCmd = new SQLiteCommand(Database_Versioning.text_for_stored_procedures.NOV10_GET_ANNOTATIONS_FOR_EVENT(user_id,event_id),con);
            
            string annotation_type;

            //then open the db connection, connect to the stored procedure and return the list of results...
            con.Open();
            SQLiteDataReader read_events = selectCmd.ExecuteReader();
            while (read_events.Read())
            {
                annotation_type = read_events[0].ToString();

                list_of_annotations.Add(annotation_type);
                //list_of_annotations.Add(new Annotation_Rep(annotation_id, annotation_type, annotation_description));
            } //end while (read_chunk_ids.Read())...
            con.Close();

            return list_of_annotations; //and finally return the annotations for this event...
        } //close method get_event_prior_annotations()...



        public static void add_event_annotation_to_database(int user_id, int event_id, string annotation_name)
        {
            SqlConnection con = new SqlConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);
            SqlCommand selectCmd = new SqlCommand("NOV10_ADD_EVENT_ANNOTATION", con);
            selectCmd.CommandType = CommandType.StoredProcedure;
            selectCmd.Parameters.Add("@USER_ID", SqlDbType.Int).Value = user_id;
            selectCmd.Parameters.Add("@EVENT_ID", SqlDbType.Int).Value = event_id;
            selectCmd.Parameters.Add("@EVENT_ANNOTATION_NAME", SqlDbType.VarChar).Value = annotation_name;
            con.Open();
            selectCmd.ExecuteNonQuery();
            con.Close();            
        } //close method add_event_annotation_to_database()...


        public static void clear_event_annotations_from_database(int user_id, int event_id)
        {
            SqlConnection con = new SqlConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);
            SqlCommand selectCmd = new SqlCommand("NOV10_CLEAR_EVENT_ANNOTATIONS", con);
            selectCmd.CommandType = CommandType.StoredProcedure;
            selectCmd.Parameters.Add("@USER_ID", SqlDbType.Int).Value = user_id;
            selectCmd.Parameters.Add("@EVENT_ID", SqlDbType.Int).Value = event_id;
            con.Open();
            selectCmd.ExecuteNonQuery();
            con.Close();
        } //close method clear_event_annotations_from_database()...

        public static void clear_event_annotations_from_database(int user_id, int event_id, string individual_annotation_text)
        {
            SqlConnection con = new SqlConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);
            SqlCommand selectCmd = new SqlCommand("AUG12_CLEAR_EVENT_ANNOTATIONS_INDIVIDUAL", con);
            selectCmd.CommandType = CommandType.StoredProcedure;
            selectCmd.Parameters.Add("@USER_ID", SqlDbType.Int).Value = user_id;
            selectCmd.Parameters.Add("@EVENT_ID", SqlDbType.Int).Value = event_id;
            selectCmd.Parameters.Add("@INDIVIDUAL_ANNOTATION_TEXT", SqlDbType.VarChar).Value = individual_annotation_text;
            con.Open();
            selectCmd.ExecuteNonQuery();
            con.Close();
        } //close method clear_event_annotations_from_database()...






        public static void add_annotation_type_to_database(string annotation_type_name)
        {
            SqlConnection con = new SqlConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);
            SqlCommand selectCmd = new SqlCommand("APR11_ADD_ANNOTATION_TYPE", con);
            selectCmd.CommandType = CommandType.StoredProcedure;
            selectCmd.Parameters.Add("@ANNOTATION_TYPE_NAME", SqlDbType.VarChar).Value = annotation_type_name;
            con.Open();
            selectCmd.ExecuteNonQuery();
            con.Close();
        } //close method add_annotation_type_to_database()...



        public static void clear_annotation_type_from_database(string annotation_type_name)
        {
            SqlConnection con = new SqlConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);
            SqlCommand selectCmd = new SqlCommand("APR11_REMOVE_ANNOTATION_TYPE", con);
            selectCmd.CommandType = CommandType.StoredProcedure;
            selectCmd.Parameters.Add("@ANNOTATION_TYPE_NAME", SqlDbType.VarChar).Value = annotation_type_name; 
            con.Open();
            selectCmd.ExecuteNonQuery();
            con.Close();
        } //close method clear_annotation_type_from_database()...


        public static void clear_all_annotation_types_from_database()
        {
            SqlConnection con = new SqlConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);
            SqlCommand selectCmd = new SqlCommand("APR11_REMOVE_ALL_ANNOTATION_TYPES", con);
            selectCmd.CommandType = CommandType.StoredProcedure;
            con.Open();
            selectCmd.ExecuteNonQuery();
            con.Close();
        } //close method clear_all_annotation_types_from_database()...

                    
    } //close class Annotation_Rep...

} //close namespace...
