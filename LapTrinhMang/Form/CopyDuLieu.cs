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
using LapTrinhMang.Networking;

namespace LapTrinhMang
{
    public partial class CopyDuLieu : Form
    {
        private List<ClientInfo> dsMay;
        private ServerSocket serverSocket;
        private ClientInfo selectedSourceClient;
        private Action<string, string> onCopyComplete;

        public CopyDuLieu()
        {
            InitializeComponent();
        }

        public CopyDuLieu(ServerSocket serverSocket, List<ClientInfo> dsMay, ClientInfo sourceClient = null, Action<string, string> onCopyComplete = null)
        {
            InitializeComponent();
            this.serverSocket = serverSocket;
            this.dsMay = dsMay;
            this.selectedSourceClient = sourceClient;
            this.onCopyComplete = onCopyComplete;
            LoadDanhSachMay();
            if (sourceClient != null)
            {
                // Tự động chọn máy nguồn
                var item = cbMayNguon.Items.Cast<ClientInfo>().FirstOrDefault(m => m.IP == sourceClient.IP);
                if (item != null)
                {
                    cbMayNguon.SelectedItem = item;
                }
            }
        }

        private void LoadDanhSachMay()
        {
            cbMayNguon.Items.Clear();
            cbMayDich.Items.Clear();

            foreach (var may in dsMay)
            {
                cbMayNguon.Items.Add(may);
                cbMayDich.Items.Add(may);
            }

            // Cấu hình hiển thị
            cbMayNguon.DisplayMember = null;
            cbMayNguon.FormattingEnabled = true;
            cbMayNguon.Format += (s, e) =>
            {
                if (e.ListItem is ClientInfo client)
                {
                    string displayText = string.IsNullOrEmpty(client.HoTen)
                        ? $"{client.IP} ({client.MSSV})"
                        : $"{client.IP} - {client.HoTen} ({client.MSSV})";
                    e.Value = displayText;
                }
            };

            cbMayDich.DisplayMember = null;
            cbMayDich.FormattingEnabled = true;
            cbMayDich.Format += (s, e) =>
            {
                if (e.ListItem is ClientInfo client)
                {
                    string displayText = string.IsNullOrEmpty(client.HoTen)
                        ? $"{client.IP} ({client.MSSV})"
                        : $"{client.IP} - {client.HoTen} ({client.MSSV})";
                    e.Value = displayText;
                }
            };
        }

        private void cbMayNguon_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbMayNguon.SelectedItem is ClientInfo client)
            {
                selectedSourceClient = client;
                string info = $"IP: {client.IP}\n";
                info += $"MSSV: {client.MSSV}\n";
                info += $"Họ tên: {client.HoTen}\n";
                info += $"Trạng thái: {(client.IsConnected ? "Đã kết nối" : "Chưa kết nối")}";
                lblThongTinNguon.Text = info;
            }
        }

        private void btnCopyDuLieu_Click(object sender, EventArgs e)
        {
            if (cbMayNguon.SelectedItem == null || cbMayDich.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn máy nguồn và máy đích!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ClientInfo mayNguon = cbMayNguon.SelectedItem as ClientInfo;
            ClientInfo mayDich = cbMayDich.SelectedItem as ClientInfo;

            if (mayNguon.IP == mayDich.IP)
            {
                MessageBox.Show("Máy nguồn và máy đích không thể giống nhau!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Xác nhận
            DialogResult result = MessageBox.Show(
                $"Bạn có chắc muốn copy dữ liệu từ máy {mayNguon.IP} ({mayNguon.MSSV}) sang máy {mayDich.IP}?\n\n" +
                $"Thông tin sẽ được copy:\n" +
                $"- MSSV: {mayNguon.MSSV}\n" +
                $"- Họ tên: {mayNguon.HoTen}\n" +
                $"- Dữ liệu bài làm (nếu có)\n\n" +
                $"Lưu ý: IP của máy đích sẽ được giữ nguyên.",
                "Xác nhận Copy dữ liệu",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                // 1. Copy thông tin sinh viên (MSSV, HoTen) từ máy nguồn sang máy đích
                mayDich.MSSV = mayNguon.MSSV;
                mayDich.HoTen = mayNguon.HoTen;
                // Giữ nguyên IP của máy đích

                // 2. Nếu máy nguồn còn kết nối, yêu cầu gửi dữ liệu bài làm
                if (mayNguon.IsConnected && serverSocket != null)
                {
                    // QUAN TRỌNG: Gọi callback TRƯỚC để thiết lập copyDataTarget trước khi gửi yêu cầu
                    // Điều này đảm bảo khi file đến, server đã biết cần chuyển tiếp đến đâu
                    onCopyComplete?.Invoke(mayNguon.IP, mayDich.IP);
                    
                    // Yêu cầu máy nguồn gửi dữ liệu với flag COPY_DATA
                    serverSocket.SendMessageToClient(mayNguon.IP, $"COPY_DATA_REQUEST|{mayDich.IP}");
                    
                    MessageBox.Show(
                        $"Đã yêu cầu máy nguồn ({mayNguon.IP}) gửi dữ liệu bài làm sang máy {mayDich.IP}.\n" +
                        $"Dữ liệu sẽ được tự động chuyển tiếp khi nhận được.",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    // Nếu máy nguồn không kết nối, chỉ copy thông tin sinh viên
                    // Vẫn gọi callback để cập nhật danh sách máy
                    onCopyComplete?.Invoke(mayNguon.IP, mayDich.IP);
                    
                    MessageBox.Show(
                        $"Đã copy thông tin sinh viên từ máy {mayNguon.IP} sang máy {mayDich.IP}.\n\n" +
                        $"Máy nguồn không kết nối nên không thể copy dữ liệu bài làm.",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                // 4. Đóng form
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi copy dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
