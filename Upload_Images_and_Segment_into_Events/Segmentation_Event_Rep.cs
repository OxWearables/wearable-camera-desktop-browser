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
using System.Drawing;
using System.IO;

namespace SenseCamBrowser1.Upload_Images_and_Segment_into_Events
{
    class Segmentation_Event_Rep
    {
        private DateTime startTime, endTime, day;        
		private string keyframe_time;




		public Segmentation_Event_Rep(Segmentation_Image_Rep[] image_list, int start_position, int end_position)
		{
			//calculate day, startTime and endTime
			day = image_list[0].get_image_day(); //i.e. get the start time of the very first image in the chunk
			startTime = image_list[start_position].get_image_time();
			endTime = image_list[end_position].get_image_time();

			keyframe_time = image_list[start_position + ((end_position-start_position)/2)].get_image_name();
		} //end constructor


        public Segmentation_Event_Rep(DateTime day, DateTime startTime, DateTime endTime,string description)
        {
            //calculate day, startTime and endTime
            this.day = day; //i.e. get the start time of the very first image in the chunk
            this.startTime = startTime;
            this.endTime = endTime;

            keyframe_time = description;
        } //end constructor


        public Segmentation_Event_Rep( DateTime startTime, DateTime endTime, string description, string folder_path_of_images)
        {
            //calculate day, startTime and endTime
            this.day = new DateTime(startTime.Year, startTime.Month, startTime.Day); //i.e. get the start time of the very first image in the chunk
            this.startTime = startTime;
            this.endTime = endTime;

            string image_extension = ".jpg";
            keyframe_time = description+image_extension;
            Segmentation_Event_Rep.DrawText_and_save_image(description, image_extension, folder_path_of_images);
        } //end constructor



        private static void DrawText_and_save_image(string text, string image_extension, string folder)
        {
            //here we create an image of the description for this user generated boundary/episode type ... to show as a description of what this episode type is ...

            if (!File.Exists(folder + text + image_extension)) //let's only create the file if it doesn't already exist...
            {
                Font font = new Font(FontFamily.GenericSansSerif, 76);
                Color textColor = Color.Red;
                Color backColor = Color.Yellow;

                //first, create a dummy bitmap just to get a graphics object
                Image img = new Bitmap(1, 1);
                Graphics drawing = Graphics.FromImage(img);

                //measure the string to see how big the image needs to be
                SizeF textSize = drawing.MeasureString(text, font);

                //free up the dummy image and old graphics object
                img.Dispose();
                drawing.Dispose();

                //create a new image of the right size
                img = new Bitmap((int)textSize.Width, (int)textSize.Height);

                drawing = Graphics.FromImage(img);

                //paint the background
                drawing.Clear(backColor);

                //create a brush for the text
                Brush textBrush = new SolidBrush(textColor);

                drawing.DrawString(text, font, textBrush, 0, 0);

                drawing.Save();

                textBrush.Dispose();
                drawing.Dispose();


                img.Save(folder + text + image_extension);
            } //close if (!File.Exists(folder + text + image_extension))...
        } //close DrawText_and_save_image()...
		


		public DateTime get_startTime()
		{
			return startTime;
		} //end method get_startTime()




		public DateTime get_endTime()
		{
			return endTime;
		} //end method get_endTime




		public DateTime get_day()
		{
			return day;
		} //end method get_day()




		public string get_keyframe_image_name()
		{
			return keyframe_time;
		} //end method get_keyframe_time()





        public static List<Segmentation_Event_Rep> read_in_list_of_user_defined_episodes_from_file(string episode_file_name, string folder_path_of_sensecam_images)
        {
            //the overall aim of this method is to read in a list of events from a CSV file in the format of: startTime, endTime, episode_description
            //the CSV file has a list of user-defined event for this user (generally taken from either accelerometer, GPS, Heart-rate, etc. data) ... the user will have created this file
            
            List<Segmentation_Event_Rep> list_of_all_sensor_values = new List<Segmentation_Event_Rep>(); //the list of user defined episodes
                        
            TextReader episode_reader = new StreamReader(episode_file_name);
            string line_input;
            string[] elements;
            DateTime startTime = new DateTime();
            DateTime endTime = new DateTime();            
            string episode_description = null;
            
            while ((line_input = episode_reader.ReadLine()) != null)
            {
                elements = line_input.Split(',');

                startTime = DateTime.Parse(elements[0]);
                endTime = DateTime.Parse(elements[1]);
                episode_description = elements[2];

                list_of_all_sensor_values.Add(new Segmentation_Event_Rep(startTime,endTime, episode_description, folder_path_of_sensecam_images));
            } //end method while (sensor_reader.Read())
            episode_reader.Close(); //closing the stream
            
            return list_of_all_sensor_values;
        } //close method read_in_list_of_user_defined_episodes_from_file()...



	} //end class Event_Rep

} //end namespace
