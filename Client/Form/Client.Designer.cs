namespace Client
{
    partial class Client
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
            this.lblIP = new System.Windows.Forms.Label();
            this.txtIP = new System.Windows.Forms.TextBox();
            this.lblTTSV = new System.Windows.Forms.Label();
            this.cbTTSV = new System.Windows.Forms.ComboBox();
            this.gbTTSV = new System.Windows.Forms.GroupBox();
            this.txtHoTen = new System.Windows.Forms.TextBox();
            this.txtMSSV = new System.Windows.Forms.TextBox();
            this.txtLop = new System.Windows.Forms.TextBox();
            this.lblHoTen = new System.Windows.Forms.Label();
            this.lblMSSV = new System.Windows.Forms.Label();
            this.lblLop = new System.Windows.Forms.Label();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnĐiemDanh = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtDeThi = new System.Windows.Forms.TextBox();
            this.gbTTSV.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblIP
            // 
            this.lblIP.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblIP.AutoSize = true;
            this.lblIP.Location = new System.Drawing.Point(34, 29);
            this.lblIP.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblIP.Name = "lblIP";
            this.lblIP.Size = new System.Drawing.Size(65, 20);
            this.lblIP.TabIndex = 0;
            this.lblIP.Text = "IP máy:";
            // 
            // txtIP
            // 
            this.txtIP.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtIP.Location = new System.Drawing.Point(120, 21);
            this.txtIP.Name = "txtIP";
            this.txtIP.Size = new System.Drawing.Size(264, 28);
            this.txtIP.TabIndex = 1;
            // 
            // lblTTSV
            // 
            this.lblTTSV.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTTSV.AutoSize = true;
            this.lblTTSV.Location = new System.Drawing.Point(34, 76);
            this.lblTTSV.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTTSV.Name = "lblTTSV";
            this.lblTTSV.Size = new System.Drawing.Size(102, 20);
            this.lblTTSV.TabIndex = 2;
            this.lblTTSV.Text = "Chọn TTSV:";
            // 
            // cbTTSV
            // 
            this.cbTTSV.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbTTSV.FormattingEnabled = true;
            this.cbTTSV.Location = new System.Drawing.Point(154, 68);
            this.cbTTSV.Name = "cbTTSV";
            this.cbTTSV.Size = new System.Drawing.Size(333, 28);
            this.cbTTSV.TabIndex = 3;
            this.cbTTSV.SelectedIndexChanged += new System.EventHandler(this.cbTTSV_SelectedIndexChanged);
            // 
            // gbTTSV
            // 
            this.gbTTSV.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbTTSV.Controls.Add(this.txtDeThi);
            this.gbTTSV.Controls.Add(this.label1);
            this.gbTTSV.Controls.Add(this.txtHoTen);
            this.gbTTSV.Controls.Add(this.txtMSSV);
            this.gbTTSV.Controls.Add(this.txtLop);
            this.gbTTSV.Controls.Add(this.lblHoTen);
            this.gbTTSV.Controls.Add(this.lblMSSV);
            this.gbTTSV.Controls.Add(this.lblLop);
            this.gbTTSV.Location = new System.Drawing.Point(38, 129);
            this.gbTTSV.Name = "gbTTSV";
            this.gbTTSV.Size = new System.Drawing.Size(449, 214);
            this.gbTTSV.TabIndex = 4;
            this.gbTTSV.TabStop = false;
            this.gbTTSV.Text = "Thông tin sinh viên:";
            // 
            // txtHoTen
            // 
            this.txtHoTen.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHoTen.Location = new System.Drawing.Point(116, 115);
            this.txtHoTen.Name = "txtHoTen";
            this.txtHoTen.Size = new System.Drawing.Size(242, 28);
            this.txtHoTen.TabIndex = 6;
            // 
            // txtMSSV
            // 
            this.txtMSSV.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMSSV.Location = new System.Drawing.Point(116, 73);
            this.txtMSSV.Name = "txtMSSV";
            this.txtMSSV.Size = new System.Drawing.Size(242, 28);
            this.txtMSSV.TabIndex = 5;
            // 
            // txtLop
            // 
            this.txtLop.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLop.Location = new System.Drawing.Point(116, 28);
            this.txtLop.Name = "txtLop";
            this.txtLop.Size = new System.Drawing.Size(242, 28);
            this.txtLop.TabIndex = 4;
            // 
            // lblHoTen
            // 
            this.lblHoTen.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblHoTen.AutoSize = true;
            this.lblHoTen.Location = new System.Drawing.Point(18, 123);
            this.lblHoTen.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblHoTen.Name = "lblHoTen";
            this.lblHoTen.Size = new System.Drawing.Size(84, 20);
            this.lblHoTen.TabIndex = 3;
            this.lblHoTen.Text = "Họ và tên:";
            // 
            // lblMSSV
            // 
            this.lblMSSV.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMSSV.AutoSize = true;
            this.lblMSSV.Location = new System.Drawing.Point(18, 81);
            this.lblMSSV.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMSSV.Name = "lblMSSV";
            this.lblMSSV.Size = new System.Drawing.Size(60, 20);
            this.lblMSSV.TabIndex = 2;
            this.lblMSSV.Text = "MSSV:";
            // 
            // lblLop
            // 
            this.lblLop.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLop.AutoSize = true;
            this.lblLop.Location = new System.Drawing.Point(18, 36);
            this.lblLop.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblLop.Name = "lblLop";
            this.lblLop.Size = new System.Drawing.Size(43, 20);
            this.lblLop.TabIndex = 1;
            this.lblLop.Text = "Lớp:";
            // 
            // btnConnect
            // 
            this.btnConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConnect.Location = new System.Drawing.Point(404, 22);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(83, 27);
            this.btnConnect.TabIndex = 5;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnĐiemDanh
            // 
            this.btnĐiemDanh.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnĐiemDanh.Location = new System.Drawing.Point(178, 370);
            this.btnĐiemDanh.Name = "btnĐiemDanh";
            this.btnĐiemDanh.Size = new System.Drawing.Size(111, 40);
            this.btnĐiemDanh.TabIndex = 6;
            this.btnĐiemDanh.Text = "Điểm danh";
            this.btnĐiemDanh.UseVisualStyleBackColor = true;
            this.btnĐiemDanh.Click += new System.EventHandler(this.btnĐiemDanh_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 168);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 20);
            this.label1.TabIndex = 7;
            this.label1.Text = "Đề thi:";
            // 
            // txtDeThi
            // 
            this.txtDeThi.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDeThi.Location = new System.Drawing.Point(116, 160);
            this.txtDeThi.Multiline = true;
            this.txtDeThi.Name = "txtDeThi";
            this.txtDeThi.Size = new System.Drawing.Size(242, 28);
            this.txtDeThi.TabIndex = 8;
            // 
            // Client
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(546, 438);
            this.Controls.Add(this.btnĐiemDanh);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.gbTTSV);
            this.Controls.Add(this.cbTTSV);
            this.Controls.Add(this.lblTTSV);
            this.Controls.Add(this.txtIP);
            this.Controls.Add(this.lblIP);
            this.Font = new System.Drawing.Font("Times New Roman", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Client";
            this.Text = "Client";
            this.gbTTSV.ResumeLayout(false);
            this.gbTTSV.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblIP;
        private System.Windows.Forms.TextBox txtIP;
        private System.Windows.Forms.Label lblTTSV;
        private System.Windows.Forms.ComboBox cbTTSV;
        private System.Windows.Forms.GroupBox gbTTSV;
        private System.Windows.Forms.TextBox txtHoTen;
        private System.Windows.Forms.TextBox txtMSSV;
        private System.Windows.Forms.TextBox txtLop;
        private System.Windows.Forms.Label lblHoTen;
        private System.Windows.Forms.Label lblMSSV;
        private System.Windows.Forms.Label lblLop;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnĐiemDanh;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtDeThi;
    }
}