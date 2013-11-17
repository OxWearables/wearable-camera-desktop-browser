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
    class Thresholding
    {
        public static double USER_THRESHOLD_VALUE = 3.4;


        public static double get_threshold_value(double[] values_to_threshold)
        {
            //WILL PERFORM MEAN WITH PARAM OF 3.4
            return mean_threshold(values_to_threshold, USER_THRESHOLD_VALUE);

        } //end vals_above_threshold()



        public static double get_non_csv_threshold(double[] values_to_threshold)
        {
            double max = double.MinValue, min = double.MaxValue;

            foreach (double val in values_to_threshold)
            {
                if (val < min)
                    min = val;
                else if (val > max)
                    max = val;
            }

            return (max - min) / 2;
        } //close method get_non_csv_threshold()...




        private static double mean_threshold(double[] values_to_threshold, double k)
        {
            //THIS METHOD TAKES IN A LIST OF VALUES AND IT SELECTS THE THRESHOLD AS FOLLOWS
            //threshold = mean + k * standard_deviation
            double mean = get_mean_value(values_to_threshold);
            double standard_deviation = get_standard_deviation(values_to_threshold);

            return mean + (k * standard_deviation);
        } //end method mean_threshold()



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




        private static double square_value(double value)
        {
            return value * value;
        } //end square_value()




        public static Segmentation_Image_Rep[] vals_above_threshold(Segmentation_Image_Rep[] all_image_values, double threshold)
        {
            //THIS METHOD GOES THROUGH ALL THE SENSOR VALUES AND RETURNS A NEW LIST WITH ONLY THOSE THAT HAVE THE ATTRIBUTE IN QUESTION ABOVE THE SPECIFIED THRESHOLD
            List<Segmentation_Image_Rep> image_list = new List<Segmentation_Image_Rep>();

            foreach (Segmentation_Image_Rep manipulated_row in all_image_values)
            {
                if (manipulated_row.get_fused_score() > threshold)
                    image_list.Add(manipulated_row);
            } //end foreach(Image_Rep manipulated_row in all_image_values)


            return image_list.ToArray();
        } //end method fusion_vals_above_threshold()




    } //end class

} //end namespace
