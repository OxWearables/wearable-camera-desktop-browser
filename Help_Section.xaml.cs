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

namespace SenseCamBrowser1
{
    /// <summary>
    /// Interaction logic for Help_Section.xaml
    /// </summary>
    public partial class Help_Section : UserControl
    {
        public Help_Section()
        {
            InitializeComponent();
        }

        private void Close1_Click(object sender, RoutedEventArgs e)
        {
            //let's log this interaction
            play_sound();
            Record_User_Interactions.log_interaction_to_database("HelpSection_Close1_click");

            this.Visibility = Visibility.Collapsed;
        }

        private void btnQuick_Add_Photos_Click(object sender, RoutedEventArgs e)
        {
            //let's log this interaction
            play_sound();
            Record_User_Interactions.log_interaction_to_database("HelpSection_btnQuick_Add_Photos_click");
        }

        private void btnQuick_Choose_Date_Click(object sender, RoutedEventArgs e)
        {
            //let's log this interaction
            play_sound();
            Record_User_Interactions.log_interaction_to_database("HelpSection_btnQuick_Choose_Date_click");
        }

        private void btnQuick_View_Photos_Click(object sender, RoutedEventArgs e)
        {
            //let's log this interaction
            play_sound();
            Record_User_Interactions.log_interaction_to_database("HelpSection_btnQuick_View_Photos_click");
        }

        private void btnQuick_Add_Label_Click(object sender, RoutedEventArgs e)
        {
            //let's log this interaction
            play_sound();
            Record_User_Interactions.log_interaction_to_database("HelpSection_btnQuick_Add_Label_click");
        }
        

        /// <summary>
        /// when this method is called, a sound will be played
        /// </summary>
        private void play_sound()
        {
            //better suited towards touchscreens...
            //sound_Click_Sound.Stop();
            //sound_Click_Sound.Play();
        } //close method play_sound()...


    } //close class...
} //close namespace...
