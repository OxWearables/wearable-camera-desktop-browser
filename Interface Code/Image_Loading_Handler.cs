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
        private bool isKeyframes;  //true=events, false=images
        private int loadingId;
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        //so the callbacks are important to issue the output of images loaded into memory...
        // Delegate that defines the signature for the callback methods.
        public delegate void All_Event_Images_Loaded_Callback();
        public delegate void Progress_Callback();
        // Delegate used to execute the callback method when the task is complete.
        private All_Event_Images_Loaded_Callback all_images_loaded_callback;
        private Progress_Callback some_images_loaded_callback;
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
        public Image_Loading_Handler(
                bool isKeyframes, //true=events, false=images
                int loadingId,
                All_Event_Images_Loaded_Callback param_images_loaded_callback,
                Progress_Callback param_some_images_loaded_callback)
        {
            this.isKeyframes = isKeyframes; //true=events, false=images
            this.loadingId = loadingId;
            this.all_images_loaded_callback = param_images_loaded_callback;
            this.some_images_loaded_callback = param_some_images_loaded_callback;
        } //close constructor()...




        /// <summary>
        /// this method is responsible for loading all the event's images into memory and then sending the list back via a callback...
        /// </summary>
        public void loadImageBitmaps()
        {
            //firstly wait so the the UI thread can display placeholder info
            System.Threading.Thread.Sleep(IMG_LOADING_WAIT_MS);
            double progressRate = 0.25; //% progress to report back to UI

            //get info on images bitmaps to load
            int overallCount;
            if (isKeyframes) {
                overallCount = Event_Rep.EventList.Count;
            } else {
                overallCount = Image_Rep.ImageList.Count;
            }
            int progressCount = (int)(overallCount * progressRate);
            int counter = 0;
            
            //then go through each image, and attempt to load its bitmap
            for(int c=0; c<overallCount; c++)
            {
                //check that the UI thread is still happy to accept bitmap updates
                if (!isKeyframes && loadingId == Image_Rep.imageLoadingId && Image_Rep.ImageList.Count>0) {
                    Image_Rep.ImageList[c].loadImage();
                }
                else if (isKeyframes && loadingId == Event_Rep.eventLoadingId && Event_Rep.EventList.Count > 0)
                {
                    Event_Rep.EventList[c].loadImage();
                } else {
                    break;
                }

                //send periodic progress updates
                counter++;
                if (counter % (progressCount+1) == 0) { //+1 to avoid /0 error
                    some_images_loaded_callback();
                    //allow UI chance to process progress update
                    System.Threading.Thread.Sleep(IMG_LOADING_WAIT_MS);
                }
            }

            //return final callback to UI (if it is still happy to accept updates)
            if ((!isKeyframes && loadingId == Image_Rep.imageLoadingId) ||
                (isKeyframes && loadingId == Event_Rep.eventLoadingId)) {
                all_images_loaded_callback();
            }
        }

        

    } //end class...

} //end namespace...
