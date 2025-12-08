namespace LapTrinhMang
{
    partial class CopyDuLieu
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
            this.label1 = new System.Windows.Forms.Label();
            this.cbMayNguon = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbMayDich = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblThongTinNguon = new System.Windows.Forms.Label();
            this.btnCopyDuLieu = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 19);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(102, 22);
            this.label1.TabIndex = 0;
            this.label1.Text = "Máy nguồn:";
            // 
            // cbMayNguon
            // 
            this.cbMayNguon.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbMayNguon.FormattingEnabled = true;
            this.cbMayNguon.Location = new System.Drawing.Point(120, 16);
            this.cbMayNguon.Name = "cbMayNguon";
            this.cbMayNguon.Size = new System.Drawing.Size(300, 30);
            this.cbMayNguon.TabIndex = 1;
            this.cbMayNguon.SelectedIndexChanged += new System.EventHandler(this.cbMayNguon_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 60);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 22);
            this.label2.TabIndex = 2;
            this.label2.Text = "Máy đích:";
            // 
            // cbMayDich
            // 
            this.cbMayDich.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbMayDich.FormattingEnabled = true;
            this.cbMayDich.Location = new System.Drawing.Point(120, 57);
            this.cbMayDich.Name = "cbMayDich";
            this.cbMayDich.Size = new System.Drawing.Size(300, 30);
            this.cbMayDich.TabIndex = 3;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblThongTinNguon);
            this.groupBox1.Location = new System.Drawing.Point(16, 100);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(404, 126);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Thông tin máy nguồn";
            // 
            // lblThongTinNguon
            // 
            this.lblThongTinNguon.AutoSize = true;
            this.lblThongTinNguon.Location = new System.Drawing.Point(6, 30);
            this.lblThongTinNguon.Name = "lblThongTinNguon";
            this.lblThongTinNguon.Size = new System.Drawing.Size(94, 22);
            this.lblThongTinNguon.TabIndex = 0;
            this.lblThongTinNguon.Text = "Chưa chọn";
            // 
            // btnCopyDuLieu
            // 
            this.btnCopyDuLieu.Location = new System.Drawing.Point(152, 232);
            this.btnCopyDuLieu.Name = "btnCopyDuLieu";
            this.btnCopyDuLieu.Size = new System.Drawing.Size(150, 47);
            this.btnCopyDuLieu.TabIndex = 5;
            this.btnCopyDuLieu.Text = "Copy dữ liệu";
            this.btnCopyDuLieu.UseVisualStyleBackColor = true;
            this.btnCopyDuLieu.Click += new System.EventHandler(this.btnCopyDuLieu_Click);
            // 
            // CopyDuLieu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 22F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(432, 291);
            this.Controls.Add(this.btnCopyDuLieu);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cbMayDich);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbMayNguon);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "CopyDuLieu";
            this.Text = "Copy dữ liệu";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbMayNguon;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbMayDich;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblThongTinNguon;
        private System.Windows.Forms.Button btnCopyDuLieu;
    }
}