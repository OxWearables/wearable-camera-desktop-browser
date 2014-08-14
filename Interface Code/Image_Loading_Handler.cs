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

namespace SenseCamBrowser1
{
    class Image_Loading_Handler
    {
        
        private int userID, eventID; //variables we need to store initially, so we know what event to get the image paths from in the database
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        //so the callbacks are important to issue the output of images loaded into memory...
        // Delegate that defines the signature for the callback methods.
        public delegate void All_Event_Images_Loaded_Callback();
        // Delegate used to execute the callback method when the task is complete.
        private All_Event_Images_Loaded_Callback all_images_loaded_callback;
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        private static int IMG_LOADING_WAIT_MS =
                int.Parse(ConfigurationManager.AppSettings["imgLoadingWaitMs"].ToString());
        

        /// <summary>
        /// constructor to get the necessary variables (user/event id's so we know which images to get from the database, and callback to send the loaded image list)
        /// </summary>
        /// <param name="param_userID"></param>
        /// <param name="param_eventID"></param>
        /// <param name="param_images_loaded_callback"></param>
        public Image_Loading_Handler(int param_userID, int param_eventID, All_Event_Images_Loaded_Callback param_images_loaded_callback)
        {
            this.userID = param_userID;
            this.eventID = param_eventID;
            this.all_images_loaded_callback = param_images_loaded_callback;
        } //close constructor()...




        /// <summary>
        /// this method is responsible for loading all the event's images into memory and then sending the list back via a callback...
        /// </summary>
        public void load_all_event_images_into_memory()
        {
            System.Threading.Thread.Sleep(IMG_LOADING_WAIT_MS);
            Image_Rep.ImageList = Image_Rep.GetEventImages(userID, eventID); ; //get all the images in the event

            //and we return our callback (to the UI)
            all_images_loaded_callback();
        } //close method load_all_event_images_into_memory()...

        

    } //end class...

} //end namespace...
