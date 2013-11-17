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
    class Standard_Calculation
    {
        //generic calculations class


        public static double get_diff_between_nums(double num1, double num2)
        { //just gets the magnitude between 2 different numbers
            if (num1 > num2)
                return num1 - num2;
            else return num2 - num1;
        } //end method get_diff_between_nums()




        public static double get_sum_value(double[] list_of_vals)
        {
            double running_total = 0.0;

            foreach (double d in list_of_vals)
                running_total += d;

            return running_total;
        } //end method get_sum_value()




        public static void smooth_array_using_median_values(double[] input_array, int smoothing_window)
        {
            //AS THE ACC_COMBINED SOURCE IS MANIPULATED USING SBD...
            //I JUST USE A SMOOTHING WINDOW OF 1 (I.E. LOOK AT 3 VALUES; THE VALUE IN QUESTION, AND IT'S IMMEDIATE ADJACENT NEIGHBOURS)

            double[] median_values = new double[input_array.Length];

            double[] list_of_temp_vals = new double[(smoothing_window * 2) + 1]; //to store the list of values to calculate the standard deviation from
            for (int tmp_counter = 0; tmp_counter < list_of_temp_vals.Length; tmp_counter++)
                list_of_temp_vals[tmp_counter] = 0.0;

            int median_counter = 0;
            foreach (double element in input_array)
            {
                add_item_to_tmp_array(list_of_temp_vals, element);
                if (median_counter > list_of_temp_vals.Length && median_counter < input_array.Length - (list_of_temp_vals.Length / 2))
                { //just checking that we've enough values stored to calculate the standard deviation of the temperature values
                    median_values[median_counter - smoothing_window] = calculate_median_value(list_of_temp_vals);
                }
                else median_values[median_counter] = 0.0; //will just be for the first and last couple of rows

                median_counter++;
            } //end foreach (DataRow dr in all_data)


            //then finally update the array passed in by reference
            for (int c = 0; c < input_array.Length; c++)
                input_array[c] = median_values[c];
        } //end method smooth_array_values




        private static void add_item_to_tmp_array(double[] old_tmp_array, double new_value)
        {
            for (int counter = 0; counter < old_tmp_array.Length - 1; counter++)
                old_tmp_array[counter] = old_tmp_array[counter + 1];

            old_tmp_array[old_tmp_array.Length - 1] = new_value;
        } //end method add_item_to_tmp_array




        public static double calculate_median_value(double[] list_of_values)
        {
            double[] tmp_array = new double[list_of_values.Length];

            for (int tmp_count = 0; tmp_count < tmp_array.Length; tmp_count++)
                tmp_array[tmp_count] = list_of_values[tmp_count];

            sortArray(tmp_array); //sort the list of values

            return tmp_array[(tmp_array.Length - 1) / 2]; //then take the median value
        } //end method calculate_median_value




        public static void sortArray(double[] value_list)
        {
            int i;
            int j;
            double temp;

            for (i = (value_list.Length - 1); i >= 0; i--)
            {
                for (j = 1; j <= i; j++)
                {
                    if (value_list[j - 1] > value_list[j])
                    {
                        temp = value_list[j - 1];
                        value_list[j - 1] = value_list[j];
                        value_list[j] = temp;
                    } //end if (value_list[j - 1] > value_list[j])
                } //end for (j = 1; j <= i; j++)
            } //end for (i = (value_list.Length - 1); i >= 0; i--)
        } //end method sortArray




        public static double calculate_weighted_median_value(double[] list_of_values, double[] value_weights)
        {
            double[] tmp_array = new double[list_of_values.Length];

            for (int tmp_count = 0; tmp_count < tmp_array.Length; tmp_count++)
                tmp_array[tmp_count] = list_of_values[tmp_count] * value_weights[tmp_count];

            return Standard_Calculation.calculate_median_value(tmp_array);
        } //end method calculate_median_value




        private static double get_mean_value(double[] list_of_vals)
        {
            double running_total = 0.0;

            foreach (double d in list_of_vals)
                running_total += d;

            return running_total / list_of_vals.Length;
        } //end method get_mean_value()




        private static double get_standard_deviation(double[] input_array)
        {
            double deviation_total, mean_of_values;
            deviation_total = 0.0;

            //1. get mean of all values
            mean_of_values = get_mean_value(input_array);

            //2. now get sum of (square of deviations)
            foreach (double individual_value in input_array)
                deviation_total += square_value(individual_value - mean_of_values);

            //3. return square root of (sum of deviations divided by count-1 of elements) ...i.e. divide by the mean
            return Math.Sqrt(deviation_total / (input_array.Length - 1));
        } //end method get_standard_deviation()




        public static double square_value(double value)
        {
            return value * value;
        } //end square_value()




        public static int get_signed_int(int to_change)
        {
            if (to_change > 32767)
                return to_change - 65536;
            return to_change;
        } //end method get_signed_int()

        public static double get_signed_int(double to_change, Upload_and_Segment_Images_Thread.DeviceType device_type)
        {
            if (device_type == Upload_and_Segment_Images_Thread.DeviceType.Revue || device_type == Upload_and_Segment_Images_Thread.DeviceType.Autographer)
                return to_change;
            else return get_signed_int((int)to_change);            
        } //end method get_signed_int()




        public static int[] get_average_value_of_all_bins(int[][] array_of_features)
        {
            int[] average_feature = new int[array_of_features[0].Length];

            //get sum of all values into average_feature array
            foreach (int[] feature in array_of_features)
            {
                for (int element = 0; element < feature.Length; element++)
                {
                    average_feature[element] += feature[element];
                } //end for(int element=0; element<feature.Length; element++)
            } //end foreach (double[] feature in array_of_features)

            //now just average the sum value by dividing by the number of images (i.e. the length of the outer part of the array passed in as a parameter)
            for (int element = 0; element < average_feature.Length; element++)
                average_feature[element] = average_feature[element] / array_of_features.Length;

            return average_feature;
        } //end methoed get_average_value_of_all_bins




        public static double get_average_value_of_double_list(double[] sensor_list)
        {
            double sum_value = 0;
            //1. sum up bin values from all the images
            foreach (double sensor_val in sensor_list)
            {
                sum_value += sensor_val;
            } //end foreach (double[] image in feature_list)

            //2. now average the summed values
            double final_result = sum_value / sensor_list.Length;

            //3. now return averaged bin values as the result
            return final_result;

        } //end method get_average_value_of_double_list()




        public static double get_combMIN_of_values(double[] list_of_vals, double[] list_of_val_weights)
        {
            //return the lowest scoring source from the scores -> TREC 2 - Combination of Multiple Searches , page 243 E. Fox, J. Shaw (Virginia Tech)
            double min_score = 99999999.99;

            for (int element = 0; element < list_of_vals.Length; element++)
            {
                if ((list_of_vals[element] * list_of_val_weights[element] < min_score) && (list_of_vals[element] * list_of_val_weights[element] >= 0))
                    min_score = list_of_vals[element] * list_of_val_weights[element];
            }

            return min_score;
        } //end method get_combMIN_of_values()




    } //end class

} //end class