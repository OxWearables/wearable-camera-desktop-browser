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
    class Peak_Scoring
    {



        public static void execute_peak_scoring(double[] scores)
        {
            //this method accepts a list of images, and a double array giving their scores (i.e. smoothed values of the similarity between an image's block and the block of the image succeeding it)
            //So this method will use Georgina and Sandrine's method to determine if an image is a boundary or not, and if so, what score it should get.

            //So how does it work in brief?
            //1. Imagine all the smoothed values graphed
            //2. We only look at where a value has images scores lower than it to either the left side or the right side (or even better both)
            //3. We then get the lowest trough to the left (where the values are successively getting lower than the reference value), and get the difference value between that trough and the current reference value
            //4. We do the same for the values to the right of the reference images
            //5. Then we add the two difference scores together, and this is the probability that this image is an event boundary

            double[] left_side_scores, right_side_scores;
            left_side_scores = get_left_difference_scores(scores);
            right_side_scores = get_right_difference_scores(scores);

            for (int image = 0; image < scores.Length; image++)
                scores[image] = left_side_scores[image] + right_side_scores[image];


            //no need to pass any score out as double array will be updated through reference
        } //end method execute_peak_scoring()




        private static double[] get_left_difference_scores(double[] scores)
        {
            double[] left_scores = new double[scores.Length];
            for (int c = 0; c < scores.Length; c++)
            {
                left_scores[c] = get_difference_to_left_trough(scores, c); //store the difference score
            }

            return left_scores;
        } //end get_left_peak_score




        private static double[] get_right_difference_scores(double[] scores)
        {
            double[] right_scores = new double[scores.Length];
            for (int c = 0; c < scores.Length; c++)
            {
                right_scores[c] = get_difference_to_right_trough(scores, c); //store the difference score
            }

            return right_scores;
        } //end get_right_peak_score()




        private static double get_difference_to_left_trough(double[] scores, int position)
        {
            double current = scores[position]; //the current point, now we'll try and get the lowest adjacent trough to its left

            if (position == 0) //we simply can't go left in the array
                return 0; //return the default value

            double trough = scores[position];

            while (scores[position - 1] <= scores[position]) //while we keep getting smaller values to the left of the reference score 
            {
                trough = scores[position - 1];
                position--;
                if (position == 0)
                    break;
            } //end while (scores[position] < scores[position - 1])

            return current - trough;
        } //end method get_difference_to_left_trough()




        private static double get_difference_to_right_trough(double[] scores, int position)
        {
            double current = scores[position]; //the current point, now we'll try and get the lowest adjacent trough to its right

            if (position == scores.Length - 1) //we simply can't go right in the array
                return 0;

            double trough = scores[position];

            while (scores[position + 1] <= scores[position]) //while we keep getting smaller values to the right of the reference score
            {
                trough = scores[position + 1];
                position++;
                if (position == scores.Length - 1)
                    break;
            } //end while (scores[position] < scores[position + 1])

            return current - trough;
        } //end method get_difference_to_right_trough()




    } //end class Peak_Scoring

} //end namespace
