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
using System.Xml;
using System.IO;

namespace SenseCamBrowser1.Upload_Images_and_Segment_into_Events
{
    class Update_Image_Dat_File
    {
        //THE PURPOSE OF THIS CLASS IS TO PROVIDE A METHOD TO UPDATE THE
        //IMAGE.DAT FILE WITH EVENT BOUNDARY INFORMATION

        //1 STEP 1 -> READ IN ALL THE IMAGE VALUES AND STORE THEM TO AN ARRAY

        //2 STEP 2 -> MARK ALL IMAGES THAT ARE EVENT BOUNDARIES

        //3 STEP 3 -> DELETE ORIGINAL IMAGE.DAT FILE

        //4 STEP 4 -> WRITE NEW IMAGE.DAT WITH EVENT BOUNDARY INFORMATION INCLUDED



        public static void update_image_dat(string local_folder, Segmentation_Image_Rep[] list_of_all_images, Segmentation_Event_Rep[] list_of_events)
        {
            //step 1 is covered as this application reads the list_of_all_images information earlier, so we just pass it into this method

            //now mark all images that are event boundaries
            mark_all_images_that_are_event_boundaries(list_of_all_images, list_of_events);

            //delete the original image.dat file
            delete_original_image_dat_file(local_folder);

            //write out the new image.dat file
            write_new_image_dat_file_with_bookmarks(local_folder, list_of_all_images);
        } //end method update_image_dat()




        private static Segmentation_Image_Rep[] read_image_information(string local_folder)
        {
            //FIRSTLY MAKE ARRAYLIST TO STORE ALL IMAGE_REPS
            List<Segmentation_Image_Rep> list_of_images = new List<Segmentation_Image_Rep>();

            string image_name;
            DateTime image_time;

            //THEN READ IMAGE.DAT FILE, AND GO THROUGH EACH ROW
            XmlTextReader image_dat_reader = new XmlTextReader(local_folder + "image.dat");

            int tmp_count = 0;
            while (image_dat_reader.Read())
            {
                if (image_dat_reader.Name.Equals("Image") && image_dat_reader.AttributeCount > 1)
                {

                    //WHILE IN ROW STORE ELEMENTS TO LOCAL VARIABLES
                    image_name = image_dat_reader.GetAttribute(0);
                    tmp_count++;

                    image_time = convert_string_to_datetime(image_dat_reader.GetAttribute(1));
                    

                    //ADD ELEMENTS TO TYPE IMAGE_REP AND ADD TO ARRAYLIST
                    list_of_images.Add(new Segmentation_Image_Rep(image_name, image_time));

                } //end if (image_dat_reader.Name.Equals("Image") && image_dat_reader.AttributeCount > 1)
            } //end while (image_dat_reader.Read())
            image_dat_reader.Close();


            //CONVERT ARRAYLIST TO AN ARRAY OF TYPE IMAGE_REP[] AND RETURN IT
            return list_of_images.ToArray();
        } //end method read_image_information()




        private static void mark_all_images_that_are_event_boundaries(Segmentation_Image_Rep[] image_list, Segmentation_Event_Rep[] list_of_events)
        {
            if (image_list.Length > 0 && list_of_events.Length > 0)
            {
                int boundary_counter = 0;

                for (int image_counter = 0; image_counter < image_list.Length; image_counter++)
                {
                    //go through all the images
                    //and then compare each one to see if it matches the time of the next boundary that we're expecting ... we look at the boundary end time so that the first image is not marked as a boundary
                    if ((image_list[image_counter].get_image_time() == list_of_events[boundary_counter].get_endTime()) && (boundary_counter != list_of_events.Length - 1))
                    {
                        //and we incrament the boundary_counter
                        boundary_counter++;
                    } //end if (image_list[image_counter].get_image_time() == list_of_events[boundary_counter].get_endTime())

                    image_list[image_counter].set_event_boundary(boundary_counter);

                } //end for (int image_counter = 0; image_counter < image_list.Length; image_counter++)			
            } //end if (image_list.Length > 0 && list_of_events.Length > 0)
        } //end method mark_all_images_that_are_event_boundaries




        private static void delete_original_image_dat_file(string folder_path)
        {
            if (File.Exists(folder_path + "image.dat"))
                File.Move(folder_path + "image.dat", folder_path + "image_" + DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second + ".dat");
        } //end method delete_original_image_dat_file()




        private static void write_new_image_dat_file_with_bookmarks(string folder_name, Segmentation_Image_Rep[] list_of_images)
        {
            XmlTextWriter new_image_dat_file = new XmlTextWriter(folder_name + "image.dat", Encoding.Unicode);
            new_image_dat_file.Formatting = Formatting.Indented;
            new_image_dat_file.Indentation = 2;
            new_image_dat_file.WriteStartDocument();
            new_image_dat_file.WriteStartElement("ImageInfoCollection");

            foreach (Segmentation_Image_Rep sample_image in list_of_images)
            {
                //and now we secondly check if the file is greater than 2KB in size (otherwise it's probably a skewed image)
                new_image_dat_file.WriteStartElement("Image");
                new_image_dat_file.WriteAttributeString("filename", sample_image.get_image_name());
                new_image_dat_file.WriteAttributeString("timestamp", convert_datetime_to_string(sample_image.get_image_time()));
                new_image_dat_file.WriteAttributeString("distance", "0");
                new_image_dat_file.WriteAttributeString("bookmark", "DCU Event " + sample_image.get_event_boundary());
                new_image_dat_file.WriteEndElement(); //end of "Image"
            } //end foreach (Image_Rep sample_image in list_of_images)

            new_image_dat_file.WriteEndElement(); //end of "ImageInfoCollection"
            new_image_dat_file.WriteEndDocument(); //end document

            new_image_dat_file.Close(); //close the writer
        } //end method write_new_image_dat_file_with_bookmarks()




        private static DateTime convert_string_to_datetime(string input)
        {
            //2006-08-25T15:58:38
            //0123456789012345678
            int year, month, day, hour, minute, second;
            
            try { year = int.Parse(input.Substring(0, 4)); } catch (Exception excep) { year = DateTime.Now.Year; }
            try { month = int.Parse(input.Substring(5, 2)); } catch (Exception excep) { month = DateTime.Now.Year; }
            try { day = int.Parse(input.Substring(8, 2)); } catch (Exception excep) { day = DateTime.Now.Year; }

            try { hour = int.Parse(input.Substring(11, 2)); } catch (Exception excep) { hour = DateTime.Now.Year; }
            try { minute = int.Parse(input.Substring(14, 2)); } catch (Exception excep) { minute = DateTime.Now.Year; }
            try { second = int.Parse(input.Substring(17, 2)); } catch (Exception excep) { second = DateTime.Now.Year; }

            return new DateTime(year, month, day, hour, minute, second);            
        } //end method convert_string_to_datetime




        private static string convert_datetime_to_string(DateTime sample_time)
        {
            //2006-08-25T15:58:38
            //0123456789012345678

            string year, month, day, hour, minute, second;


            year = sample_time.Year.ToString();

            if (sample_time.Month > 9)
                month = sample_time.Month.ToString();
            else month = "0" + sample_time.Month;

            if (sample_time.Day > 9)
                day = sample_time.Day.ToString();
            else day = "0" + sample_time.Day;

            if (sample_time.Hour > 9)
                hour = sample_time.Hour.ToString();
            else hour = "0" + sample_time.Hour;

            if (sample_time.Minute > 9)
                minute = sample_time.Minute.ToString();
            else minute = "0" + sample_time.Minute;

            if (sample_time.Second > 9)
                second = sample_time.Second.ToString();
            else second = "0" + sample_time.Second;


            //2006-08-25T15:58:38
            return year + "-" + month + "-" + day + "T" + hour + ":" + minute + ":" + second;
        } //end method convert_datetime_to_string()




        public static Boolean image_dat_file_already_processed(string local_folder)
        {
            //THEN READ IMAGE.DAT FILE, AND GO THROUGH EACH ROW
            XmlTextReader image_dat_reader = new XmlTextReader(local_folder + @"\image.dat");

            Boolean event_attribute_present = false;
            while (image_dat_reader.Read() && !event_attribute_present)
            {
                if (image_dat_reader.Name.Equals("Image") && image_dat_reader.AttributeCount > 1)
                {
                    //and now basically keep checking to see if there are more than 4 attributes, if there are ... it means a bookmark is present, as well as a utc_timestamp, and hence this has probably been processed already
                    if (image_dat_reader.AttributeCount > 4)
                        event_attribute_present = true;
                } //end if (image_dat_reader.Name.Equals("Image") && image_dat_reader.AttributeCount > 1)
            } //end while (image_dat_reader.Read())
            image_dat_reader.Close();

            return event_attribute_present;
        } //end method read_image_information()




    } //end class

} //end namesapce
