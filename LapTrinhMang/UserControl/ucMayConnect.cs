using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LapTrinhMang.Models;

namespace LapTrinhMang
{
    public partial class ucMayConnect : UserControl
    {
        private ToolTip toolTip = new ToolTip();
        public ClientInfo ClientInfo { get; set; }
        public new ContextMenuStrip ContextMenuStrip { get; set; }

        public ucMayConnect()
        {
            InitializeComponent();
        }

        public void SetInfo(string mssv, string ip, string hoTen)
        {
            txtMSSV.Text = mssv;
            txtIP.Text = "IP: " + ip;

            toolTip.SetToolTip(this, hoTen);
            toolTip.SetToolTip(txtMSSV, hoTen);
            toolTip.SetToolTip(txtIP, hoTen);
            toolTip.SetToolTip(pictureBox1, hoTen);
        }

        private void ucMayConnect_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && ContextMenuStrip != null)
            {
                ContextMenuStrip.Show(this, e.Location);
            }
        }
    }
}
