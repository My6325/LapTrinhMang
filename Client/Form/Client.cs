using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Client.Models;
using Client.Networking;
using System.Text.Json;

namespace Client
{
    public partial class Client : Form
    {
        private ClientSocket socket = new ClientSocket();
        private StudentInfo selectedStudent;
        private int serverPort = 8888;

        private List<StudentInfo> dsSinhVienClient = new List<StudentInfo>();

        private bool IsConnected => socket.IsConnected;

        public Client()
        {
            InitializeComponent();
            LoadInitialData();
        }

        private void LoadInitialData()
        {
            txtIP.Text = GetLocalIPAddress();

            cbTTSV.DataSource = dsSinhVienClient;
            cbTTSV.DisplayMember = "HoTen";
            cbTTSV.ValueMember = "MSSV";

            UpdateUI(false);
        }

        private void UpdateUI(bool connected)
        {
            btnConnect.Text = connected ? "Disconnect" : "Connect";
            btnĐiemDanh.Enabled = connected;
            btnNopBai.Enabled = connected; // Giả sử nút Nộp bài tồn tại
            txtIP.Enabled = !connected;

            // Chỉ cho phép chọn TTSV khi đã kết nối và chưa điểm danh
            cbTTSV.Enabled = connected;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            string ip = txtIP.Text.Trim();
            int port = 8888;

            if (string.IsNullOrEmpty(ip))
            {
                MessageBox.Show("Vui lòng nhập IP Server!");
                return;
            }

            bool ok = socket.Connect(ip, port);
            if (ok)
            {
                MessageBox.Show("Kết nối server thành công!");
            }
            else
            {
                MessageBox.Show("Kết nối thất bại! Hãy kiểm tra IP hoặc Server chưa chạy.");
            }

            socket.OnReceiveMessage += (msg) =>
            {
                MessageBox.Show("Server gửi: " + msg);
            };

            socket.OnReceiveFile += (fileBytes) =>
            {
                MessageBox.Show("Đã nhận file từ Server!");
            };

            socket.OnDisconnected += () =>
            {
                MessageBox.Show("Mất kết nối Server!");
            };
        }

        private void btnĐiemDanh_Click(object sender, EventArgs e)
        {
            if (selectedStudent == null)
            {
                MessageBox.Show("Vui lòng chọn tên sinh viên trước khi điểm danh.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!IsConnected)
            {
                MessageBox.Show("Chưa kết nối Server.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Gửi MSSV đã chọn lên Server
                string mssv = selectedStudent.MSSV;
                socket.SendMessage($"DIEMDANH|{mssv}");

                MessageBox.Show($"Đã gửi điểm danh cho {selectedStudent.HoTen} ({mssv})!");
                btnĐiemDanh.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gửi điểm danh: {ex.Message}");
            }
        }

        private void cbTTSV_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedStudent = cbTTSV.SelectedItem as StudentInfo;
            if (selectedStudent != null)
            {
                txtLop.Text = selectedStudent.Lop;
                txtMSSV.Text = selectedStudent.MSSV;
                txtHoTen.Text = selectedStudent.HoTen;
            }
        }

        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }
    }
}
