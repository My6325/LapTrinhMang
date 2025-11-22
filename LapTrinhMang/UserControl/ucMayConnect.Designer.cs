namespace LapTrinhMang
{
    partial class ucMayConnect
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ucMayConnect));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.txtMSSV = new System.Windows.Forms.TextBox();
            this.txtIP = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(150, 130);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // txtMSSV
            // 
            this.txtMSSV.BackColor = System.Drawing.Color.White;
            this.txtMSSV.Enabled = false;
            this.txtMSSV.Location = new System.Drawing.Point(0, 136);
            this.txtMSSV.Name = "txtMSSV";
            this.txtMSSV.ShortcutsEnabled = false;
            this.txtMSSV.Size = new System.Drawing.Size(150, 28);
            this.txtMSSV.TabIndex = 1;
            this.txtMSSV.Text = "....";
            this.txtMSSV.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtIP
            // 
            this.txtIP.BackColor = System.Drawing.Color.White;
            this.txtIP.Enabled = false;
            this.txtIP.Location = new System.Drawing.Point(0, 169);
            this.txtIP.Name = "txtIP";
            this.txtIP.ShortcutsEnabled = false;
            this.txtIP.Size = new System.Drawing.Size(150, 28);
            this.txtIP.TabIndex = 2;
            this.txtIP.Text = "IP: ...";
            this.txtIP.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // ucMayConnect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.txtIP);
            this.Controls.Add(this.txtMSSV);
            this.Controls.Add(this.pictureBox1);
            this.Font = new System.Drawing.Font("Times New Roman", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "ucMayConnect";
            this.Size = new System.Drawing.Size(150, 200);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox txtMSSV;
        private System.Windows.Forms.TextBox txtIP;
    }
}
