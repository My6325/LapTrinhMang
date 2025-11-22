namespace LapTrinhMang
{
    partial class Server
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
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnDisConnect = new System.Windows.Forms.Button();
            this.btnLayDS = new System.Windows.Forms.Button();
            this.lblGuiDeThi = new System.Windows.Forms.Label();
            this.lblLuuBaiThi = new System.Windows.Forms.Label();
            this.txtGuiDeThi = new System.Windows.Forms.TextBox();
            this.txtLuuBaiThi = new System.Windows.Forms.TextBox();
            this.btnChonGuiDT = new System.Windows.Forms.Button();
            this.btnChonLuuBT = new System.Windows.Forms.Button();
            this.gbDeThi = new System.Windows.Forms.GroupBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnThem = new System.Windows.Forms.Button();
            this.btnXoa = new System.Windows.Forms.Button();
            this.btnPhatDe = new System.Windows.Forms.Button();
            this.btnThuBai = new System.Windows.Forms.Button();
            this.lblTG = new System.Windows.Forms.Label();
            this.nupThoiGian = new System.Windows.Forms.NumericUpDown();
            this.lblTGConLai = new System.Windows.Forms.Label();
            this.lblDemTG = new System.Windows.Forms.Label();
            this.gbDSMay = new System.Windows.Forms.GroupBox();
            this.flpnDanhSachMay = new System.Windows.Forms.FlowLayoutPanel();
            this.gbDeThi.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nupThoiGian)).BeginInit();
            this.gbDSMay.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(13, 22);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(4);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(136, 68);
            this.btnConnect.TabIndex = 0;
            this.btnConnect.Text = "Bắt đầu kết nối Server";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnDisConnect
            // 
            this.btnDisConnect.Location = new System.Drawing.Point(168, 22);
            this.btnDisConnect.Margin = new System.Windows.Forms.Padding(4);
            this.btnDisConnect.Name = "btnDisConnect";
            this.btnDisConnect.Size = new System.Drawing.Size(123, 68);
            this.btnDisConnect.TabIndex = 1;
            this.btnDisConnect.Text = "Ngắt kết nối Server";
            this.btnDisConnect.UseVisualStyleBackColor = true;
            // 
            // btnLayDS
            // 
            this.btnLayDS.Location = new System.Drawing.Point(312, 22);
            this.btnLayDS.Margin = new System.Windows.Forms.Padding(4);
            this.btnLayDS.Name = "btnLayDS";
            this.btnLayDS.Size = new System.Drawing.Size(124, 68);
            this.btnLayDS.TabIndex = 2;
            this.btnLayDS.Text = "Lấy danh sách";
            this.btnLayDS.UseVisualStyleBackColor = true;
            this.btnLayDS.Click += new System.EventHandler(this.btnLayDS_Click);
            // 
            // lblGuiDeThi
            // 
            this.lblGuiDeThi.AutoSize = true;
            this.lblGuiDeThi.Location = new System.Drawing.Point(12, 117);
            this.lblGuiDeThi.Name = "lblGuiDeThi";
            this.lblGuiDeThi.Size = new System.Drawing.Size(85, 20);
            this.lblGuiDeThi.TabIndex = 3;
            this.lblGuiDeThi.Text = "Gửi đề thi:";
            // 
            // lblLuuBaiThi
            // 
            this.lblLuuBaiThi.AutoSize = true;
            this.lblLuuBaiThi.Location = new System.Drawing.Point(12, 163);
            this.lblLuuBaiThi.Name = "lblLuuBaiThi";
            this.lblLuuBaiThi.Size = new System.Drawing.Size(92, 20);
            this.lblLuuBaiThi.TabIndex = 4;
            this.lblLuuBaiThi.Text = "Lưu bài thi:";
            // 
            // txtGuiDeThi
            // 
            this.txtGuiDeThi.Location = new System.Drawing.Point(115, 109);
            this.txtGuiDeThi.Name = "txtGuiDeThi";
            this.txtGuiDeThi.Size = new System.Drawing.Size(221, 28);
            this.txtGuiDeThi.TabIndex = 5;
            // 
            // txtLuuBaiThi
            // 
            this.txtLuuBaiThi.Location = new System.Drawing.Point(115, 155);
            this.txtLuuBaiThi.Name = "txtLuuBaiThi";
            this.txtLuuBaiThi.Size = new System.Drawing.Size(221, 28);
            this.txtLuuBaiThi.TabIndex = 6;
            // 
            // btnChonGuiDT
            // 
            this.btnChonGuiDT.Location = new System.Drawing.Point(364, 109);
            this.btnChonGuiDT.Name = "btnChonGuiDT";
            this.btnChonGuiDT.Size = new System.Drawing.Size(75, 28);
            this.btnChonGuiDT.TabIndex = 7;
            this.btnChonGuiDT.Text = "Chọn";
            this.btnChonGuiDT.UseVisualStyleBackColor = true;
            // 
            // btnChonLuuBT
            // 
            this.btnChonLuuBT.Location = new System.Drawing.Point(364, 154);
            this.btnChonLuuBT.Name = "btnChonLuuBT";
            this.btnChonLuuBT.Size = new System.Drawing.Size(75, 28);
            this.btnChonLuuBT.TabIndex = 8;
            this.btnChonLuuBT.Text = "Chọn";
            this.btnChonLuuBT.UseVisualStyleBackColor = true;
            // 
            // gbDeThi
            // 
            this.gbDeThi.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.gbDeThi.Controls.Add(this.textBox1);
            this.gbDeThi.Location = new System.Drawing.Point(16, 207);
            this.gbDeThi.Name = "gbDeThi";
            this.gbDeThi.Size = new System.Drawing.Size(423, 160);
            this.gbDeThi.TabIndex = 9;
            this.gbDeThi.TabStop = false;
            this.gbDeThi.Text = "Danh sách đề thi:";
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Location = new System.Drawing.Point(3, 24);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(417, 133);
            this.textBox1.TabIndex = 10;
            // 
            // btnThem
            // 
            this.btnThem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnThem.Location = new System.Drawing.Point(16, 382);
            this.btnThem.Name = "btnThem";
            this.btnThem.Size = new System.Drawing.Size(88, 33);
            this.btnThem.TabIndex = 10;
            this.btnThem.Text = "Thêm";
            this.btnThem.UseVisualStyleBackColor = true;
            // 
            // btnXoa
            // 
            this.btnXoa.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnXoa.Location = new System.Drawing.Point(129, 382);
            this.btnXoa.Name = "btnXoa";
            this.btnXoa.Size = new System.Drawing.Size(88, 33);
            this.btnXoa.TabIndex = 11;
            this.btnXoa.Text = "Xóa";
            this.btnXoa.UseVisualStyleBackColor = true;
            // 
            // btnPhatDe
            // 
            this.btnPhatDe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnPhatDe.Location = new System.Drawing.Point(238, 382);
            this.btnPhatDe.Name = "btnPhatDe";
            this.btnPhatDe.Size = new System.Drawing.Size(88, 33);
            this.btnPhatDe.TabIndex = 12;
            this.btnPhatDe.Text = "Phát đề";
            this.btnPhatDe.UseVisualStyleBackColor = true;
            // 
            // btnThuBai
            // 
            this.btnThuBai.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnThuBai.Location = new System.Drawing.Point(351, 382);
            this.btnThuBai.Name = "btnThuBai";
            this.btnThuBai.Size = new System.Drawing.Size(88, 33);
            this.btnThuBai.TabIndex = 13;
            this.btnThuBai.Text = "Thu bài";
            this.btnThuBai.UseVisualStyleBackColor = true;
            // 
            // lblTG
            // 
            this.lblTG.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblTG.AutoSize = true;
            this.lblTG.Location = new System.Drawing.Point(15, 448);
            this.lblTG.Name = "lblTG";
            this.lblTG.Size = new System.Drawing.Size(131, 20);
            this.lblTG.TabIndex = 14;
            this.lblTG.Text = "Thời gian (phút):";
            // 
            // nupThoiGian
            // 
            this.nupThoiGian.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nupThoiGian.Location = new System.Drawing.Point(163, 440);
            this.nupThoiGian.Name = "nupThoiGian";
            this.nupThoiGian.Size = new System.Drawing.Size(120, 28);
            this.nupThoiGian.TabIndex = 15;
            // 
            // lblTGConLai
            // 
            this.lblTGConLai.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblTGConLai.AutoSize = true;
            this.lblTGConLai.Location = new System.Drawing.Point(15, 497);
            this.lblTGConLai.Name = "lblTGConLai";
            this.lblTGConLai.Size = new System.Drawing.Size(134, 20);
            this.lblTGConLai.TabIndex = 16;
            this.lblTGConLai.Text = "Thời gian còn lại:";
            // 
            // lblDemTG
            // 
            this.lblDemTG.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblDemTG.AutoSize = true;
            this.lblDemTG.Location = new System.Drawing.Point(159, 497);
            this.lblDemTG.Name = "lblDemTG";
            this.lblDemTG.Size = new System.Drawing.Size(71, 20);
            this.lblDemTG.TabIndex = 17;
            this.lblDemTG.Text = "00:00:00";
            // 
            // gbDSMay
            // 
            this.gbDSMay.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbDSMay.Controls.Add(this.flpnDanhSachMay);
            this.gbDSMay.Location = new System.Drawing.Point(445, 20);
            this.gbDSMay.Name = "gbDSMay";
            this.gbDSMay.Size = new System.Drawing.Size(994, 530);
            this.gbDSMay.TabIndex = 18;
            this.gbDSMay.TabStop = false;
            this.gbDSMay.Text = "Danh sách máy con:";
            // 
            // flpnDanhSachMay
            // 
            this.flpnDanhSachMay.AutoScroll = true;
            this.flpnDanhSachMay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpnDanhSachMay.Location = new System.Drawing.Point(3, 24);
            this.flpnDanhSachMay.Name = "flpnDanhSachMay";
            this.flpnDanhSachMay.Size = new System.Drawing.Size(988, 503);
            this.flpnDanhSachMay.TabIndex = 0;
            // 
            // Server
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1451, 562);
            this.Controls.Add(this.gbDSMay);
            this.Controls.Add(this.lblDemTG);
            this.Controls.Add(this.lblTGConLai);
            this.Controls.Add(this.nupThoiGian);
            this.Controls.Add(this.lblTG);
            this.Controls.Add(this.btnThuBai);
            this.Controls.Add(this.btnPhatDe);
            this.Controls.Add(this.btnXoa);
            this.Controls.Add(this.btnThem);
            this.Controls.Add(this.gbDeThi);
            this.Controls.Add(this.btnChonLuuBT);
            this.Controls.Add(this.btnChonGuiDT);
            this.Controls.Add(this.txtLuuBaiThi);
            this.Controls.Add(this.txtGuiDeThi);
            this.Controls.Add(this.lblLuuBaiThi);
            this.Controls.Add(this.lblGuiDeThi);
            this.Controls.Add(this.btnLayDS);
            this.Controls.Add(this.btnDisConnect);
            this.Controls.Add(this.btnConnect);
            this.Font = new System.Drawing.Font("Times New Roman", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Server";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Server";
            this.gbDeThi.ResumeLayout(false);
            this.gbDeThi.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nupThoiGian)).EndInit();
            this.gbDSMay.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnDisConnect;
        private System.Windows.Forms.Button btnLayDS;
        private System.Windows.Forms.Label lblGuiDeThi;
        private System.Windows.Forms.Label lblLuuBaiThi;
        private System.Windows.Forms.TextBox txtGuiDeThi;
        private System.Windows.Forms.TextBox txtLuuBaiThi;
        private System.Windows.Forms.Button btnChonGuiDT;
        private System.Windows.Forms.Button btnChonLuuBT;
        private System.Windows.Forms.GroupBox gbDeThi;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnThem;
        private System.Windows.Forms.Button btnXoa;
        private System.Windows.Forms.Button btnPhatDe;
        private System.Windows.Forms.Button btnThuBai;
        private System.Windows.Forms.Label lblTG;
        private System.Windows.Forms.NumericUpDown nupThoiGian;
        private System.Windows.Forms.Label lblTGConLai;
        private System.Windows.Forms.Label lblDemTG;
        private System.Windows.Forms.GroupBox gbDSMay;
        private System.Windows.Forms.FlowLayoutPanel flpnDanhSachMay;
    }
}

