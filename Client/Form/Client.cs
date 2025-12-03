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
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Client
{
    public partial class Client : Form
    {
        private ClientSocket socket = new ClientSocket();
        private StudentInfo sinhVienDaChon;
        private int serverPort = 8888;

        private List<StudentInfo> dsSinhVienClient = new List<StudentInfo>();
        private bool daDangKySuKien = false;
        private string tenFileHienTai = "";
        private string duongDanLuuHienTai = "";

        private bool DaKetNoi => socket.IsConnected;

        public Client()
        {
            InitializeComponent();
            TaiDuLieuBanDau();
        }

        private void TaiDuLieuBanDau()
        {
            txtIP.Text = LayDiaChiIPLocal();

            cbTTSV.DataSource = null;

            CapNhatGiaoDien(false);
            duongDanLuuHienTai = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "DeThiBaiLam");
           // Console.WriteLine($"Đề thi sẽ được lưu tại: {deThiPath}");
        }

        private void CapNhatGiaoDien(bool connected)
        {
            btnConnect.Text = connected ? "Disconnect" : "Connect";
            btnĐiemDanh.Enabled = connected;
            txtIP.Enabled = !connected;

            // Chỉ cho phép chọn TTSV khi đã kết nối và chưa điểm danh
            cbTTSV.Enabled = connected;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            string ip = txtIP.Text.Trim();
            int port = serverPort;

            if (string.IsNullOrEmpty(ip))
            {
                MessageBox.Show("Vui lòng nhập IP Server!");
                return;
            }

            if (DaKetNoi)
            {
                socket.Disconnect();
                CapNhatGiaoDien(false);
                return;
            }

            if (!daDangKySuKien)
            {
                socket.OnReceiveMessage += (msg) =>
                {
                    if (msg.StartsWith("DSSV|"))
                    {
                        string json = msg.Substring("DSSV|".Length);

                        try
                        {
                            var ds = JsonSerializer.Deserialize<List<StudentInfo>>(json);
                            if (ds != null)
                            {
                                Invoke(new Action(() =>
                                {
                                    dsSinhVienClient = ds;
                                    cbTTSV.DataSource = null;
                                    cbTTSV.DataSource = dsSinhVienClient;
                                    cbTTSV.DisplayMember = "HoTen";
                                    cbTTSV.ValueMember = "MSSV";
                                }));
                            }
                        }
                        catch (Exception ex)
                        {
                            Invoke(new Action(() =>
                            {
                                MessageBox.Show("Lỗi parse DSSV từ server: " + ex.Message);
                            }));
                        }
                    }
                    else if (msg.StartsWith("SAVEPATH|")) 
                    {
                        duongDanLuuHienTai = msg.Substring("SAVEPATH|".Length).Trim();
                        Invoke(new Action(() =>
                        {
                            Console.WriteLine($"Đã nhận đường dẫn lưu: {duongDanLuuHienTai}");
                        }));
                    }
                    else if (msg.StartsWith("FILENAME|"))
                    {
                        // Lưu tên file để dùng khi nhận file
                        tenFileHienTai = msg.Substring("FILENAME|".Length);
                        Invoke(new Action(() =>
                        {
                            Console.WriteLine($"Chuẩn bị nhận file: {tenFileHienTai}");
                        }));
                    }
                    else if (msg == "YEUCAU_NOPBAI")
                    {
                        Invoke(new Action(() =>
                        {
                            GuiBaiLamLenServer();
                        }));
                    }
                    else if (msg.StartsWith("COPY_DATA_REQUEST"))
                    {
                        Invoke(new Action(() =>
                        {
                            GuiDuLieuCopyLenServer();
                        }));
                    }
                    else if (msg.StartsWith("COPY_STUDENT_INFO|"))
                    {
                        // Nhận thông tin sinh viên từ máy nguồn khi copy dữ liệu
                        string jsonSinhVien = msg.Substring("COPY_STUDENT_INFO|".Length).Trim();
                        try
                        {
                            var sinhVien = JsonSerializer.Deserialize<StudentInfo>(jsonSinhVien);
                            if (sinhVien != null)
                            {
                                Invoke(new Action(() =>
                                {
                                    // Tìm sinh viên trong danh sách
                                    var sinhVienTimThay = dsSinhVienClient.FirstOrDefault(s => s.MSSV == sinhVien.MSSV);
                                    if (sinhVienTimThay != null)
                                    {
                                        // Cập nhật thông tin nếu có khác biệt
                                        sinhVienTimThay.HoTen = sinhVien.HoTen;
                                        sinhVienTimThay.Lop = sinhVien.Lop;
                                        
                                        // Chọn sinh viên trong combobox
                                        cbTTSV.SelectedValue = sinhVien.MSSV;
                                        
                                        // Cập nhật các textbox
                                        txtMSSV.Text = sinhVien.MSSV;
                                        txtHoTen.Text = sinhVien.HoTen;
                                        txtLop.Text = sinhVien.Lop;
                                    }
                                    else
                                    {
                                        // Nếu không tìm thấy, thêm vào danh sách
                                        dsSinhVienClient.Add(sinhVien);
                                        cbTTSV.DataSource = null;
                                        cbTTSV.DataSource = dsSinhVienClient;
                                        cbTTSV.DisplayMember = "HoTen";
                                        cbTTSV.ValueMember = "MSSV";
                                        cbTTSV.SelectedValue = sinhVien.MSSV;
                                        
                                        // Cập nhật các textbox
                                        txtMSSV.Text = sinhVien.MSSV;
                                        txtHoTen.Text = sinhVien.HoTen;
                                        txtLop.Text = sinhVien.Lop;
                                    }
                                }));
                            }
                        }
                        catch (Exception ex)
                        {
                            Invoke(new Action(() =>
                            {
                                Console.WriteLine($"Lỗi parse thông tin sinh viên: {ex.Message}");
                            }));
                        }
                    }
                    else if (msg.StartsWith("BATDAU|"))
                    {
                        int soPhut = int.Parse(msg.Split('|')[1]);
                        Invoke(new Action(() =>
                        {
                            MessageBox.Show($"Bài thi bắt đầu! Thời gian: {soPhut} phút", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }));
                    }
                    else if (msg == "HETGIO")
                    {
                        Invoke(new Action(() =>
                        {
                        }));
                    }
                    else if (msg == "THU_HOI_DE_THI")
                    {
                        // Thu hồi đề thi: xóa tất cả file trong thư mục đề thi
                        Invoke(new Action(() =>
                        {
                            ThuHoiDeThi();
                        }));
                    }
                };

                socket.OnReceiveFile += (duLieuFile) =>
                {
                    Invoke(new Action(() =>
                    {
                        try
                        {
                            string thuMuc = string.IsNullOrEmpty(duongDanLuuHienTai)
                                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DefaultDeThi") // Dùng Documents làm dự phòng
                                : duongDanLuuHienTai;
                            if (!Directory.Exists(thuMuc))
                                Directory.CreateDirectory(thuMuc);

                            // Lưu file với tên đã nhận hoặc tên mặc định
                            string tenFile = string.IsNullOrEmpty(tenFileHienTai)
                                ? $"DeThi_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                                : tenFileHienTai;

                            string duongDanFile = Path.Combine(thuMuc, tenFile);
                            File.WriteAllBytes(duongDanFile, duLieuFile);

                            string tenFileHienThi = Path.GetFileName(tenFile);
                            Invoke(new Action(() =>
                            {
                                txtDeThi.Text = tenFileHienThi;
                            }));

                            tenFileHienTai = "";
                            //duongDanLuuHienTai = "";
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Lỗi khi lưu file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }));
                };

                socket.OnDisconnected += () =>
                {
                    Invoke(new Action(() =>
                    {
                        MessageBox.Show("Mất kết nối Server!");
                        CapNhatGiaoDien(false);
                    }));
                };

                daDangKySuKien = true;
            }

            // 2SAU KHI GẮN EVENT MỚI CONNECT
            bool thanhCong = socket.Connect(ip, port);
            if (thanhCong)
            {
                MessageBox.Show("Kết nối server thành công!");
                CapNhatGiaoDien(true);
            }
            else
            {
                string thongBaoLoi = !string.IsNullOrEmpty(socket.LastError) 
                    ? socket.LastError 
                    : "Kết nối thất bại! Hãy kiểm tra IP hoặc Server chưa chạy.";
                MessageBox.Show(thongBaoLoi, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void btnĐiemDanh_Click(object sender, EventArgs e)
        {
            if (sinhVienDaChon == null)
            {
                MessageBox.Show("Vui lòng chọn tên sinh viên trước khi điểm danh.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!DaKetNoi)
            {
                MessageBox.Show("Chưa kết nối Server.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Gửi MSSV đã chọn lên Server
                string mssv = sinhVienDaChon.MSSV;
                socket.SendMessage($"DIEMDANH|{mssv}");

                MessageBox.Show($"Đã gửi điểm danh cho {sinhVienDaChon.HoTen} ({mssv})!");
                btnĐiemDanh.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gửi điểm danh: {ex.Message}");
            }
        }

        private void cbTTSV_SelectedIndexChanged(object sender, EventArgs e)
        {
            sinhVienDaChon = cbTTSV.SelectedItem as StudentInfo;
            if (sinhVienDaChon != null)
            {
                txtLop.Text = sinhVienDaChon.Lop;
                txtMSSV.Text = sinhVienDaChon.MSSV;
                txtHoTen.Text = sinhVienDaChon.HoTen;
            }
        }

        private string LayDiaChiIPLocal()
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
        private void GuiBaiLamLenServer()
        {
            if (sinhVienDaChon == null)
            {
                MessageBox.Show("Chưa chọn thông tin sinh viên để nộp bài", "Lỗi Nộp bài", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!DaKetNoi) return;

            string thuMucNguon = duongDanLuuHienTai;
            string tenFileZip = $"{sinhVienDaChon.MSSV}_{DateTime.Now:yyyyMMdd_HHmmss}.zip";

            // Tạo đường dẫn file ZIP tạm thời (sử dụng thư mục Temp của hệ thống)
            string duongDanZipTam = Path.Combine(Path.GetTempPath(), tenFileZip);

            if (string.IsNullOrEmpty(thuMucNguon) || !Directory.Exists(thuMucNguon))
            {
                MessageBox.Show($"Thư mục bài làm không tồn tại", "Lỗi Nén", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                if (File.Exists(duongDanZipTam)) File.Delete(duongDanZipTam);

                // Thực hiện nén
                ZipFile.CreateFromDirectory(thuMucNguon, duongDanZipTam, CompressionLevel.Fastest, false);
                socket.SendMessage($"NOPBAI_FILENAME|{tenFileZip}");
                Thread.Sleep(300);

                //Gửi nội dung file ZIP
                byte[] duLieuFile = File.ReadAllBytes(duongDanZipTam);
                socket.SendFile(duLieuFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gửi bài làm: {ex.Message}", "Lỗi ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                if (File.Exists(duongDanZipTam)) File.Delete(duongDanZipTam);
            }
        }

        /// <summary>
        /// Gửi dữ liệu copy: bài làm (ZIP) và các file từ thư mục phát đề
        /// </summary>
        private void GuiDuLieuCopyLenServer()
        {
            if (sinhVienDaChon == null)
            {
                MessageBox.Show("Chưa chọn thông tin sinh viên để copy dữ liệu", "Lỗi Copy", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!DaKetNoi) return;

            string thuMucNguon = duongDanLuuHienTai;

            if (string.IsNullOrEmpty(thuMucNguon) || !Directory.Exists(thuMucNguon))
            {
                MessageBox.Show($"Thư mục phát đề không tồn tại", "Lỗi Copy", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Gửi thông tin sinh viên trước khi gửi file
                if (sinhVienDaChon != null)
                {
                    string jsonSinhVien = JsonSerializer.Serialize(sinhVienDaChon);
                    socket.SendMessage($"COPY_STUDENT_INFO|{jsonSinhVien}");
                    Thread.Sleep(300);
                }

                // Gửi các file từ thư mục phát đề (không nén)
                string[] danhSachFile = Directory.GetFiles(thuMucNguon);
                int soLuongFile = 0;

                foreach (string duongDanFile in danhSachFile)
                {
                    // Bỏ qua file ZIP nếu có
                    if (Path.GetExtension(duongDanFile).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                        continue;

                    string tenFile = Path.GetFileName(duongDanFile);
                    byte[] duLieuFile = File.ReadAllBytes(duongDanFile);

                    // Gửi tên file trước
                    socket.SendMessage($"FILENAME|{tenFile}");
                    Thread.Sleep(300);

                    // Gửi nội dung file
                    socket.SendFile(duLieuFile);
                    Thread.Sleep(500); // Đợi giữa các file

                    soLuongFile++;
                }

                // Gửi tín hiệu hoàn thành copy
                socket.SendMessage("COPY_DATA_COMPLETE");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gửi dữ liệu copy: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Thu hồi đề thi: xóa tất cả file trong thư mục đề thi
        /// </summary>
        private void ThuHoiDeThi()
        {
            try
            {
                string thuMucDeThi = string.IsNullOrEmpty(duongDanLuuHienTai)
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DefaultDeThi")
                    : duongDanLuuHienTai;

                if (!Directory.Exists(thuMucDeThi))
                {
                    MessageBox.Show("Thư mục đề thi không tồn tại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Lấy danh sách tất cả file trong thư mục
                string[] danhSachFile = Directory.GetFiles(thuMucDeThi);
                int soLuongFile = danhSachFile.Length;

                if (soLuongFile == 0)
                {
                    return; // Không có file nào, không cần thông báo
                }

                // Xóa tất cả file (không cần xác nhận)
                int soFileDaXoa = 0;
                foreach (string duongDanFile in danhSachFile)
                {
                    try
                    {
                        File.Delete(duongDanFile);
                        soFileDaXoa++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Không thể xóa file {duongDanFile}: {ex.Message}");
                    }
                }

                // Xóa tên file hiển thị trên form
                txtDeThi.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Lỗi khi thu hồi đề thi: {ex.Message}",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
