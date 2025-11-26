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

            int soMayCanTao;
            try
            {
                soMayCanTao = TinhSoMay(ipStart, ipEnd);
                if (soMayCanTao <= 0)
                {
                    MessageBox.Show("IP cuối phải lớn hơn IP đầu!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Định dạng IP không hợp lệ! " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            List<ClientInfo> ds = TaoDanhSachMay(ipStart, soMayCanTao);

            OnDanhSachMayCreated?.Invoke(ds);

            this.Close();
        }

        private int TinhSoMay(string ipStart, string ipEnd)
        {
            string[] partsStart = ipStart.Split('.');
            string[] partsEnd = ipEnd.Split('.');

            if (partsStart.Length != 4 || partsEnd.Length != 4)
            {
                throw new Exception("IP phải có 4 phần.");
            }

            // Giả định chỉ thay đổi octet cuối cùng
            int lastStart = int.Parse(partsStart[3]);
            int lastEnd = int.Parse(partsEnd[3]);

            if (lastEnd < lastStart) 
                return 0;

            // Số máy con = (IP cuối) - (IP đầu) + 1
            return lastEnd - lastStart + 1;
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
