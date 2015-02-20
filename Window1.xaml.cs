/*
Copyright (c) 2013, CLARITY: Centre for Sensor Web Technologies, DCU (Dublin City University)
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of CLARITY: Centre for Sensor Web Technologies, DCU (Dublin City University) nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
 
* ANY RESEARCHERS USING THIS SOFTWARE SHOULD CITE:

 Doherty, Aiden R., Moulin, Chris J.A., and Smeaton, Alan F. (2011) Automatically Assisting Human Memory: A SenseCam Browser., Memory: Special Issue on SenseCam: The Future of Everyday Research? Taylor and Francis, 19(7), 785-795

Bibtex entry
@article{SenseCam_Browser,
author = {Doherty, Aiden R. and Moulin, Chris J. A. and Smeaton, Alan F.},
title = {Automatically assisting human memory: A SenseCam browser},
journal = {Memory},
volume= {19},
number = {7},
pages = {785-795},
year = {2011},
doi = {10.1080/09658211.2010.509732},
URL = {http://www.tandfonline.com/doi/abs/10.1080/09658211.2010.509732},
eprint = {http://www.tandfonline.com/doi/pdf/10.1080/09658211.2010.509732}
}

 * */

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
using System.Windows.Media.Animation;
using System.Management;
using System.Configuration;
using System.IO;
using Microsoft.Win32;
using System.Threading;

namespace SenseCamBrowser1
{
    //todo general list
    /*
     *  list of things to-do:
     *  
     * minor: for multiple folder upload, give exact progress percentage upload (i.e. get estimate of total num images across all subfolders, then start processing and calculating the progress vs. this number)
       minor: double CAM line... (requires a fair rewrite of CSV processing code to handle this...)
     * 
     * 
 - export to excel button (list of annotations on events) ... remember to record it in user annotations table too
 - movie of a day in the life of... (possible to put an Oxford CLARITY watermark on it?) ... remember to record it in user annotations table too
     * SenseCam browser -> play through movie of whole day
     * SenseCam browser -> give explicit instructions on adding in images from USB stick (i.e. copy to local machine at first)
     * Oxford in-house -> write app to share images...
 
 
     * 
  DOCUMENTATION
-add sample day of SenseCam images + pdf of most recent changes to codeplex
- make voice-over (or captioned) video showing browser in operation, and various features available
- make video of the whole process being done for a single day to get the summary for a person...

FAQ item:
how to find where my SenseCam images are stored?

     */



    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        enum Time_Period_On_Display { Morning, Afternoon, Evening } //this is used to help us identify in the UI whether we're showing morning, afternoon, or evening events (useful for pagenation feature)

        #region properties to store the user id and the list of days data available for this user and also the list of events for the morning, afternoon, and evening

            public static int OVERALL_userID = User_Object.OVERALL_userID; //originally was like this -> int.Parse(ConfigurationSettings.AppSettings["userID"].ToString());
            private DateTime current_day_on_display = new DateTime();
        
        #endregion properties to store the user id and the list of days data available for this user and also the list of events for the morning, afternoon, and evening




        /// <summary>
        /// the constructor which is called when the application first loads up...
        /// </summary>
        public Window1()
        {
            //let's firstly get all the components initialised and set up...
            InitializeComponent();

            //let's provide reaffirmation on what user we're showing images for...
            lblUserName.Content = User_Object.OVERALL_USER_NAME;

            //for some reason, the lstDisplayEvents isn't at the top of the layout in XAML code
            //therefore here we programatically bring it to the front
            Canvas.SetZIndex(LstDisplayEvents, 10);

            //initialise the calendar, so that we can use it to select new dates
            calSenseCamDay.initialise_calendar_popup_datecallback(Calendar_Data_Selected_Callback);

            //and now let's show the most recent day's data...
            show_new_day_on_UI(calSenseCamDay.list_of_available_days[0]);
            

            //to enable scrolling of the listbox containing all the events in a day
            double screen_resolution_height = System.Windows.SystemParameters.PrimaryScreenHeight;
            LstDisplayEvents.Height = screen_resolution_height - 230; //for some strange reason I've got to set the height of this listbox so that the automatic scrolling kicks in (can't set the height to "auto" or "*", and I don't know why not)


            //let's log this interaction
            Record_User_Interactions.log_interaction_to_database("Window1_appload", current_day_on_display.ToString());

        } //end Window1()...






        #region displaying a day of information on the UI

            private void show_new_day_on_UI(DateTime day_to_show)
            {
                Event_Rep.eventLoadingId = int.MinValue; //stop "leaked threads" running
                //image_loading_thread.Abort();                

                //firstly update the interface to give info on how many images in the day, the start time, and end time too...
                update_interface_with_information_on_day(day_to_show);

                //and let's update the events on display...
                update_display_page_events(day_to_show);
                
            } //close method show_new_day_on_UI()...

        

            private void update_display_page_events(DateTime day_to_display)
            {
                //an array to store the total list of events to be displayed...
                Event_Rep.EventList = 
                        Event_Rep.GetDayEvents(
                            Window1.OVERALL_userID, day_to_display, false);
                
                // add info to UI on how many events there are in this time period...
                txtEventNumber.Text = Event_Rep.EventList.Count + " Events";

                // make UI display the first page of events
                LstDisplayEvents.ItemsSource = Event_Rep.EventList;
                if (LstDisplayEvents.Items.Count == 0)
                    No_Images.Visibility = Visibility.Visible;
                else
                {                    
                    No_Images.Visibility = Visibility.Hidden;
                    startLoadingImageBitmaps();
                }

                //in the background, start loading all the images for this event
                //startLoadingImageBitmaps();
            } //close method update_display_page_events()...




            /// <summary>
            /// this method is used to update the main screen of the interface to show the event/image information associated with this day
            /// </summary>
            /// <param name="day_to_display"></param>
            private void update_interface_with_information_on_day(DateTime day_to_display)
            {
                //firstly let's update the overall interface variable on what day it is...
                current_day_on_display = day_to_display;
                           
                //let's update the UI with what day and day this data is relating to
                txtDayDisplay.Text = day_to_display.DayOfWeek.ToString();
                txtDateDisplay.Text = day_to_display.ToLongDateString();


                //now display the time span of the events of this day
                DateTime day_startTime = new DateTime();
                DateTime day_endTime = day_startTime;
                Image_Rep.GetDayStartEndTime(Window1.OVERALL_userID, day_to_display, ref day_startTime, ref day_endTime); //firstly we get start/end time data directly from the database

                if (day_startTime != day_endTime) //if the start time isn't equal to the end time, it means that we've successfully retrieved information from the database...
                {
                    txtTimeFrame.Text ="(" + day_startTime.ToString ("HH:mm tt") + " - ";
                    txtTimeFrame.Text += day_endTime.ToString("HH:mm tt") + ")";
                } //close if(day_startTime != day_endTime)...
                else txtTimeFrame.Text = " - "; //if no data has been retrieved, well then we've no information to show...
                
                //and also let's display the number of images associated with this day...
                txtImageNumber.Text = Image_Rep.GetNumImagesInDay(Window1.OVERALL_userID, day_to_display).ToString() + " Photos";


                //finally give a breakdown of the amount of time spent on various activities...
                lstDailyActivitySummary.ItemsSource = Daily_Annotation_Summary.getDayAnnotationSummary(Window1.OVERALL_userID, day_to_display);

                //and also a breakdown of the individual annotated events...
                lstIndividual_Journeys.ItemsSource = Interface_Code.Event_Activity_Annotation.getAnnotatedEventsDay(Window1.OVERALL_userID, day_to_display);
            } //end method display_days_events_on_interface()

        
        #endregion displaying a day of information on the UI



        #region code to integrate image loading thread into the Image Viewer UI
            //thread and handler for image bitmap loading
            private Thread image_loading_thread;
            private Image_Loading_Handler image_loading_handler_obj;

            /// <summary>
            /// This method is responsible for starting the thread to read image bitmaps
            /// </summary>
            private void startLoadingImageBitmaps()
            {
                image_loading_handler_obj = new Image_Loading_Handler(
                        true, //i.e. event keyframes
                        current_day_on_display.DayOfYear,
                        allImagesLoadedCallback,
                        someImagesLoadedCallback);
                image_loading_thread = new Thread(new ThreadStart(
                    image_loading_handler_obj.loadImageBitmaps));
                image_loading_thread.IsBackground = true;
                image_loading_thread.Priority = ThreadPriority.BelowNormal;
                image_loading_thread.Start();
                Event_Rep.eventLoadingId = current_day_on_display.DayOfYear;
            }


            public void allImagesLoadedCallback()
            {
                Event_Rep.eventLoadingId = int.MinValue;
                
                //update UI to say all images are loaded
                //invoke delegate which calls the method to allow the user exit the
                //application again...
                this.Dispatcher.BeginInvoke(
                        new updateUIAfterAllImageLoadUpdate_Delegate(
                                updateUIAfterAllImagesLoaded));
            }

            public void someImagesLoadedCallback()
            {
                //update UI to show some images are loaded
                this.Dispatcher.BeginInvoke(
                        new updateUIAfterSomeImageLoadUpdate_Delegate(
                                updateUIAfterSomeImagesLoaded));
            }


            public void updateUIAfterAllImagesLoaded()
            {
                //then update UI as normal based on image loading feedback...
                //todo if user is in movie view, should I force going back to image
                //wall by calling the method below?
                updateUiWallImageBitmaps();
                update_UI_based_on_newly_loaded_images();
            }
            private delegate void updateUIAfterAllImageLoadUpdate_Delegate();
            //delegate for the above method must be called to update the UI

            public void updateUIAfterSomeImagesLoaded()
            {
                updateUiWallImageBitmaps();
            }
            private delegate void updateUIAfterSomeImageLoadUpdate_Delegate();

            private void updateUiWallImageBitmaps()
            {
                LstDisplayEvents.ItemsSource = null;
                LstDisplayEvents.Items.Clear();
                LstDisplayEvents.ItemsSource = Event_Rep.EventList;
            }

            /// <summary>
            /// this method is invoked via a delegate to update the Image viewer UI
            /// based on when all of the events images have been loaded into memory...
            /// </summary>
            public void update_UI_based_on_newly_loaded_images()
            {
                /*
                //now let's re-enable some of the controls after the images have loaded...
                txtPlease_Wait.Visibility = Visibility.Hidden;
                btnDelete.IsEnabled = true;
                txtCaption.Visibility = Visibility.Visible;

                //also let's give visual feedback that these controls are enabled
                PlayBtn.Opacity = 1;
                NextBtn.Opacity = 1;
                PreviousBtn.Opacity = 1;
                btnDelete.Opacity = 1;

                //and now let's show the first image in the event
                array_position_of_current_image = 0;
                update_display_image();

                set_to_image_wall_mode();
                 */ 
            }

        #endregion code to integrate image loading thread into the Image Viewer UI







        #region receiving callbacks from other classes

            /// <summary>
            /// this method is invoked via a callback when the user selects a new date from the calendar
            /// </summary>
            /// <param name="date_selected"></param>
            public void Calendar_Data_Selected_Callback(DateTime date_selected)
            {
                show_new_day_on_UI(date_selected);
            } //close method Calendar_Data_Selected_Callback()...




            /// <summary>
            /// this method is called when the user has updated an event's information, so we want to update the main UI to reflect this change...
            /// </summary>
            public void New_Event_Comment_Callback()
            {
                LstDisplayEvents.Items.Refresh();
            } //close method New_Event_Comment_Callback()...



            /// <summary>
            /// this method is called when the user has sent/merged images from one event to the next, so we want to update the main UI to now reflect this change...
            /// </summary>
            public void Images_Moved_Between_Events_Callback()
            {
                //to highlight to the user the event they were just looking at, we'll store this index before refreshing events on display
                int currently_selected_index = LstDisplayEvents.SelectedIndex;

                //most important step here...
                //refresh the whole day on display, so that we can then highlight all the annotated events of interest
                show_new_day_on_UI(current_day_on_display);

                //then if we were just on the last event and merged all images into the previous one, we have 1 less event overall
                if (currently_selected_index == LstDisplayEvents.Items.Count)
                    currently_selected_index--; //therefore let's just decrament by one, so we can select the last item

                //and finally let's select the event the user is just viewing...
                LstDisplayEvents.SelectedIndex = currently_selected_index;
            } //close method Images_Moved_Between_Events_Callback()...




            /// <summary>
            /// this method is called when the user has deleted the event in question, so we want to update the main UI to reflect this change...
            /// </summary>
            public void Event_Deleted_Callback(Event_Rep param_deleted_event)
            {
                //let's firstly just make sure this isn't the only event in the day that we've just deleted
                //if so, let's refresh the list of available days, and then show the most recent...
                if (LstDisplayEvents.Items.Count == 1)
                {
                    refresh_calendar_to_reflect_updated_list_of_available_days();
                } //close if (LstDisplayEvents.Items.Count == 1)...
                else
                {
                    //todo bug when I delete all images in event?
                    LstDisplayEvents.Items.Remove(param_deleted_event);
                    LstDisplayEvents.Items.Refresh();
                } //close else ... if (LstDisplayEvents.Items.Count == 1)...
            } //close method Event_Deleted_Callback()...




            /// <summary>
            /// this method is called when all images have been deleted from the SenseCam ... means the user can now see the new images (another external process will be deleting images from the SenseCam in the meantime, but it's nothing to do with our current browser from this moment onwards)
            /// </summary>
            public void All_Images_Deleted_from_SenseCam()
            {
                this.Dispatcher.BeginInvoke(new Update_UI_To_Show_Latest_Data_While_Still_Deleting_SC_Images_Delegate(Update_UI_To_Show_Latest_Data_While_Still_Deleting_SC_Images)); //invoke the delegate which calls the method to disable the user from exiting the application...
            } //close method All_Images_Deleted_from_SenseCam()...


            /// <summary>
            /// this method is invoked via a delegate to update the UI based on feedback from the "upload images" thread/user control ... update the calendar and day on view based on the newly uploaded images...
            /// </summary>
            public void Update_UI_To_Show_Latest_Data_While_Still_Deleting_SC_Images()
            {
                //firstly refresh the calendar to show the new days on display...
                refresh_calendar_to_reflect_updated_list_of_available_days();

                //finally let's close down the user control to let the user see their latest images!
                ucUploadNewImages.Visibility = Visibility.Collapsed;
            
            } //close method Update_UI_To_Show_Latest_Data_While_Still_Deleting_SC_Images()...
            private delegate void Update_UI_To_Show_Latest_Data_While_Still_Deleting_SC_Images_Delegate(); //and a delegate for the above method must be called if we want to update the UI, hence we declare this line


            private void refresh_calendar_to_reflect_updated_list_of_available_days()
            {
                calSenseCamDay.list_of_available_days = calendar_control.get_list_of_available_days_for_user(Window1.OVERALL_userID); //refresh the list of available days

                //and then call the calendar_control method which updates the internal dictionary that the calendar display mechanism relys on
                calendar_control.update_days_on_calendar();

                //initialise the calendar, so that we can use it to select new dates
                calSenseCamDay.initialise_calendar_popup_datecallback(Calendar_Data_Selected_Callback);

                //and now let's show the most recent day's data...
                show_new_day_on_UI(calSenseCamDay.list_of_available_days[0]);
            } //close method refresh_calendar_to_reflect_updated_list_of_available_days()...

        #endregion receiving callbacks from other classes






        #region interface control methods...

            /// <summary>
            /// when the user wants to select a new day from the calendar, they click on the "calendar" button and we show this user control as visible
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void CalendarView_Click(object sender, RoutedEventArgs e)
            {
                //let's log this interaction
                Record_User_Interactions.log_interaction_to_database("Window1_calendar_click", current_day_on_display.ToString());
                calSenseCamDay.Visibility = Visibility.Visible;                
            } //close method CalendarView_Click()...




            /// <summary>
            /// this method is called when the user selects one of the events on display, which means that they want to see the images within this event
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void LstDisplayEvents_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
            {
                if (LstDisplayEvents.SelectedIndex >= 0)
                {                   

                    txtEventNumber.UpdateLayout();
                    Event_Rep event_rep = (Event_Rep)LstDisplayEvents.SelectedItem;
                    //let's log this interaction
                    Record_User_Interactions.log_interaction_to_database("Window1_eventdetail_click", event_rep.eventID.ToString());
                    
                    sc_img_viewer.update_event_on_display(Window1.OVERALL_userID, event_rep, New_Event_Comment_Callback, Event_Deleted_Callback, Images_Moved_Between_Events_Callback);
                    sc_img_viewer.Visibility = Visibility.Visible;                    
                    
                } //close if (LstDisplayEvents.SelectedIndex >= 0)                
            } //close LstDisplayEvents_MouseLeftButtonUp()...





            /// <summary>
            /// the method is called when the user wishes to upload new images from the SenseCam...
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void btnUpload_New_Images_Click(object sender, RoutedEventArgs e)
            {
                //let's log this interaction
                Record_User_Interactions.log_interaction_to_database("Window1_btnupload_new_images_click");
                ucUploadNewImages.update_user_control_with_drive_information(User_Object.OVERALL_userID, User_Object.OVERALL_USER_NAME, All_Images_Deleted_from_SenseCam);
                ucUploadNewImages.Visibility = Visibility.Visible;                                
            } //close btnUpload_New_Images_Click()...




            /// <summary>
            /// this method is called whenever the user wants to exit our SenseCam browser...
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void btnClose_App_Click(object sender, RoutedEventArgs e)
            {
                //incase any threads have been left running
                Event_Rep.eventLoadingId = int.MinValue;
                image_loading_thread.Abort();

                //let's log this interaction
                Record_User_Interactions.log_interaction_to_database("Window1_appclose", current_day_on_display.ToString());

                //and now's let's close the application
                if (!ucUploadNewImages.Is_Deletion_Process_Currently_Running())
                {
                    //shut_down_pc();
                    Application.Current.Shutdown();
                } //close if (ucUploadNewImages.Is_Deletion_Process_Currently_Running())...
                else
                {
                    MessageBox.Show("Please wait a few more minutes while the SenseCam is being cleared so that you can use it again i.e. unto the red light at the top of the camera goes off");
                    //let's log this interaction
                    Record_User_Interactions.log_interaction_to_database("Window1_appclose_denied_currently_deleting_images_from_SenseCam");
                } //close else ... if (ucUploadNewImages.Is_Deletion_Process_Currently_Running())...
                
            } //close method btnClose_App_Click()...




            /// <summary>
            /// this method is called whenever the user wants to export annotations...
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void btnExport_Click(object sender, RoutedEventArgs e)
            {
                int userId = User_Object.OVERALL_userID;
                string userName = User_Object.OVERALL_USER_NAME;
                //will suggest saving output to participant's most recent data folder
                //todo for some reason, calling User_Object below causes a problem
                //on some computers (try testing this more thoroughly)
                string suggestedPath = "";// User_Object.get_likely_PC_destination_root(
                         //userId, userName);
                
                //prompt researcher on where to store annotations
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.InitialDirectory = suggestedPath;
                saveFileDialog.FileName = userName + ".csv";
                saveFileDialog.ShowDialog();
                if (!saveFileDialog.FileName.Equals(""))
                {
                    //save annotations for this day to file
                    string fileName = saveFileDialog.FileName;
                    Daily_Annotation_Summary.writeAllAnnotationsToCsv(userId,
                            fileName);
                    Record_User_Interactions.log_interaction_to_database(
                            "Window1_btnExport_Click", fileName);
                }
            } //close method btnExport_Click()...


            private void btnImport_Click(object sender, RoutedEventArgs e)
            {
                int userId = User_Object.OVERALL_userID;
                string userName = User_Object.OVERALL_USER_NAME;
                //will suggest saving output to participant's most recent data folder
                //todo for some reason, calling User_Object below causes a problem
                //on some computers (try testing this more thoroughly)
                string suggestedPath = "";// User_Object.get_likely_PC_destination_root(
                //userId, userName);

                //prompt researcher on where to store annotations
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = suggestedPath;
                openFileDialog.ShowDialog();
                string fileName = openFileDialog.FileName;
                if (!fileName.Equals("")) {
                    Event_Rep.rewriteEventList(userId, fileName);
                    show_new_day_on_UI(current_day_on_display);                    
                }
            } 




            /// <summary>
            /// this method is responsible for highlighting all events belonging to a certain activity...
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void lstDailyActivitySummary_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (lstDailyActivitySummary.SelectedItem != null) //firstly check whether a valid item has been selected...
                {
                    //firstly get the type of activity we want to highlight...
                    Daily_Annotation_Summary selected_activity = (Daily_Annotation_Summary)lstDailyActivitySummary.SelectedItem;
                    //then find the event ids displayed today which are part of the given activity type
                    List<int> eventIDs_to_highlight = Daily_Annotation_Summary.ActivityEventIds(Window1.OVERALL_userID, current_day_on_display, selected_activity.annType);

                    foreach (Event_Rep displayed_event in LstDisplayEvents.Items)
                    {
                        //by defaut reset the border colour for this event...
                        displayed_event.borderColour = Event_Rep.DefaultKeyframeBorderColour;

                        //then we try to see if this event is in our list of target event ids to highlight
                        foreach (int target_id in eventIDs_to_highlight)
                        {
                            if (displayed_event.eventID == target_id)
                                displayed_event.borderColour = "red"; //if this is the target event, let's highlight it's border colour...
                        } //close foreach(int target_id in eventIDs_to_highlight)...
                    } //close foreach (Event_Rep displayed_event in LstDisplayEvents.Items)...

                    //finally refresh the list of events on display to highlight the target events...
                    LstDisplayEvents.Items.Refresh();

                    //let's record this user interaction for later analysis...
                    Record_User_Interactions.log_interaction_to_database("Window1_lstDailyActivitySummary_SelectionChanged", selected_activity.annType);

                } //close if (lstDailyActivitySummary.SelectedItem != null) //firstly check whether a valid item has been selected...
            } //close method lstDailyActivitySummary_SelectionChanged()...




            /// <summary>
            /// this method is responsible for highlighting an individual annotated event...
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void lstIndividual_Journeys_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
            {
                if (lstIndividual_Journeys.SelectedItem != null) //firstly check whether a valid item has been selected...
                {
                    //firstly get the event we to highlight...
                    Interface_Code.Event_Activity_Annotation selected_event = (Interface_Code.Event_Activity_Annotation)lstIndividual_Journeys.SelectedItem;
                    //then store the event id that we'll want to highlight...
                    int eventID_to_highlight = selected_event.eventID;

                    foreach (Event_Rep displayed_event in LstDisplayEvents.Items)
                    {
                        //by defaut reset the border colour for this event...
                        displayed_event.borderColour = Event_Rep.DefaultKeyframeBorderColour;

                        //then let's see if this is the event we want to highlight...
                        if (displayed_event.eventID == eventID_to_highlight)
                                displayed_event.borderColour = "red"; //if this is the target event, let's highlight it's border colour...
                        
                    } //close foreach (Event_Rep displayed_event in LstDisplayEvents.Items)...

                    //finally refresh the list of events on display to highlight the target events...
                    LstDisplayEvents.Items.Refresh();

                    //let's record this user interaction for later analysis...
                    Record_User_Interactions.log_interaction_to_database("Window1_lstIndividual_Journeys_PreviewMouseLeftButtonUp", selected_event.eventID+","+selected_event.annotation);
                } //if (lstIndividual_Journeys.SelectedItem != null) //firstly check whether a valid item has been selected...
            } //close method lstIndividual_Journeys_PreviewMouseLeftButtonUp()...



        
            /// <summary>
            /// clicking on this should decrease the zooming size of the keyframes on display...
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void small_rectangle_icon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                //we'll move down in incraments of 10% ... range is 640-80 = 560 ... then 10% of that is 56
                if(ZoomSlider.Value>136) //i.e. 80+56
                    ZoomSlider.Value -= 56; //i.e. 10% of range of 80-640...

                //let's record this user interaction for later analysis...
                Record_User_Interactions.log_interaction_to_database("Window1_small_rectangle_icon_MouseLeftButtonDown", "decrease_size");
            } //close method small_rectangle_icon_MouseLeftButtonDown()...




            /// <summary>
            /// clicking on this should increase the zooming size of the keyframes on display...
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void large_rectangle_icon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                //we'll move up in incraments of 10% ... range is 640-80 = 560 ... then 10% of that is 56
                if (ZoomSlider.Value < 586) //i.e. 640-56
                    ZoomSlider.Value += 56; //i.e. 10% of range of 80-640...

                //let's record this user interaction for later analysis...
                Record_User_Interactions.log_interaction_to_database("Window1_large_rectangle_icon_MouseLeftButtonDown", "increase_size");
            } //close method large_rectangle_icon_MouseLeftButtonDown()...
                   

        #endregion interface control methods...



    } //end class...
} //end namespace...
