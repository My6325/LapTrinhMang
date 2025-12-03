using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LapTrinhMang
{
    public partial class DanhSachDiemDanh : Form
    {
        public DanhSachDiemDanh()
        {
            InitializeComponent();
        }

        private void DanhSachDiemDanh_Load(object sender, EventArgs e)
        {
            LoadLogDiemDanh();
        }

        public void LoadLogDiemDanh()
        {
            lvDSDD.Items.Clear();
            string today = DateTime.Now.ToString("ddMMyyyy");
            string logFileName = $"DiemDanh-{today}.txt";

            string currentDir = Directory.GetCurrentDirectory();
            string logFilePath = Path.Combine(Path.GetFullPath(Path.Combine(currentDir, @"..\..\..")), logFileName);

            if (!File.Exists(logFilePath))
            {
                MessageBox.Show($"Chưa có sinh viên nào điểm danh trong ngày hôm nay ({logFileName})!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var lines = File.ReadAllLines(logFilePath).Skip(1);

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split(',');
                    if (parts.Length >= 4)
                    {
                        string mssv = parts[0].Trim();
                        string hoTen = parts[1].Trim();
                        string lop = parts[2].Trim();
                        string gioDiemDanh = parts[3].Trim(); 

                        ListViewItem item = new ListViewItem(mssv);
                        item.SubItems.Add(hoTen);
                        item.SubItems.Add(lop);
                        item.SubItems.Add(gioDiemDanh);

                        lvDSDD.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi đọc file log điểm danh: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
