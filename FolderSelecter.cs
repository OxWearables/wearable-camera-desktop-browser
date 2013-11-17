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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SenseCamBrowser1
{
    public partial class FolderSelecter : Form
    {
        private OpenFileDialog openFileDialog1;

        public bool select_file_rather_than_folder { get; set; }
        public string selected_path { get; set; }




        public FolderSelecter()
        {
            InitializeComponent();
        } //close method FolderSelecter()...



        private void FolderSelecter_Load(object sender, EventArgs e)
        {
            if (select_file_rather_than_folder)
            {
                openFileDialog1.InitialDirectory = selected_path;
                openFileDialog1.ShowDialog();
                selected_path = openFileDialog1.FileName;
            }
            else
            {
                fbdSelectFolder.SelectedPath = selected_path;
                fbdSelectFolder.ShowDialog();
                selected_path = fbdSelectFolder.SelectedPath + @"\";
            }
            this.Close();

        }




    } //close class

} //end namespace...