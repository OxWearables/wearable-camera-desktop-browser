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
using System.Configuration;

namespace SenseCamBrowser1.Upload_Images_and_Segment_into_Events
{
    class Fuse_And_Identify_Segments
    {
        private static double ACC_COMBINED_WEIGHT = 0.25;
        private static double ACC_X_WEIGHT = 0.35;
        private static double PIR_WEIGHT = 0.40;
        public static int AUTOMATIC_EVENT_SEGMENTATION_ENABLED = int.Parse(ConfigurationSettings.AppSettings["automatic_event_segmentation_enabled"].ToString());
        public static int MINIMUM_LENGTH_OF_AUTOMATICALLY_SEGMENTED_EVENTS = int.Parse(ConfigurationSettings.AppSettings["minimum_length_of_automatically_segmented_events"].ToString());
        


        public static Segmentation_Event_Rep[] get_boundary_times_for_all_images(Segmentation_Image_Rep[] list_of_images)
        {
            //BASICALLY RETURNS A LIST OF THE BOUNDARY TIMES (AND CHUNK BOUNDARY TIMES)
            //READ IN THE new_images TABLE FROM THE DATABASE
            //NORMALISE COMBINED IMAGES AND COMBINED ACC COLUMNS
            //FUSE BOTH SOURCES TOGETHER
            //THRESHOLD FUSED SCORES (KAPUR 64 BINS)
            //REMOVE BOUNDARIES THAT OCCUR WITHIN 3 MINUTES OF A PREVIOUS BOUNDARY
            //RETURN LIST OF BOUNDARY TIMES FOR WHOLE LIST (SO BOUNDS + CHUNK START TIMES (APART FROM CHUNK 0 OF COURSE))

            int[] all_chunk_ids; //to store all the possible chunk ids the list
            Segmentation_Image_Rep[] chunk_raw_values = new Segmentation_Image_Rep[1]; //will store the information of all the images in each chunk
            List<Segmentation_Event_Rep> all_events = new List<Segmentation_Event_Rep>();

            //for each user get the number that the chunks count up to
            all_chunk_ids = get_chunk_ids_for_user(list_of_images);

            foreach (int chunk_id in all_chunk_ids)
            { // then go through each chunk
                //1. then get the all the Image_Rep values for this chunk
                chunk_raw_values = get_image_values_from_chunk(list_of_images, chunk_id);

                if (chunk_raw_values.Length >= 1) //let's make sure that there are actually some images in this chunk...
                {
                    //2. NORMALISE AND FUSE DATA SOURCES
                    normalise_and_fuse_data_sources(chunk_raw_values);


                    //3. PROCESS PEAK SCORING
                    peak_score_image_array(chunk_raw_values);


                    //4. THRESHOLD IMAGE TRIGGERED MANIPULATED VALUES
                    Segmentation_Image_Rep[] vals_above_threshold = fused_image_vals_above_dynamic_threshold(chunk_raw_values);


                    //5. CLEAN UP BOUNDARY IMAGES (I.E. ONES WITHING SAY 5 MINUTES OF EACH OTHER)
                    //todo, must define a variable to indicate number end images to ignore (i.e. not hard coded as 4)
                    Segmentation_Image_Rep[] final_boundary_images = cluster_boundary_list(vals_above_threshold, MINIMUM_LENGTH_OF_AUTOMATICALLY_SEGMENTED_EVENTS, chunk_raw_values.Length, 4);
                    
                    
                    //6. APPEND LIST OF BOUNDARY IMAGES TO AN ARRAY LIST OF EVENTS
                    if (final_boundary_images != null && AUTOMATIC_EVENT_SEGMENTATION_ENABLED==1)
                      add_new_events_from_chunk_to_event_list(all_events, final_boundary_images, chunk_raw_values);
                    else all_events.Add(new Segmentation_Event_Rep(chunk_raw_values, 0, chunk_raw_values.Length - 1));
                } //close if (chunk_raw_values.Length >= 1)...

            } //end foreach (int chunk_id in user_chunk_ids)

            if (all_events.Count == 0 && chunk_raw_values.Length > 1)
                return new Segmentation_Event_Rep[] { new Segmentation_Event_Rep(chunk_raw_values, 0, chunk_raw_values.Length - 1) };
            else if (all_events.Count == 0 && chunk_raw_values.Length <= 1)
                return null;

            //finally convert the ArrayList to an Array, and return
            return all_events.ToArray();
        } //end method get_boundary_times_for_all_images()



        public static Segmentation_Event_Rep[] SET_boundary_times_for_all_images(Segmentation_Image_Rep[] list_of_images, List<Segmentation_Event_Rep> list_of_user_defined_boundary_times)
        {
            //the main aim of this method is to take in a list of user-defined boundary start/end times from accelerometer, or GPS, or heart rate, or ... data ... and then to add it to the SenseCam database...
            List<Segmentation_Event_Rep> all_events = new List<Segmentation_Event_Rep>();
            
            //in case the user mistakingly enters in no boundary times, we'll then just segment the data as normal using the SenseCam processing method of "get_boundary_times_for_all_images()..."
            if (list_of_user_defined_boundary_times.Count == 0)
                return get_boundary_times_for_all_images(list_of_images);

            //however if we do have a list of boundary times, let's record them...
            else if (list_of_user_defined_boundary_times.Count >= 1)
            {
                for (int counter = 0; counter < list_of_user_defined_boundary_times.Count; counter++)
                    all_events.Add(new Segmentation_Event_Rep(list_of_user_defined_boundary_times[counter].get_day(), list_of_user_defined_boundary_times[counter].get_start_time(), list_of_user_defined_boundary_times[counter].get_end_time(), list_of_user_defined_boundary_times[counter].get_keyframe_image_name()));

            } //close else if (list_of_boundary_times.Count >= 1)...
            
            //finally convert the ArrayList to an Array, and return
            return all_events.ToArray();
        } //end method get_boundary_times_for_all_images()




        private static Segmentation_Image_Rep[] get_image_values_from_chunk(Segmentation_Image_Rep[] all_images, int chunk_id)
        {
            List<Segmentation_Image_Rep> chunk_images = new List<Segmentation_Image_Rep>();
            int array_position_counter = 0;

            foreach (Segmentation_Image_Rep sample_image in all_images)
            {
                if (sample_image.get_chunk_id() > chunk_id)
                    break; //no point in reading any more we've no passed through the desired chunk

                if (sample_image.get_chunk_id() == chunk_id)
                { //so if the chunk id's match up, add it to our list
                    sample_image.set_array_position(array_position_counter);
                    array_position_counter++; //for the next image

                    chunk_images.Add(sample_image);
                } //end if (sample_image.get_chunk_id() == chunk_id)
            } //end foreach (Image_Rep sample_image in all_images)

            return chunk_images.ToArray();
        } //end method get_image_values_from_chunk()




        private static void add_new_events_from_chunk_to_event_list(List<Segmentation_Event_Rep> list_of_events, Segmentation_Image_Rep[] boundary_images_from_chunk, Segmentation_Image_Rep[] all_images_in_chunk)
        {
            //THIS METHOD IS RESPOINSIBLE FOR DETERMING THE DAY, START, AND END TIME VALUES OF ALL THE NEW EVENTS, GIVEN THE BOUNDARY IMAGES IN A CHUNK
            //METHOD APPENDS NEW EVENTS TO ARRAYLIST PASSED IN

            //so I treat the individual images from final_boundary_images as the start_time image of each event!!!

            //a. mark event between start of chunk and first detected boundary...
            list_of_events.Add(new Segmentation_Event_Rep(all_images_in_chunk,
                                                0, //position is from the very first position in the chunk
                                                boundary_images_from_chunk[0].get_array_position() - 1) //and the end position is from the image just 1 position before the start position of the first identified event
                                            );


            //b. now mark all the events that don't deal with the first image's event or last image's event in the chunk
            for (int bound_image_counter = 0; bound_image_counter < boundary_images_from_chunk.Length - 1; bound_image_counter++)
            {

                list_of_events.Add(new Segmentation_Event_Rep(all_images_in_chunk,
                                                    boundary_images_from_chunk[bound_image_counter].get_array_position(), //position is from the boundary image in question
                                                    boundary_images_from_chunk[bound_image_counter + 1].get_array_position() - 1) //and the end position is from just 1 position before the start time of the nexy boundary image in question
                                                );

            } //end for(int bound_image_counter=0; bound_image_counter<final_boundary_images.Length; bound_image_counter++){


            //c. mark event between lasst detected boundary and last image in chunk...
            list_of_events.Add(new Segmentation_Event_Rep(all_images_in_chunk,
                                                boundary_images_from_chunk[boundary_images_from_chunk.Length - 1].get_array_position(), //position is from the very last boundary position
                                                all_images_in_chunk.Length - 1) //and the end position is from the very last image in the chunk!
                                            );

        } //end method add_new_events_from_chunk_to_event_list()




        private static void normalise_and_fuse_data_sources(Segmentation_Image_Rep[] list_of_images)
        {
            //I'M GOING TO NORMALISE THE 2 SOURCES USING SUM NORMALISATION!

            double[] acc_x_source, acc_comb_source, pir_source;
            acc_x_source = new double[list_of_images.Length];
            acc_comb_source = new double[list_of_images.Length];
            pir_source = new double[list_of_images.Length];


            //1. CONVERT VALUES TO DOUBLE ARRAYS
            for (int counter = 0; counter < acc_x_source.Length; counter++)
            {
                acc_x_source[counter] = list_of_images[counter].get_manipulated_acc_x();
                acc_comb_source[counter] = list_of_images[counter].get_manipulated_acc_combined();
                pir_source[counter] = list_of_images[counter].get_manipulated_pir();
            } //end method for (int counter = 0; counter < acc_x_source.Length; counter++)

            //2. NORMALISE SCALAR VALUES
            sum_normalisation(acc_x_source);
            sum_normalisation(acc_comb_source);
            sum_normalisation(pir_source);


            //3. RESET VALUES IN IMAGE ARRAY
            //and VERY IMPORTANTLY, WE CALCULATE THE FUSED SCORE FOR EACH IMAGE TOO
            for (int counter = 0; counter < acc_x_source.Length; counter++)
            {
                list_of_images[counter].set_manipulated_acc_x(acc_x_source[counter]);
                list_of_images[counter].set_manipulated_acc_combined(acc_comb_source[counter]);
                list_of_images[counter].set_manipulated_pir(pir_source[counter]);

                //FUSION PART HERE!!
                //given the normalised image and sensor scores...
                //we can also calculate the fused score too
                list_of_images[counter].set_fused_score(get_combMIN_of_values(acc_comb_source[counter], acc_x_source[counter], pir_source[counter]));

            } //end method for (int counter = 0; counter < acc_x_source; counter++)


        } //end method normalise_and_fuse_data_sources()




        private static void sum_normalisation(double[] input_array)
        { //i.e. shift min to 0, scale max to 1 ... from Montague & Aslam, CIKM'01
            double min_score = 9999999.99;
            double sum_score = 0; //to help determine what the largest values is

            for (int c = 0; c < input_array.Length; c++)
            { //go through all the elements of the array to determine what the largest value is
                sum_score += input_array[c];

                if (input_array[c] < min_score)
                    min_score = input_array[c];
            } //end going thorough each element to get the largest score to normalise against

            for (int c = 0; c < input_array.Length; c++)
            { //then go through every element and normalise it, given we now know the maximum value
                if (sum_score > 0)
                    input_array[c] = (input_array[c] - min_score) / sum_score; //normalise this element against the largest element value
                else input_array[c] = 0.0;
            } //end going through each element to normalise it
        } //end method sum_normalisation




        private static int[] get_chunk_ids_for_user(Segmentation_Image_Rep[] all_the_images)
        {
            //basically get the chunk ids of the first and last images
            //then return the sequence of numbers from the first chunk to the last chunk all incramented by 1 (as they will be as determined by Upload_Manipulated_Sensor_Data)

            int start_chunk, end_chunk;

            //basically get the chunk ids of the first and last images
            start_chunk = all_the_images[0].get_chunk_id();
            end_chunk = all_the_images[all_the_images.Length - 1].get_chunk_id();

            int[] chunk_list = new int[1 + (end_chunk - start_chunk)];

            //then return the sequence of numbers from the first chunk to the last chunk all incramented by 1 (as they will be as determined by Upload_Manipulated_Sensor_Data)
            for (int c = 0; c < chunk_list.Length; c++)
            {
                chunk_list[c] = start_chunk + c;
            }

            return chunk_list;
        } //end method get_chunk_ids_for_user()




        private static double get_combMIN_of_values(double acc_comb_score, double acc_x_score, double pir_source)
        {
            //return the median value of all the scores -> TREC 2 - Combination of Multiple Searches , page 243 E. Fox, J. Shaw (Virginia Tech)
            double[] list_of_vals = { acc_comb_score, acc_x_score, pir_source };
            double[] list_of_val_weights = { ACC_COMBINED_WEIGHT, ACC_X_WEIGHT, PIR_WEIGHT };

            return Standard_Calculation.get_combMIN_of_values(list_of_vals, list_of_val_weights);
        } //end method get_combMED_of_values




        private static void peak_score_image_array(Segmentation_Image_Rep[] image_array)
        {
            //1. get fused values into a double array
            double[] list_of_fused_values = get_all_fused_values_into_double_array(image_array);


            //2. peak score these double values
            Peak_Scoring.execute_peak_scoring(list_of_fused_values);


            //3. finally set the values in the image array with their new updated and peak scored fusion scores
            for (int count = 0; count < list_of_fused_values.Length; count++)
                image_array[count].set_fused_score(list_of_fused_values[count]);
        } //end method peak_score_image_array()




        private static Segmentation_Image_Rep[] fused_image_vals_above_dynamic_threshold(Segmentation_Image_Rep[] image_readings_in_chunk)
        {
            //THIS METHOD IS RESPONSIBLE FOR TAKING IN THE MANIPULATED SENSOR VALUES ASSOCIATED WITH ALL THE IMAGES IN A PARTICULAR CHUNK
            //AND THEN PERFORMS DYNAMIC THRESHOLDING ON ALL THE VALUES TO
            //DETERMINE WHICH IMAGES ARE LIKELY TO BE EVENT BOUNDARIES

            //1. GET THE RELEVANT VALUES INTO A DOUBLE ARRAY
            double[] manipulated_image_values = get_all_fused_values_into_double_array(image_readings_in_chunk);


            //2. GET THE THRESHOLD FIGURE BASED ON THOSE VALUES
            double threshold_value = Thresholding.get_threshold_value(manipulated_image_values);


            //3. GO THROUGH ALL THE SENSOR READINGS AND ONLY STORE THOSE THAT ARE ABOVE THE THRESHOLD
            Segmentation_Image_Rep[] images_above_threshold = Thresholding.vals_above_threshold(image_readings_in_chunk, threshold_value);

            //AD fix on 25/01/11
            //if no boundary image has been identified in this chunk, we'll just select the middle image to break it up
            //this is in response to later processing steps seeing no boundary for a chunk, and then presuming we don't want to see any images from it
            //so now the code below fixes this...
            if (images_above_threshold.Length == 0)
            {
                try
                {
                    //now make boundary image, the middle image in this chunk...
                    images_above_threshold = new Segmentation_Image_Rep[] { image_readings_in_chunk[image_readings_in_chunk.Length / 2] };
                } //close try...
                catch (Exception excep) { }; //close try...catch
            } //close if (images_above_threshold.Length == 0)...

            //4. RETURN THE NEW LIST OF SENSOR READINGS THAT ARE ABOVE THAT THRESHOLD
            return images_above_threshold;
        } //end method sensor_vals_above_dynamic_threshold()




        private static double[] get_all_fused_values_into_double_array(Segmentation_Image_Rep[] image_array)
        {
            //1. get fused values into a double array
            double[] list_of_fused_values = new double[image_array.Length];

            for (int count = 0; count < list_of_fused_values.Length; count++)
                list_of_fused_values[count] = image_array[count].get_fused_score();

            return list_of_fused_values;
        } //end method get_all_fused_values_into_double_array()




        private static Segmentation_Image_Rep[] cluster_boundary_list(Segmentation_Image_Rep[] orig_sensor_list, int minute_threshold, int overall_chunk_size, int num_end_images_to_ignore)
        { //This method is used to delete any boundaries that are within <minute_threshold> minutes of each other, deleting the 2nd/succeeding such boundary

            //so if we've too little images in this chunk/upload we'll simply return zero events
            if ((overall_chunk_size <= (num_end_images_to_ignore * 2)) || (orig_sensor_list.Length == 0) || (orig_sensor_list == null))
                return null;


            //convert orig_sensor_list to an array to make it easier to work with, i.e. easier to delete items
            List<Segmentation_Image_Rep> orig_list = orig_sensor_list.ToList<Segmentation_Image_Rep>();

            DateTime current_read_time, previous_read_time = new DateTime();            
            TimeSpan current_row_time_diff;
            Segmentation_Image_Rep image_value;



            //num_end_images_to_ignore VARIABLE IS USED TO REMOVE ANY PROPOSED BOUNDARIES TOO NEAR THE START AND END OF THE CHUNK
            //will remove a specified number of images at the start and end of the list (i.e. we don't want to mark the first couple and last couple of images as events, as there's naturally a lot of chopping and changing that the wearer is going through when switching the SenseCam on ... which creates a _lot_ of noise!)
            for (int c = 0; (c < num_end_images_to_ignore && orig_list.Count > 2); c++)
            {
                image_value = orig_list[0];

                if (image_value.get_array_position() < num_end_images_to_ignore
                    || (image_value.get_array_position() > overall_chunk_size - num_end_images_to_ignore - 1)
                    )
                {
                    orig_list.RemoveAt(0); //so remove first image
                    orig_list.RemoveAt(orig_list.Count - 1); //and remove last image in list
                } //end big long if statement to see if the particular image occurs too near the start of end of the chunk
            } //end for (int c = 0; c < num_end_images_to_ignore; c++)


            previous_read_time = orig_list[0].get_image_time(); //loop will start from 1st item, here I store 0th item
            for (int counter = 1; counter < orig_list.Count; counter++)
            {
                //loop to deal with items clustered closely together
                //which will delete any items within <minute_threshold> minutes of a previously recorded item
                //i.e. two event boundaries suggested too closely to each other
                image_value = orig_list[counter];
                current_read_time = image_value.get_image_time();
                current_row_time_diff = current_read_time - previous_read_time;

                if (current_row_time_diff.TotalMinutes < minute_threshold)
                { //i.e. if this is within 5 minutes of the previous row, we can delete it
                    orig_list.RemoveAt(counter); //delete this row
                    counter--; //we don't want to incrament value by 1 next time around as list is now shifted 1 to the left
                } //end if (current_row_time_diff.TotalMinutes < minute_threshold)

                previous_read_time = current_read_time; //just for the next time around
            } //end for (int counter = 0; counter < orig_list.Count; counter++)


            //then convert the arraylist to an array
            return orig_list.ToArray();
        } //end method cluster_boundary_list()



        public static Segmentation_Event_Rep[] get_temporal_boundary_times_for_all_images(Segmentation_Image_Rep[] list_of_images)
        {
            int num_images_per_event = 95;

            //BASICALLY RETURNS A LIST OF THE BOUNDARY TIMES (AND CHUNK BOUNDARY TIMES)
            //because there is no sensor information, we just segment into events on every 95th image
            //RETURN LIST OF BOUNDARY TIMES FOR WHOLE LIST (SO BOUNDS + CHUNK START TIMES (APART FROM CHUNK 0 OF COURSE))

            int[] all_chunk_ids; //to store all the possible chunk ids the list
            Segmentation_Image_Rep[] chunk_raw_values = new Segmentation_Image_Rep[1]; //will store the information of all the images in each chunk
            List<Segmentation_Event_Rep> all_events = new List<Segmentation_Event_Rep>();
            List<Segmentation_Image_Rep> boundaries_in_chunk;

            //for each user get the number that the chunks count up to
            all_chunk_ids = get_chunk_ids_for_user(list_of_images);

            foreach (int chunk_id in all_chunk_ids)
            { // then go through each chunk
                

                //1. then get the all the Image_Rep values for this chunk
                chunk_raw_values = get_image_values_from_chunk(list_of_images, chunk_id);


                boundaries_in_chunk = new List<Segmentation_Image_Rep>();
                for (int c = num_images_per_event; c < chunk_raw_values.Length; c += num_images_per_event)
                    boundaries_in_chunk.Add(chunk_raw_values[c]);


                Segmentation_Image_Rep[] vals_above_threshold = new Segmentation_Image_Rep[boundaries_in_chunk.Count];
                for (int c = 0; c < vals_above_threshold.Length; c++)
                    vals_above_threshold[c] = (Segmentation_Image_Rep)boundaries_in_chunk[c];




                //5. CLEAN UP BOUNDARY IMAGES (I.E. ONES WITHING SAY 5 MINUTES OF EACH OTHER)
                //the minimum gap between boundaries shall be 3
                Segmentation_Image_Rep[] final_boundary_images = cluster_boundary_list(vals_above_threshold, 3, chunk_raw_values.Length, 4);


                //6. APPEND LIST OF BOUNDARY IMAGES TO AN ARRAY LIST OF EVENTS
                if (final_boundary_images != null)
                    add_new_events_from_chunk_to_event_list(all_events, final_boundary_images, chunk_raw_values);


            } //end foreach (int chunk_id in user_chunk_ids)

            if (all_events.Count == 0 && chunk_raw_values.Length > 1)
                return new Segmentation_Event_Rep[] { new Segmentation_Event_Rep(chunk_raw_values, 0, chunk_raw_values.Length - 1) };
            else if (all_events.Count == 0 && chunk_raw_values.Length <= 1)
                return null;


            //finally convert the ArrayList to an Array
            return all_events.ToArray();
        } //end method get_temporal_boundary_times_for_all_images()






        public static Segmentation_Event_Rep[] get_boundary_times_for_all_non_csv_images(Segmentation_Image_Rep[] list_of_images)
        {
            //BASICALLY RETURNS A LIST OF THE BOUNDARY TIMES (AND CHUNK BOUNDARY TIMES)
            //READ IN THE new_images TABLE FROM THE DATABASE
            //NORMALISE COMBINED IMAGES AND COMBINED ACC COLUMNS
            //FUSE BOTH SOURCES TOGETHER
            //THRESHOLD FUSED SCORES (KAPUR 64 BINS)
            //REMOVE BOUNDARIES THAT OCCUR WITHIN 3 MINUTES OF A PREVIOUS BOUNDARY
            //RETURN LIST OF BOUNDARY TIMES FOR WHOLE LIST (SO BOUNDS + CHUNK START TIMES (APART FROM CHUNK 0 OF COURSE))

            int[] all_chunk_ids; //to store all the possible chunk ids the list
            Segmentation_Image_Rep[] chunk_raw_values = new Segmentation_Image_Rep[1]; //will store the information of all the images in each chunk
            List<Segmentation_Event_Rep> all_events = new List<Segmentation_Event_Rep>();

            //for each user get the number that the chunks count up to
            all_chunk_ids = get_chunk_ids_for_user(list_of_images);

            foreach (int chunk_id in all_chunk_ids)
            { // then go through each chunk

                //1. then get the all the Image_Rep values for this chunk
                chunk_raw_values = get_image_values_from_chunk(list_of_images, chunk_id);


                //2. NORMALISE AND FUSE DATA SOURCES
                normalise_and_fuse_data_sources(chunk_raw_values);


                //3. PROCESS PEAK SCORING ... we don't do this for non-csv folders...
                //peak_score_image_array(chunk_raw_values);


                //4. THRESHOLD IMAGE TRIGGERED MANIPULATED VALUES
                Segmentation_Image_Rep[] vals_above_threshold = fused_image_vals_above_dynamic_threshold_non_csv(chunk_raw_values);


                //5. CLEAN UP BOUNDARY IMAGES (I.E. ONES WITHING SAY 5 MINUTES OF EACH OTHER)
                //todo, must define a variable to indicate number end images to ignore (i.e. not hard coded as 4)
                Segmentation_Image_Rep[] final_boundary_images = cluster_boundary_list_non_csv(vals_above_threshold, MINIMUM_LENGTH_OF_AUTOMATICALLY_SEGMENTED_EVENTS, chunk_raw_values.Length, 4);


                //6. APPEND LIST OF BOUNDARY IMAGES TO AN ARRAY LIST OF EVENTS
                if (final_boundary_images != null && AUTOMATIC_EVENT_SEGMENTATION_ENABLED==1)
                    add_new_events_from_chunk_to_event_list(all_events, final_boundary_images, chunk_raw_values);
                else all_events.Add(new Segmentation_Event_Rep(chunk_raw_values, 0, chunk_raw_values.Length - 1));


            } //end foreach (int chunk_id in user_chunk_ids)

            if (all_events.Count == 0 && chunk_raw_values.Length > 1)
                return new Segmentation_Event_Rep[] { new Segmentation_Event_Rep(chunk_raw_values, 0, chunk_raw_values.Length - 1) };
            else if (all_events.Count == 0 && chunk_raw_values.Length <= 1)
                return null;


            //finally convert the ArrayList to an Array, and return
            return all_events.ToArray();
        } //end method get_boundary_times_for_all_non_csv_images()




        private static Segmentation_Image_Rep[] fused_image_vals_above_dynamic_threshold_non_csv(Segmentation_Image_Rep[] image_readings_in_chunk)
        {
            //THIS METHOD IS RESPONSIBLE FOR TAKING IN THE MANIPULATED SENSOR VALUES ASSOCIATED WITH ALL THE IMAGES IN A PARTICULAR CHUNK
            //AND THEN PERFORMS DYNAMIC THRESHOLDING ON ALL THE VALUES TO
            //DETERMINE WHICH IMAGES ARE LIKELY TO BE EVENT BOUNDARIES

            //1. GET THE RELEVANT VALUES INTO A DOUBLE ARRAY
            double[] manipulated_image_values = get_all_fused_values_into_double_array(image_readings_in_chunk);


            //2. GET THE THRESHOLD FIGURE BASED ON THOSE VALUES
            double threshold_value = Thresholding.get_non_csv_threshold(manipulated_image_values);

            //3. GO THROUGH ALL THE SENSOR READINGS AND ONLY STORE THOSE THAT ARE ABOVE THE THRESHOLD
            Segmentation_Image_Rep[] images_above_threshold = Thresholding.vals_above_threshold(image_readings_in_chunk, threshold_value);


            //4. RETURN THE NEW LIST OF SENSOR READINGS THAT ARE ABOVE THAT THRESHOLD
            return images_above_threshold;
        } //end method fused_image_vals_above_dynamic_threshold_non_csv()




        private static Segmentation_Image_Rep[] cluster_boundary_list_non_csv(Segmentation_Image_Rep[] orig_sensor_list, int minute_threshold, int overall_chunk_size, int num_end_images_to_ignore)
        { //This method is used to delete any boundaries that are within 3 minutes of the previous boundary ... presuming user passes in parameter 3 for minute_threshold

            //so if we've too little images in this chunk we'll simply return zero events
            if ((overall_chunk_size <= (num_end_images_to_ignore * 2)) || (orig_sensor_list.Length == 0) || (orig_sensor_list == null))
                return null;


            //convert orig_sensor_list to an array to make it easier to work with, i.e. easier to delete items
            List<Segmentation_Image_Rep> orig_list = orig_sensor_list.ToList<Segmentation_Image_Rep>();

            int current_read_position, previous_read_position = -1;
            int current_read_position_diff;
            Segmentation_Image_Rep image_value;



            //num_end_images_to_ignore VARIABLE IS USED TO REMOVE ANY PROPOSED BOUNDARIES TOO NEAR THE START AND END OF THE CHUNK
            //will remove a specified number of images at the start and end of the list (i.e. we don't want to mark the first couple and last couple of images as events, as there's naturally a lot of chopping and changing that the wearer is going through when switching the SenseCam on ... which creates a _lot_ of noise!)
            for (int c = 0; (c < num_end_images_to_ignore && orig_list.Count > 2); c++)
            {
                image_value = orig_list[0];

                if (image_value.get_array_position() < num_end_images_to_ignore
                    || (image_value.get_array_position() > overall_chunk_size - num_end_images_to_ignore - 1)
                    )
                {
                    orig_list.RemoveAt(0); //so remove first image
                    orig_list.RemoveAt(orig_list.Count - 1); //and remove last image in lise
                } //end big long if statement to see if the particular image occurs too near the start of end of the chunk
            } //end for (int c = 0; c < num_end_images_to_ignore; c++)


            previous_read_position = orig_list[0].get_array_position(); //loop will start from 1st item, here I store 0th item
            for (int counter = 1; counter < orig_list.Count; counter++)
            {
                //loop to deal with items clustered closely together
                //which will delete any items within <minute_threshold> minutes of a previously recorded item
                //i.e. two event boundaries suggested too closely to each other                
                image_value = orig_list[counter];
                current_read_position = image_value.get_array_position();
                current_read_position_diff = current_read_position - previous_read_position;

                if (current_read_position_diff < minute_threshold)
                { //i.e. if this ist within 5 minutes (i.e. reads!) of the previous row, we can delete it
                    orig_list.RemoveAt(counter); //delete this row
                    counter--; //we don't want to incrament value by 1 next time around as list is now shifted 1 to the left
                } //end if (current_row_time_diff.TotalMinutes < minute_threshold)

                previous_read_position = current_read_position; //just for the next time around
            } //end for (int counter = 0; counter < orig_list.Count; counter++)


            //and finally return that array
            return orig_list.ToArray();

        } //end method cluster_boundary_list_non_csv()



    } //end class

} //end namespace
