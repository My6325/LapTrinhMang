using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LapTrinhMang
{
    public partial class ucMayDisconnect : UserControl
    {
        private ToolTip toolTip = new ToolTip();

        public ucMayDisconnect()
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
    }
}
