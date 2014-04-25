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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;

namespace SenseCamBrowser1
{
    /// <summary>
    /// Interaction logic for ImgViewer.xaml
    /// </summary>
    public partial class ImgViewer : UserControl
    {

        

        #region properties and initially setting up this user control (includes speed of image playback)

            private int current_userID; //stores the current user id
            private Event_Rep current_event; //stores the current event
            private List<Image_Rep> list_of_event_images; //stores all the images in this event
            private int array_position_of_current_image; //the position in the list of images for this event that we're currently showing
            private DispatcherTimer update_image_timer; //a timer used for a play pause button
            private string original_uploaded_comment;
            private List<Image_Rep> list_of_images_to_delete; //stores a list of the images to delete...
            private int SPEED_SLIDER_DEFAULT_VALUE; //to determine the slowest base speed at which images are played...

            ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
            ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
            ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
            //so the callbacks are important to issue whether an event has been updated or deleted in someway
            // Delegate that defines the signature for the callback methods.
            public delegate void Event_Updated_Callback();
            public delegate void Images_Moved_Between_Events_Callback();
            public delegate void Event_Deleted_Callback(Event_Rep deleted_event);
            // Delegate used to execute the callback method when the task is complete.
            private Event_Updated_Callback current_event_updated_callback;
            private Event_Deleted_Callback current_event_deleted_callback;
            private Images_Moved_Between_Events_Callback current_event_merged_callback;
            ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
            ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
            ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
            

            public ImgViewer()
            {
                InitializeComponent();
                SPEED_SLIDER_DEFAULT_VALUE = 950;

                //credit note: ... http://weblogs.asp.net/psheriff/archive/2012/07/23/wpf-tree-view-with-multiple-levels.aspx was very helpful for a syntax template...
                lst_event_concept_types.ItemsSource = Annotation_Rep.get_list_of_annotation_types().FirstGeneration[0].Children;

                annotation_editor.prepare_annotation_type_tool(New_Annotation_Types_To_Display_Callback);

                //and let's start our timer
                update_image_timer = new DispatcherTimer();
                update_image_timer.Interval = TimeSpan.FromMilliseconds(SPEED_SLIDER_DEFAULT_VALUE/2); //playback speed, with gap between images in milliseconds
                update_image_timer.Tick += new EventHandler(updateImageTimer_Tick);
                update_image_timer.Stop();
            } //close constructor()...

        #endregion properties and initially setting up this user control (includes speed of image playback)






        #region code to receive and update information on the event to display...

            /// <summary>
            /// this method is called by the main screen just before showing this user control, so as to update this control with information on the relevant event
            /// </summary>
            /// <param name="param_userID"></param>
            /// <param name="param_event"></param>
            public void update_event_on_display(int param_userID, Event_Rep param_event, Event_Updated_Callback param_comment_callback, Event_Deleted_Callback param_event_deleted_callback, Images_Moved_Between_Events_Callback param_event_merged_callback)
            {
                //firstly update the image viewer properties for this event...
                current_userID = param_userID;
                current_event = param_event;
                current_event_updated_callback = param_comment_callback;
                current_event_deleted_callback = param_event_deleted_callback;
                current_event_merged_callback = param_event_merged_callback;
                txt_img_date.Text = param_event.startTime.ToLongDateString();
                TimeSpan event_duration = param_event.eventDuration;
                txt_event_length.Text = event_duration.Hours + "h " + event_duration.Minutes + "m " + event_duration.Seconds + "s";

                //next, we'll update a list of any images for deletion (after being selected by the user)
                list_of_images_to_delete = new List<Image_Rep>(); //let's refresh this list for the new event being displayed...
                btnUndo_Delete.Visibility = Visibility.Hidden;

                //update the UI to show the list of annotations associated with this event...
                update_UI_with_list_of_annotations(param_userID, param_event.eventID);
                
                
                //thereafter we'll update the caption text
                if (current_event.comment.Equals(""))
                    txtCaption.Text = Event_Rep.DefaultImageCaption;
                else txtCaption.Text = current_event.comment;
                original_uploaded_comment = txtCaption.Text; //let's store this locally too, and it'll let us know if we should update the database when the user closes the event viewer (i.e. we check to see if they changed the caption)
                

                //it usually take a while to load all the images in an event, so we'll initially just show the keyframe image and a loading message...
                list_of_event_images = new List<Image_Rep>() { new Image_Rep(current_event.startTime, current_event.keyframePath, 0) }; //the keyframe image...
                update_UI_based_on_newly_loaded_images();
                

                //and now let's start loading all the images for this event
                Start_Loading_Event_Images();
                                
            } //close method update_event_on_display()...

            /// <summary>
            /// this method updates the UI with the list of annotations associated with this event in the main DB...
            /// </summary>
            /// <param name="userID"></param>
            /// <param name="eventID"></param>
            private void update_UI_with_list_of_annotations(int userID, int eventID)
            {
                lst_episode_codings_in_database.ItemsSource = Annotation_Rep.get_event_prior_annotations(userID, eventID);
                /*
                txt_priorAnnotations.Text = "";
                List<Annotation_Rep> prior_annotations = Annotation_Rep.get_event_prior_annotations(userID, eventID);
                foreach (Annotation_Rep individual_annotation in prior_annotations)
                    txt_priorAnnotations.Text += individual_annotation.annotation_type + ", ";
                 */ 
            } //close method update_UI_with_list_of_annotations...



            /// <summary>
            /// this method saves back to the database any new comment made for this event
            /// </summary>
            private void save_current_event_information()
            {                
                //firstly let's update the database reflecting that this event has been viewed another time...
                if (list_of_event_images.Count > 1)
                    Event_Rep.UpdateTimesViewed(Window1.OVERALL_userID, current_event.eventID);

                if (!txtCaption.Text.Equals("") && !txtCaption.Text.Equals(Event_Rep.DefaultImageCaption) && !txtCaption.Text.Equals(original_uploaded_comment))
                {
                    //and now we update the database with the comment
                    Event_Rep.UpdateComment(current_userID, current_event.eventID, txtCaption.Text);

                    //and let's update the caption text...
                    current_event.comment = txtCaption.Text;
                    current_event.shortComment = Event_Rep.GetStringStart(current_event.comment, 15);

                    //and let's give a callback to the main UI, so it can now also reflect this comment being viewed on it
                    current_event_updated_callback();
                } //close if (!txtCaption.Text.Equals("") && !txtCaption.Text.Equals(Event_Rep.DEFAULT_IMAGE_CAPTION_TEXT))...


                //then let's go through all the images (if any) that we want to delete...
                foreach (Image_Rep image_for_deletion in list_of_images_to_delete)
                {
                    //and we now delete this image from the database...
                    Image_Rep.DeleteEventImage(Window1.OVERALL_userID, current_event.eventID, image_for_deletion.imgTime, image_for_deletion.imgPath);

                    //but just in case this image was the keyframe image ...
                    if (image_for_deletion.imgPath.Equals(current_event.keyframePath)) //we'll update the event keyframe image ...
                    {
                        Event_Rep.UpdateKeyframe(Window1.OVERALL_userID, current_event.eventID, list_of_event_images[list_of_event_images.Count / 2].imgPath); //... to now be the middle image from the new set of images in this event (minus the deleted ones)
                        current_event.keyframePath = list_of_event_images[list_of_event_images.Count / 2].imgPath;
                        current_event.keyframeSource = Image_Rep.GetImgBitmap(current_event.keyframePath, true);
                        //and let's give a callback to the main UI, so it can now also reflect the new keyframe for this event...                        
                        current_event_updated_callback();
                    } //close if (image_for_deletion.image_path.Equals(current_event.str_keyframe_path)) //we'll update the event keyframe image ...
                    // the above 2 lines will only be called once, as the "list_of_event_images" has already been updated with the removed images, so it'll give the same answer with each individual deletion here...

                } //close foreach (Image_Rep image_for_deletion in list_of_images_to_delete)...                
            } //close method save_current_event_information()...




            /// <summary>
            /// this method is called when the keyboard control signals that a new comment has been made for this event
            /// </summary>
            /// <param name="comment_made"></param>
            public void Comment_Updated_Callback(string comment_made)
            {
                txtCaption.Text = comment_made;
            } //Heart_Rate_Sensor_New_Data_Received_Callback()...





        #endregion code to receive and update information on the event to display...






        #region this region is all the code needed to integrate the image loading thread into the Image Viewer UI

        private Thread image_loading_thread; //this thread is used to read the rss feeds...
        private Image_Loading_Handler image_loading_handler_obj; //this is our class which is responsible for retrieving RSS items...

        /// <summary>
        /// This method is responsible for starting the thread to read the RSS feed values...
        /// </summary>
        private void Start_Loading_Event_Images()
        {
            image_loading_handler_obj = new Image_Loading_Handler(Window1.OVERALL_userID, current_event.eventID, Event_Images_Loaded_Callback);
            image_loading_thread = new Thread(new ThreadStart(image_loading_handler_obj.load_all_event_images_into_memory));
            image_loading_thread.IsBackground = true;
            image_loading_thread.Start();
            
            //now let's disable some of the controls while the images are loading...
            txtPlease_Wait.Visibility = Visibility.Visible;
            PlayBtn.IsEnabled = false;
            NextBtn.IsEnabled = false;
            PreviousBtn.IsEnabled = false;
            btnDelete.IsEnabled = false;
            txtCaption.Visibility = Visibility.Hidden;
            
            
            //also let's visual feedback that these controls are disabled
            PlayBtn.Opacity = 0.3;
            NextBtn.Opacity = 0.3;
            PreviousBtn.Opacity = 0.3;
            btnDelete.Opacity = 0.3;
            
        } //close method Open_Heart_Rate_Sensor_Thread()...


        public void Event_Images_Loaded_Callback()
        {
            
            //firstly we update the list of images belonging to this class (it's ok to do this, since we don't need to interfere with the UI thread)
            list_of_event_images = Image_Rep.ImageList;
            
            //next we'll want to update the UI, so we invoke a delegate
            this.Dispatcher.BeginInvoke(new update_UI_based_on_newly_loaded_images_Delegate(update_UI_based_on_newly_loaded_images)); //invoke the delegate which calls the method to allow the user exit the application again...
                        
        } //close method Event_Images_Loaded_Callback(()...


        /// <summary>
        /// this method is invoked via a delegate to update the Image viewer UI based on when all of the events images have been loaded into memory...
        /// </summary>
        public void update_UI_based_on_newly_loaded_images()
        {
            //now let's re-enable some of the controls after the images have loaded...
            txtPlease_Wait.Visibility = Visibility.Hidden;            
            //PlayBtn.IsEnabled = true;
            //NextBtn.IsEnabled = true;
            //PreviousBtn.IsEnabled = true;
            btnDelete.IsEnabled = true;
            txtCaption.Visibility = Visibility.Visible;
            
            //also let's give visual feedback that these controls are enabled
            PlayBtn.Opacity = 1;
            NextBtn.Opacity = 1;
            PreviousBtn.Opacity = 1;
            btnDelete.Opacity = 1;

            
            //let's update the slider, which gives feedback on how far through the movie person is...
            //EventPlaySlider.Minimum = 0;
            //EventPlaySlider.Maximum = list_of_event_images.Count - 1;
            //EventPlaySlider.LargeChange = (list_of_event_images.Count - 1) / 10;
            //EventPlaySlider.Visibility = Visibility.Visible;

            
            //and now let's show the first image in the event
            array_position_of_current_image = 0;
            update_display_image();

            //here we toggle the playback timer...
            //start_playback();
            //if (update_image_timer.IsEnabled)
              //  stop_playback();
            //else start_playback();

            //set_to_movie_mode();
            set_to_image_wall_mode();
            //start_playback();
        } //close method update_UI_based_on_newly_loaded_images()...
        private delegate void update_UI_based_on_newly_loaded_images_Delegate(); //and a delegate for the above method must be called if we want to update the UI, hence we declare this line

        #endregion this region is all the code needed to integrate the image loading thread into the Image Viewer UI






        #region this region is to deal with all the code needed to change the list of annotation types...


        /// <summary>
        /// this method is used to call up the annotation types editor, to allow researchers construct a relevant set of concepts that they're interested in working with...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEditConceptType_Click(object sender, RoutedEventArgs e)
        {            
            annotation_editor.Visibility = Visibility.Visible;

            //let's log this interaction
            Record_User_Interactions.log_interaction_to_database("scImgViewer_EditConceptType_Click", "");
        } //close method btnEditConceptType_Click()...


        public void New_Annotation_Types_To_Display_Callback()
        {

            //next we'll want to update the UI, so we invoke a delegate
            this.Dispatcher.BeginInvoke(new update_UI_based_on_newly_loaded_annotation_types_Delegate(update_UI_based_on_newly_loaded_annotation_types)); //invoke the delegate which calls the method to update the UI with info on the list of annotations...

        } //close method New_Annotation_Types_To_Display_Callback(()...


        /// <summary>
        /// this method is invoked via a delegate to update the Image viewer UI with the new list of event type annotations possible...
        /// </summary>
        public void update_UI_based_on_newly_loaded_annotation_types()
        {
            lst_event_concept_types.ItemsSource = Annotation_Rep.get_list_of_annotation_types().FirstGeneration[0].Children;
        } //close method update_UI_based_on_newly_loaded_annotation_types()...
        private delegate void update_UI_based_on_newly_loaded_annotation_types_Delegate(); //and a delegate for the above method must be called if we want to update the UI, hence we declare this line


        #endregion this region is to deal with all the code needed to change the list of annotation types...






        #region interface button code




        private void btnToggle_Movie_Wall_View_Click(object sender, RoutedEventArgs e)
        {
            if (PlayBtn.IsEnabled)
            {
                //let's log this interaction
                Record_User_Interactions.log_interaction_to_database("scImgViewer_btnToggleMovieWallView_Click", "set_to_image_wall_mode");
                set_to_image_wall_mode();
            } //close if (PlayBtn.IsEnabled)...
            else
            {
                //let's log this interaction
                Record_User_Interactions.log_interaction_to_database("scImgViewer_btnToggleMovieWallView_Click", "set_to_movie_mode");
                set_to_movie_mode();
            } //close else ... if (PlayBtn.IsEnabled)...
            
        } //close method btnToggle_Movie_Wall_View_Click()...

        private void set_to_movie_mode()
        {
            if (list_of_event_images.Count > 0)
            {

                if (lst_display_images.SelectedIndex != -1)
                    array_position_of_current_image = lst_display_images.SelectedIndex;
                else array_position_of_current_image = 0;

                lst_display_images.Visibility = Visibility.Collapsed;
                img_to_show.Visibility = Visibility.Visible;

                btnSplit_Event.Visibility = Visibility.Collapsed;
                btnMerge_With_Previous.Visibility = Visibility.Collapsed;
                btnMerge_With_Next.Visibility = Visibility.Collapsed;

                btnDelete.Visibility = Visibility.Visible;
                stop_playback();
                PlayBtn.Visibility = Visibility.Visible;
                PlayBtn.IsEnabled = true;
                NextBtn.Visibility = Visibility.Visible;
                PreviousBtn.Visibility = Visibility.Visible;
                EventPlaySlider.Minimum = 0;
                EventPlaySlider.Maximum = list_of_event_images.Count - 1;
                EventPlaySlider.LargeChange = (list_of_event_images.Count - 1) / 10;
                EventPlaySlider.Value = array_position_of_current_image;
                EventPlaySlider.Visibility = Visibility.Visible;

                EventZoomSlider.Value = 375;
                EventZoomSlider.Visibility = Visibility.Collapsed;

                //and this one will be to set the visibility of the speed slider control (which can be used in image playback mode)
                SpeedSlider.Visibility = Visibility.Visible;
                minus_icon.Visibility = Visibility.Visible;
                plus_icon.Visibility = Visibility.Visible;
                lblSpeed.Visibility = Visibility.Visible;

                lst_display_images.ItemsSource = null;
                lst_display_images.Items.Clear();
                lst_display_images.Items.Add(list_of_event_images[array_position_of_current_image]);
                update_display_image();

            } //close if (list_of_event_images.Count > 0)..
        } //close method set_to_movie_mode()...


        private void set_to_image_wall_mode()
        {
            lst_display_images.Visibility = Visibility.Visible;
            img_to_show.Visibility = Visibility.Collapsed;

            btnSplit_Event.Visibility = Visibility.Visible;
            btnMerge_With_Previous.Visibility = Visibility.Visible;
            btnMerge_With_Next.Visibility = Visibility.Visible;

            btnDelete.Visibility = Visibility.Collapsed;
            stop_playback();
            PlayBtn.Visibility = Visibility.Collapsed;
            PlayBtn.IsEnabled = false;
            NextBtn.Visibility = Visibility.Collapsed;
            PreviousBtn.Visibility = Visibility.Collapsed;

            EventZoomSlider.Minimum = 80;
            EventZoomSlider.Maximum = 640;
            EventZoomSlider.Value = 160;
            EventZoomSlider.Visibility = Visibility.Visible;
            EventPlaySlider.Visibility = Visibility.Collapsed;

            //and this one will be to set the visibility of the speed slider control (which can be used in image playback mode)
            SpeedSlider.Visibility = Visibility.Collapsed;
            minus_icon.Visibility = Visibility.Collapsed;
            plus_icon.Visibility = Visibility.Collapsed;
            lblSpeed.Visibility = Visibility.Collapsed;

            lst_display_images.ItemsSource = null;
            lst_display_images.Items.Clear();
            lst_display_images.ItemsSource = list_of_event_images;            
            lst_display_images.SelectedIndex = array_position_of_current_image;            
            //update_display_image();
        } //close set_to_image_wall_mode()...


        /// <summary>
        /// this method closes the event playback viewer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close1_Click(object sender, RoutedEventArgs e)
        {
            stop_playback(); //let's disable the timer, so there's no "leaked threads" running

            //let's log this interaction
            play_sound();
            Record_User_Interactions.log_interaction_to_database("scImgViewer_Close1_Click", current_event.eventID.ToString());
            
            save_current_event_information(); //let's save event information
            close_viewer_control();
        } //close method Close1_Click()



        private void close_viewer_control()
        {
            lst_display_images.ItemsSource = null;            
            Image_Rep.ReleaseImgBitmaps(list_of_event_images);

            Visibility = Visibility.Collapsed; //and let's close/collapse this user control                        
        } //close method close_viewer_control()...




        /// <summary>
        /// This method is called when the user wants to see the previous image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviousBtn_Click(object sender, RoutedEventArgs e)
        {
            stop_playback(); //in case we're playing through the images, let's stop the player

            //let's log this interaction
            play_sound();
            Record_User_Interactions.log_interaction_to_database("scImgViewer_PreviousBtn_Click", current_event.eventID.ToString() + "," + array_position_of_current_image);
                       

            //then let's move back 1 image
            if (array_position_of_current_image > 0)
                array_position_of_current_image--;

            //and finally update the UI to show this image
            update_display_image();                        
        } //close method PreviousBtn_Click()...




        /// <summary>
        /// This method is called when the user wants to see the next image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            stop_playback(); //in case we're playing through the images, let's stop the player

            //let's log this interaction
            play_sound();
            Record_User_Interactions.log_interaction_to_database("scImgViewer_NextBtn_Click", current_event.eventID.ToString() + "," + list_of_event_images[array_position_of_current_image].imgPath);
                       

            //then let's move forward 1 image
            if (array_position_of_current_image < list_of_event_images.Count - 1)
                array_position_of_current_image++;

            //and finally update the UI to show this image
            update_display_image();            
        } //close method NextBtn_Click()...






        /// <summary>
        /// this method is used when the user clicks on the caption overlayed on the images, i.e. if they want to temporarily get rid of it, or show it again ... i.e. toggle it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtCaption_Click(object sender, RoutedEventArgs e)
        {
            //let's log this interaction
            play_sound();
            Record_User_Interactions.log_interaction_to_database("scImgViewer_txtCaption_Click", current_event.eventID.ToString() + "," + list_of_event_images[array_position_of_current_image].imgPath + "," + txtCaption.Text);

            stop_playback(); //let's disable the timer, so there's no "leaked threads" running

            if (txtCaption.Text.Equals(Event_Rep.DefaultImageCaption))
                txtCaption.Text = current_event.comment;
            else
                txtCaption.Text = Event_Rep.DefaultImageCaption;
        } //close method txtCaption_Click()...




        /// <summary>
        /// this method is called by both the play and pause buttons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            //let's log this interaction
            play_sound();
            Record_User_Interactions.log_interaction_to_database("scImgViewer_PlayBtn_Click", current_event.eventID.ToString() + "," + list_of_event_images[array_position_of_current_image].imgPath);

            //here we toggle the playback timer...
            if (update_image_timer.IsEnabled)
                stop_playback();
            else start_playback();
        } //close method PlayBtn_Click()...




        /// <summary>
        /// this method is called to clear the list of images that the user intended for deletion (meaning none will then be deleted from the event in question)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUndo_Delete_Click(object sender, RoutedEventArgs e)
        {
            //let's log this interaction
            play_sound();
            Record_User_Interactions.log_interaction_to_database("scImgViewer_btnUndoDelete_Click", current_event.eventID.ToString() + "," + list_of_event_images[array_position_of_current_image].imgPath);

            //firstly let's restore the deleted images into their
            foreach (Image_Rep restore_image in list_of_images_to_delete)
            {
                if (restore_image.position < list_of_event_images.Count)
                    list_of_event_images.Insert(restore_image.position, restore_image); //we can insert it into any position except the final one...
                else list_of_event_images.Add(restore_image); //here we just insert into the end of the list...
            } //close foreach (Image_Rep restore_image in list_of_images_to_delete)...
            
            //then we clear the list that's being prepared for deletion...
            list_of_images_to_delete.Clear();

            //and now we hide this option again, as we've reset it back that no images will be deleted
            btnUndo_Delete.Visibility = Visibility.Hidden;

            //and now let's update the UI to reflect we've undo-ed these images that were originally set aside for deletion...
            update_display_image();
        } //close method btnUndo_Delete_Click()...




        /// <summary>
        /// This method is called when the delete button is clicked, and deletes the current image from the event (and updates the database accordingly too), if there's just 1 image left in the event, the entire event is deleted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            stop_playback(); //in case we're playing through the images, let's stop the player                

            //let's log this interaction
            play_sound();
            Record_User_Interactions.log_interaction_to_database("scImgViewer_btnDelete_click", current_event.eventID + "," + list_of_event_images[array_position_of_current_image].imgPath);


            //firstly let's store this image in a List of images that we'd like to delete... later this will be deleted from the database along with other (selected) images from this event
            list_of_images_to_delete.Add(list_of_event_images[array_position_of_current_image]);


            //next we'll remove this image from its position in the array...
            list_of_event_images.RemoveAt(array_position_of_current_image);

            //also in case we've deleted the final image in the event, let's update the position index (to avoid an error being thrown)
            if (array_position_of_current_image >= list_of_event_images.Count)
                array_position_of_current_image--;


            //here we'll then update the UI to reflect this deleted image
            if (list_of_event_images.Count != 0) //if there's still images left...
                update_display_image(); //we'll update the image viewer UI to reflect this image is no longer needed
            else
            {
                //let's log this interaction
                play_sound();
                Record_User_Interactions.log_interaction_to_database("scImgViewer_btnDelete_click_last_image_in_event_deleted", current_event.eventID.ToString());

                //else ... if there's no images left in this event after it being deleted, we'll delete this event from the database and close the image viewer for this event (since the event has now been deleted)
                Event_Rep.DeleteEvent(Window1.OVERALL_userID, current_event.eventID);
                current_event_deleted_callback(current_event);
                Visibility = Visibility.Collapsed; //and let's close/collapse this user control                                        
            } //end if... else... for checking if there's any images left in the event


            btnUndo_Delete.Visibility = Visibility.Visible; //and also we'll then give the user the option to unto any deletions...
        } //close method btnDelete_Click()...






        private void btnCopy_Images_to_Clipboard_Click(object sender, RoutedEventArgs e)
        {
            if (list_of_event_images.Count > 0)
            {
                if (PlayBtn.IsEnabled)
                {
                    stop_playback(); //in case we're playing through the images, let's stop the player                
                    Clipboard.SetImage(Image_Rep.GetImgBitmap(list_of_event_images[array_position_of_current_image].imgPath, false));
                }
                else Clipboard.SetImage(Image_Rep.GetImgBitmap(list_of_event_images[lst_display_images.SelectedIndex].imgPath, false));

                //let's log this interaction
                play_sound();
                Record_User_Interactions.log_interaction_to_database("btnCopy_Images_to_Clipboard_Click", current_event.eventID + "," + list_of_event_images[array_position_of_current_image].imgPath);

            } //close if (list_of_event_images.Count > 0)...
            
        } //close method btnCopy_Images_to_Clipboard_Click()...



        /// <summary>
        /// This method merges all the start images (unto, and including, the selected one), with the previous event...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMerge_With_Previous_Click(object sender, RoutedEventArgs e)
        {
            if (lst_display_images.SelectedIndex != -1)
            {
                //this is our normal case where all the start images (unto, and including, the selected one), are merged with the previous event...
                Image_Rep selected_photo_in_event = (Image_Rep)lst_display_images.SelectedItem;

                //the database stored procedure also considers special cases where *all* the images in the event are merged with the previous event... it just simply deletes this source event
                //call this method to execute the database stored procedure to handle rearranging of images into different events
                Event_Rep.MoveImagesToPreviousEvent(Window1.OVERALL_userID, current_event.eventID, selected_photo_in_event.imgTime);

                //now what I'll need to do is close the current window, and then reload the day (to reflect the new images/events situation after clicking on this button)
                current_event_merged_callback();
                close_viewer_control(); //and then close the current viewer, to give feedback to user that the change has happened

                //let's log this interaction
                Record_User_Interactions.log_interaction_to_database("scImgViewer_btnMerge_With_Previous_Click", current_event.eventID + "," + selected_photo_in_event.imgTime);
            } //close if (lst_display_images.SelectedIndex != -1)                
            else MessageBox.Show("Only available in image wall view. Please select a boundary image to group in with the previous set of images");

        } //close method btnMerge_With_Previous_Click()...



        /// <summary>
        /// This method merges all the end images (after, and including, the selected one), with the next event...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMerge_With_Next_Click(object sender, RoutedEventArgs e)
        {
            if (lst_display_images.SelectedIndex != -1)
            {
                //this is our normal case where all the end images (after, and including, the selected one), are merged with the next event...
                Image_Rep selected_photo_in_event = (Image_Rep)lst_display_images.SelectedItem;

                //the database stored procedure also considers special cases where *all* the images in the event are merged with the next event... if just simply deletes this source event
                //call this method to execute the database stored procedure to handle rearranging of images into different events
                Event_Rep.MoveImagesToNextEvent(Window1.OVERALL_userID, current_event.eventID, selected_photo_in_event.imgTime);

                //now what I'll need to do is close the current window, and then reload the day (to reflect the new images/events situation after clicking on this button)
                current_event_merged_callback();
                close_viewer_control(); //and then close the current viewer, to give feedback to user that the change has happened

                //let's log this interaction
                Record_User_Interactions.log_interaction_to_database("scImgViewer_btnMerge_With_Next_Click", current_event.eventID + "," + selected_photo_in_event.imgTime);
            } //close if (lst_display_images.SelectedIndex != -1)                
            else MessageBox.Show("Only available in image wall view. Please select a boundary image to group in with the next set of images");
        } //close method btnMerge_With_Next_Click()...





        /// <summary>
        /// this method splits an event into two...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSplit_Event_Click(object sender, RoutedEventArgs e)
        {
                        
            if (lst_display_images.SelectedItems.Count > 1)
            {
                int current_eventID = current_event.eventID;

                List<Image_Rep> selected_images_for_splitting = lst_display_images.SelectedItems.Cast<Image_Rep>().ToList();
                Image_Rep.sortImagesByID(selected_images_for_splitting);

                foreach (Image_Rep selected_image_to_start_new_event in selected_images_for_splitting)
                {    
                    if (selected_image_to_start_new_event.position > 0 && selected_image_to_start_new_event.position < lst_display_images.Items.Count - 1)
                    {
                        //call this method to execute the database stored procedure to handle splitting the event into two
                        current_eventID = Event_Rep.SplitEvent(Window1.OVERALL_userID, current_eventID, selected_image_to_start_new_event.imgTime); //current event id will be updated to be the new split event... (i.e. so next pass of loop treats it like we're now in this new event and trying to split the first image in it...)

                        //let's log this interaction
                        Record_User_Interactions.log_interaction_to_database("scImgViewer_btnSplit_Event_Click_splitting_multiple", current_event.eventID + "," + selected_image_to_start_new_event.imgTime);
                    }
                    else Record_User_Interactions.log_interaction_to_database("scImgViewer_btnSplit_Event_Click_splitting_multiple_first_last_images", current_event.eventID + "," + selected_image_to_start_new_event.imgTime);
                }

                //now what I'll need to do is close the current window, and then reload the day (to reflect the new images/events situation after clicking on this button)
                current_event_merged_callback();
                close_viewer_control(); //and then close the current viewer, to give feedback to user that the change has happened
            } //close for case when multiple items were selected
            
            
            else if (lst_display_images.SelectedIndex >0 && lst_display_images.SelectedIndex < lst_display_images.Items.Count-1) //no point in trying to split an event when person selects first image (as it puts the current image and all others after it into the new event...)
            {
                //this is our normal case where the event is split into two ... all imagee after, and including, the selected one, are merged with the next event...
                Image_Rep selected_photo_in_event = (Image_Rep)lst_display_images.SelectedItem;

                //call this method to execute the database stored procedure to handle splitting the event into two
                Event_Rep.SplitEvent(Window1.OVERALL_userID, current_event.eventID, selected_photo_in_event.imgTime);

                //now what I'll need to do is close the current window, and then reload the day (to reflect the new images/events situation after clicking on this button)
                current_event_merged_callback();
                close_viewer_control(); //and then close the current viewer, to give feedback to user that the change has happened

                //let's log this interaction
                Record_User_Interactions.log_interaction_to_database("scImgViewer_btnSplit_Event_Click", current_event.eventID + "," + selected_photo_in_event.imgTime);
            } //close if (lst_display_images.SelectedIndex != -1)                
            else MessageBox.Show("Only available in image wall view. Please select a boundary image (other than the first) to split this event into two");
        } //close method btnSplit_Event_Click()...








        private void btnAdd_Annotations_Click(object sender, RoutedEventArgs e)
        {
            //get the selected items
            //foreach (Annotation_Rep_Tree_Data_Model annotated_item in lst_event_concept_types.SelectedItem)
            if(lst_event_concept_types.SelectedItem != null)
            {
                Annotation_Rep_Tree_Data_Model annotated_item = (Annotation_Rep_Tree_Data_Model) lst_event_concept_types.SelectedItem;
                string database_annotation_entry = Annotation_Rep_Tree_Data.convert_tree_node_to_delimited_string(annotated_item);
                Annotation_Rep.add_event_annotation_to_database(current_userID, current_event.eventID, database_annotation_entry);
                //let's log this interaction
                Record_User_Interactions.log_interaction_to_database("scImgViewer_lst_event_concept_types_PreviewMouseLeftButtonUp", current_event.eventID + "," + database_annotation_entry);
                Thread.Sleep(50); //for some reason I have to introduce this command to allow the annotations be added to the database (2 lines up)
            } //close foreach (string annotated_item in lst_event_concept_types.SelectedItems)...

            //and update the interface to reflect the daily summary based on the new annotations now...
            update_interface_to_reflect_new_annotations();
        }


        
        /// <summary>
        /// this method allows the user to cancel all annotations they made for this event...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Annotations_Click(object sender, RoutedEventArgs e)
        {
            if (lst_episode_codings_in_database.SelectedIndex == -1)
                Annotation_Rep.clear_event_annotations_from_database(current_userID, current_event.eventID); //then delete all prior annotations from database...
            else
            {
                foreach (string annotation_to_delete in lst_episode_codings_in_database.SelectedItems)
                    Annotation_Rep.clear_event_annotations_from_database(current_userID, current_event.eventID, annotation_to_delete);
            } //close if ... else... to delete selected codings from database...

            //visually update the listbox to reflect no items will now be selected...
            lst_episode_codings_in_database.SelectedIndex = -1;
            
            //and update the interface to reflect the daily summary based on the new annotations now...
            update_interface_to_reflect_new_annotations();

            //let's log this interaction
            Record_User_Interactions.log_interaction_to_database("scImgViewer_btnCancel_Annotations_Click", current_event.eventID+"");
        } //close method btnCancel_Annotations_Click()...


        private void update_interface_to_reflect_new_annotations()
        {
            //and update the interface to reflect the daily summary based on the new annotations now...
            update_UI_with_list_of_annotations(current_userID, current_event.eventID);
            current_event_merged_callback();         
        } //close method update_interface_to_reflect_new_annotations()...



        #endregion interface button code






        #region displaying image on UI and play/pause code

        /// <summary>
        /// this method updates the UI to show the image at "array_position_of_current_image" position in the list of images associated with this event
        /// </summary>
        public void update_display_image()
        {
            //thanks to Sam in UCSD who recognised some strange multi-threading behaviour sending phantom calls to call this method
            //which would then mean list_of_event_images was null, therefore we have now added in this first if statement

            if (list_of_event_images != null)
            {
                if (list_of_event_images.Count > 0)
                {

                    txt_image_number.Text = (array_position_of_current_image + 1).ToString() + " of " + list_of_event_images.Count + " photos";
                    txt_img_time.Text = list_of_event_images[array_position_of_current_image].imgTime.ToString("HH:mm tt");

                    //img_to_show.Source = list_of_event_images[array_position_of_current_image].scaled_image_src; //list_of_event_images[array_position_of_current_image].image_source(); //show full size image...
                    if (PlayBtn.IsEnabled)
                    {
                        img_to_show.Source = list_of_event_images[array_position_of_current_image].scaledImgSource;
                        //lst_display_images.ItemsSource = null;
                        //lst_display_images.Items.Clear();
                        //lst_display_images.Items.Add(list_of_event_images[array_position_of_current_image]);
                        EventPlaySlider.Value = array_position_of_current_image; //update slider position
                    }
                    else
                    {
                        lst_display_images.ItemsSource = null;
                        lst_display_images.Items.Clear();
                        lst_display_images.ItemsSource = list_of_event_images;
                    }


                    //let's see if we should have the nextbtn enabled after showing this image...
                    if (array_position_of_current_image < list_of_event_images.Count - 1) { NextBtn.Opacity = 1.0; NextBtn.IsEnabled = true; }
                    else { NextBtn.Opacity = 0.3; NextBtn.IsEnabled = false; }

                    //let's see if we should have the previousbtn enabled after showing this image...
                    if (array_position_of_current_image >= 1) { PreviousBtn.Opacity = 1.0; PreviousBtn.IsEnabled = true; }
                    else { PreviousBtn.Opacity = 0.3; PreviousBtn.IsEnabled = false; }


                    //txt_image_number.Text = (array_position_of_current_image + 1).ToString() + " of " + list_of_event_images.Count + " images";

                    ////img_to_show.Source = list_of_event_images[array_position_of_current_image].image_source(); //show full size image...
                    //lst_display_images.ItemsSource = null;
                    //lst_display_images.Items.Clear();
                    //lst_display_images.Items.Add(list_of_event_images[array_position_of_current_image]);

                    //EventPlaySlider.Value = array_position_of_current_image;

                } //end if(list_of_event_images.Count>0)...
            } //end if(list_of_event_images!=null)
        }//close method update_display_image()...




        /// <summary>
        /// this method updates the timer (called by default 10 times per second)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updateImageTimer_Tick(object sender, EventArgs e)
        {
            if (array_position_of_current_image < list_of_event_images.Count - 1)
                array_position_of_current_image++;
            else stop_playback(); //then we stop the playback if we can't move on any further...
            update_display_image();
        } //close method updateImageTimer_Tick()...




        /// <summary>
        /// This method stops the "playback" mode
        /// </summary>
        private void stop_playback()
        {
            PlayBtn.Visibility = Visibility.Visible;
            PauseBtn.Visibility = Visibility.Hidden;
            update_image_timer.Stop();
        } //close method stop_playback()...




        /// <summary>
        /// this method starts the "playback" mode
        /// </summary>
        private void start_playback()
        {
            PauseBtn.Visibility = Visibility.Visible;
            PlayBtn.Visibility = Visibility.Hidden;

            update_image_timer.Start();
        } //close method start_playback()...




        /// <summary>
        /// this method is called when the "position slidebar" is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EventPlaySlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            //stop_playback();

            ////let's log this interaction
            //play_sound();
            ////Record_User_Interactions.log_interaction_to_database("scImgViewer_EventPlaySlider_PreviewMouseUp", current_event.eventID.ToString() + "," + list_of_event_images[array_position_of_current_image].image_path + "," + list_of_event_images[(int)EventPlaySlider.Value].image_path);
            
            ////array_position_of_current_image = (int)EventPlaySlider.Value;
            //update_display_image();

            stop_playback();
            array_position_of_current_image = (int)EventPlaySlider.Value;

            txt_image_number.Text = (array_position_of_current_image + 1).ToString() + " of " + list_of_event_images.Count + " images";

            //img_to_show.Source = list_of_event_images[array_position_of_current_image].image_source(); //show full size image...
            lst_display_images.ItemsSource = null;
            lst_display_images.Items.Clear();
            lst_display_images.Items.Add(list_of_event_images[array_position_of_current_image]);

            update_display_image();

            //let's log this interaction
            Record_User_Interactions.log_interaction_to_database("scImgViewer_EventPlaySlider_PreviewMouseUp", txt_image_number.Text);
        }//close method EventPlaySlider_PreviewMouseUp()...



        /// <summary>
        /// this method is called when the "speed slidebar" is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SpeedSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            update_image_timer.Interval = TimeSpan.FromMilliseconds(SPEED_SLIDER_DEFAULT_VALUE - (SpeedSlider.Value*90) ); //playback speed, with gap between images in milliseconds
            Record_User_Interactions.log_interaction_to_database("scImgViewer_SpeedSlider_PreviewMouseUp", current_event.eventID.ToString() + "," + update_image_timer.Interval);
        } //close method SpeedSlider_PreviewMouseUp()...




        /// <summary>
        /// this method is called when the user clicks on the plus button on the speed slider...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void plus_icon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //we'll move up in incraments of 10% ... range is 0-10 = 10 ... then 10% of that is 1
            if (SpeedSlider.Value < 9) //i.e. 10-1
                SpeedSlider.Value += 1; //i.e. 10% of range of 0-10...

            //and update the speed...
            update_image_timer.Interval = TimeSpan.FromMilliseconds(SPEED_SLIDER_DEFAULT_VALUE - (SpeedSlider.Value * 90)); //playback speed, with gap between images in milliseconds

            //let's log this interaction
            Record_User_Interactions.log_interaction_to_database("scImgViewer_plus_icon_MouseLeftButtonDown", "increase_speed");
        } //close method plus_icon_MouseLeftButtonDown()...




        /// <summary>
        /// this method is called when the user clicks on the minus button on the speed slider...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void minus_icon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {            
            //we'll move down in incraments of 10% ... range is 0-10 = 10 ... then 10% of that is 1
            if (SpeedSlider.Value > 1) //i.e. 0+1
                SpeedSlider.Value -= 1; //i.e. 10% of range of 0-10...

            //and update the speed...
            update_image_timer.Interval = TimeSpan.FromMilliseconds(SPEED_SLIDER_DEFAULT_VALUE - (SpeedSlider.Value * 90)); //playback speed, with gap between images in milliseconds

            //let's log this interaction
            Record_User_Interactions.log_interaction_to_database("scImgViewer_plus_icon_MouseLeftButtonDown", "decrease_speed");
        } //close method minus_icon_MouseLeftButtonDown()...

                

        #endregion displaying image on UI and play/pause code




        #region methods to support interface controls...

            /// <summary>
            /// when this method is called, a sound will be played
            /// </summary>
            private void play_sound()
            {
                //again better suited towards a touchscreen device I think
                //sound_Click_Sound.Stop();
                //sound_Click_Sound.Play();
            }

            private void UserControl_Loaded(object sender, RoutedEventArgs e)
            {

            }//close method play_sound()...

        #endregion methods to support interface controls...



    }   //end class...
}   //end namespace...
