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

namespace SenseCamBrowser1.Upload_Images_and_Segment_into_Events
{
    class Segmentation_Image_Rep
    {
        //THIS CLASS IS USED AS A DATA STORE FOR EACH IMAGE ... THERE'LL BE A LOT OF NULL PROPERTIES WHICH WILL BE FILLED IN LATER ON BY OTHER PROCESSES RUNNING ON THE DATABASE (NAMELY THE SENSOR VALUES AND LOCATION BEING APPENDED TO THIS)
		private int array_position, chunk_id;
		private string image_name;
		private DateTime image_time, image_day;        
		private double manipulated_acc_combined, manipulated_acc_x, manipulated_pir, fused_score;
		private int event_boundary;

		

		public Segmentation_Image_Rep(int chunk_id, string img_name, DateTime image_day, DateTime image_time, double man_acc_comb, double man_acc_x, double man_pir)
		{
			this.chunk_id = chunk_id;
			this.image_name = img_name;
			this.image_day = image_day; //the first time in the chunk
			this.image_time = image_time;
            
			this.manipulated_acc_combined = man_acc_comb;
			this.manipulated_acc_x = man_acc_x;
			this.manipulated_pir = man_pir;

			event_boundary = 0;
			fused_score = 0.0;
		} //end constructor()


		public int get_chunk_id()
		{
			return chunk_id;
		} //end method get_chunk_id()


		public int get_array_position()
		{
			return array_position;
		} //end method get_array_position()


		public void set_array_position(int pos)
		{
			array_position = pos;
		} //end method set_array_position()



		public void set_fused_score(double score)
		{
			fused_score = score;
		}



		public double get_fused_score()
		{
			return fused_score;
		}




		public string get_image_name()
		{
			return image_name;
		} //end method get_image_name()



		public DateTime get_image_time()
		{
			return image_time;
		} //end method get_image_time()



		public DateTime get_image_day()
		{
			return image_day;
		} //end method get_image_day()


		public double get_manipulated_acc_combined()
		{
			return manipulated_acc_combined;
		}


		public double get_manipulated_acc_x()
		{
			return manipulated_acc_x;
		}


		public double get_manipulated_pir()
		{
			return manipulated_pir;
		}



		public void set_manipulated_acc_combined(double val)
		{
			manipulated_acc_combined = val;
		}


		public void set_manipulated_acc_x(double val)
		{
			manipulated_acc_x = val;
		}



		public void set_manipulated_pir(double val)
		{
			manipulated_pir = val;
		}




		public Segmentation_Image_Rep(string img_name, DateTime img_time)
		{
			image_name = img_name;
			image_time = img_time;
			event_boundary = 0;
		} //end alternative constructor (used to update image.dat file)


		public void set_event_boundary(int bound_num)
		{
			event_boundary = bound_num;
		} //end method set_as_event_boundary() (used to update image.dat file)


		public int get_event_boundary()
		{
			return event_boundary;
		} //end method get_event_boundary() (used to update image.dat file)



	} //end class

} //end namespace
