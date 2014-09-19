using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace SenseCamBrowser1
{
    /// <summary>
    /// Interaction logic for Edit_List_of_Event_Types.xaml
    /// </summary>
    public partial class Edit_List_of_Event_Types : UserControl
    {

        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        //so the callbacks are important to issue whether an event has been updated or deleted in someway
        // Delegate that defines the signature for the callback methods.
        public delegate void List_of_Annotation_Types_Updated();
        // Delegate used to execute the callback method when the task is complete.
        private List_of_Annotation_Types_Updated current_annotation_types_updated_callback;
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////


        public Edit_List_of_Event_Types()
        {
            InitializeComponent();

            //and update the list of annotation types the first time we open this...
            update_list_on_display_with_latest_database_snapshot();
        } //close constructor for Edit_List_of_Event_Types()...




        public void prepare_annotation_type_tool(List_of_Annotation_Types_Updated param_callback_when_finished_updating_annotation_types)
        {            
            //let's store the callback information...
            current_annotation_types_updated_callback = param_callback_when_finished_updating_annotation_types;                       
        } //close method prepare_annotation_type_tool()...


        static TreeViewItem GetParentItem(TreeViewItem item)
        {
            for (var i = VisualTreeHelper.GetParent(item); i != null; i = VisualTreeHelper.GetParent(i))
                if (i is TreeViewItem)
                    return (TreeViewItem)i;

            return null;
        }

        /// <summary>
        /// this method adds the text box entry to the list of event types...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_New_EventType_Name_Click(object sender, RoutedEventArgs e)
        {
            //1. get new item type name from the relevant textbox...
            string annotation_type_name = txtNew_EventType_Name.Text;
                                  
            if (lst_Current_Event_Types.SelectedItem != null)
            {
                Annotation_Rep_Tree_Data_Model tree_node = (Annotation_Rep_Tree_Data_Model)lst_Current_Event_Types.SelectedItem;
                string tree_node_db_entry = Annotation_Rep_Tree_Data.convert_tree_node_to_delimited_string(tree_node);
                if(!tree_node_db_entry.Equals(""))
                annotation_type_name = tree_node_db_entry + ";" + annotation_type_name;
            }
            

            if (!annotation_type_name.Equals("")) //let's just make sure the user didn't click the button by mistake...
            {
                //2. then add it to the database...
                Annotation_Rep.AddAnnotationType(annotation_type_name);

                //3. finally update the display list to reflect this new change...
                update_list_on_display_with_latest_database_snapshot();

                //and also reset the relevant textbox back to appear blank...
                txtNew_EventType_Name.Text = "";
                
                //let's log this interaction
                Record_User_Interactions.log_interaction_to_database("EditListofEventTypes_New_Typename", annotation_type_name);
            } //close error checking... if (!annotation_type_name.Equals(""))
        } //close method btnAdd_New_EventType_Name_Click()...

              




        /// <summary>
        /// this method removes an item from the list of event types...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRemove_EventType_Name_Click(object sender, RoutedEventArgs e)
        {
            if (lst_Current_Event_Types.SelectedItem != null)
            {
                //1. get item selected in the list
                Annotation_Rep_Tree_Data_Model annotation_type_delete = (Annotation_Rep_Tree_Data_Model)lst_Current_Event_Types.SelectedItem;

                //2. convert it to the database storage type...
                string annotation_type_database_text_entry = Annotation_Rep_Tree_Data.convert_tree_node_to_delimited_string(annotation_type_delete);
                
                //3. then delete it from the database...
                Annotation_Rep.RmAnnotationType(annotation_type_database_text_entry);

                //4. finally update the display list to reflect this new change...
                update_list_on_display_with_latest_database_snapshot();

                //let's log this interaction
                Record_User_Interactions.log_interaction_to_database("EditListofEventTypes_Remove_Typename", annotation_type_database_text_entry);
            } //close if (lst_Current_Event_Types.SelectedItem != null)...
        } //close method btnRemove_EventType_Name_Click()...




        /// <summary>
        /// this method removes all items from the list of event types...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRemove_All_EventType_Names_Click(object sender, RoutedEventArgs e)
        {
            Annotation_Rep.RmAllAnnotationTypes();

            //and update the display list to reflect this new change...
            update_list_on_display_with_latest_database_snapshot();

            //let's log this interaction
            Record_User_Interactions.log_interaction_to_database("EditListofEventTypes_Remove_All_Typenames", "");
        } //close method btnRemove_All_EventType_Names_Click()...



        /// <summary>
        /// this method updates the list on display with the latest database status...
        /// </summary>
        private void update_list_on_display_with_latest_database_snapshot()
        {
            lst_Current_Event_Types.ItemsSource = Annotation_Rep.GetAnnotationTypes().FirstGeneration[0].Children;            
        } //close method update_list_on_display_with_latest_database_snapshot()...
                




        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseBtn1_Click(object sender, RoutedEventArgs e)
        {
            //let's first of all send a callback to notify the image viewer that there's now a new list of annotations available and the viewer should be updated to reflect this...
            current_annotation_types_updated_callback();

            Visibility = Visibility.Collapsed; //and let's close/collapse this user control

            //let's log this interaction
            Record_User_Interactions.log_interaction_to_database("EditListofEventTypes_Close_UserControl", "");
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            //todo for some reason, calling User_Object below causes a problem
            //on some computers (try testing this more thoroughly)
            string suggestedPath = "";// User_Object.get_likely_PC_destination_root(
            //userId, userName);

            //prompt researcher on where to store annotations
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = suggestedPath;
            if (openFileDialog.ShowDialog() == true)
            {
                //save annotations for this day to file
                string fileName = openFileDialog.FileName;
                Daily_Annotation_Summary.readAnnotationSchemaCsv(fileName);
                Record_User_Interactions.log_interaction_to_database(
                        "EditListOfEventTypes_btnImportClick", fileName);
                update_list_on_display_with_latest_database_snapshot();
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            //todo for some reason, calling User_Object below causes a problem
            //on some computers (try testing this more thoroughly)
            string suggestedPath = "";// User_Object.get_likely_PC_destination_root(
            //userId, userName);

            //prompt researcher on where to store annotations
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = suggestedPath;
            saveFileDialog.FileName = "myAnnotationSchema.csv";
            if (saveFileDialog.ShowDialog() == true)
            {
                //save annotations for this day to file
                string fileName = saveFileDialog.FileName;
                Daily_Annotation_Summary.writeAnnotationSchemaToCsv(fileName);
                Record_User_Interactions.log_interaction_to_database(
                        "EditListOfEventTypes_btnExportClick", fileName);
            }
        }






        



    } //close class...
} //close namespace...
