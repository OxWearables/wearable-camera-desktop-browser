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
using System.Text;
using System.Data;
using System.Collections;
using System.Data.SqlClient;
using System.IO;
using System.Configuration;
using System.Data.SQLite;
using System.Data.Common;

namespace SenseCamBrowser1.Upload_Images_and_Segment_into_Events
{
    class Upload_Manipulated_Sensor_Data
    {


        private static int MINIMUM_FILE_SIZE = 48;//AD 11/09/12 2048; //THE SMALLEST SIZE (IN BYTES) ANY SENSECAM IMAGE IS ALLOWED TO BE ... IF IT'S SMALLER THAN THIS WE DISREGARD IT (AS OTHERWISE IT MAY AFFECT any subsequent image PROCESSING as these images are generally corrupt images or uniform black ones)
        public static int MAXIMUM_NUMBER_MINUTES_BETWEEN_IMAGES_ALLOWED_TO_STAY_IN_THE_SAME_EVENT = int.Parse(ConfigurationManager.AppSettings["maximum_num_minutes_between_images_allowed_to_stay_in_same_event"].ToString());
        public static int USER_HOUR_TIME_ADJUSTMENT_HOURS = int.Parse(ConfigurationManager.AppSettings["hour_offset_of_uploaded_data"].ToString());


        public static Segmentation_Image_Rep[] process_csv_file(string local_folder, int user_id, Upload_and_Segment_Images_Thread.DeviceType device_type)
        {
            //read the sensor.csv file in the given folder
            //From reading that it'll store the sensor values
            //secondly we manipulate the sensor values (namely the accelerometer combined source)
            //FINALLY WE RETURN A LIST OF IMAGES WITH MANIPULATED SENSOR VALUES (I.E. THOSE SENSOR READINGS WHICH CORRESPOND TO AN IMAGE CAPTURE)

            //read in from the csv file
            Sensor_Reading[] sensor_values;
            if (device_type == Upload_and_Segment_Images_Thread.DeviceType.Autographer)
                sensor_values = read_Autographer_image_table_txt_file(local_folder + "image_table.txt", USER_HOUR_TIME_ADJUSTMENT_HOURS);
            else if (device_type == Upload_and_Segment_Images_Thread.DeviceType.Revue)
                sensor_values = read_Vicon_Revue_csv_file(local_folder + "sensor.csv", USER_HOUR_TIME_ADJUSTMENT_HOURS);
            else sensor_values = read_SenseCam_csv_file(local_folder + "sensor.csv", USER_HOUR_TIME_ADJUSTMENT_HOURS);

            //NOW MANIPULATE THE SENSOR VALUES!
            manipulate_sensor_information(sensor_values, device_type);

            //now upload the sensor readings to the database...
            upload_sensor_readings_to_db(sensor_values, user_id, device_type);

            //FINALLY RETURN A LIST OF IMAGES WITH MANIPULATED SENSOR VALUES
            return get_image_list(sensor_values, local_folder);
        } //end method process_csv_files()




        private static Segmentation_Image_Rep[] get_image_list(Sensor_Reading[] sensor_vals, string folder_name)
        {
            //THIS RETURNS A LIST OF IMAGES WITH MANIPULATED SENSOR VALUES
            //BASICALLY GO THROUGH ALL THE SENSOR READINGS AND ONLY STORE THOSE THAT HAVE AN IMAGE NAME
            //15/10/07, AND IN PARTICULAR JUST THOSE IMAGES THAT EXIST AND THAT ARE OVER 2KB IN SIZE!
            List<Segmentation_Image_Rep> list_of_images = new List<Segmentation_Image_Rep>();

            FileInfo individual_file; //USED TO INVESTIGATE IF A FILE EXISTS AND ALSO TO CHECK IT'S FILE SIZE

            if (sensor_vals.Length > 0) //and just double check that we're now dealing with a spurious sensor.csv file...
            {
                DateTime time_of_first_image_in_chunk = sensor_vals[0].get_sample_time(); //used to annotate what day each image belongs to
                int current_chunk_id = -5;
                foreach (Sensor_Reading reading in sensor_vals)
                {
                    //firstly let's set the day that this reading belongs to ... i.e. the first image/sensor time in the chunk
                    if (reading.get_chunk_id() != current_chunk_id)
                    {
                        time_of_first_image_in_chunk = reading.get_sample_time();
                        current_chunk_id = reading.get_chunk_id();
                    } //close if (reading.get_chunk_id() != current_chunk_id)...

                    
                    if(reading.get_image_name() != null)
                    {
                        try //just in case a bogus image name has been read in... see next line
                        {
                            individual_file = new FileInfo(folder_name + reading.get_image_name()); //LET'S GET THE INFORMATION FOR THIS FILE
                            if (individual_file.Exists)
                            {
                                //firstly check that the file exists, then if it does...

                                if (individual_file.Length > MINIMUM_FILE_SIZE)
                                {
                                    list_of_images.Add(new Segmentation_Image_Rep(reading.get_chunk_id(), reading.get_image_name(), time_of_first_image_in_chunk, reading.get_sample_time(), reading.get_manipulated_acc_combined(), reading.get_manipulated_acc_x(), reading.get_manipulated_pir()));
                                } //end if (individual_file.Length > MINIMUM_FILE_SIZE)

                            } //end if (individual_file.Exists)
                        }
                        catch (Exception excep) { }
                    } //end if (reading.get_image_name() != null)
                } //end foreach (Sensor_Reading[] reading in sensor_vals)
            } //close if (sensor_vals.Length > 0)...

            //now return an array of type Image_Rep which represents all the images in the folder (plus their associated manipulated acc_comb and acc_x values)
            return list_of_images.ToArray();
        } //end method get_image_list()




        private static void manipulate_sensor_information(Sensor_Reading[] sensor_list, Upload_and_Segment_Images_Thread.DeviceType device_type)
        {
            //1. I must split the data into chunks, then manipulate the chunks
            int[] starting_chunk_positions = get_start_positions_of_all_chunks(sensor_list);

            //2. now go through each chunk and manipulate its values!
            for (int counter = 0; counter < starting_chunk_positions.Length - 1; counter++)
            {
                Sensor_Manipulation.manipulate_relevant_sensor_values(sensor_list, starting_chunk_positions[counter], starting_chunk_positions[counter + 1] - 1, device_type); //i.e. 2nd one is the next starting point slided back 1 (i.e. position of start of next chunk is just one past the end of the current chunk
            }
            //finally do the very last chunk ... if there's only 1 chunk, the for loop above will not be executed!
            if(starting_chunk_positions.Length > 0) //let's just double check there's relevant chunk information at first
                Sensor_Manipulation.manipulate_relevant_sensor_values(sensor_list, starting_chunk_positions[starting_chunk_positions.Length - 1], sensor_list.Length - 1, device_type); //now calculate the final chunk (it ends with the very last sensor in the list!)

        } //end method manipulate_image_information()




        private static int[] get_start_positions_of_all_chunks(Sensor_Reading[] sensor_list)
        {
            if (sensor_list.Length == 0)
                return new int[]{};
            else
            {
                //firstly get the number of chunks
                int number_chunks = (sensor_list[sensor_list.Length - 1].get_chunk_id() - sensor_list[0].get_chunk_id()) + 1; //i.e. +1 because if only 1 chunk, we don't want a value of zero!
                //int[] chunk_start_positions = new int[number_chunks];
                List<int> chunk_start_positions = new List<int>();


                //now identify the chunk boundaries, and store the starting chunk_id positions
                int last_chunk_id, current_chunk_id; // chunk_counter;  // AD 20/12/12
                last_chunk_id = sensor_list[0].get_chunk_id();

                //chunk_start_positions[0] = 0; //the starting position will always be zero!  // AD 20/12/12
                chunk_start_positions.Add(0); //the starting position will always be zero!  // AD 20/12/12

                //chunk_counter = 1; // AD 20/12/12
                for (int c = 1; c < sensor_list.Length; c++)
                {
                    current_chunk_id = sensor_list[c].get_chunk_id();

                    if (current_chunk_id != last_chunk_id)
                    { //now we're at the boundary
                        //chunk_start_positions[chunk_counter] = c; //store the current position
                        chunk_start_positions.Add(c);
                        //chunk_counter++; //incrament the chunk counter array  // AD 20/12/12
                    } //end if(current_chunk_id!=last_chunk_id)

                    last_chunk_id = current_chunk_id;
                } //end for (int c = 1; c < sensor_list.Length; c++)

                return chunk_start_positions.ToArray(); //all elements should be filled after loop above  // AD 20/12/12
            } //close else ... if (sensor_list.Length == 0)
        } //end method get_start_positions_of_all_chunk()




        private static Sensor_Reading[] read_SenseCam_csv_file(string sensor_file, int user_hour_time_adjustment)
        {
            //this method accepts a folder path as a parameter
            //1. read the elements and store them to an arraylist
            //2. convert array list to array of type Sensor_Reading and return
            List<Sensor_Reading> list_of_all_sensor_values = new List<Sensor_Reading>();
            Sensor_Reading line_of_sensor_vals;

            int chunk_minute_threshold = MAXIMUM_NUMBER_MINUTES_BETWEEN_IMAGES_ALLOWED_TO_STAY_IN_THE_SAME_EVENT;//hour threshold between sensor readings to determine if reading should be declared in a new chunk

            TextReader sensor_reader = new StreamReader(sensor_file);
            string line_input;
            int acc_x = 0, acc_y = 0, acc_z = 0, white_val = 0, battery = 0, temperature = 0, pir = 0;
            char trigger_code = Sensor_Reading.NULL_CHAR;
            string image_name = null;
            DateTime sample_time = new DateTime();
            DateTime current_date = new DateTime(); //only used to store what the current day is


            DateTime previous_sample_time = new DateTime(); //for calculating chunk_id
            int chunk_id = 0;
            Boolean first_reading = true; //for calculating chunk_id ... just to check first sensor value against the db to see what chunk id if should belong to ... i.e. if it's just a continuation of what's in the database (e.g. big folder being split up into subfolders, and just starting to read 2nd subfolder), we'll want readings to belong to the same chunk ... else we'll start a new chunk (i.e. previous incramented by one)

            while ((line_input = sensor_reader.ReadLine()) != null)
            {
                if (line_input.Length > 2) //just make sure we can actually read the first 3 characters in subsequent lines...
                {

                    if (line_input.Substring(0, 3).Equals("RTC")) //i.e. getting the date for the current readings
                    {
                        current_date = get_current_date_from_RTC_line(line_input);
                    } //end else if(line_input.Substring(0, 3).Equals("RTC")
                    if (line_input.Substring(0, 3).Equals("CAM"))
                    {
                        image_name = get_image_name_from_CAM_line(line_input);
                        trigger_code = get_trigger_from_CAM_line(line_input);
                    } //end if (line_input.Substring(0, 3).Equals("CAM"))

                    if (line_input.Substring(0, 3).Equals("ACC"))
                    {
                        sample_time = get_datetime_from_ACC_line(current_date, line_input);
                        acc_x = get_acc_x_from_ACC_line(line_input);
                        acc_y = get_acc_y_from_ACC_line(line_input);
                        acc_z = get_acc_z_from_ACC_line(line_input);
                    } //end if (line_input.Substring(0, 3).Equals("ACC"))

                    if (line_input.Substring(0, 3).Equals("TMP"))
                    {
                        temperature = get_temperature_from_TMP_line(line_input);
                    } //end if (line_input.Substring(0, 3).Equals("TMP"))

                    if (line_input.Substring(0, 3).Equals("CLR"))
                    {
                        white_val = get_light_level_from_CLR_line(line_input);
                    } //end if (line_input.Substring(0, 3).Equals("CLR"))

                    if (line_input.Substring(0, 3).Equals("PIR"))
                    {
                        pir = get_pir_from_PIR_line(line_input);
                    } //end if (line_input.Substring(0, 3).Equals("PIR"))

                    if (line_input.Substring(0, 3).Equals("BAT"))
                    {
                        battery = get_battery_level_from_BAT_line(line_input);

                        //since BAT is always the last line in a sensor read, now is the time to store this group of sensor readings

                        //now let's determine the chunk_id
                        if (first_reading)
                            chunk_id = get_chunk_id(sample_time, chunk_minute_threshold);
                        else chunk_id = calculate_chunk_id(chunk_id, sample_time, previous_sample_time, chunk_minute_threshold);
                                                
                        line_of_sensor_vals = new Sensor_Reading(chunk_id, sample_time, acc_x, acc_y, acc_z, white_val, battery, temperature, pir, trigger_code, image_name);
                        //and add it to the ArrayList below
                        if(timestamp_is_sane(line_of_sensor_vals.get_sample_time())) //just make sure the date is fine though...
                            list_of_all_sensor_values.Add(line_of_sensor_vals);

                        //and just reset all the CAM elements for the next read (no guarantee that it'll always be present)
                        image_name = null;
                        trigger_code = Sensor_Reading.NULL_CHAR;
                        
                        previous_sample_time = sample_time; //for calculating the chunk_id of the next image

                        if (first_reading)
                            first_reading = false; //we're no longer with the first reading ... this is to check the initial starting chunk_id value
                    } //end if (line_input.Substring(0, 3).Equals("BAT"))				
                } //close if (line_input.Length > 2)...
            } //end method while (sensor_reader.Read())
            sensor_reader.Close(); //closing the stream



            //now will convert the ArrayList to an array and return it
            return list_of_all_sensor_values.ToArray();
        } //end method read_SenseCam_csv_file()


        /// <summary>
        /// this method is needed to deal with instances where the sensor file is garbled up, so needs to be sanity checked...
        /// </summary>
        /// <param name="proposed_datetime"></param>
        /// <returns></returns>
        private static bool timestamp_is_sane(DateTime proposed_datetime)
        {
            if(proposed_datetime.Year>=2000 && proposed_datetime.Year<2200) //allowing values up to the year 2200, think this browser will in all likelihood be obselete then! :) Aiden
                return true;
            else return false;            
        } //close method timestamp_is_sane()...



        private static Sensor_Reading[] read_Vicon_Revue_csv_file(string sensor_file, int user_hour_time_adjustment)
        {
            //this method accepts a folder path as a parameter
            //1. read the elements and store them to an arraylist
            //2. convert array list to array of type Sensor_Reading and return
            List<Sensor_Reading> list_of_all_sensor_values = new List<Sensor_Reading>();
            Sensor_Reading line_of_sensor_vals;

            int chunk_minute_threshold = MAXIMUM_NUMBER_MINUTES_BETWEEN_IMAGES_ALLOWED_TO_STAY_IN_THE_SAME_EVENT;//hour threshold between sensor readings to determine if reading should be declared in a new chunk

            TextReader sensor_reader = new StreamReader(sensor_file);
            string line_input;
            double acc_x = 0.0, acc_y = 0.0, acc_z = 0.0,temperature = 0.0;
            int white_val = 0, battery = 0, pir = 0, mag_x=0, mag_y=0, mag_z=0;
            char trigger_code = Sensor_Reading.NULL_CHAR;
            string image_name = null;
            DateTime sample_time = new DateTime();
            DateTime image_sample_time = new DateTime();
            TimeSpan time_diff_between_ACC_and_CAM_lines;


            DateTime previous_sample_time = new DateTime(); //for calculating chunk_id
            int chunk_id = 0;
            bool first_reading = true; //for calculating chunk_id ... just to check first sensor value against the db to see what chunk id if should belong to ... i.e. if it's just a continuation of what's in the database (e.g. big folder being split up into subfolders, and just starting to read 2nd subfolder), we'll want readings to belong to the same chunk ... else we'll start a new chunk (i.e. previous incramented by one)

            while ((line_input = sensor_reader.ReadLine()) != null)
            {
                if (line_input.Length > 2) //just make sure we can actually read the first 3 characters in subsequent lines...
                {
                    
                    if (line_input.Substring(0, 3).Equals("ACC"))
                    {
                        //now since ACC is always the first line in a sensor read, now is the time to store the previous group of sensor readings

                        //now let's determine the chunk_id
                        if (first_reading)
                        {
                            sample_time = get_datetime_from_Vicon_Revue_ACC_line(line_input); //this method call also works for the VER line too, if I ever want to get the time from it ...
                            chunk_id = get_chunk_id(sample_time, chunk_minute_threshold);

                            first_reading = false; //we're no longer with the first reading ... this was just to check the initial starting chunk_id value
                        } //close if (first_reading)...
                        else
                        {
                            chunk_id = calculate_chunk_id(chunk_id, sample_time, previous_sample_time, chunk_minute_threshold);

                            line_of_sensor_vals = new Sensor_Reading(chunk_id, sample_time, acc_x, acc_y, acc_z, white_val, battery, temperature, pir, trigger_code, image_name, mag_x, mag_y, mag_z);
                            //and add it to the ArrayList below
                            if (timestamp_is_sane(line_of_sensor_vals.get_sample_time())) //just make sure the date is fine though...
                                list_of_all_sensor_values.Add(line_of_sensor_vals);

                            //and just reset all the CAM elements for the next read (no guarantee that it'll always be present)
                            image_name = null;
                            trigger_code = Sensor_Reading.NULL_CHAR;


                            previous_sample_time = sample_time; //for calculating the chunk_id of the next image
                        } //close else ... if (first_reading)...



                        //after dealing with the last group of values, we now continue reading on and store the next group of values...
                        sample_time = get_datetime_from_Vicon_Revue_ACC_line(line_input);
                        acc_x = get_acc_x_from_Vicon_Revue_ACC_line(line_input);
                        acc_y = get_acc_y_from_Vicon_Revue_ACC_line(line_input);
                        acc_z = get_acc_z_from_Vicon_Revue_ACC_line(line_input);
                    } //end if (line_input.Substring(0, 3).Equals("ACC"))

                    else if (line_input.Substring(0, 3).Equals("TMP"))
                    {
                        temperature = get_temperature_from_Vicon_Revue_TMP_line(line_input);
                    } //end if (line_input.Substring(0, 3).Equals("TMP"))

                    else if (line_input.Substring(0, 3).Equals("CLR"))
                    {
                        white_val = get_light_level_from_Vicon_Revue_CLR_line(line_input);
                    } //end if (line_input.Substring(0, 3).Equals("CLR"))

                    else if (line_input.Substring(0, 3).Equals("PIR"))
                    {
                        pir = get_pir_from_Vicon_Revue_PIR_line(line_input);
                    } //end if (line_input.Substring(0, 3).Equals("PIR"))

                    else if (line_input.Substring(0, 3).Equals("BAT"))
                    {
                        battery = get_battery_level_from_Vicon_Revue_BAT_line(line_input);
                    } //end if (line_input.Substring(0, 3).Equals("BAT"))
                    
                    else if (line_input.Substring(0, 3).Equals("MAG"))
                    {
                        mag_x = get_mag_x_from_Vicon_Revue_MAG_line(line_input);
                        mag_y = get_mag_y_from_Vicon_Revue_MAG_line(line_input);
                        mag_z = get_mag_z_from_Vicon_Revue_MAG_line(line_input);
                    } //end if (line_input.Substring(0, 3).Equals("ACC"))

                    else if (line_input.Substring(0, 3).Equals("CAM")) //this line only appears when a picture is taken...
                    {
                        //let's just make sure that the image recorded will have an appropriate time marked next to it (i.e. it was recorded within 1 minute of the most recent ACC line) ... if not, we will just not record it
                        image_sample_time = get_datetime_from_Vicon_Revue_ACC_line(line_input); //this method call also works for the VER line too, if I ever want to get the time from it ...

                        time_diff_between_ACC_and_CAM_lines = image_sample_time-sample_time;

                        if (time_diff_between_ACC_and_CAM_lines.TotalSeconds < 60) //i.e. (i.e. if it was recorded within 1 minute from the most recent ACC line, we can record it) ... if so, we will just not record it (it'll confuse end users)
                        {
                            image_name = get_image_name_from_Vicon_Revue_CAM_line(line_input);
                            trigger_code = get_trigger_from_Vicon_Revue_CAM_line(line_input);
                        }
                    } //end if (line_input.Substring(0, 3).Equals("CAM"))
                                        

                } //close if (line_input.Length > 2)...
            } //end method while (sensor_reader.Read())
            sensor_reader.Close(); //closing the stream



            //we haven't stored the very last group of sensor values, so now is the time to store the previous group of sensor readings ...
            //and no need to worry about setting and previous/current values, as we will not be reading any subsequent values...
            //now let's determine the chunk_id
            chunk_id = calculate_chunk_id(chunk_id, sample_time, previous_sample_time, chunk_minute_threshold);

            line_of_sensor_vals = new Sensor_Reading(chunk_id, sample_time, acc_x, acc_y, acc_z, white_val, battery, temperature, pir, trigger_code, image_name, mag_x, mag_y, mag_z);
            //and add it to the ArrayList below
            if (timestamp_is_sane(line_of_sensor_vals.get_sample_time())) //just make sure the date is fine though...
                list_of_all_sensor_values.Add(line_of_sensor_vals);



            //now will convert the ArrayList to an array and return it
            return list_of_all_sensor_values.ToArray();
        } //end method read_Vicon_Revue_csv_file()




        private static Sensor_Reading[] read_Autographer_image_table_txt_file(string sensor_file, int user_hour_time_adjustment)
        {
            //this method accepts a folder path as a parameter
            //1. read the elements and store them to an arraylist
            //2. convert array list to array of type Sensor_Reading and return
            List<Sensor_Reading> list_of_all_sensor_values = new List<Sensor_Reading>();
            Sensor_Reading line_of_sensor_vals;

            int chunk_minute_threshold = MAXIMUM_NUMBER_MINUTES_BETWEEN_IMAGES_ALLOWED_TO_STAY_IN_THE_SAME_EVENT;//hour threshold between sensor readings to determine if reading should be declared in a new chunk

            TextReader sensor_reader = new StreamReader(sensor_file);
            string line_input;
            double acc_x = 0.0, acc_y = 0.0, acc_z = 0.0, temperature = 0.0, mag_x = 0, mag_y = 0, mag_z = 0;
            int white_val = 0, battery = 0, pir = 0;
            char trigger_code = Sensor_Reading.NULL_CHAR;
            string image_name = null;
            DateTime sample_time = new DateTime();
            string[] line_elements;
            //todo Autographer - what do the sensor values mean? The values recorded at the instant that the image was taken? Or a summary measure of the sensed readings since the previous picture was taken?
            //#dt                     ,id                              ,sz  ,typ ,p,accx  ,accy  ,accz  ,magx  ,magy  ,magz  ,red  ,green,blue ,lum  ,tem  ,g, lat    , lon    , alt, gs , herr, verr, exp,gain ,rbal ,gbal ,bbal ,xor   ,yor   ,zor   ,stags   ,tags
            //2013-10-28T14:15:22+0100,B00000000_21I4ZX_20131028_141522,4014,0002,1, 0.064,-0.396, 0.817,-0.015, 0.053,-0.390, 3226, 3369, 1953,10265, 19.1,0, 0.00000, 0.00000, 0.0, 0.0, 0.00, 0.00, 5  ,12.00,1.313,1.000,1.813,33.642,-152.9,8.3824,00000000, 
            //0                       ,1                               ,2   ,3   ,4, 5    ,6     ,7     ,8     ,9     ,10    ,11   ,12   ,13   ,14   ,15   ,16, 17    , 18     , 19 , 20 , 21  , 22  , 23 ,24   ,25   ,26   ,27   ,28    ,29    ,30    ,31      ,32

            DateTime previous_sample_time = new DateTime(); //for calculating chunk_id
            int chunk_id = 0;
            bool first_reading = true; //for calculating chunk_id ... just to check first sensor value against the db to see what chunk id if should belong to ... 
                                        //i.e. if it's just a continuation of what's in the database (e.g. big folder being split up into subfolders, and just starting to read 2nd subfolder),
                                        //we'll want readings to belong to the same chunk ... else we'll start a new chunk (i.e. previous incramented by one)
            while ((line_input = sensor_reader.ReadLine()) != null)
            {
                if (line_input.Length > 2 && !line_input[0].Equals('#')) //just make sure we can actually read the first 3 characters in subsequent lines... and also that it's not a comment (starting with '#')
                {
                    line_elements = line_input.Split(',');
                    //after dealing with the last group of values, we now continue reading on and store the next group of values...
                    sample_time = get_datetime_from_Autographer_txt_line_string(line_elements[0]);
                    acc_x = attempt_to_parse_string_to_double(line_elements[5]);
                    acc_y = attempt_to_parse_string_to_double(line_elements[6]);
                    acc_z = attempt_to_parse_string_to_double(line_elements[7]);
                    white_val = attempt_to_parse_string_to_int(line_elements[14]);
                    battery = attempt_to_parse_string_to_int(line_elements[2]); //todo ask Vicon if there is a battery field!
                    temperature = attempt_to_parse_string_to_double(line_elements[15]);
                    pir = attempt_to_parse_string_to_int(line_elements[4]); //todo double check I have selected the right field here
                    trigger_code = '-'; //todo Autographer confirm that there is no trigger code associated with images in the CSV file? ... maybe it's element 3?
                    image_name = line_elements[1] + "E.JPG"; //todo why does each image end with the letter E??
                    //todo important update - I now need to update Sensor_Readings table in the database to recognise mag x/y/z values as double rather than int!
                    mag_x = attempt_to_parse_string_to_double(line_elements[8]);
                    mag_y = attempt_to_parse_string_to_double(line_elements[9]);
                    mag_z = attempt_to_parse_string_to_double(line_elements[10]);
                    //todo need to update Sensor_Readings table in the database to now store the newly calculated Autographer sensor fields

                    //now let's determine the chunk_id
                    if(first_reading)
                        chunk_id = get_chunk_id(sample_time, chunk_minute_threshold);
                    else chunk_id = calculate_chunk_id(chunk_id, sample_time, previous_sample_time, chunk_minute_threshold);

                    line_of_sensor_vals = new Sensor_Reading(chunk_id, sample_time, acc_x, acc_y, acc_z, white_val, battery, temperature, pir, trigger_code, image_name, mag_x, mag_y, mag_z);
                    //and add it to the ArrayList below
                    if (timestamp_is_sane(line_of_sensor_vals.get_sample_time())) //just make sure the date is fine though...
                        list_of_all_sensor_values.Add(line_of_sensor_vals);
                    
                    //now reset variables for next iteration of the loop
                    first_reading = false;
                    image_name = null;
                    trigger_code = Sensor_Reading.NULL_CHAR;
                    previous_sample_time = sample_time; //for calculating the chunk_id of the next image
                } //close if (line_input.Length > 2)...
            } //end method while (sensor_reader.Read())
            sensor_reader.Close(); //closing the stream
            
            //now will convert the ArrayList to an array and return it
            return list_of_all_sensor_values.ToArray();
        } //end method read_Autographer_csv_file()










        #region SenseCam parsing

        private static string get_image_name_from_CAM_line(string line)
        {
            //CAM,13,36,07,00325305.JPG,P
            //012345678901234567890123456

            return line.Substring(13, 12);
        } //end method get_image_name_from_CAM_line()




        private static char get_trigger_from_CAM_line(string line)
        {
            //CAM,13,36,07,00325305.JPG,P
            //012345678901234567890123456

            return line.Substring(line.Length - 1, 1).ToCharArray()[0];
        } //end method get_trigger_from_CAM_line()




        private static DateTime get_datetime_from_ACC_line(DateTime current_date, string line)
        {
            int day, month, year, hour, minute, second;
            day = current_date.Day;
            month = current_date.Month;
            year = current_date.Year;

            //ACC,13,36,07,00003,00864,00039
            //012345678901234567890123456789
            hour = attempt_to_parse_string_to_int(line.Substring(4, 2));
            minute = attempt_to_parse_string_to_int(line.Substring(7, 2));
            second = attempt_to_parse_string_to_int(line.Substring(10, 2));

            return new DateTime(year, month, day, hour, minute, second).AddHours(USER_HOUR_TIME_ADJUSTMENT_HOURS);
        } //end method get_datetime_from_CAM_line




        private static int get_acc_x_from_ACC_line(string line)
        {
            //ACC,13,36,07,00003,00864,00039
            //012345678901234567890123456789

            return attempt_to_parse_string_to_int(line.Substring(13, 5));
        } //end method get_acc_x_from_ACC_line()




        private static int get_acc_y_from_ACC_line(string line)
        {
            //ACC,13,36,07,00003,00864,00039
            //012345678901234567890123456789

            return attempt_to_parse_string_to_int(line.Substring(19, 5));
        } //end method get_acc_y_from_ACC_line()




        private static int get_acc_z_from_ACC_line(string line)
        {
            //ACC,13,36,07,00003,00864,00039
            //012345678901234567890123456789

            return attempt_to_parse_string_to_int(line.Substring(25, 5));
        } //end method get_acc_z_from_ACC_line()

        private static int get_temperature_from_TMP_line(string line)
        {
            //TMP,13,36,07,00060
            //012345678901234567

            return attempt_to_parse_string_to_int(line.Substring(13, 5));
        } //end method get_temperature_from_TMP_line()




        private static DateTime get_current_date_from_RTC_line(string line)
        {
            //RTC,2006,11,20,13,35,54
            //01234567890123456789012
            int year, month, day;

            year = attempt_to_parse_string_to_int(line.Substring(4, 4));
            month = attempt_to_parse_string_to_int(line.Substring(9, 2));
            day = attempt_to_parse_string_to_int(line.Substring(12, 2));

            return new DateTime(year, month, day);
        } //end method get_current_date_from_RTC_line




        private static int get_light_level_from_CLR_line(string line)
        {
            //CLR,13,36,07,06781
            //012345678901234567

            return attempt_to_parse_string_to_int(line.Substring(13, 5));
        } //end method get_light_level_from_CLR_line()




        private static int get_pir_from_PIR_line(string line)
        {
            //PIR,13,36,07,1
            //01234567890123

            return attempt_to_parse_string_to_int(line.Substring(line.Length - 1, 1));
        } //end method get_pir_from_PIR_line()




        private static int get_battery_level_from_BAT_line(string line)
        {
            //BAT,13,36,07,41626
            //012345678901234567

            return attempt_to_parse_string_to_int(line.Substring(13, 5));
        } //end method get_battery_level_from_BAT_line()


        #endregion SenseCam parsing









        #region Vicon Revue parsing

        /// <summary>
        /// this method is generally called to parse the ACC line in Vicon Revue sensor file, but it's also used to check whether this line belongs to a Vicon Revue SENSOR.CSV or a SenseCam SENSOR.CSV
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static DateTime get_datetime_from_Vicon_Revue_ACC_line(string line)
        {
            //ACC,2010/05/10 15:28:15,-0.020,00.976,00.039
            //0  ,          1        , 2    , 3    ,    4
            // or sometimes...
            //VER,2010/05/10 15:23:38,4,0,0
            //0  ,          1        ,2,3,4
            // or other times ...
            //VER,2010/05/11 12:17:39,4,0,0
            //0  ,          1        ,2,3,4

            //2010/05/10 15:28:15
            //0123456789012345678
            try //so in case we're dealing with a SenseCam sensor.csv (called in initial checks), we'll put the code below in a try...catch statement, in case it belongs to the SenseCam and can't be parsed
            {
                string[] date_time_string_element = line.Split(',')[1].Split(' ');

                //2010/05/10
                //0123456789
                int day, month, year, hour, minute, second;
                year = attempt_to_parse_string_to_int(date_time_string_element[0].Split('/')[0]);
                month = attempt_to_parse_string_to_int(date_time_string_element[0].Split('/')[1]);
                day = attempt_to_parse_string_to_int(date_time_string_element[0].Split('/')[2]);


                //15:28:15
                //01234567
                hour = attempt_to_parse_string_to_int(date_time_string_element[1].Split(':')[0]);
                minute = attempt_to_parse_string_to_int(date_time_string_element[1].Split(':')[1]);
                second = attempt_to_parse_string_to_int(date_time_string_element[1].Split(':')[2]);

                return new DateTime(year, month, day, hour, minute, second).AddHours(USER_HOUR_TIME_ADJUSTMENT_HOURS);
            } //close try //so in case we're dealing with a SenseCam sensor.csv (called in initial checks), we'll put the code below in a try...catch statement, in case it belongs to the SenseCam and can't be parsed
            catch (Exception excep)
            {
                return new DateTime(1, 1, 1, 1, 1, 1); //return an unhelpful datetime, the calling method will then determine this line doesn't belong to a Vicon Revue SENSOR.CSV
            } //close catch ... try catch ... try //so in case we're dealing with a SenseCam sensor.csv (called in initial checks), we'll put the code below in a try...catch statement, in case it belongs to the SenseCam and can't be parsed
        } //end method get_datetime_from_Vicon_Revue_ACC_line


        private static double get_acc_x_from_Vicon_Revue_ACC_line(string line)
        {
            //ACC,2010/05/10 15:28:15,-0.020,00.976,00.039
            //0  ,          1        , 2    , 3    ,    4
            return attempt_to_parse_string_to_double(line.Split(',')[2]);
        } //end method get_acc_x_from_Vicon_Revue_ACC_line()

        private static double get_acc_y_from_Vicon_Revue_ACC_line(string line)
        {
            //ACC,2010/05/10 15:28:15,-0.020,00.976,00.039
            //0  ,          1        , 2    , 3    ,    4
            return attempt_to_parse_string_to_double(line.Split(',')[3]);
        } //end method get_acc_y_from_Vicon_Revue_ACC_line()

        private static double get_acc_z_from_Vicon_Revue_ACC_line(string line)
        {
            //ACC,2010/05/10 15:28:15,-0.020,00.976,00.039
            //0  ,          1        , 2    , 3    ,    4
            return attempt_to_parse_string_to_double(line.Split(',')[4]);
        } //end method get_acc_z_from_Vicon_Revue_ACC_line()

        
        private static double get_temperature_from_Vicon_Revue_TMP_line(string line)
        {
            //TMP,2010/05/10 15:28:15,0026.0
            //

            return attempt_to_parse_string_to_double(line.Split(',')[2]);
        } //end method get_temperature_from_Vicon_Revue_TMP_line()


        private static int get_light_level_from_Vicon_Revue_CLR_line(string line)
        {
            //CLR,2010/05/10 15:28:15,00078
            //

            return attempt_to_parse_string_to_int(line.Split(',')[2]);
        } //end method get_light_level_from_Vicon_Revue_CLR_line()


        private static int get_pir_from_Vicon_Revue_PIR_line(string line)
        {
            //PIR,2010/05/10 15:28:15,0
            //

            return attempt_to_parse_string_to_int(line.Split(',')[2]);
        } //end method get_pir_from_Vicon_Revue_PIR_line()


        private static int get_battery_level_from_Vicon_Revue_BAT_line(string line)
        {
            //BAT,2010/05/10 15:28:15,38259
            //

            return attempt_to_parse_string_to_int(line.Split(',')[2]);
        } //end method get_battery_level_from_Vicon_Revue_BAT_line()


        private static int get_mag_x_from_Vicon_Revue_MAG_line(string line)
        {
            //MAG,2010/05/10 15:28:15,886,1011,-1469
            //0  ,          1        , 2 , 3  ,  4
            return attempt_to_parse_string_to_int(line.Split(',')[2]);
        } //end method get_may_x_from_Vicon_Revue_MAG_line()

        private static int get_mag_y_from_Vicon_Revue_MAG_line(string line)
        {
            //MAG,2010/05/10 15:28:15,886,1011,-1469
            //0  ,          1        , 2 , 3  ,  4
            return attempt_to_parse_string_to_int(line.Split(',')[3]);
        } //end method get_mag_y_from_Vicon_Revue_MAG_line()

        private static int get_mag_z_from_Vicon_Revue_MAG_line(string line)
        {
            //MAG,2010/05/10 15:28:15,886,1011,-1469
            //0  ,          1        , 2 , 3  ,  4
            return attempt_to_parse_string_to_int(line.Split(',')[4]);
        } //end method get_mag_z_from_Vicon_Revue_MAG_line()


        private static string get_image_name_from_Vicon_Revue_CAM_line(string line)
        {
            //CAM,2010/05/10 15:28:35,00000399.JPG,P
            //0  ,          1        , 2          ,3

            return line.Split(',')[2];
        } //end method get_image_name_from_CAM_line()

        private static char get_trigger_from_Vicon_Revue_CAM_line(string line)
        {
            //CAM,2010/05/10 15:28:35,00000399.JPG,P
            //0  ,          1        , 2          ,3

            return line.Split(',')[3].ToCharArray()[0];
        } //end method get_trigger_from_CAM_line()


        #endregion Vicon Revue parsing









        #region Autographer parsing

        /// <summary>
        /// this method is generally called to parse the BAT line in Autographer sensor file
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static DateTime get_datetime_from_Autographer_txt_line_string(string line)
        {
            //2013-04-08T02:35:37+0100
            //_123456789_123456789_123
            try //so in case we're dealing with an Autographer sensor.csv (called in initial checks), we'll put the code below in a try...catch statement, in case it can't be parsed for some reason
            {
                string[] date_time_string_element = line.Split('T');

                //2013-04-08
                //0123456789
                int day, month, year, hour, minute, second;
                year = attempt_to_parse_string_to_int(date_time_string_element[0].Split('-')[0]);
                month = attempt_to_parse_string_to_int(date_time_string_element[0].Split('-')[1]);
                day = attempt_to_parse_string_to_int(date_time_string_element[0].Split('-')[2]);

                //02:35:37
                //01234567                
                hour = attempt_to_parse_string_to_int(date_time_string_element[1].Split(':')[0]);
                minute = attempt_to_parse_string_to_int(date_time_string_element[1].Split(':')[1]);
                second = attempt_to_parse_string_to_int(date_time_string_element[1].Split(':')[2].Substring(0,2)); //todo Autographer, make sure I'm parsing in the time ok .. also what to do with +0100 part?
                return new DateTime(year, month, day, hour, minute, second).AddHours(USER_HOUR_TIME_ADJUSTMENT_HOURS);
            } //close try //so in case we're dealing with a SenseCam sensor.csv (called in initial checks), we'll put the code below in a try...catch statement, in case it belongs to the SenseCam and can't be parsed
            catch (Exception excep)
            {
                return new DateTime(1, 1, 1, 1, 1, 1); //return an unhelpful datetime, the calling method will then determine this line doesn't belong to a Vicon Revue SENSOR.CSV
            } //close catch ... try catch ... try //so in case we're dealing with a SenseCam sensor.csv (called in initial checks), we'll put the code below in a try...catch statement, in case it belongs to the SenseCam and can't be parsed
        } //end method get_datetime_from_Vicon_Revue_ACC_line


        #endregion Autographer parsing












        private static int attempt_to_parse_string_to_int(string val)
        {
            try
            {
                return int.Parse(val);
            }
            catch (Exception excep)
            {
                return 1;
            }
        } //close method attempt_to_parse_string_to_int...

        private static double attempt_to_parse_string_to_double(string val)
        {
            try
            {
                return double.Parse(val);
            }
            catch (Exception excep)
            {
                return 1.0;
            }
        } //close method attempt_to_parse_string_to_double...



        private static int get_chunk_id(DateTime first_reading_time, int minute_threshold)
        {
            return 0; // this is sensor processing and the output will be merged with new_images
            //new_images already has the correct chunk_id and the sensor chunk_id will be discarded

        } //end method get_chunk_id()




        private static int calculate_chunk_id(int current_chunk_id, DateTime current_reading_time, DateTime last_reading_time, int minute_threshold)
        {
            TimeSpan minute_difference = current_reading_time - last_reading_time;
            
            if (Math.Abs(minute_difference.TotalMinutes) > minute_threshold) //i.e. if the difference between them is greater than the threshold, we'll make a new chunk
                return current_chunk_id + 1;
            else return current_chunk_id; //else it's part of the same chunk
        } //end method calculate_chunk_id()




        //below we produce a list of images when there's no csv information()....


        public static Segmentation_Image_Rep[] process_folder_with_no_csv_information(string local_folder)
        {
            //RETURN A LIST OF IMAGES ... '*.JPG' read from the folder
            return read_folder_images(local_folder);
        } //end method process_folder_with_no_csv_information...





        private static Segmentation_Image_Rep[] read_folder_images(string folder_path)
        {
            //this method accepts a folder path as a parameter
            //1. reads all the images
            //2. convert array list to array of type Image_Rep
            List<Segmentation_Image_Rep> list_of_all_images = new List<Segmentation_Image_Rep>();

            int chunk_hour_threshold = 2;//hour threshold between sensor readings to determine if reading should be declared in a new chunk

            string[] list_of_files = Directory.GetFiles(folder_path, "*.JPG");

            FileInfo current_image;
            int chunk_id = -5, previous_chunk_id = -5;            
            DateTime chunk_start_time = new DateTime(), previous_sample_time = new DateTime(), current_image_time;
            bool first_reading = true;

            //these two variables are used to segment events into equal sizes (in terms of the number of images they contain)
            int non_csv_segmentation_counter = 0;
            int NUM_NON_CSV_IMAGES_TO_SEGMENT_INTO_EVENTS = 95;

            //these variables are used to put in dummy motion sensor values, so that most of the rest of the algorithms can easily adapt to these values...
            int blob_length = 10; //the blob length and count values refer to a blob/blurb of (simulated) spiked motion values, so as to trigger a change of event
            int blob_count = blob_length;
            int current_non_csv_acccomb_value_change = 38000; //this variables is used to record what a value a blurb of motion is
            int current_non_csv_acccomb_value = current_non_csv_acccomb_value_change; //and this variable is what we store with each image, is usually equal to variable above, or 0
            
            foreach (string jpg_path in list_of_files)
            {
                current_image = new FileInfo(jpg_path);

                if (current_image.Length > MINIMUM_FILE_SIZE)
                {
                    //firstly check that the current image isn't a null image or entirely dark either... (i.e. it must be the minimum file size)

                    //get the current image_time which is needed to calculate the chunk_id...
                    current_image_time = current_image.LastWriteTime;


                    //////////////////// CHUNK ID STUFF ///////////////////////
                    //now let's determine the chunk_id
                    if (first_reading)
                        chunk_id = get_chunk_id(current_image_time, chunk_hour_threshold);
                    else chunk_id = calculate_chunk_id(chunk_id, current_image_time, previous_sample_time, chunk_hour_threshold);

                    if (chunk_id != previous_chunk_id)
                    {
                        previous_chunk_id = chunk_id;
                        chunk_start_time = current_image_time;
                    } //end if(chunk_id!=previous_chunk_id)

                    //now we've read the first image, set this to false (it was set to true, to help calculate the initial 
                    if (first_reading)
                        first_reading = false;

                    previous_sample_time = current_image_time;

                    //////////////////// END CHUNK ID STUFF ///////////////////////

                    non_csv_segmentation_counter++; //this counter is incramented right throughout all the images we've got...
                    //if we've reached every 95th image, we'll start adding "blurb" motion values to the next "blob_length" images (currently variable hard coded to 10 on 08/01/10)...
                    blob_count++; //we always keep incramenting this counter
                    if (non_csv_segmentation_counter % NUM_NON_CSV_IMAGES_TO_SEGMENT_INTO_EVENTS == 0) //then at every 95th image
                        blob_count = 0; //reset the blob_count variable

                    if (blob_count < blob_length) //then when this blob_count variable is reset (i.e. we've reached a 95th image boundary marker)
                        current_non_csv_acccomb_value = current_non_csv_acccomb_value_change; //then we say the current image will have a blob/blurb of motion associated with it (i.e. it's a change of activity and hence an event change...)
                    else current_non_csv_acccomb_value = 0; //else we say there's no motion here (i.e. don't trigger this as an event boundary)...


                    //and add the image to our list...
                    list_of_all_images.Add(new Segmentation_Image_Rep(chunk_id, current_image.Name, chunk_start_time, current_image_time, current_non_csv_acccomb_value, current_non_csv_acccomb_value, current_non_csv_acccomb_value));

                } //end if (current_image.Length > 4096)
                
            } //end foreach (string jpg_path in list_of_files)


            //now will convert the ArrayList to an array and return it
            return list_of_all_images.ToArray();
        } //end method read_folder_images()





        #region upload sensor readings to db...

        private static void upload_sensor_readings_to_db(Sensor_Reading[] sensor_readings, int user_id, Upload_and_Segment_Images_Thread.DeviceType device_type)
        {
            // http://sqlite.phxsoftware.com/forums/t/134.aspx
            DbConnection con = new SQLiteConnection(global::SenseCamBrowser1.Properties.Settings.Default.DBConnectionString);
            con.Open();
            using (DbTransaction dbTrans = con.BeginTransaction())
            {
                using (DbCommand cmd = con.CreateCommand())
                {
                    //cmd.CommandText = "INSERT INTO TestCase(MyValue) VALUES(?)";
                    cmd.CommandText = "INSERT INTO Sensor_Readings(user_id,sample_time,acc_x,acc_y,acc_z,white_val,battery,temperature,pir,trigger_code,image_name,mag_x,mag_y,mag_z) VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?)";
                    DbParameter user_id_field, sample_time_field,acc_x_field,acc_y_field,acc_z_field,white_val_field,battery_field,temperature_field,pir_field,trigger_code_field,image_name_field,mag_x_field,mag_y_field,mag_z_field;
                    user_id_field = cmd.CreateParameter();
                    sample_time_field = cmd.CreateParameter();
                    acc_x_field = cmd.CreateParameter();
                    acc_y_field = cmd.CreateParameter();
                    acc_z_field = cmd.CreateParameter();
                    white_val_field = cmd.CreateParameter();
                    battery_field = cmd.CreateParameter();
                    temperature_field = cmd.CreateParameter();
                    pir_field = cmd.CreateParameter();
                    trigger_code_field = cmd.CreateParameter();
                    image_name_field = cmd.CreateParameter();
                    mag_x_field = cmd.CreateParameter();
                    mag_y_field = cmd.CreateParameter();
                    mag_z_field = cmd.CreateParameter();
                    cmd.Parameters.Add(user_id_field);
                    cmd.Parameters.Add(sample_time_field);
                    cmd.Parameters.Add(acc_x_field);
                    cmd.Parameters.Add(acc_y_field);
                    cmd.Parameters.Add(acc_z_field);
                    cmd.Parameters.Add(white_val_field);
                    cmd.Parameters.Add(battery_field);
                    cmd.Parameters.Add(temperature_field);
                    cmd.Parameters.Add(pir_field);
                    cmd.Parameters.Add(trigger_code_field);
                    cmd.Parameters.Add(image_name_field);
                    cmd.Parameters.Add(mag_x_field);
                    cmd.Parameters.Add(mag_y_field);
                    cmd.Parameters.Add(mag_z_field);

                    for (int row_counter = 0; row_counter < sensor_readings.Length; row_counter++)
                    {
                        user_id_field.Value = user_id;
                        sample_time_field.Value = sensor_readings[row_counter].get_sample_time();
                        acc_x_field.Value = Standard_Calculation.get_signed_int(sensor_readings[row_counter].get_acc_x(), device_type);
                        acc_y_field.Value = Standard_Calculation.get_signed_int(sensor_readings[row_counter].get_acc_y(), device_type);
                        acc_z_field.Value = Standard_Calculation.get_signed_int(sensor_readings[row_counter].get_acc_z(), device_type);
                        white_val_field.Value = sensor_readings[row_counter].get_white_val();
                        battery_field.Value = sensor_readings[row_counter].get_battery();
                        temperature_field.Value = sensor_readings[row_counter].get_temperature();
                        pir_field.Value = sensor_readings[row_counter].get_pir();
                        trigger_code_field.Value = sensor_readings[row_counter].get_trigger_code();
                        image_name_field.Value = sensor_readings[row_counter].get_image_name();
                        mag_x_field.Value = sensor_readings[row_counter].mag_x;
                        mag_y_field.Value = sensor_readings[row_counter].mag_y;
                        mag_z_field.Value = sensor_readings[row_counter].mag_z;
                        cmd.ExecuteNonQuery();
                    }
                }
                dbTrans.Commit();
            }
            con.Close();
        } //end method upload_sensor_readings_to_db()...


        #endregion upload sensor readings to db...





        public static int get_num_images_in_csv_file(string sensor_file)
        {
            try
            {
                //this method accepts a folder path as a parameter
                TextReader sensor_reader = new StreamReader(sensor_file);
                string line_input;
                int image_counter = 0;

                //quickly loop through the whole file...
                while ((line_input = sensor_reader.ReadLine()) != null)
                {
                    if (line_input.Length > 2) //just make sure we can actually read the first 3 characters in subsequent lines...
                    {

                        if (line_input.Substring(0, 3).Equals("CAM"))
                            image_counter++;

                    } //close if (line_input.Length > 2)...
                } //end method while (sensor_reader.Read())
                sensor_reader.Close(); //closing the stream

                return image_counter;
            }
            catch (Exception excep) { return 1; };

        } //end method get_num_images_in_csv_file()



    } //end class Upload_Sensor_Data

} //end namespace