namespace LapTrinhMang
{
    partial class GuiTinNhanClients
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
            this.txtTinNhan = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rd_GuiTNMotMay = new System.Windows.Forms.RadioButton();
            this.rd_GuiTNNhieuMay = new System.Windows.Forms.RadioButton();
            this.btnGuiTinNhan = new System.Windows.Forms.Button();
            this.txtIPPC = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(187, 22);
            this.label1.TabIndex = 0;
            this.label1.Text = "Nhập tin nhắn cần gửi:";
            // 
            // txtTinNhan
            // 
            this.txtTinNhan.Location = new System.Drawing.Point(205, 13);
            this.txtTinNhan.Multiline = true;
            this.txtTinNhan.Name = "txtTinNhan";
            this.txtTinNhan.Size = new System.Drawing.Size(326, 86);
            this.txtTinNhan.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtIPPC);
            this.groupBox1.Controls.Add(this.rd_GuiTNMotMay);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(13, 115);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(249, 193);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Gửi một máy";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rd_GuiTNNhieuMay);
            this.groupBox2.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(268, 115);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(263, 193);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Gửi tất cả máy";
            // 
            // rd_GuiTNMotMay
            // 
            this.rd_GuiTNMotMay.AutoSize = true;
            this.rd_GuiTNMotMay.Location = new System.Drawing.Point(7, 30);
            this.rd_GuiTNMotMay.Name = "rd_GuiTNMotMay";
            this.rd_GuiTNMotMay.Size = new System.Drawing.Size(232, 26);
            this.rd_GuiTNMotMay.TabIndex = 0;
            this.rd_GuiTNMotMay.TabStop = true;
            this.rd_GuiTNMotMay.Text = "Gửi tin nhắn cho một máy";
            this.rd_GuiTNMotMay.UseVisualStyleBackColor = true;
            // 
            // rd_GuiTNNhieuMay
            // 
            this.rd_GuiTNNhieuMay.AutoSize = true;
            this.rd_GuiTNNhieuMay.Location = new System.Drawing.Point(6, 29);
            this.rd_GuiTNNhieuMay.Name = "rd_GuiTNNhieuMay";
            this.rd_GuiTNNhieuMay.Size = new System.Drawing.Size(245, 26);
            this.rd_GuiTNNhieuMay.TabIndex = 0;
            this.rd_GuiTNNhieuMay.TabStop = true;
            this.rd_GuiTNNhieuMay.Text = "Gửi tin nhắn cho tất cả máy";
            this.rd_GuiTNNhieuMay.UseVisualStyleBackColor = true;
            // 
            // btnGuiTinNhan
            // 
            this.btnGuiTinNhan.Font = new System.Drawing.Font("Times New Roman", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGuiTinNhan.Location = new System.Drawing.Point(166, 332);
            this.btnGuiTinNhan.Name = "btnGuiTinNhan";
            this.btnGuiTinNhan.Size = new System.Drawing.Size(187, 56);
            this.btnGuiTinNhan.TabIndex = 3;
            this.btnGuiTinNhan.Text = "Gửi Tin nhắn";
            this.btnGuiTinNhan.UseVisualStyleBackColor = true;
            // 
            // txtIPPC
            // 
            this.txtIPPC.Location = new System.Drawing.Point(7, 111);
            this.txtIPPC.Name = "txtIPPC";
            this.txtIPPC.Size = new System.Drawing.Size(232, 30);
            this.txtIPPC.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(42, 76);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(165, 22);
            this.label2.TabIndex = 0;
            this.label2.Text = "Nhập IP PC cần gửi";
            // 
            // GuiTinNhanClients
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(543, 415);
            this.Controls.Add(this.btnGuiTinNhan);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.txtTinNhan);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "GuiTinNhanClients";
            this.Text = "Gửi tin nhắn cho Clients";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtTinNhan;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rd_GuiTNMotMay;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton rd_GuiTNNhieuMay;
        private System.Windows.Forms.Button btnGuiTinNhan;
        private System.Windows.Forms.TextBox txtIPPC;
        private System.Windows.Forms.Label label2;
    }
}