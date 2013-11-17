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
using System.Text;


namespace SenseCamBrowser1.Upload_Images_and_Segment_into_Events
{
    class Sensor_Reading
    {


        private int chunk_id, white_val, battery, pir; //I'm not going to bother storing user_id, as it will be the same for all these records and only will tax system resources
        public int mag_x { get; set; }
        public int mag_y { get; set; }
        public int mag_z { get; set; }
        private double acc_x, acc_y, acc_z, temperature, manipulated_acc_combined_value, manipulated_acc_x_value, manipulated_pir_value;
        private char trigger_code;
        private string image_name;
        private DateTime sample_time;
        

        public static char NULL_CHAR = '*';
        public static int NUM_FIELDS = 12; //i.e. the number of columns that this "row" represents




        //for SenseCam...
        public Sensor_Reading(int chunk_id, DateTime sample_time, int acc_x, int acc_y, int acc_z, int white_val, int battery, int temperature, int pir, char trigger_code, string image_name)
        {
            this.chunk_id = chunk_id;
            this.sample_time = sample_time;
            this.acc_x = acc_x;
            this.acc_y = acc_y;
            this.acc_z = acc_z;
            this.white_val = white_val;
            this.battery = battery;
            this.temperature = temperature;
            this.pir = pir;
            this.trigger_code = trigger_code;
            this.image_name = image_name;
            this.mag_x = 0;
            this.mag_y = 0;
            this.mag_z = 0;

            this.manipulated_acc_combined_value = 0.0;
            this.manipulated_acc_x_value = 0.0;
            this.manipulated_pir_value = 0.0;
        } //end method Sensor_Reading() i.e. the constructor


        //for Vicon Revue...
        public Sensor_Reading(int chunk_id, DateTime sample_time, double acc_x, double acc_y, double acc_z, int white_val, int battery, double temperature, int pir, char trigger_code, string image_name, int mag_x, int mag_y, int mag_z)
        {
            this.chunk_id = chunk_id;
            this.sample_time = sample_time;
            this.acc_x = acc_x;
            this.acc_y = acc_y;
            this.acc_z = acc_z;
            this.white_val = white_val;
            this.battery = battery;
            this.temperature = temperature;
            this.pir = pir;
            this.trigger_code = trigger_code;
            this.image_name = image_name;

            this.mag_x = mag_x;
            this.mag_y = mag_y;
            this.mag_z = mag_z;

            this.manipulated_acc_combined_value = 0.0;
            this.manipulated_acc_x_value = 0.0;
            this.manipulated_pir_value = 0.0;
        } //end method Sensor_Reading() i.e. the constructor




        public int get_chunk_id()
        {
            return chunk_id;
        } //end method get_chunk_id()




        public DateTime get_sample_time()
        {
            return sample_time;
        }



        public double get_raw_acc_combined(Upload_and_Segment_Images_Thread.DeviceType device_type)
        {
            return Math.Sqrt(Standard_Calculation.square_value(Standard_Calculation.get_signed_int(acc_x, device_type))
                            + Standard_Calculation.square_value(Standard_Calculation.get_signed_int(acc_y, device_type))
                            + Standard_Calculation.square_value(Standard_Calculation.get_signed_int(acc_z, device_type)));
        } //end method get_raw_acc_combined()



        public double get_acc_x()
        {
            return acc_x;
        }




        public double get_acc_y()
        {
            return acc_y;
        }




        public double get_acc_z()
        {
            return acc_z;
        }




        public int get_white_val()
        {
            return white_val;
        }




        public int get_battery()
        {
            return battery;
        }




        public double get_temperature()
        {
            return temperature;
        }




        public int get_pir()
        {
            return pir;
        }




        public char get_trigger_code()
        {
            return trigger_code;
        }




        public string get_image_name()
        {
            return image_name;
        }




        public void set_manipulated_acc_x(double val)
        {
            manipulated_acc_x_value = val;
        }


        public double get_manipulated_acc_x()
        {
            return manipulated_acc_x_value;
        }



        public double get_manipulated_acc_combined()
        {
            return manipulated_acc_combined_value;
        } //end method get_manipulated_acc_combined()




        public void set_manipulated_acc_combined(double input_val)
        {
            manipulated_acc_combined_value = input_val;
        } //end method set_manipulated_acc_combined()



        public void set_manipulated_pir(double input_val)
        {
            manipulated_pir_value = input_val;
        } //end method set_manipulated_pir()



        public double get_manipulated_pir()
        {
            return manipulated_pir_value;
        } //end method get_manipulated_pir()



    } //end class Sensor_Reading

} //end namespace
