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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Data.Common;

namespace SenseCamBrowser1
{
    public class Image_Rep
    {
        public static List<Image_Rep> ImageList;
        private static string BackupImage = "Image-Unavailable.gif";
        private static int ScaledImagePixelWidth = 120;
        private static string DbString = global::SenseCamBrowser1.Properties.Settings.Default.DBConnectionString;

        //Image properties.
        public DateTime image_time { get; set; }
        public string short_image_time { get; set; }
        public string image_path { get; set; }
        public ImageSource scaled_image_src { get; set; }
        public int array_position_in_event { get; set; }

        public Image_Rep(DateTime imgTime, string path, int position)
        {
            this.image_time = imgTime;
            this.image_path = path;
            this.array_position_in_event = position;
            this.scaled_image_src = Image_Rep.GetImgBitmap(path, true);
            this.short_image_time = image_time.ToLongTimeString();
        }


        public Image_Rep(DateTime imgTime, ImageSource imgSource, int position)
        {
            this.image_time = imgTime;
            this.array_position_in_event = position;
            this.scaled_image_src = imgSource;
        }


        public static BitmapImage GetImgBitmap(string imgPath, bool scaled)
        {
            //If we want to load a smaller image in memory, set scaled=True.
            BitmapImage tmpBitmap = new BitmapImage();
            try
            {
                tmpBitmap.BeginInit();
                if (scaled)
                    tmpBitmap.DecodePixelWidth = ScaledImagePixelWidth;

                tmpBitmap.CacheOption = BitmapCacheOption.OnLoad;
                tmpBitmap.UriSource = new Uri(imgPath, UriKind.RelativeOrAbsolute);                
                tmpBitmap.EndInit();
                tmpBitmap.Freeze();
            }
            catch (Exception excep)
            {
                //If we can't load the image, display a backup image instead.
                tmpBitmap = new BitmapImage(
                    new Uri(BackupImage, UriKind.RelativeOrAbsolute));                
            }

            //Allow image to be loaded in a background thread.
            tmpBitmap.Freeze();
            return tmpBitmap;
        }


        public static void ReleaseImgBitmaps(List<Image_Rep> imageList)
        {
            //This method releases all image bitmaps from memory.
            //todo we're not releasing memory of the bitmap image correctly
            //todo perhaps use a Virtualizing Wrap Panel to display the images
            //properly ...
            //see C:\software development\Code Directory\Little Apps\image scroller\virtuallist1
            
            //Release bitmap/image-source from memory for every image.
            foreach (Image_Rep img in imageList)
            {
                img.scaled_image_src = null;
            }
            imageList.Clear();
        }


        public static int GetNumImagesInDay(int userID, DateTime day)
        {
            int numImages = 0;
            string query = Database_Versioning.text_for_stored_procedures.spGet_Num_Images_In_Day(
                userID,
                day);
            SQLiteConnection con = new SQLiteConnection(DbString);
            SQLiteCommand selectCmd = new SQLiteCommand(query, con);
            con.Open();
            try
            {
                numImages = int.Parse(selectCmd.ExecuteScalar().ToString());
            }
            catch (Exception excep) { }
            con.Close();
            return numImages;
        }
        
        
        public static List<Image_Rep> GetEventImages(int userID, int eventID)
        {
            List<Image_Rep> imageList = new List<Image_Rep>();

            //Image properties.
            int counter = 0;
            DateTime imgTime;
            String imgPath;

            //Get images from database for event.
            string query = Database_Versioning.text_for_stored_procedures.spGet_Paths_Of_All_Images_In_Event(
                userID,
                eventID);
            SQLiteConnection con = new SQLiteConnection(DbString);
            SQLiteCommand selectCmd = new SQLiteCommand(query, con);
            con.Open();
            SQLiteDataReader read_images = selectCmd.ExecuteReader();
            while (read_images.Read())
            {
                imgTime = DateTime.Parse(read_images[0].ToString());
                imgPath = read_images[1].ToString();
                imageList.Add(new Image_Rep(imgTime, imgPath, counter));
                counter++;
            }
            con.Close();
            return imageList;
        }
        
        
        public static void GetDayStartEndTime(
            int userID,
            DateTime day,
            ref DateTime startTime,
            ref DateTime endTime)
        {
            //Get the start and end times of images captured in a given day.
            //This method returns void and updates two parameters by reference.
            string query = Database_Versioning.text_for_stored_procedures.spGet_Day_Start_and_End_Times(
                userID,
                day);
            SQLiteConnection con = new SQLiteConnection(DbString);
            SQLiteCommand command = new SQLiteCommand(query, con);
            con.Open();
            SQLiteDataReader readTimes = command.ExecuteReader();
            readTimes.Read();
            try
            {
                startTime = DateTime.Parse(readTimes[0].ToString());
                endTime = DateTime.Parse(readTimes[1].ToString());
            }
            catch (Exception excep) { }
            con.Close();
        }
        

        public static void DeleteEventImage(
            int userID,
            int eventID,
            DateTime imgTime,
            string imgPath)
        {
            //This method deletes an image from the database and from the PC.
            string query = Database_Versioning.text_for_stored_procedures.spDelete_Image_From_Event(
                userID,
                eventID,
                imgTime);
            SQLiteConnection con = new SQLiteConnection(DbString);
            SQLiteCommand selectCmd = new SQLiteCommand(query, con);
            con.Open();
            selectCmd.ExecuteNonQuery();
            con.Close();

            //Also delete the actual image file too
            System.IO.File.Delete(imgPath);
        }
        
        
        public static void sortImagesByID(List<Image_Rep> imageList)
        {
            //This method uses the BubbleSort algorithm to sort images by ID
            int i;
            int j;
            Image_Rep temp = new Image_Rep(DateTime.Now,"",-99);

            for (i = (imageList.Count - 1); i >= 0; i--)
            {
                for (j = 1; j <= i; j++)
                {
                    if (imageList[j - 1].array_position_in_event > imageList[j].array_position_in_event)
                    {
                        //swap the 2 images if they are in the wrong order
                        temp.array_position_in_event = imageList[j - 1].array_position_in_event;
                        temp.image_path = imageList[j - 1].image_path;
                        temp.image_time = imageList[j - 1].image_time;
                        temp.scaled_image_src = imageList[j - 1].scaled_image_src;
                        temp.short_image_time = imageList[j - 1].short_image_time;

                        imageList[j - 1].array_position_in_event = imageList[j].array_position_in_event;
                        imageList[j - 1].image_path = imageList[j].image_path;
                        imageList[j - 1].image_time = imageList[j].image_time;
                        imageList[j - 1].scaled_image_src = imageList[j].scaled_image_src;
                        imageList[j - 1].short_image_time = imageList[j].short_image_time;

                        imageList[j].array_position_in_event = temp.array_position_in_event;
                        imageList[j].image_path = temp.image_path;
                        imageList[j].image_time = temp.image_time;
                        imageList[j].scaled_image_src = temp.scaled_image_src;
                        imageList[j].short_image_time = temp.short_image_time;
                    }
                }
            }
        }
        
    }
}