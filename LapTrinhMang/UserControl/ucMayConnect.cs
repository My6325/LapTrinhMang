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
    public partial class ucMayConnect : UserControl
    {
        public ucMayConnect()
        {
            InitializeComponent();
        }

        public void SetInfo(string mssv, string ip)
        {
            txtMSSV.Text = mssv;
            txtIP.Text = "IP: " + ip;
        }
    }
}
