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
using System.IO;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Data.SQLite;
using System.Data.Common;

namespace SenseCamBrowser1.Upload_Images_and_Segment_into_Events
{
    class Upload_and_Segment_Images_Thread
    {




        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        //so the callbacks are important to issue the output of an RSS crawl...
        // Delegate that defines the signature for the callback methods.
        public delegate void Information_Callback(string param_random_feed_value);
        public delegate void Processing_Finished_Callback();
        // Delegate used to execute the callback method when the task is complete.
        private Information_Callback data_feedback_callback;
        private Processing_Finished_Callback data_processing_finished_callback;
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        private static int MINIMUM_FILE_SIZE = 2048; //THE SMALLEST SIZE (IN BYTES) ANY SENSECAM IMAGE IS ALLOWED TO BE ... IF IT'S SMALLER THAN THIS WE DISREGARD IT
        private static int MAXIMUM_FILE_SIZE = 85002048; //THE LARGEST SIZE (IN BYTES) ANY SENSECAM IMAGE IS ALLOWED TO BE (85MB roughly) ... IF IT'S LARGER THAN THIS WE DISREGARD IT ... 

        private bool upload_is_direct_from_sensecam, is_multiple_folder_upload_class_variable;
        private string SenseCam_data_directory, current_root_folder, episode_data_csv_file_obj;
        private int user_id;
        public Upload_and_Segment_Images_Thread(Information_Callback param_feedback_callback, Processing_Finished_Callback param_data_processing_finished, string SC_data_directory, string PC_root_folder, int param_user_id, bool uploading_direct_from_sensecam, bool is_multiple_folder_upload, string episode_data_csv_file_obj)
        {
            data_feedback_callback = param_feedback_callback;
            data_processing_finished_callback = param_data_processing_finished;
            SenseCam_data_directory = SC_data_directory;
            current_root_folder = PC_root_folder;
            this.user_id = param_user_id;
            this.upload_is_direct_from_sensecam = uploading_direct_from_sensecam;
            this.is_multiple_folder_upload_class_variable = is_multiple_folder_upload;
            this.episode_data_csv_file_obj = episode_data_csv_file_obj;
        } //close method Upload_and_Segment_Images_Thread()...


        private void write_output(string msg)
        {
            Record_User_Interactions.log_interaction_to_database("Upload_And_Segment_Images_Thread_write_output", msg);
            data_feedback_callback(msg); //and report this back to the UI...
        } //close method write_output()...




        /// <summary>
        /// this is the public method that will be called by the thread caller
        /// </summary>
        public void upload_sc_data()
        {
            //and now let's call the method below which adds the capability of uploading many subfolders...
            upload_wearable_camera_data(is_multiple_folder_upload_class_variable, SenseCam_data_directory, SenseCam_data_directory);
        } //close method upload_sc_data()...



        /// <summary>
        /// this method is responsible to uploading SenseCam/Revue data from one folder, or multiple folders from a given root directory...
        /// </summary>
        /// <param name="is_multiple_folder_upload"></param>
        /// <param name="selected_folder"></param>
        /// <param name="overall_root_directory"></param>
        private void upload_wearable_camera_data(bool is_multiple_folder_upload, string selected_folder, string overall_root_directory)
        {
            if (is_multiple_folder_upload)
            {
                if (selected_folder.Equals(overall_root_directory))
                { //ELSE IF IT'S THE MAIN ROOT FOLDER WE'RE IN
                    //THEN WE'LL KICK OFF THE RECURSION PROCESS AND START READING THE SUBFOLDERS AND REPEAT
                    foreach (string subfolder in Directory.GetDirectories(overall_root_directory))
                        upload_wearable_camera_data(is_multiple_folder_upload, subfolder+@"\", overall_root_directory); //NOTE THAT I NEED THE @"\" AT THE END SO AS TO BE ABLE TO READ FILES WITHIN THE DIRECTORY

                } //end if (selected_folder.Equals(overall_root_directory))...
            } //close if (is_multiple_folder_upload)...

            //and let's keep updating the SenseCam_data_directory class variable to make sure we're always processing the right folder...
            SenseCam_data_directory = selected_folder;

            //and to process this individual folder...
            //1. check first line of sensor.csv file to see if we're dealing with a SenseCam or a Vicon Revue...
            DeviceType device_type = get_type_of_device_connected();

            //and then upload the data, segment it, etc.
            if ( (episode_data_csv_file_obj.Equals("")) || (!episode_data_csv_file_obj.Equals("") && selected_folder.Equals(overall_root_directory)) )
                upload_device_data(device_type, is_multiple_folder_upload, episode_data_csv_file_obj);            
            

            //and when we're finished the recursion (if using multiple folder upload)...
            //let's give a data processing finished call back..
            if (is_multiple_folder_upload && selected_folder.Equals(overall_root_directory))
            {
                write_output(DateTime.Now.ToLongTimeString());
                data_processing_finished_callback();
            } //close if (is_multiple_folder_upload)...

        } //close upload_wearable_camera_data()...



        /// <summary>
        /// this method detects if we're reading from a Vicon Revue or SenseCam or Autographer
        /// </summary>
        /// <returns>type of device</returns>
        private DeviceType get_type_of_device_connected()
        {
            //to achieve this, we'll read the first line of the sensor.csv file..
            FileInfo[] sensor_csv = attempt_to_retrieve_files_from_directory(SenseCam_data_directory, "*.CSV", "*.txt");

            string first_line_in_sensor_file = "";
            //so firstly check that the sensor.csv file 
            if (sensor_csv.Length >= 1) //to see if we've found any csv files in the directory...
            {
                if ((sensor_csv[0].Length >= MINIMUM_FILE_SIZE / 2) && (sensor_csv[0].Length < MAXIMUM_FILE_SIZE)) //to deal with potentially rogue sensor.csv files that are 0KB in size... meaning that we then just do a segmentation based on the image count...
                {
                    TextReader txt_reader = new StreamReader(SenseCam_data_directory + @"\" + sensor_csv[0].Name);
                    first_line_in_sensor_file = txt_reader.ReadLine();
                    txt_reader.Close(); //and close our reader object...
                } //close if (sensor_csv[0].Length >= MINIMUM_FILE_SIZE / 2)....
            } //close if(sensor_csv.Length>=1)...

            //Vicon Revue if looks like this -> VER,2010/05/11 12:17:39,4,0,0
            //SenseCam if looks like this ----> VER,2,6,7
            //Autographer if looks like this -> BAT,2013-04-08T02:11:01+0100,3971

            if (first_line_in_sensor_file.Substring(0, 8).Equals("#Version"))
                return DeviceType.Autographer;
            else if (Upload_Manipulated_Sensor_Data.get_datetime_from_Vicon_Revue_ACC_line(first_line_in_sensor_file, get_local_hours_ahead_of_utc_time()).Year >= 2000)
                return DeviceType.Revue; ////so see if we can read a Vicon revue timestamp from the first line to see if it's a valid time (i.e. say like something since 2000, then it's meant we could read it successfully
            else return DeviceType.SenseCam; //else we couldn't read a valid time, hence we're probably dealing with a SenseCam...

        } //close method is_Vicon_Revue_connected()

        public enum DeviceType
        {
            SenseCam, Revue, Autographer
        } //close enum DeviceType...




        private void upload_device_data(DeviceType device_type, bool is_part_of_multiple_folder_upload, string csv_file_of_associated_episodes)
        {

            write_output(DateTime.Now.ToLongTimeString() + " processing, please wait...");

            string local_machine_folder_path_for_new_images;
            //1. find the SenseCam device
            if (!SenseCam_data_directory.Equals(""))
            {
                if (!current_root_folder.Equals(""))
                {
                    //and now let's see if we're uploading directly from the SenseCam ... or if we're adding in old data that have been copied on to the hard drive already...
                    if (upload_is_direct_from_sensecam)
                    {

                        //1. we write the TIME.CSV file to the SenseCam "SYSTEM" directory, and this helps keep it on time
                        //we'll actually write the UTC time, which makes time synchronisation with GPS much easier in the long run...
                        if (device_type == DeviceType.Autographer)
                        {
                            //todo Autographer, do I need to write a Time.CSV file?
                            //todo Autographer, should I remove files from the LOGS directory to clean things up?
                            //write_Autographer_time_file(); //todo, it appears this should be done as near as possible to when the Autographer USB is disconnected (i.e. as late as possible)
                        }
                        else if (device_type == DeviceType.Revue)
                        {
                            write_Vicon_Revue_time_csv_file();
                            write_Vicon_Revue_boot_bat_file(); //ALSO FOR VICON REVUE, WE WRITE THE BOOT.BAT FILE
                        } //close if (device_type == DeviceType.Revue)...
                        else //if (device_type == DeviceType.SenseCam)
                        {
                            write_SenseCam_time_csv_file();
                        }
                        


                        //2. create a new folder on the current machine, where the SenseCam images are installed
                        //the folder name will be a reflection of the current datetime
                        DateTime current_folder_time = DateTime.Now;
                        local_machine_folder_path_for_new_images = current_root_folder + current_folder_time.Year + "-" + format_number(current_folder_time.Month) + "-" + format_number(current_folder_time.Day) + " " +
                                format_number(current_folder_time.Hour) + "-" + format_number(current_folder_time.Minute) + "-" + format_number(current_folder_time.Second);
                        Directory.CreateDirectory(local_machine_folder_path_for_new_images);


                        //3. copy the sensor.csv file and all the image files from the "data" folder in the SenseCam device, to the new folder we created on the local desktop machine
                        write_output(DateTime.Now.ToLongTimeString() + " copying files across...");
                        //todo image uploader status needs to be updated for Autographer!
                        copy_SenseCam_files_to_local_machine(SenseCam_data_directory, local_machine_folder_path_for_new_images, false);
                        write_output(DateTime.Now.ToLongTimeString() + " files copied across...");


                        //4. delete the files from the SenseCam - this is a seperate process, so we may as well kick start it as early as possible, i.e. just after uploading the images
                        //while at the same time we could have another thread/process that extracts the image features that we'd like...
                        write_output(DateTime.Now.ToLongTimeString() + " now deleting images from SenseCam device in another process...");
                        delete_files_on_SenseCam(SenseCam_data_directory); //we now call up a seperate process to delete the images from the SenseCam...

                    } //close if (upload_is_direct_from_sensecam)...
                    else local_machine_folder_path_for_new_images = SenseCam_data_directory;


                    //5. segment the data into events and upload the database...                                        
                    write_output(DateTime.Now.ToLongTimeString() + " all files have now been copied across, now segmenting data into events...");
                    if(!csv_file_of_associated_episodes.Equals(""))
                        segment_folder_images_into_events_and_upload_to_db(user_id, local_machine_folder_path_for_new_images, device_type,csv_file_of_associated_episodes);
                    else segment_folder_images_into_events_and_upload_to_db(user_id, local_machine_folder_path_for_new_images, device_type);
                    write_output(DateTime.Now.ToLongTimeString() + " all images have now been segmented into events and uploaded to the database...");


                    //6. as there's a problem with the EXIF header of the SenseCam images, we'll call a method to fix this header, so WPF can display the images we've just copied across...
                    //thanks to Dian Zhang, a CLARITY intern student for help with this code!
                    write_output(DateTime.Now.ToLongTimeString() + " now fixing exif header on all images...");
                    if(upload_is_direct_from_sensecam && device_type!=DeviceType.Autographer)
                        fix_JPG_header_of_all_images_in_folder(local_machine_folder_path_for_new_images, 50, 100);
                    else if(device_type!=DeviceType.Autographer)
                        fix_JPG_header_of_all_images_in_folder(local_machine_folder_path_for_new_images, 0, 100);
                    write_output(DateTime.Now.ToLongTimeString() + " exif headers fixed...");


                    //7. inform user when all files are deleted, hence we're finished...
                    if (device_type == DeviceType.Autographer)
                    {
                        //write_Autographer_time_file(); //todo, it appears this should be done as near as possible to when the Autographer USB is disconnected (i.e. as late as possible)
                        write_output(DateTime.Now.ToLongTimeString() + " all processing finished, you may now plug out your Autographer whenever you wish"); //todo can I make this dialog box that demands the users attention to unplug the device?
                    }
                    else if (device_type == DeviceType.Revue)
                        write_output(DateTime.Now.ToLongTimeString() + " all processing finished, you may now plug out your Vicon Revue whenever you wish");
                    else write_output(DateTime.Now.ToLongTimeString() + " all processing finished, you may now plug out your SenseCam whenever you wish");

                } //close if (!current_root_folder.Equals(""))...
                else
                {
                    write_output("Please specify a root folder");
                } //close else ... if (!current_root_folder.Equals(""))...
            } //close if (!SenseCam_data_directory.Equals(""))...
            else
            {
                write_output("Please make sure SenseCam is plugged in");
            } //close ... else ... if (!SenseCam_data_directory.Equals(""))

            //and indicated that we're finished processing the data...
            if (!is_part_of_multiple_folder_upload) //but only if it isn't part of a multiple folder upload
            { //as we want to allow this thread move on to the next image afterwards...
                write_output(DateTime.Now.ToLongTimeString());
                data_processing_finished_callback();
            } //close if (!is_part_of_multiple_folder_upload)...

        } //end method upload_SenseCam_data()...



        private void write_SenseCam_time_csv_file()
        {
            try
            {
                DateTime utc_now = DateTime.UtcNow;
                TextWriter time_csv_file = new StreamWriter(SenseCam_data_directory.Replace("DATA", "SYSTEM") + "TIME.CSV");

                //TIM,09,02,12 (hour,minute,second)
                //DAT,09,06,12 (year,month,day)
                time_csv_file.WriteLine("TIM," + utc_now.Hour + "," + utc_now.Minute + "," + utc_now.Second);
                time_csv_file.WriteLine("DAT," + utc_now.Year + "," + utc_now.Month + "," + utc_now.Day);

                time_csv_file.Close(); //and finally let's close the file writer object...
            }
            catch (Exception excep)
            {
                write_output("Couldn't write time file to SenseCam!!!");
            }
        } //close method write_SenseCam_time_csv_file()...



        private void write_Vicon_Revue_time_csv_file()
        {
            try
            {
                DateTime utc_now = DateTime.UtcNow;
                TextWriter time_csv_file = new StreamWriter(SenseCam_data_directory.Replace("DATA", "SYSTEM") + "TIME.CSV");

                time_csv_file.WriteLine("tim " + utc_now.Hour + " " + utc_now.Minute + " " + utc_now.Second);
                time_csv_file.WriteLine();
                time_csv_file.WriteLine("dat " + utc_now.Year + " " + utc_now.Month + " " + utc_now.Day);
                time_csv_file.WriteLine();
                time_csv_file.Close(); //and finally let's close the file writer object...
            }
            catch (Exception excep)
            {
                write_output("Couldn't write time file to SenseCam!!!");
            }
        } //close method write_Vicon_Revue_time_csv_file()...

        private void write_Vicon_Revue_boot_bat_file()
        {
            try
            {
                TextWriter boot_bat_file = new StreamWriter(SenseCam_data_directory.Replace("DATA", "SYSTEM") + "BOOT.BAT");

                boot_bat_file.WriteLine("# Vicon Revue boot file executed after every USB cable # disconnect or hard reset.");
                boot_bat_file.WriteLine("# the trig command below will enable the manual, time");
                boot_bat_file.WriteLine("# triggered, light triggered and accelerator triggered ");
                boot_bat_file.WriteLine("# image captures ");
                boot_bat_file.WriteLine("#trig 0x1B02");
                boot_bat_file.WriteLine();
                boot_bat_file.WriteLine("# to activate PIR triggered captures, use trig 0x1F02");
                boot_bat_file.WriteLine("trig 0x1F02");

                boot_bat_file.Close(); //and finally let's close the file writer object...
            }
            catch (Exception excep)
            {
                write_output("Couldn't write time file to SenseCam!!!");
            }
        } //close method write_Vicon_Revue_boot_bat_file()...

        private void write_Autographer_time_file()
        {
            try
            {
                DateTime utc_now = DateTime.UtcNow;
                TextWriter autographer_auto_ini_file = new StreamWriter(SenseCam_data_directory.Replace(@"\DATA", "") + "auto.ini");

                autographer_auto_ini_file.WriteLine("#");
                autographer_auto_ini_file.WriteLine("#    auto.ini file V10a");
                autographer_auto_ini_file.WriteLine("#");
                autographer_auto_ini_file.WriteLine("");
                autographer_auto_ini_file.WriteLine("[Settings]");
                autographer_auto_ini_file.WriteLine("");
                autographer_auto_ini_file.WriteLine("#DateTime=2013/06/11 16:20:00"); //todo when writing the time, where can I indicate whether it is UTC time, or local time +0x:00 hrs (matching sensor.csv)??
                autographer_auto_ini_file.WriteLine("DateTime=" + utc_now.Year + "/" + format_number(utc_now.Month) + "/" + format_number(utc_now.Day) + " " + format_number(utc_now.Hour) + ":" + format_number(utc_now.Minute) + ":" + format_number(utc_now.Second));
                autographer_auto_ini_file.WriteLine("SaveRaw=0");
                autographer_auto_ini_file.WriteLine("");
                autographer_auto_ini_file.WriteLine("ExposureThld=1");
                autographer_auto_ini_file.WriteLine("MvmntThld=120");
                autographer_auto_ini_file.WriteLine("GainThld=48");
                autographer_auto_ini_file.WriteLine("");
                autographer_auto_ini_file.WriteLine("#AvgLumThld=");
                autographer_auto_ini_file.WriteLine("#BrightPixelValueThld=");
                autographer_auto_ini_file.WriteLine("#BrightPixelNumThld=");
                autographer_auto_ini_file.WriteLine("");
                autographer_auto_ini_file.WriteLine("ImageScoreThld=1000");
                autographer_auto_ini_file.WriteLine("");
                autographer_auto_ini_file.WriteLine("SensorLogger=0");
                autographer_auto_ini_file.WriteLine("#CalibrateMagnetometer=");
                autographer_auto_ini_file.WriteLine("");
                autographer_auto_ini_file.WriteLine("#PowerDownAfterUSBUnplug=0");
                autographer_auto_ini_file.WriteLine("");
                autographer_auto_ini_file.WriteLine("[Format]");
                autographer_auto_ini_file.WriteLine("");
                autographer_auto_ini_file.WriteLine("#FormatVolume=true");
                
                autographer_auto_ini_file.Close(); //and finally let's close the file writer object...
            }
            catch (Exception excep)
            {
                write_output("Couldn't write time file to SenseCam!!!");
            }
        } //close method write_Autographer_time_file()...



        private string format_number(int num)
        {
            if (num < 10)
                return "0" + num;
            else return num.ToString();
        } //end format_number()




        #region copying files from SenseCam to PC

        //these variables must be declared outside methods, as they will be updated by recursively called folders (i.e. must transfer multiple (sub)folders
        int upload_transer_num_images_to_copy_in_total = 0;
        int upload_transfer_num_images_copied_so_far = 0;

        private void copy_SenseCam_files_to_local_machine(string SenseCam_folder, string local_machine_directory, bool sensor_csv_found)
        {
            //firstly let's check for the Sensor.csv file, if it hasn't already been found yet...
            //if (type_of_device == DeviceType.Revue || type_of_device == DeviceType.SenseCam)
            if(!sensor_csv_found)
            {
                FileInfo[] sensor_csv = attempt_to_retrieve_files_from_directory(SenseCam_folder, "*.CSV", "*.txt");
                foreach (FileInfo csv_file in sensor_csv)
                {
                    if ((csv_file.Length > MINIMUM_FILE_SIZE / 2) && (csv_file.Length < MAXIMUM_FILE_SIZE)) //to deal with potentially rogue sensor.csv files that are 0KB in size... meaning that we then just to a segmentation based on the images...
                    {
                        attempt_to_copy_csv_file(SenseCam_folder + @"\" + csv_file.Name, local_machine_directory + @"\" + csv_file.Name);
                        sensor_csv_found = true;
                    } //close if (csv_file.Length > MINIMUM_FILE_SIZE / 2) ...
                } //close foreach (FileInfo csv_file in sensor_csv)...

            } //close if (!sensor_csv_found)...


            //if there are images here, we'll copy them all over to the local desktop directory
            //foreach (FileInfo image_file in new DirectoryInfo(SenseCam_folder).GetFiles("*.JPG"))
            foreach (FileInfo image_file in attempt_to_retrieve_files_from_directory(SenseCam_folder, "*.JPG"))// new DirectoryInfo(SenseCam_folder).GetFiles("*.JPG"))
                attempt_to_copy_image_file(SenseCam_folder, local_machine_directory, image_file);


            //let's inform the user on how we're getting on
            send_image_transfer_progress_update_to_user(0, 50); //for copying we'll update progress between 0 and 50 percent

                        
            //and let's traverse through all the folders on the SenseCam system
            foreach (string subfolder in attempt_to_retrieve_subdirectories_from_directory(SenseCam_folder))
            {
                if(!(subfolder.EndsWith("256_192") || subfolder.EndsWith("640_480"))) //for the Autographer, a break clause is needed to not look at folders "256_192" and "640_480" (they're just low resolution versions of images already copied across)
                    copy_SenseCam_files_to_local_machine(subfolder + @"\", local_machine_directory, sensor_csv_found); //NOTE THAT I NEED THE @"\" AT THE END SO AS TO BE ABLE TO READ FILES WITHIN THE DIRECTORY
            }

        } //end method copy_SenseCam_files_to_local_machine()



        private void send_image_transfer_progress_update_to_user(int start_percent, int end_percent)
        {
            //now use the class/object variables to send update on transfer progress so far
            data_feedback_callback("file_transfer_progress:" + get_percentage_of_files_transferred(start_percent, end_percent)); //just adding in plus 1 so we'll have no divide by zero errors and the like
        } //close method send_progress_update_to_user;


        private int get_percentage_of_files_transferred(int start_number, int end_number)
        {
            try
            {
                if (upload_transer_num_images_to_copy_in_total >= upload_transfer_num_images_copied_so_far)
                    return start_number + (int)((end_number - start_number) * ((upload_transfer_num_images_copied_so_far + 1) / (decimal)(upload_transer_num_images_to_copy_in_total + 1)));
                else return start_number + (int)((end_number - start_number) * ((upload_transfer_num_images_copied_so_far + 1) / (decimal)(20000))); ;
            }
            catch (Exception excep) { return DateTime.Now.Second; }
        } //close method get_percentage_of_files_transferred()...


        /// <summary>
        /// this method copies a file, with error handling
        /// </summary>
        /// <param name="source_file"></param>
        /// <param name="destination_file"></param>
        private void attempt_to_copy_csv_file(string source_file, string destination_file)
        {
            try
            {
                File.Copy(source_file, destination_file);
                upload_transer_num_images_to_copy_in_total = Upload_Manipulated_Sensor_Data.get_num_images_in_csv_file(destination_file);
            } //close try
            catch (Exception excep)
            {
                write_output("Couldn't copy file: " + source_file);
            } //close catch ... try ...
        } //end method attempt_to_copy_file()


        /// <summary>
        /// this method copies an image file, with error handling
        /// </summary>
        /// <param name="source_file"></param>
        /// <param name="destination_file"></param>
        private void attempt_to_copy_image_file(string SenseCam_folder, string local_machine_directory, FileInfo image_file)
        {
            try
            {
                if ((image_file.Length > MINIMUM_FILE_SIZE) && (image_file.Length < MAXIMUM_FILE_SIZE) && image_file.Exists)
                {
                    File.Copy(SenseCam_folder + @"\" + image_file.Name, local_machine_directory + @"\" + image_file.Name);
                    upload_transfer_num_images_copied_so_far++;
                } //close if (image_file.Length > MINIMUM_FILE_SIZE & image_file.Exists)...
            } //close try
            catch (Exception excep)
            {
                write_output("Couldn't copy file: " + image_file.Name);
            } //close catch ... try ...
        } //end method attempt_to_copy_image_file()


        /// <summary>
        /// this method retrieves files (of a type) from a directory, with error handling
        /// </summary>
        /// <param name="directory_path"></param>
        /// <param name="file_type"></param>
        /// <returns></returns>
        private FileInfo[] attempt_to_retrieve_files_from_directory(string directory_path, string file_type)
        {
            try
            {
                return new DirectoryInfo(directory_path).GetFiles(file_type);
            }
            catch (Exception excep)
            {
                return new FileInfo[0];
            } //end try...catch
        } //close method attempt_to_retrieve_files_from_directory()...

        private FileInfo[] attempt_to_retrieve_files_from_directory(string directory_path, string file_type1, string file_type2)
        {
            //here we'll try to return files of "file_type2" if we can't retrieve files of "file_type1"
            FileInfo[] output_files = attempt_to_retrieve_files_from_directory(directory_path, file_type1); //firstly try to get files of type1

            if (output_files.Length == 0) //if we haven't found anything of this type...
                output_files = attempt_to_retrieve_files_from_directory(directory_path, file_type2); //instead we'll try to get files of type2

            return output_files; //then return our output files
        } //close method attempt_to_retrieve_files_from_directory()...private F


        /// <summary>
        /// this method retrieves subdirectories from a directory, with error handling
        /// </summary>
        /// <param name="directory_path"></param>
        /// <param name="file_type"></param>
        /// <returns></returns>
        private static string[] attempt_to_retrieve_subdirectories_from_directory(string directory_path)
        {
            try
            {
                return Directory.GetDirectories(directory_path);
            }
            catch (Exception excep)
            {
                return new string[0];
            } //end try...catch
        } //close method attempt_to_retrieve_subdirectories_from_directory()...

        #endregion copying files from SenseCam to PC




        #region deleting files from SenseCam

        private void delete_files_on_SenseCam(string SenseCam_data_directory)
        {
            //firstly let's declare the process we're calling to delete the images from the SenseCam, and also pass in the SenseCam_data_directory as an argument to help the process locate the destination where files should be removed from
            char double_quote = '"';
            string correct_path_format_for_deletion_process = SenseCam_data_directory.Substring(0, SenseCam_data_directory.Length - 1);
            ProcessStartInfo psi = new ProcessStartInfo("delete_images_from_sensecam", double_quote + correct_path_format_for_deletion_process + double_quote);

            //these 2 lines below will make the process a "silent" one, meaning the user will not be aware of what's going on...
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;

            //now finally we'll declare the process
            Process p = new Process();
            p.StartInfo = psi; //and associate the setup information with it
            p.Start(); //and finally we start the process...
        } //close method delete_files_on_SenseCam()...

        #endregion end deleting files from SenseCam




        #region fixing JPG headers of SenseCam images

        /// <summary>
        /// this method fixes the EXIF headers of all the SenseCam images...
        /// </summary>
        /// <param name="selected_folder"></param>
        private void fix_JPG_header_of_all_images_in_folder(string selected_folder, int start_percent_progress_to_show_to_user, int end_percent_progress_to_show_to_user)
        {
            //thanks to Dian Zhang, a CLARITY intern, who helped figured out how to change the JPG header so "raw" SenseCam images can be displayed on a WPF browser

            selected_folder += @"\";

            //1. let's get all the image files in the directory
            string[] all_jpeg_files_in_directory = attempt_to_retrieve_string_array_of_files_from_directory(selected_folder, "*.JPG");
            try { upload_transer_num_images_to_copy_in_total = all_jpeg_files_in_directory.Length; }
            catch (Exception excep) { }


            //2. then let's make a FileStream object, and also a "valid header" to replace any invalid exif headers
            FileStream jpeg_filestream;
            byte[] valid_exif_header = new byte[] { 17, 74, 70, 73, 70, 0, 1, 1, 1, 0, 96, 0, 96, 0, 0, 255 }; //from the 5th byte onwards


            int update_progress_counter = 0, update_progress_send_message = 50;
            upload_transfer_num_images_copied_so_far = 0;

            //3. now let's go through each image file
            byte[] current_header = new byte[20];
            foreach (string individual_image_path in all_jpeg_files_in_directory)
            {
                try
                {
                    jpeg_filestream = new FileStream(individual_image_path, FileMode.Open, FileAccess.ReadWrite);

                    //now let's get what the current header is with this image (the first 20 bytes)
                    jpeg_filestream.Read(current_header, 0, current_header.Length);


                     //AD fix for journey to school project on 16-May-2011...
                    //unscramble images here!!!
                    //now we check if the first 2 bytes are 0, which means that it's not recognised as a JPEG
                    if (current_header[0] == 0 && current_header[1] == 0)
                    {
                        jpeg_filestream.Seek(0, SeekOrigin.Begin);

                        //and now fix the invalid header
                        jpeg_filestream.WriteByte(255); //the first 2 bytes should be 255 & 216
                        jpeg_filestream.WriteByte(216); //as this corresponds to Hexidecimal 0xFFD8 (JPEG header which indicates it's actually a JPEG)
                    } //close if (current_header[0] == 0 && current_header[1] == 0)...
                    
                    
                    //now we check if the 5th byte is 17 and the 19th byte is 9, which means the image has an invalid header
                    //the 19th bit check see whether the image has been fixed or not. 
                    //if they're invalid, we just rewrite the whole header (using a valid header from a pre-defined "good" image)
                    if (current_header[5] == 17 && current_header[19] == 9)
                    {
                        jpeg_filestream.Seek(5, SeekOrigin.Begin);

                        //and now fix the invalid header
                        for (int c = 0; c < valid_exif_header.Length; c++)
                            jpeg_filestream.WriteByte(valid_exif_header[c]);
                    } //close if (current_header[5] == 17 && current_header[19] == 9)...
                    

                    //and let's close the filestream that's open, to allievate any memory leak problems that may occur
                    jpeg_filestream.Close();

                    //let's inform the user of progress on fixing the EXIF headers...
                    upload_transfer_num_images_copied_so_far++;
                    update_progress_counter++;
                    if (update_progress_counter % update_progress_send_message == 0)
                    {
                        //let's inform the user on how we're getting on
                        send_image_transfer_progress_update_to_user(start_percent_progress_to_show_to_user, end_percent_progress_to_show_to_user); //for fixing exif we'll update progress between 50 and 100 percent
                        update_progress_counter = 0; //and reset the counter... makes division slightly easier I think
                    } //close if (update_progress_counter % update_progress_send_message == 0)
                } //close try...
                catch (Exception excep) { } //and in case there's any errors in dealing with "jpeg_filestream", we attempt to handle them by skipping the problem image in question
            } //close foreach (string individual_image_path in all_jpeg_files_in_directory)...

        } //end method fix_JPG_header_of_all_images_in_folder()


        /// <summary>
        /// this method retrieves files (of a type) from a directory, with error handling
        /// </summary>
        /// <param name="directory_path"></param>
        /// <param name="file_type"></param>
        /// <returns></returns>
        private string[] attempt_to_retrieve_string_array_of_files_from_directory(string directory_path, string file_type)
        {
            try
            {
                return Directory.GetFiles(directory_path, file_type);
            }
            catch (Exception excep)
            {
                return null;
            } //end try...catch
        } //close method attempt_to_retrieve_string_array_of_files_from_directory()...

        #endregion fixing JPG headers of SenseCam images




        private void segment_folder_images_into_events_and_upload_to_db(int user_id, string selected_folder, DeviceType device_type)
        {
            selected_folder += @"\";
            //1. READ SENSOR.CSV INFORMATION IN FOLDER
            //MANIPULATE THAT INFORMATION
            //write_output("start manipulated_images => " + DateTime.Now.ToString());
            Segmentation_Image_Rep[] manipulated_images;
            bool sensor_file_exists = File.Exists(selected_folder + "sensor.csv") || File.Exists(selected_folder+"image_table.txt");
            if (sensor_file_exists)
                manipulated_images = Upload_Manipulated_Sensor_Data.process_csv_file(selected_folder, user_id, get_local_hours_ahead_of_utc_time(), device_type);
            else manipulated_images = Upload_Manipulated_Sensor_Data.process_folder_with_no_csv_information(selected_folder);

            if (manipulated_images.Length != 0)
            {
                //2. NOW NORMALISE, FUSE, THRESHOLD THE DATA SOURCES (ACC COMBINED AND ACC X)
                //BASICALLY GET A LIST OF THE BOUNDARY TIMES (AND CHUNK BOUNDARY TIMES)
                //READ IN THE new_images TABLE FROM THE DATABASE
                //NORMALISE ACC X AND COMBINED ACC COLUMNS
                //FUSE BOTH SOURCES TOGETHER
                //THRESHOLD FUSED SCORES (KAPUR 64 BINS)
                //REMOVE BOUNDARIES THAT OCCUR WITHIN 3 MINUTES OF A PREVIOUS BOUNDARY
                //RETURN LIST OF BOUNDARY TIMES FOR WHOLE LIST (SO BOUNDS + CHUNK START TIMES (APART FROM CHUNK 0 OF COURSE))
                //write_output("start list of calculated events => " + DateTime.Now.ToString());
                Segmentation_Event_Rep[] list_of_calculated_events;
                
                if (sensor_file_exists)
                    list_of_calculated_events = Fuse_And_Identify_Segments.get_boundary_times_for_all_images(manipulated_images);
                else list_of_calculated_events = Fuse_And_Identify_Segments.get_boundary_times_for_all_non_csv_images(manipulated_images);
                
                if (list_of_calculated_events != null)
                {
                    //3. AND WRITE OUT THE LIST OF IMAGES AND EVENTS TO THE DATABASE!			
                    upload_all_data_to_database(manipulated_images, list_of_calculated_events, selected_folder, user_id, get_local_hours_ahead_of_utc_time());


                    //4. FINALLY UPDATE THE IMAGE.DAT FILE SO AS TO REFLECT THE NEW BOUNDARIES AS BOOKMARKS!
                    //the reason I leave this step to last is that there may be a problem in updating the database ... if there is, I don't want to update the image.dat file as that would then mean that if I try to redo this process it'll think that it's already successfully completed (going by the image.dat file being updated) ... now this will not happen as image.dat isn't updated unto after the database updating
                    //write_output("start update image.dat => " + DateTime.Now.ToString());                    
                    Update_Image_Dat_File.update_image_dat(selected_folder, manipulated_images, list_of_calculated_events, get_local_hours_ahead_of_utc_time());

                    write_output(selected_folder + " -> Images segmented into distinct events/activities!");
                } //end if (list_of_calculated_events != null)
                else
                {
                    write_output(selected_folder + " -> There must be more than 1 image present to enable the calculation of events -> " + selected_folder);
                } //end if (list_of_calculated_events != null)
            } //end if (manipulated_images.Length != 0)
            else
            {
                write_output(selected_folder + " -> No images detected for -> " + selected_folder);
            } //end if ... else ... if (manipulated_images.Length != 0)
        } //end method segment_folder_images_into_events_and_upload_to_db()



        private void segment_folder_images_into_events_and_upload_to_db(int user_id, string selected_folder, DeviceType device_type, string external_episode_definition_csv_file)
        {
            selected_folder += @"\";
            //1. READ SENSOR.CSV INFORMATION IN FOLDER
            //MANIPULATE THAT INFORMATION
            //write_output("start manipulated_images => " + DateTime.Now.ToString());
            Segmentation_Image_Rep[] manipulated_images;
            bool sensor_file_exists = File.Exists(selected_folder + "sensor.csv");
            if (sensor_file_exists)
                manipulated_images = Upload_Manipulated_Sensor_Data.process_csv_file(selected_folder, user_id, get_local_hours_ahead_of_utc_time(), device_type);
            else manipulated_images = Upload_Manipulated_Sensor_Data.process_folder_with_no_csv_information(selected_folder);

            if (manipulated_images.Length != 0)
            {
                //2. NOW NORMALISE, FUSE, THRESHOLD THE DATA SOURCES (ACC COMBINED AND ACC X)
                //BASICALLY GET A LIST OF THE BOUNDARY TIMES (AND CHUNK BOUNDARY TIMES)
                //READ IN THE new_images TABLE FROM THE DATABASE
                //NORMALISE ACC X AND COMBINED ACC COLUMNS
                //FUSE BOTH SOURCES TOGETHER
                //THRESHOLD FUSED SCORES (KAPUR 64 BINS)
                //REMOVE BOUNDARIES THAT OCCUR WITHIN 3 MINUTES OF A PREVIOUS BOUNDARY
                //RETURN LIST OF BOUNDARY TIMES FOR WHOLE LIST (SO BOUNDS + CHUNK START TIMES (APART FROM CHUNK 0 OF COURSE))
                //write_output("start list of calculated events => " + DateTime.Now.ToString());
                Segmentation_Event_Rep[] list_of_calculated_events;

                //todo think of better way to visualise request for CSV file of events formatted correctly in format of: start_time, end_time, description
                List<Segmentation_Event_Rep> user_defined_episodes = new List<Segmentation_Event_Rep>();
                user_defined_episodes = Segmentation_Event_Rep.read_in_list_of_user_defined_episodes_from_file(external_episode_definition_csv_file, selected_folder);
                list_of_calculated_events = Fuse_And_Identify_Segments.SET_boundary_times_for_all_images(manipulated_images, user_defined_episodes);
                

                if (list_of_calculated_events != null)
                {
                    //3. AND WRITE OUT THE LIST OF IMAGES AND EVENTS TO THE DATABASE!			
                    upload_all_data_to_database(manipulated_images, list_of_calculated_events, selected_folder, user_id, get_local_hours_ahead_of_utc_time());


                    //4. FINALLY UPDATE THE IMAGE.DAT FILE SO AS TO REFLECT THE NEW BOUNDARIES AS BOOKMARKS!
                    //the reason I leave this step to last is that there may be a problem in updating the database ... if there is, I don't want to update the image.dat file as that would then mean that if I try to redo this process it'll think that it's already successfully completed (going by the image.dat file being updated) ... now this will not happen as image.dat isn't updated unto after the database updating
                    //write_output("start update image.dat => " + DateTime.Now.ToString());                    
                    Update_Image_Dat_File.update_image_dat(selected_folder, manipulated_images, list_of_calculated_events, get_local_hours_ahead_of_utc_time());

                    write_output(selected_folder + " -> Images segmented into distinct events/activities!");
                } //end if (list_of_calculated_events != null)
                else
                {
                    write_output(selected_folder + " -> There must be more than 1 image present to enable the calculation of events -> " + selected_folder);
                } //end if (list_of_calculated_events != null)
            } //end if (manipulated_images.Length != 0)
            else
            {
                write_output(selected_folder + " -> No images detected for -> " + selected_folder);
            } //end if ... else ... if (manipulated_images.Length != 0)
        } //end method segment_folder_images_into_events_and_upload_to_db()


        private int get_local_hours_ahead_of_utc_time()
        {
            //todo, how to deal with uploading data when it's winter time, but the data was collected ages ago in summer time ... then the UTC/now time difference changes by one ... how to account for this?
            //return 12; //for actical_sensecam processing...
            TimeSpan time_diff = DateTime.Now - DateTime.UtcNow;
            return time_diff.Hours;
        } //close method get_utc_hours_ahead_of_local_time()...




        private void upload_all_data_to_database(Segmentation_Image_Rep[] all_images, Segmentation_Event_Rep[] all_events, string images_folder, int user_id, int local_hours_ahead_of_utc_time)
        {
            //write_output("start upload_all_images => " + DateTime.Now.ToString());
            //1 upload all the images to the database
            upload_new_images(all_images, images_folder, user_id);
            
            //write_output("start upload_new_events => " + DateTime.Now.ToString());
            //2 upload all the events to the database
            upload_new_events(all_events, images_folder, user_id, local_hours_ahead_of_utc_time);

            //write_output("start update_image_event_ids => " + DateTime.Now.ToString());
            //3 call a database stored procedure to update the event_id field in the all_Images table
            update_event_id_field_of_all_images_table(user_id);
            //write_output("end update_image_event_ids => " + DateTime.Now.ToString());
        } //end method upload_all_data_to_database()




        private void upload_new_images(Segmentation_Image_Rep[] image_list, string images_folder, int user_id)
        {
            // http://sqlite.phxsoftware.com/forums/t/134.aspx
            DbConnection con = new SQLiteConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);

            con.Open();
            using (DbTransaction dbTrans = con.BeginTransaction())
            {
                using (DbCommand cmd = con.CreateCommand())
                {
                    //cmd.CommandText = "INSERT INTO TestCase(MyValue) VALUES(?)";
                    cmd.CommandText = "INSERT INTO All_Images(user_id,image_path,image_time) VALUES(?,?,?)";
                    DbParameter user_id_field, image_path_field, image_time_field;
                    user_id_field = cmd.CreateParameter();
                    image_path_field = cmd.CreateParameter();
                    image_time_field = cmd.CreateParameter();
                    cmd.Parameters.Add(user_id_field);
                    cmd.Parameters.Add(image_path_field);
                    cmd.Parameters.Add(image_time_field);

                    for (int n = 0; n < image_list.Length; n++)
                    {
                        user_id_field.Value = user_id;
                        image_path_field.Value = images_folder + image_list[n].get_image_name();
                        image_time_field.Value = image_list[n].get_image_time();
                        cmd.ExecuteNonQuery();
                    }
                }
                dbTrans.Commit();
            }
            con.Close();
        } //end method uplodat_images_to_database()
        



        private void upload_new_events(Segmentation_Event_Rep[] event_list, string images_folder, int user_id, int local_hours_ahead_of_utc_time)
        {
            // http://sqlite.phxsoftware.com/forums/t/134.aspx
            DbConnection con = new SQLiteConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);
            con.Open();
            using (DbTransaction dbTrans = con.BeginTransaction())
            {
                using (DbCommand cmd = con.CreateCommand())
                {
                    //cmd.CommandText = "INSERT INTO TestCase(MyValue) VALUES(?)";
                    cmd.CommandText = "INSERT INTO All_Events(user_id,day,utc_day,start_time,end_time,keyframe_path,number_times_viewed) VALUES(?,?,?,?,?,?,?)";
                    DbParameter user_id_field, day_field, utc_day_field, start_time_field, end_time_field,keyframe_path_field,number_times_viewed_field;
                    user_id_field = cmd.CreateParameter();
                    day_field = cmd.CreateParameter();
                    utc_day_field = cmd.CreateParameter();
                    start_time_field = cmd.CreateParameter();
                    end_time_field = cmd.CreateParameter();
                    keyframe_path_field = cmd.CreateParameter();
                    number_times_viewed_field = cmd.CreateParameter();
                    cmd.Parameters.Add(user_id_field);
                    cmd.Parameters.Add(day_field);
                    cmd.Parameters.Add(utc_day_field);
                    cmd.Parameters.Add(start_time_field);
                    cmd.Parameters.Add(end_time_field);
                    cmd.Parameters.Add(keyframe_path_field);
                    cmd.Parameters.Add(number_times_viewed_field);

                    for (int row_counter = 0; row_counter < event_list.Length; row_counter++)
                    {
                        user_id_field.Value = user_id;
                        day_field.Value = event_list[row_counter].get_day(); //local time day
                        utc_day_field.Value = event_list[row_counter].get_day().AddHours(-local_hours_ahead_of_utc_time); //utc_day              
                        start_time_field.Value = event_list[row_counter].get_start_time(); //local time start time
                        end_time_field.Value = event_list[row_counter].get_end_time(); //local time end time
                        keyframe_path_field.Value = images_folder + event_list[row_counter].get_keyframe_image_name();
                        number_times_viewed_field.Value = 0;
                        cmd.ExecuteNonQuery();
                    }
                }
                dbTrans.Commit();
            }
            con.Close();

        } //end method upload_new_events()



        
        private void update_event_id_field_of_all_images_table(int user_id)
        {
            SQLiteConnection cnn = new SQLiteConnection(global::SenseCamBrowser1.Properties.Settings.Default.DCU_SenseCamConnectionString);
            SQLiteCommand command = new SQLiteCommand(cnn);
            cnn.Open();
            //firstly get the most recent event not updated...
            command.CommandText = Database_Versioning.text_for_stored_procedures.spGet_most_recent_event_id_for_user(user_id);
            int most_recent_event_id = int.Parse(command.ExecuteScalar().ToString());
            
            //then update images and sensor_readings table with relevant event id values
            command.CommandText = Database_Versioning.text_for_stored_procedures.spUpdate_newly_uploaded_images_and_sensor_readings_with_relevant_event_id(user_id, most_recent_event_id);
            command.ExecuteNonQuery();

            //finally tidy up spurious events from database...
            command.CommandText = Database_Versioning.text_for_stored_procedures.spUpdate_newly_uploaded_images_tidy_up_spurious_events(user_id);
            command.ExecuteNonQuery();

            cnn.Close();
        } //end method update_event_id_field_of_all_images_table


    } //end class...
} //end namespace...
