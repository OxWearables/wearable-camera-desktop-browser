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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using System.Threading;
using System.Diagnostics;
using System.Data.SQLite;
using System.Data.Common;

namespace SenseCamBrowser1
{
    /// <summary>
    /// Interaction logic for Upload_New_Images.xaml
    /// </summary>
    public partial class Upload_New_Images : UserControl
    {


        #region class properties, and data setup + misc small methods

        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        // Delegate that defines the signature for the callback methods.
        public delegate void Processing_Finished_Callback();
        public delegate void images_uploaded_to_PC_and_now_deleting_from_SenseCam();
        // Delegate used to execute the callback method when the task is complete.
        private Processing_Finished_Callback data_processing_finished_callback;
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        private int object_userID;
        private string object_user_name;
        private bool uploading_from_flash_drive = true;



        public Upload_New_Images()
        {
            InitializeComponent();
        } //end constructor...




        /// <summary>
        /// called before the user control is displayed, which updates the likely image/event paths...
        /// </summary>
        /// <param name="userID"></param>
        public void update_user_control_with_drive_information(int userID, string user_name, Processing_Finished_Callback param_all_images_deleted_callback)
        {
            //firstly let's check that we're not currently deleting SenseCam images (i.e. make sure user hasn't opened this again due to impatience!)
            if (!Is_Deletion_Process_Currently_Running())
            {
                txtSCPath.Text = detect_Autographer_USB_data_directory("");
                txtPCPath.Text = User_Object.get_likely_PC_destination_root(userID, user_name);
                this.object_userID = userID;
                this.object_user_name = user_name;

                //in case other images were uploaded before, we'll reset this button to be enabled...
                btnUpload.IsEnabled = true;
                btnClose.IsEnabled = true;
                btnUpload.Opacity = 1.0;
                btnClose.Opacity = 1.0;
                Loading_Animation.Visibility = Visibility.Collapsed;

                data_processing_finished_callback = param_all_images_deleted_callback;
            } //close if (!Is_Deletion_Process_Currently_Running())
            else
            {
                //now let's give the user feedback that they should be waiting on the deletion process to finish...
                txtSCPath.Text = "still clearing images from SenseCam...";
                txtPCPath.Text = "still clearing images from SenseCam...";
                this.object_userID = userID;
                this.object_user_name = user_name;

                //in case other images were uploaded before, we'll reset this button to be enabled...
                btnUpload.IsEnabled = false;
                btnClose.IsEnabled = true;
                btnUpload.Opacity = 0.3;
                btnClose.Opacity = 1.0;
                Loading_Animation.Visibility = Visibility.Visible;

                data_processing_finished_callback = param_all_images_deleted_callback;
            } //close else ... if (!Is_Deletion_Process_Currently_Running())
        } //close method update_user_control_with_drive_information()...




        /// <summary>
        /// method to display messages to the user
        /// </summary>
        /// <param name="msg"></param>
        private void write_output(string msg)
        {
            richTextBox1.AppendText("\n" + msg);
            richTextBox1.ScrollToEnd();
        } //close method write_output()...

        #endregion class properties, and data setup + misc small methods





        #region detecting likely source and destination folders for the SenseCam images

        /// <summary>
        /// this method is used to automatically detect the SenseCam drive name, and also the "DATA" directory...
        /// </summary>
        /// <returns></returns>
        private string detect_SenseCam_USB_data_directory(string seed)
        {
            string[] candidate_drives = new string[] { seed, "D:", "E:", "F:", "G:", "H:"};
            string data_directory_drive = "";
            string sensor_csv_file = @"\DATA\SENSOR.CSV";

            //so go through each drive letter
            foreach (string candidate_drive in candidate_drives)
            {
                if (File.Exists(candidate_drive + sensor_csv_file)) //then a sensor.csv file is found in a "data" directory...
                {
                    data_directory_drive = candidate_drive + @"\DATA\"; //we presume this is the SenseCam directory ... if not the user can still ammend this later
                    btnUpload.Visibility = Visibility.Visible;
                    btnSCOldImages.Visibility = Visibility.Hidden;
                    break; //and break out of the loop as we've found what we want...
                } //close if (File.Exists(candidate_drive + sensor_csv_file))...
                else
                {
                    btnSCOldImages.Visibility = Visibility.Visible;
                    btnUpload.Visibility = Visibility.Hidden;
                }
            } //close foreach (char candidate_drive in candidate_drives)...

            return data_directory_drive;
        } //close method detect_SenseCam_USB_data_directory()...




        /// <summary>
        /// this method is used to automatically detect the Autographer drive name, and also the "DATA" directory...
        /// </summary>
        /// <returns></returns>
        private string detect_Autographer_USB_data_directory(String seed)
        {
            seed = seed.Split(new String[]{@"\DATA"}, StringSplitOptions.None)[0];
            string[] candidate_drives = new string[] { seed, "D:", "E:", "F:", "G:", "H:"};
            string data_directory_drive = "";
            string sensor_csv_file = @"\DATA\image_table.txt";

            //so go through each drive letter
            foreach (string candidate_drive in candidate_drives)
            {
                if (File.Exists(candidate_drive + sensor_csv_file)) //then a sensor.csv file is found in a "data" directory...
                {
                    data_directory_drive = candidate_drive + @"\DATA\"; //we presume this is the SenseCam directory ... if not the user can still ammend this later
                    btnUpload.Visibility = Visibility.Visible;
                    btnSCOldImages.Visibility = Visibility.Hidden;
                    break; //and break out of the loop as we've found what we want...
                } //close if (File.Exists(candidate_drive + sensor_csv_file))...
                else
                {
                    btnSCOldImages.Visibility = Visibility.Visible;
                    btnUpload.Visibility = Visibility.Hidden;
                }
            } //close foreach (char candidate_drive in candidate_drives)...

            if (data_directory_drive.Equals("")) //i.e. we haven't detected Autographers's image table...
                return detect_SenseCam_USB_data_directory(seed); //instead we'll check if we can detect SenseCam data instead
            return data_directory_drive; //however if we have detected an Autographer image table, let's return its path
        } //close method detect_SenseCam_USB_data_directory()...




        /// <summary>
        /// this method checks to see if the SenseCam deletion process is currently running, meaning we don't want to call the detect_SenseCam_USB_data_directory() method as it'll probably hang trying to access the SenseCam device...
        /// </summary>
        /// <returns></returns>
        public bool Is_Deletion_Process_Currently_Running()
        {
            string deletion_process = "delete_images_from_sensecam";
            //here we're going to get a list of all running processes on the computer
            foreach (Process clsProcess in Process.GetProcesses())
            {
                if (clsProcess.ProcessName.Contains(deletion_process))
                    return true; //if the process is found to be running then we return a true                    
            } //close foreach (Process clsProcess in Process.GetProcesses())...

            return false; //otherwise, if the process isn't found, we return a false
        } //close Is_Deletion_Process_Currently_Running()...
        

        #endregion detecting likely source and destination folders for the SenseCam images






        #region this region is all the code needed to integrate the CALLBACK functions for the sc copy, segment, and delete functions (they may take a bit of time, so let's not slow the UI)

        private Thread data_transfer_thread; //this thread is used to do the data transfer...
        private Upload_Images_and_Segment_into_Events.Upload_and_Segment_Images_Thread data_transfer_obj; //this is our class which is responsible for data transfer from SenseCam to local machine...

        /// <summary>
        /// This method is responsible for starting the thread to upload and process new SenseCam images...
        /// </summary>
        private void initiate_data_transfer_process(string SenseCam_data_directory, string current_root_folder, bool uploading_direct_from_sensecam)
        {
            //firstly let's just check if it's ok to process this folder (i.e. user is sure this is what they want)
            bool ok_to_process = false, is_multiple_folder_upload=false;
            string episode_data_csv_file_obj = txtEpisodeCSVPath.Text;            

            //check if this is a multiple past folder upload, ask the user if they're sure this is what they want to do
            if (!File.Exists(SenseCam_data_directory + "sensor.csv") && !File.Exists(SenseCam_data_directory + "image_table.txt")) //firstly let's see if they've selected a root folder...
            {
                //let's ask the user if they are sure they want to upload all sub
                MessageBoxResult user_agrees_to_process = MessageBox.Show("You are about to ingest all subfolders below this root folder and associated the images with this participant. Do not be alarmed if the percentage progress keeps resetting itself back to zero, this merely indicates it is starting to upload the next subfolder. Are you sure you want to continue with this operation?","Upload Images from Multiple Folders?",MessageBoxButton.YesNo);
                
                //and then if they do agree to process, let's update our processing variable to say it's ok to process
                if (user_agrees_to_process == MessageBoxResult.Yes)
                {
                    ok_to_process = true;
                    is_multiple_folder_upload = true; //and also let's just record that we're uploading multiple folders
                } //close if (user_agrees_to_process == MessageBoxResult.Yes)...
            } //close if (!File.Exists(SenseCam_data_directory + "sensor.csv"))...
            else ok_to_process = true; //if this is just a single folder with a .CSV file in it, then we know this is in all likelihood a SenseCam images folder, and we go ahead without prompting the user


            if (ok_to_process)
            {
                data_transfer_obj = new Upload_Images_and_Segment_into_Events.Upload_and_Segment_Images_Thread(Data_Transer_New_Info_Received_Callback, Data_Transfer_Processing_Finished_Callback, SenseCam_data_directory, current_root_folder, object_userID, uploading_direct_from_sensecam, is_multiple_folder_upload, episode_data_csv_file_obj);
                data_transfer_thread = new Thread(new ThreadStart(data_transfer_obj.upload_sc_data));
                data_transfer_thread.IsBackground = true;
                data_transfer_thread.Start();

                //let's give some additional feedback on the UI that we're beginning to upload data...
                btnUpload.IsEnabled = false;
                btnClose.IsEnabled = false;
                btnUpload.Opacity = 0.3;
                btnClose.Opacity = 0.3;
                Loading_Animation.Visibility = Visibility.Visible;
                photo_image.Visibility = Visibility.Visible;
                grdPCDestinationIcon.Visibility = Visibility.Visible;
                sc_image.Visibility = Visibility.Visible;
                lblUpload_Status.Visibility = Visibility.Visible;
                lblUpload_Status.Content = "0 %";
            } //close if (ok_to_process)...

        } //close method Open_Heart_Rate_Sensor_Thread()...


        #region callback methods invoked (these can't directly update the UI, and must use delegates)

        public void Data_Transer_New_Info_Received_Callback(string message)
        {
            //delegate to display the message...
            this.Dispatcher.BeginInvoke(new Data_Transfer_UI_Text_Delegate(Update_UI_with_Data_Transfer_Message), new object[] { message });
        } //Data_Transer_New_Info_Received_Callback()...

        public void Data_Transfer_Processing_Finished_Callback()
        {
            data_processing_finished_callback(); //send a message back to the main UI that it's now ok to exit the application whenever the user wishes again...
            string wrapping_up_message = "All processing finished!!!";
            this.Dispatcher.BeginInvoke(new Data_Transfer_UI_Text_Delegate(Update_UI_with_Data_Transfer_Message), new object[] { wrapping_up_message });
            data_transfer_thread.Abort();
        } //close method Data_Transfer_Processing_Finished_Callback()...                

        #endregion callback methods invoked (these can't directly update the UI, and must use delegates)


        #region methods and their delegates which the callback threads call, so that the UI can be updated

        private void Update_UI_with_Data_Transfer_Message(string message)
        {
            write_output(message);

            //let's give user feedback on how many images have been copied so far...
            if (message.Contains("file_transfer_progress:"))
            {
                lblUpload_Status.Content = message.Split(':')[1].ToString() + " %";
            }
            else if (message.Equals("All processing finished!!!"))
            {
                Loading_Animation.Visibility = Visibility.Hidden;
                photo_image.Visibility = Visibility.Hidden;
                grdPCDestinationIcon.Visibility = Visibility.Hidden;
                sc_image.Visibility = Visibility.Hidden;
                lblUpload_Status.Visibility = Visibility.Hidden;
            } //close else if (message.Contains("All processing finished!!!"))...
        } //close method Update_UI_with_Data_Transfer_Message()...
        private delegate void Data_Transfer_UI_Text_Delegate(string message); //and a delegate for the above method must be called if we want to update the UI, hence we declare this line

        #endregion methods and their delegates which the callback threads call, so that the UI can be updated

        #endregion this region is all the code needed to integrate the CALLBACK functions for the sc copy, segment, and delete functions (they may take a bit of time, so let's not slow the UI)






        #region user interface buttons

        /// <summary>
        /// called when the close button is clicked by the user, i.e. so we can hide the control...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            //let's log this interaction            
            play_sound();
            Record_User_Interactions.log_interaction_to_database("UploadNewImages_btnclose_click");

            this.Visibility = Visibility.Collapsed;
        } //end method btnClose_Click()...




        /// <summary>
        /// this method is called when the user commits to uploading images form the SenseCam
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            //firstly let's log this interaction...
            string sc_path_for_log = txtSCPath.Text;
            if (sc_path_for_log.Length > 100)
                sc_path_for_log = sc_path_for_log.Substring(sc_path_for_log.Length - 51, 50);

            string pc_path_for_log = txtPCPath.Text;
            if (pc_path_for_log.Length > 100)
                pc_path_for_log = pc_path_for_log.Substring(pc_path_for_log.Length - 51, 50);
            //let's log this interaction
            play_sound();
            Record_User_Interactions.log_interaction_to_database("UploadNewImages_btnupload_click", sc_path_for_log + "," + pc_path_for_log);
            //end logging this interaction

            initiate_data_transfer_process(txtSCPath.Text, txtPCPath.Text, uploading_from_flash_drive);
        } //end method get_number_of_images_in_event()




        /// <summary>
        /// to help users select the source folder on the SenseCam...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSCPath_Click(object sender, RoutedEventArgs e)
        {
            FolderSelecter f = new FolderSelecter();
            f.select_file_rather_than_folder = false;
            f.selected_path = txtSCPath.Text;
            f.Show();
            txtSCPath.Text = f.selected_path;
            f.Close();

            //to allow user then upload images
            if ( (!uploading_from_flash_drive && !txtSCPath.Text.Equals(""))
                        || (uploading_from_flash_drive && !detect_Autographer_USB_data_directory(txtSCPath.Text).Equals(""))
                )
                btnUpload.Visibility = Visibility.Visible;

            //let's log this interaction
            play_sound();
            Record_User_Interactions.log_interaction_to_database("UploadNewImages_btnSCPath_click", txtSCPath.Text);
        } //close method btnSCPath_Click()...




        /// <summary>
        /// to help users select the destination folder on their PC...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPCPath_Click(object sender, RoutedEventArgs e)
        {
            FolderSelecter f = new FolderSelecter();
            f.select_file_rather_than_folder = false;
            f.selected_path = txtPCPath.Text;
            f.Show();
            txtPCPath.Text = f.selected_path;
            f.Close();

            //let's log this interaction
            play_sound();
            Record_User_Interactions.log_interaction_to_database("UploadNewImages_btnPCPath_click", txtPCPath.Text);
        } //close method btnPCPath_Click()...




        /// <summary>
        /// to help users select the destination episodes CSV file on their PC...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>        
        private void btnEpisodeCSVPath_Click(object sender, RoutedEventArgs e)
        {
            FolderSelecter f = new FolderSelecter();
            f.select_file_rather_than_folder = true;
            f.selected_path = txtEpisodeCSVPath.Text;
            f.Show();
            txtEpisodeCSVPath.Text = f.selected_path;
            f.Close();

            //let's log this interaction
            play_sound();
            Record_User_Interactions.log_interaction_to_database("UploadNewImages_btnEpisodeCSVPath_click", txtEpisodeCSVPath.Text);
        } //close btnEpisodeCSVPath_Click()...


        /// <summary>
        /// this methods shows the user some settings options...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            //let's log this interaction
            play_sound();
            Record_User_Interactions.log_interaction_to_database("UploadNewImages_btnSettings_click");

            btnSettings_show.Visibility = Visibility.Hidden;
            btnSettings_hide.Visibility = Visibility.Visible;
        } //close method btnSettings_Click()...




        /// <summary>
        /// this method hides the settings options from the user...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSettings_hide_Click(object sender, RoutedEventArgs e)
        {
            //let's log this interaction
            play_sound();
            Record_User_Interactions.log_interaction_to_database("UploadNewImages_btnSettings_hide_click");

            btnSettings_show.Visibility = Visibility.Visible;
            btnSettings_hide.Visibility = Visibility.Hidden;
        } //close btnSettings_hide_Click()...



        /// <summary>
        /// this method is the code to enable the researcher upload old data gathered before they had access to the DCU SenseCam management system...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSCOldImages_Click(object sender, RoutedEventArgs e)
        {
            //let's toggle if we're uploading from the flash drive...
            uploading_from_flash_drive = !uploading_from_flash_drive;

            //if not uploading from the flash drive, i.e. we're uploading old images from another folder on the PC...
            if (!uploading_from_flash_drive)
            {
                lblImages_Source.Content = "Path of images already on your PC:";
                btnSCOldImages.Content = "Download from SenseCam/Revue...";
                txtPCPath.Text = "Images already stored on PC...";
                txtPCPath.IsEnabled = false;
                btnPCPath.Visibility = Visibility.Hidden;

                //let's log this interaction
                Record_User_Interactions.log_interaction_to_database("UploadNewImages_btnSCOldImages_Click", "upload_from_PC");
            }
            else //else we are uploading from the flash drive (i.e. SC should be connected)...
            {
                lblImages_Source.Content = "SenseCam or Vicon Revue path:";
                btnSCOldImages.Content = "Download from PC...";
                txtSCPath.Text = detect_Autographer_USB_data_directory("");
                txtPCPath.IsEnabled = false;
                txtPCPath.Text = User_Object.get_likely_PC_destination_root(object_userID, object_user_name);
                btnPCPath.Visibility = Visibility.Visible;

                if (detect_Autographer_USB_data_directory("").Equals(""))
                    btnUpload.Visibility = Visibility.Hidden;
                else btnUpload.Visibility = Visibility.Visible;

                //let's log this interaction
                Record_User_Interactions.log_interaction_to_database("UploadNewImages_btnSCOldImages_Click", "upload_from_Revue");
            } //close else ... if (!uploading_from_flash_drive)

        } //close method btnSCOldImages_Click()...

        #endregion user interface buttons




        #region methods to support interface controls...

        /// <summary>
        /// when this method is called, a sound will be played
        /// </summary>
        private void play_sound()
        {
            sound_Click_Sound.Stop();
            sound_Click_Sound.Play();
        } //close method play_sound()...

        #endregion methods to support interface controls...

    } //end class...

} //end namespace...
