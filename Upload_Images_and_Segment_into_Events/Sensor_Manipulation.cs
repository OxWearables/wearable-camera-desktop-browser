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

namespace SenseCamBrowser1.Upload_Images_and_Segment_into_Events
{
    class Sensor_Manipulation
    {
        //THIS CLASS IS RESPONSIBLE FOR MANIPULATING AN ARRAY OF SENSOR VALUE
        //IT SHALL OPERATE AS FOLLOWS (BASED ON MY SEGMENTATION EXPERIMENTS)
        //1. SHOT BOUNDARY DETECTION APPROACH (I.E. JUST COMPARE ADJACENT READINGS)


        public static void manipulate_relevant_sensor_values(Sensor_Reading[] sensor_vals, int start_index, int end_index, Upload_and_Segment_Images_Thread.DeviceType device_type)
        {
            //NOTE THE START AND END INDEX ... THESE SIGNIFY THE START AND END OF A PARTICULAR CHUNK IN THE ARRAY

            //1. get the list of sensor values
            //NOTE THE START AND END INDEX ... THESE SIGNIFY THE START AND END OF A PARTICULAR CHUNK IN THE ARRAY

            //FIRSTLY STORE THE RAW VALUES TO LOCAL DOUBLE ARRAYS
            double[] original_acc_combined_values = new double[(end_index - start_index) + 1];
            double[] original_acc_x_values = new double[original_acc_combined_values.Length];
            double[] original_pir_values = new double[original_acc_combined_values.Length];

            for (int counter = 0; counter < original_acc_combined_values.Length; counter++)
            {
                original_acc_combined_values[counter] = sensor_vals[start_index + counter].get_raw_acc_combined(device_type);
                original_acc_x_values[counter] = Standard_Calculation.get_signed_int(sensor_vals[start_index + counter].get_acc_x(), device_type);
                original_pir_values[counter] = sensor_vals[start_index + counter].get_pir();
            } //end for (int counter = 0; counter < original_sensor_values.Length; counter++)



            //---------------------------------------------------------------


            //2. perform SDB comparisons on acc_comb and texttiling on acc x
            double[] acc_comb_manipulated_scores, acc_x_manipulated_scores, pir_manipulated_scores;

            //perform SDB comparisons on acc_comb
            acc_comb_manipulated_scores = calculate_sbd_scores(original_acc_combined_values);

            //and TextTiling of block size 2 on the acc_x component
            acc_x_manipulated_scores = calculate_texttiling_scores(original_acc_x_values, 2);

            //and finally the PIR with a block size of 78
            pir_manipulated_scores = calculate_texttiling_scores(original_pir_values, 78);


            //---------------------------------------------------------------


            //3. smooth the values over a window size of just 1 (i.e. 3 readings just taken, ref, and 1 before + 1 after)
            Standard_Calculation.smooth_array_using_median_values(acc_comb_manipulated_scores, 1);
            Standard_Calculation.smooth_array_using_median_values(acc_x_manipulated_scores, 1);
            Standard_Calculation.smooth_array_using_median_values(original_pir_values, 1);


            //---------------------------------------------------------------


            //4. update Sensor_Reading[] array with newly calculated manipulated values
            set_relevant_manipulated_sensor_values(sensor_vals, acc_comb_manipulated_scores, acc_x_manipulated_scores, pir_manipulated_scores, start_index);

        } //end method manipulate_relevant_sensor_values




        private static double[] calculate_sbd_scores(double[] element_list)
        {
            //So here, for each element, we compare it's value to the previous value, and that's the difference score
            double[] element_dissimilarity_score = new double[element_list.Length];

            element_dissimilarity_score[0] = 0.0; //the first element will always be 0 since you can't compare to a previous value!!!

            //now go through each element, and compare it's value to the previous value, and that's the difference score
            for (int element = 1; element < element_dissimilarity_score.Length; element++)
            {
                element_dissimilarity_score[element] = Standard_Calculation.get_diff_between_nums(element_list[element], element_list[element - 1]);

            } //end for (int element = 0; element < element_dissimilarity_score.Length; element++)


            return element_dissimilarity_score;
        } //end method calculate_texttiling_scores()





        private static void set_relevant_manipulated_sensor_values(Sensor_Reading[] raw_values, double[] newly_calculated_Acc_Comb_Manipulated_Values, double[] newly_calculated_Acc_X_Manipulated_Values, double[] newly_calculated_PIR_Manipulated_Values, int start_index)
        {
            //AGAIN NOTE THE START AND END INDEX...

            for (int counter = 0; counter < newly_calculated_Acc_Comb_Manipulated_Values.Length; counter++)
            {
                raw_values[start_index + counter].set_manipulated_acc_combined(newly_calculated_Acc_Comb_Manipulated_Values[counter]);
                raw_values[start_index + counter].set_manipulated_acc_x(newly_calculated_Acc_X_Manipulated_Values[counter]);
                raw_values[start_index + counter].set_manipulated_pir(newly_calculated_PIR_Manipulated_Values[counter]);
            } //end for (int counter = 0; counter < newly_calculated_Sensor_Manipulated_Values.Length; counter++)

        } //end method set_relevant_manipulated_sensor_values()




        private static double[] calculate_texttiling_scores(double[] element_list, int texttiling_block_size)
        {
            //So here, for each element, we compare it's block, to the block for the next element and record that similarity value
            double[] element_dissimilarity_score = new double[element_list.Length];

            double left_block, right_block;
            //now go through each element, and compare it's block_size to the next 
            for (int element = 0; element < element_dissimilarity_score.Length; element++)
            {

                if (element >= texttiling_block_size && element < element_list.Length - texttiling_block_size) //have -block_size because we'll be looking at the number of blocks ahead, so can't calculate for the last few images ... or the first few either ... which is no big deal as an event is unlikely to occur so near the end and only be a couple of images long
                {
                    //firstly get a value to represent the block of elements for the block corresponding to this element and the next element
                    left_block = get_left_block(element_list, element, texttiling_block_size);
                    right_block = get_right_block(element_list, element + 1, texttiling_block_size);

                    //then calculate their similarity score and store it to the similarity score array
                    element_dissimilarity_score[element] = Standard_Calculation.get_diff_between_nums(left_block, right_block);
                }
                else element_dissimilarity_score[element] = 0.0;

            } //end for (int element = 0; element < element_dissimilarity_score.Length; element++)


            return element_dissimilarity_score;
        } //end method calculate_texttiling_scores




        private static double get_left_block(double[] sensor_list, int element_position, int block_size)
        {
            return get_block_value_for_element(sensor_list, element_position - block_size, block_size);
        } //end method get_left_block()




        private static double get_right_block(double[] sensor_list, int element_position, int block_size)
        {
            return get_block_value_for_element(sensor_list, element_position, block_size);
        } //end methoed get_right_block()




        private static double get_block_value_for_element(double[] list_of_all_values, int element_position, int block_size)
        {
            //so this method will return a representative value for the various relevant values (i.e. from element_position -> [element_position + BLOCK_SIZE]) ... i.e. all the values to the right of this
            double[] list_of_values_in_question = new double[block_size];

            //firstly get the list of values in question
            for (int element_num = element_position; element_num < element_position + block_size; element_num++)
            {
                list_of_values_in_question[element_num - element_position] = list_of_all_values[element_num];
            } //end for (int element_num = element_position; image_num < element_position + IMAGE_BLOCK_SIZE; element_num++)

            return Standard_Calculation.get_sum_value(list_of_values_in_question);

        } //end method get_block_value_for_image()




    } //end class Sensor_Manipulation

} //end namespace
