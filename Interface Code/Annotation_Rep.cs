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
        private static string DbString =
            global::SenseCamBrowser1.Properties.Settings.Default.DBConnectionString;

        public int annID { get; set; }
        public string annType { get; set; }
        public string annDesc { get; set; }
        
        public Annotation_Rep(int annID, string annType, string annDesc)
        {
            this.annID = annID;
            this.annType = annType;
            this.annDesc = annDesc;
        }


        public static AnnotationTreeViewModel GetAnnotationTypes()
        {
            //this method gets the types of annotations available
            //and allows them to be displayed in a XAML treeview format
            List<string> annTypeList = new List<string>();

            //annotation type properties
            int annID;
            string annType, annDesc;
            
            string query = Database_Versioning.text_for_stored_procedures.spGet_list_of_annotation_types();
            SQLiteConnection con = new SQLiteConnection(DbString);
            SQLiteCommand selectCmd = new SQLiteCommand(query, con);
            con.Open();
            SQLiteDataReader readAnnotations = selectCmd.ExecuteReader();
            while (readAnnotations.Read())
            {
                annID = int.Parse(readAnnotations[0].ToString());
                annType = readAnnotations[1].ToString();
                annDesc = readAnnotations[2].ToString();
                annTypeList.Add(annType);
            }
            con.Close();

            //now convert the list of event annotations to XAML treeview
            Annotation_Rep_Tree_Data hierarchy_of_annotation_types = 
                Annotation_Rep_Tree_Data.convert_delimited_string_collection_to_tree_hierarchy(
                annTypeList);
            //Syntax template:
            //http://weblogs.asp.net/psheriff/archive/2012/07/23/wpf-tree-view-with-multiple-levels.aspx
            return new AnnotationTreeViewModel(hierarchy_of_annotation_types);
        }


        public static List<string> GetEventAnnotations(int userID, int eventID)
        {
            List<string> annList = new List<string>();
            string query = 
                Database_Versioning.text_for_stored_procedures.spGet_event_annotations(
                userID,
                eventID);
            SQLiteConnection con = new SQLiteConnection(DbString);
            SQLiteCommand selectCmd = new SQLiteCommand(query,con);            
            con.Open();
            SQLiteDataReader readAnnotations = selectCmd.ExecuteReader();
            while (readAnnotations.Read())
            {
                annList.Add(readAnnotations[0].ToString());
            }
            con.Close();
            return annList;
        }


        public static void AddEventAnnotation(
            int userID,
            int eventID,
            string annotation_name)
        {
            string query = 
                Database_Versioning.text_for_stored_procedures.spAdd_event_annotation(
                userID,
                eventID,
                annotation_name);
            SQLiteConnection con = new SQLiteConnection(DbString);
            SQLiteCommand selectCmd = new SQLiteCommand(query, con);
            con.Open();
            selectCmd.ExecuteNonQuery();
            con.Close();            
        }


        public static void RmEventAnnotations(int userID, int eventID)
        {
            string query = 
                Database_Versioning.text_for_stored_procedures.spClear_event_annotations(
                userID,
                eventID);
            SQLiteConnection con = new SQLiteConnection(DbString);
            SQLiteCommand selectCmd = new SQLiteCommand(query, con);
            con.Open();
            selectCmd.ExecuteNonQuery();
            con.Close();
        }


        public static void RmEventAnnotation(int userID, int eventID, string individual_annotation_text)
        {
            string query = 
                Database_Versioning.text_for_stored_procedures.spClear_event_annotations(
                userID,
                eventID,
                individual_annotation_text);
            SQLiteConnection con = new SQLiteConnection(DbString);
            SQLiteCommand selectCmd = new SQLiteCommand(query, con);
            con.Open();
            selectCmd.ExecuteNonQuery();
            con.Close();
        }


        public static void AddAnnotationType(string annotation_type_name)
        {
            string query = 
                Database_Versioning.text_for_stored_procedures.spAdd_annotation_type(
                annotation_type_name);
            SQLiteConnection con = new SQLiteConnection(DbString);
            SQLiteCommand selectCmd = new SQLiteCommand(query, con);
            con.Open();
            selectCmd.ExecuteNonQuery();
            con.Close();
        }
        

        public static void RmAnnotationType(string annotation_type_name)
        {
            string query = 
                Database_Versioning.text_for_stored_procedures.spRemove_annotation_type(
                annotation_type_name);
            SQLiteConnection con = new SQLiteConnection(DbString);
            SQLiteCommand selectCmd = new SQLiteCommand(query, con);
            con.Open();
            selectCmd.ExecuteNonQuery();
            con.Close();
        }


        public static void RmAllAnnotationTypes()
        {
            string query = 
                Database_Versioning.text_for_stored_procedures.spRemove_all_annotation_types();
            SQLiteConnection con = new SQLiteConnection(DbString);
            SQLiteCommand selectCmd = new SQLiteCommand(query, con);
            con.Open();
            selectCmd.ExecuteNonQuery();
            con.Close();
        }
                    
    }

}
