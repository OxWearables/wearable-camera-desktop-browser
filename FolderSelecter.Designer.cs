namespace SenseCamBrowser1
{
    partial class FolderSelecter
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.fbdSelectFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();            
            this.SuspendLayout();
            // 
            // FolderSelecter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(187, 77);
            this.openFileDialog1.FileName = "openFileDialog1";

            this.Name = "FolderSelecter";
            this.Text = "FolderSelecter";
            this.Load += new System.EventHandler(this.FolderSelecter_Load);
            this.ResumeLayout(false);

            
            
        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog fbdSelectFolder;
        //private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}