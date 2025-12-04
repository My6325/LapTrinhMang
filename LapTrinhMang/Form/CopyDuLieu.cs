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

            // Kiểm tra máy nguồn đã điểm danh chưa
            if (string.IsNullOrEmpty(mayNguon.MSSV) || mayNguon.MSSV == "Mới/Chưa ĐD")
            {
                MessageBox.Show(
                    $"Máy nguồn ({mayNguon.IP}) chưa điểm danh!\n\n" +
                    $"Vui lòng yêu cầu máy này điểm danh trước khi copy dữ liệu.",
                    "Cảnh báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Kiểm tra máy đích CHƯA điểm danh (chỉ copy sang máy chưa điểm danh)
            if (!string.IsNullOrEmpty(mayDich.MSSV) && mayDich.MSSV != "Mới/Chưa ĐD")
            {
                MessageBox.Show(
                    $"Không thể copy dữ liệu sang máy đích ({mayDich.IP})!\n\n" +
                    $"Máy đích đã có điểm danh ({mayDich.MSSV} - {mayDich.HoTen}).\n" +
                    $"Chỉ có thể copy dữ liệu sang máy chưa điểm danh.",
                    "Cảnh báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Kiểm tra máy nguồn đã kết nối chưa
            if (!mayNguon.IsConnected)
            {
                MessageBox.Show(
                    $"Máy nguồn ({mayNguon.IP}) chưa kết nối đến server!\n\n" +
                    $"Vui lòng đảm bảo máy này đã kết nối trước khi copy dữ liệu.",
                    "Cảnh báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Kiểm tra máy đích đã kết nối chưa
            if (!mayDich.IsConnected)
            {
                MessageBox.Show(
                    $"Máy đích ({mayDich.IP}) chưa kết nối đến server!\n\n" +
                    $"Vui lòng đảm bảo máy này đã kết nối trước khi copy dữ liệu.",
                    "Cảnh báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Xác nhận
            string thongTinMayDich = string.IsNullOrEmpty(mayDich.MSSV) || mayDich.MSSV == "Mới/Chưa ĐD"
                ? "Chưa điểm danh"
                : $"{mayDich.MSSV} - {mayDich.HoTen}";
            
            DialogResult result = MessageBox.Show(
                $"Bạn có chắc muốn copy dữ liệu từ máy {mayNguon.IP} ({mayNguon.MSSV} - {mayNguon.HoTen}) sang máy {mayDich.IP} ({thongTinMayDich})?\n\n" +
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

                // 2. Yêu cầu máy nguồn gửi dữ liệu bài làm (đã kiểm tra kết nối ở trên)
                if (serverSocket != null)
                {
                    onCopyComplete?.Invoke(mayNguon.IP, mayDich.IP);
                    
                    // Yêu cầu máy nguồn gửi dữ liệu với flag COPY_DATA (không hiển thị thông báo)
                    serverSocket.SendMessageToClient(mayNguon.IP, $"COPY_DATA_REQUEST|{mayDich.IP}");
                }
                else
                {
                    onCopyComplete?.Invoke(mayNguon.IP, mayDich.IP);
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
