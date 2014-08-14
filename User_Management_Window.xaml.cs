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
using System.Configuration;

namespace SenseCamBrowser1
{



    /// <summary>
    /// Interaction logic for User_Management_Window.xaml
    /// </summary>
    public partial class User_Management_Window : Window
    {

        private static string TEXT_FOR_NEW_USER_TEXTBOX = "Please enter new participant name...";
        private static string TEXT_FOR_NEW_USER_NAME = "<new participant>";
        private static List<User_Object> list_of_users_in_db;


        public User_Management_Window()
        {

            //todo now change things here, so that the localDB database will be installed in the working director (Deployment) ... and then make sure that SQL Server LocalDB is firstly installed before/with SenseCam browser
            //todo also install some sample data with the browser too, and show it annotated... maybe not in the current version (unto published?)...
            InitializeComponent();
            lblHeading.Content = "Welcome to DCU CLARITY SenseCam browser";
            
            //firstly get the list of users we have stored in the database...
            list_of_users_in_db = User_Object.get_list_of_users_in_database();

            //secondly let's add an option to add a new user to the end of the list...
            list_of_users_in_db.Add(new User_Object(1, TEXT_FOR_NEW_USER_NAME, TEXT_FOR_NEW_USER_NAME, TEXT_FOR_NEW_USER_NAME));

            //and now let's populate the relevant drop down box..., so as the program has started the user can select the desired user...
            cboUserList.ItemsSource = list_of_users_in_db;

            //let's hide the add user text request
            txtUser.Visibility = Visibility.Hidden;
            lblUser.Visibility = Visibility.Hidden;
        } //close constructor()...




        private void btnShowBrowser_Click(object sender, RoutedEventArgs e)
        {
            int userID_NOT_VALID = -1;
            int selected_userID = userID_NOT_VALID;
            string selected_username = "";

            //firstly let's get the user id and check it's valid
            if (cboUserList.SelectedIndex != -1)
            {
                User_Object selected_user = (User_Object)cboUserList.SelectedItem;

                if (selected_user.name.Equals(TEXT_FOR_NEW_USER_NAME))
                {
                    //check that this username doesn't already exist...
                    string newly_entered_username = txtUser.Text;
                    bool name_already_exists = false;
                    foreach (User_Object user_info in list_of_users_in_db)
                    {
                        if (user_info.name.Equals(newly_entered_username))
                            name_already_exists = true;
                    } //close foreach (User_Object user_info in list_of_users_in_db)

                    //now if this is a valid new user name...
                    if (!name_already_exists)
                    {
                        //then let's insert it into the database, and also get it's id...
                        selected_userID = User_Object.insert_new_user_into_database_and_get_id(newly_entered_username);
                        selected_username = newly_entered_username;
                    } //close if (!name_already_exists)....
                    else MessageBox.Show("Participant '" + newly_entered_username + "' already exists. Please enter a unique name."); //else this isn't a valid username...

                } //close if (selected_user.name.Equals(TEXT_FOR_NEW_USER_NAME))...
                else //i.e. it's an existing user we want to see...
                {
                    selected_userID = selected_user.userID;
                    selected_username = selected_user.username;
                } //close else ... if (selected_user.name.Equals(TEXT_FOR_NEW_USER_NAME))

            } //close if (cboUserList.SelectedIndex != -1)...
            else MessageBox.Show("Please select a participant from the drop down list");


            //finally if we've selected a valid user id, let's start the browsing session!
            if (selected_userID != userID_NOT_VALID)
                set_overall_userID_and_start_browsing_session(selected_userID, selected_username);

        } //close method btnShowBrowser_Click()...

        


        /// <summary>
        /// this method is responsible for starting the browsing session, and closing this current form!
        /// </summary>
        /// <param name="userID"></param>
        private void set_overall_userID_and_start_browsing_session(int userID, string user_name)
        {
            //IMPORTANT, now set the overall user id for this session login...
            User_Object.OVERALL_userID = userID;
            User_Object.OVERALL_USER_NAME = user_name;

            this.Hide();
            Window1 win1 = new Window1();
            win1.Show();
        } //close method set_overall_userID_and_start_browsing_session()...




        private void txtUser_GotFocus(object sender, RoutedEventArgs e)
        {           
            if (txtUser.Text.Equals(TEXT_FOR_NEW_USER_TEXTBOX))
            {
                txtUser.Text = "";
                btnShowBrowser.Visibility = Visibility.Visible;
            }
        } //close method txtUser_GotFocus()...


        //then update a static variable somewhere, to open browser for relevant user...
        private void cboUserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboUserList.SelectedIndex != -1)
            {
                User_Object selected_user = (User_Object)cboUserList.SelectedItem;

                if (selected_user.name.Equals(TEXT_FOR_NEW_USER_NAME))
                {
                    txtUser.Text = TEXT_FOR_NEW_USER_TEXTBOX;
                    txtUser.Visibility = Visibility.Visible;
                    lblUser.Visibility = Visibility.Visible;
                    btnShowBrowser.Visibility = Visibility.Hidden;
                }
                else
                {
                    txtUser.Text = selected_user.name;
                    btnShowBrowser.Visibility = Visibility.Visible;
                } //close else ... if (selected_user.name.Equals(TEXT_FOR_NEW_USER_NAME))...
            } //close if (cboUserList.SelectedIndex != -1)...
        } //close method cboUserList_SelectionChanged()...



    } //end class...

} //end namespace...
