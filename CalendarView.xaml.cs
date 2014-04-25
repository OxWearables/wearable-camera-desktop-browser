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
using System.Windows.Shapes;

namespace SenseCamBrowser1
{
    /// <summary>
    /// Interaction logic for CalendarView.xaml
    /// </summary>
    public partial class CalendarView : UserControl
    {


        #region properties associated with our calendar control

        //the list of available days that this calendar control has
        public DateTime[] list_of_available_days;

        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        //so the callbacks are important to issue the output of a selected date in the calendar...
        // Delegate that defines the signature for the callback methods.
        public delegate void DateSelected_Callback(DateTime param_selected_date);
        // Delegate used to execute the callback method when the task is complete.
        private DateSelected_Callback date_selected_callback;
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////
        ///////////////////////////// THREAD CALLBACK PROPERTIES /////////////////////////////////////////////

        #endregion properties associated with our calendar control




        #region startup methods that should be called before the calendar is displayed to the user

        public CalendarView()
        {
            InitializeComponent();
            //and let's get all the available days for the user...
            list_of_available_days = calendar_control.get_list_of_available_days_for_user(Window1.OVERALL_userID);            
        } //close constructor CalendarView()...

        



        /// <summary>
        /// here we pass a callback parameter to the calendar control, which we pass the newly selected date to
        /// </summary>
        /// <param name="param_callback"></param>
        public void initialise_calendar_popup_datecallback(DateSelected_Callback param_callback)
        {            
            this.date_selected_callback = param_callback;
            //and we'll also set the display date to the most recent day of data
            calSenseCamDay.DisplayDate = calendar_control.get_list_of_available_days_for_user(Window1.OVERALL_userID)[0];
        } //close method initialise_calendar_popup_datecallback()...
        
        #endregion startup methods that should be called before the calendar is displayed to the user




        #region interface control methods

        /// <summary>
        /// to close the Calendar control view...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            //let's log this interaction
            play_sound();
            Record_User_Interactions.log_interaction_to_database("CalendarView_CloseBtn_click", calSenseCamDay.DisplayDate.ToString());

            close_control();
        } //close method CloseBtn_Click()...




        /// <summary>
        /// this is called when the user selects a new day...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void calSenseCamDay_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            //PreviewMouseLeftButtonUp="calSenseCamDay_MouseLeftButtonUp" ... used on touchscreen device...
            //SelectedDatesChanged="calSenseCamDay_SelectedDatesChanged" ... old "reliable" method should we have problems with this method firing
            if (calSenseCamDay.SelectedDate.HasValue) //this is also fired when the user toggles through the months, so we only call this when a date is selected (i.e. when the user wants to view some data)
            {
                //let's log this interaction
                play_sound();
                Record_User_Interactions.log_interaction_to_database("CalendarView_calSenseCamDay_MouseLeftButtonUp", calSenseCamDay.SelectedDate.Value.ToString());
                
                date_selected_callback(calSenseCamDay.SelectedDate.Value);
                close_control();
            } //close if (calSenseCamDay.SelectedDate.HasValue)
        } //close method calSenseCamDay_SelectedDatesChanged()...

        #endregion interface control methods

        




        #region methods to support interface controls...
            
            /// <summary>
            /// Called when the user wants to exit the calendar control, sets its visibility to collapsed...
            /// </summary>
            private void close_control()
            {
                Visibility = Visibility.Collapsed;
            } //close method close_control()...




            /// <summary>
            /// when this method is called, a sound will be played
            /// </summary>
            private void play_sound()
            {
                //again better on touchscreen/tablet devices
                //sound_Click_Sound.Stop();
                //sound_Click_Sound.Play();
            } //close method play_sound()...

        #endregion methods to support interface controls...


    } //close class...

} //close namespace...
