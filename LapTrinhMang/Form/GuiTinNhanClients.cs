using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LapTrinhMang.Networking;
using LapTrinhMang.Models;

namespace LapTrinhMang
{
    public partial class GuiTinNhanClients : Form
    {
        private ServerSocket serverSocket;
        private List<ClientInfo> dsMay;

        public GuiTinNhanClients()
        {
            InitializeComponent();
            InitializeEvents();
        }

        public GuiTinNhanClients(ServerSocket serverSocket, List<ClientInfo> dsMay)
        {
            InitializeComponent();
            this.serverSocket = serverSocket;
            this.dsMay = dsMay;
            InitializeEvents();
        }

        public GuiTinNhanClients(ServerSocket serverSocket, List<ClientInfo> dsMay, ClientInfo selectedClient)
        {
            InitializeComponent();
            this.serverSocket = serverSocket;
            this.dsMay = dsMay;
            InitializeEvents();
            if (selectedClient != null)
            {
                rd_GuiTNMotMay.Checked = true;
                txtIPPC.Text = selectedClient.IP;
            }
        }

        private void InitializeEvents()
        {
            // Làm cho hai radio button loại trừ lẫn nhau
            rd_GuiTNMotMay.CheckedChanged += Rd_GuiTNMotMay_CheckedChanged;
            rd_GuiTNNhieuMay.CheckedChanged += Rd_GuiTNNhieuMay_CheckedChanged;
            btnGuiTinNhan.Click += BtnGuiTinNhan_Click;
        }

        private void Rd_GuiTNMotMay_CheckedChanged(object sender, EventArgs e)
        {
            if (rd_GuiTNMotMay.Checked)
            {
                rd_GuiTNNhieuMay.Checked = false;
            }
        }

        private void Rd_GuiTNNhieuMay_CheckedChanged(object sender, EventArgs e)
        {
            if (rd_GuiTNNhieuMay.Checked)
            {
                rd_GuiTNMotMay.Checked = false;
            }
        }

        private void BtnGuiTinNhan_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTinNhan.Text))
            {
                MessageBox.Show("Vui lòng nhập tin nhắn cần gửi!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!rd_GuiTNMotMay.Checked && !rd_GuiTNNhieuMay.Checked)
            {
                MessageBox.Show("Vui lòng chọn phương thức gửi tin nhắn!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (rd_GuiTNMotMay.Checked && string.IsNullOrWhiteSpace(txtIPPC.Text))
            {
                MessageBox.Show("Vui lòng nhập IP PC cần gửi!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (serverSocket == null || dsMay == null)
            {
                MessageBox.Show("Lỗi: Không có kết nối server hoặc danh sách máy!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string tinNhan = txtTinNhan.Text.Trim();
                string message = $"{tinNhan}";

                if (rd_GuiTNMotMay.Checked)
                {
                    string ipPC = txtIPPC.Text.Trim();
                    
                    var may = dsMay.FirstOrDefault(m => 
                        (m.MSSV != null && m.MSSV.Equals(ipPC, StringComparison.OrdinalIgnoreCase)) ||
                        (m.IP != null && m.IP.Equals(ipPC, StringComparison.OrdinalIgnoreCase)) ||
                        (m.HoTen != null && m.HoTen.IndexOf(ipPC, StringComparison.OrdinalIgnoreCase) >= 0)
                    );

                    if (may == null)
                    {
                        MessageBox.Show($"Không tìm thấy máy với IP: {ipPC}!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (!may.IsConnected)
                    {
                        MessageBox.Show($"Máy với IP {ipPC} hiện không kết nối!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Kiểm tra lại IP trước khi gửi
                    if (string.IsNullOrEmpty(may.IP))
                    {
                        MessageBox.Show($"IP của máy không hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    serverSocket.SendMessageToClient(may.IP, message);
                    MessageBox.Show($"Đã gửi tin nhắn đến {may.HoTen} ({may.MSSV}) - IP: {may.IP}!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (rd_GuiTNNhieuMay.Checked)
                {
                    // Gửi tin nhắn đến tất cả máy
                    serverSocket.BroadcastMessage(message);
                    int soMayConnected = dsMay.Count(m => m.IsConnected);
                    MessageBox.Show($"Đã gửi tin nhắn đến {soMayConnected} máy đang kết nối!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gửi tin nhắn: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
