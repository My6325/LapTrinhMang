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
    public partial class FrmNhapIP : Form
    {
        public event Action<List<ClientInfo>> OnDanhSachMayCreated;

        public FrmNhapIP()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            string ipStart = txtIPDau.Text.Trim();
            string ipEnd = txtIPCuoi.Text.Trim();
            string subnet = txtSubnet.Text.Trim();

            int soMay;
            if (!int.TryParse(txtSoMay.Text, out soMay))
            {
                MessageBox.Show("Số máy không hợp lệ!");
                return;
            }

            List<ClientInfo> ds = TaoDanhSachMay(ipStart, soMay);

            OnDanhSachMayCreated?.Invoke(ds);

            this.Close();
        }

        private List<ClientInfo> TaoDanhSachMay(string ipStart, int soMay)
        {
            List<ClientInfo> ds = new List<ClientInfo>();

            string[] parts = ipStart.Split('.');
            int last = int.Parse(parts[3]); 

            for (int i = 0; i < soMay; i++)
            {
                ds.Add(new ClientInfo()
                {
                    IP = $"{parts[0]}.{parts[1]}.{parts[2]}.{last + i}"
                });
            }

            return ds;
        }
    }
}
