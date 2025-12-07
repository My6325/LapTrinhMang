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
                                    cbTTSV.ValueMember = "MSSV";
                                    cbTTSV.DisplayMember = null;
                                    cbTTSV.FormattingEnabled = true;

                                    if (cbTTSV.Tag == null)
                                    {
                                        cbTTSV.Format += CbTTSV_Format;
                                        cbTTSV.Tag = true; // Đánh dấu đã đăng ký
                                    }
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
                            Console.WriteLine($"Đã nhận đường dẫn lưu từ server: {duongDanLuuHienTai}");
                        }));
                    }
                    else if (msg.StartsWith("DIRECTORY|"))
                    {
                        // Nhận thông tin thư mục cần tạo
                        string duongDanThuMuc = msg.Substring("DIRECTORY|".Length).Trim();
                        Invoke(new Action(() =>
                        {
                            try
                            {
                                string thuMuc = string.IsNullOrEmpty(duongDanLuuHienTai)
                                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DefaultDeThi")
                                    : duongDanLuuHienTai;
                                
                                if (!Directory.Exists(thuMuc))
                                    Directory.CreateDirectory(thuMuc);

                                // Tạo thư mục con
                                string duongDanThuMucDayDu = Path.Combine(thuMuc, duongDanThuMuc);
                                if (!Directory.Exists(duongDanThuMucDayDu))
                                {
                                    Directory.CreateDirectory(duongDanThuMucDayDu);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Lỗi khi tạo thư mục: {ex.Message}");
                            }
                        }));
                    }
                    else if (msg.StartsWith("FILENAME|"))
                    {
                        // Lưu tên file để dùng khi nhận file
                        tenFileHienTai = msg.Substring("FILENAME|".Length).Trim();
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
                        Console.WriteLine($"Nhận COPY_STUDENT_INFO: {msg}");
                        string jsonSinhVien = msg.Substring("COPY_STUDENT_INFO|".Length).Trim();
                        try
                        {
                            var sinhVien = JsonSerializer.Deserialize<StudentInfo>(jsonSinhVien);
                            if (sinhVien != null)
                            {
                                Invoke(new Action(() =>
                                {
                                    Console.WriteLine($"Đã parse thông tin sinh viên: {sinhVien.MSSV} - {sinhVien.HoTen}");
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
                                        cbTTSV.DisplayMember = null;
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
                            MessageBox.Show("Đã hết thời gian làm bài.\nBạn có 1 phút để lưu bài!",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }));
                    }
                    else if (msg.StartsWith("THU_HOI_DE_THI"))
                    {
                        // Thu hồi đề thi: xóa các file đề thi được chỉ định
                        Invoke(new Action(() =>
                        {
                            if (msg.Contains("|"))
                            {
                                // Có danh sách file cụ thể
                                string jsonDanhSachFile = msg.Substring("THU_HOI_DE_THI|".Length);
                                try
                                {
                                    var danhSachTenFile = JsonSerializer.Deserialize<List<string>>(jsonDanhSachFile);
                                    ThuHoiDeThi(danhSachTenFile);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Lỗi khi parse danh sách file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            else
                            {
                                // Không có danh sách, xóa tất cả (tương thích với code cũ)
                                ThuHoiDeThi(null);
                            }
                        }));
                    }
                    else if (msg.StartsWith("DIEMDANH|"))
                    {
                        Invoke(new Action(() =>
                        {
                            string mssv = msg.Substring("DIEMDANH|".Length);
                            var sv = dsSinhVienClient.FirstOrDefault(s => s.MSSV == mssv);

                            string hoTen = sv != null ? sv.HoTen : "Sinh viên không có trong danh sách";
                            MessageBox.Show($"Điểm danh thành công cho {hoTen} ({mssv})!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }));
                        
                    }
                    else if (msg.StartsWith("DIEMDANH_DA_CO|"))
                    {
                        string mssv = msg.Substring("DIEMDANH_DA_CO|".Length);
                        if (sinhVienDaChon != null && sinhVienDaChon.MSSV == mssv)
                        {
                            MessageBox.Show($"{sinhVienDaChon.HoTen} ({mssv}) đã được điểm danh trước đó!",
                                "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            btnĐiemDanh.Enabled = true;
                        }
                    }

                    else if (msg == "DISCONNECT_REQUEST")
                    {
                        Invoke(new Action(() =>
                        {
                            // 1. Client chủ động đóng kết nối (Gửi tín hiệu DISCONNECTED tới Server)
                            if (socket.IsConnected)
                            {
                                socket.Disconnect();

                                // 2. Cập nhật giao diện Client
                                CapNhatGiaoDien(false);

                                MessageBox.Show("Server đã yêu cầu ngắt kết nối.", "Ngắt kết nối", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }));
                    }
                    else
                    {
                        // Tin nhắn thông thường - hiển thị MessageBox
                        Invoke(new Action(() =>
                        {
                            try
                            {
                                MessageBox.Show(msg, "Tin nhắn từ Server", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Lỗi khi hiển thị tin nhắn: {ex.Message}");
                            }
                        }));
                    }
                };

                socket.OnReceiveFile += (duLieuFile) =>
                {
                    Invoke(new Action(() =>
                    {
                        try
                        {
                            Console.WriteLine($"Nhận file từ server, kích thước: {duLieuFile.Length} bytes, tên file hiện tại: {tenFileHienTai}");
                            string thuMuc = string.IsNullOrEmpty(duongDanLuuHienTai)
                                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DefaultDeThi") // Dùng Documents làm dự phòng
                                : duongDanLuuHienTai;
                            if (!Directory.Exists(thuMuc))
                                Directory.CreateDirectory(thuMuc);

                            // Lưu file với tên đã nhận hoặc tên mặc định
                            string tenFile = string.IsNullOrEmpty(tenFileHienTai)
                                ? $"DeThi_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                                : tenFileHienTai;

                            // Tạo đường dẫn đầy đủ, bao gồm cả thư mục con nếu có
                            string duongDanFile = Path.Combine(thuMuc, tenFile);
                            
                            // Tạo thư mục cha nếu chưa tồn tại (để hỗ trợ thư mục con)
                            string thuMucCha = Path.GetDirectoryName(duongDanFile);
                            if (!string.IsNullOrEmpty(thuMucCha) && !Directory.Exists(thuMucCha))
                            {
                                Directory.CreateDirectory(thuMucCha);
                            }

                            File.WriteAllBytes(duongDanFile, duLieuFile);
                            Console.WriteLine($"Đã lưu file: {duongDanFile}");

                            // Chỉ hiển thị tên file gốc (không có đường dẫn) nếu là file ở thư mục gốc
                            if (!tenFile.Contains("\\") && !tenFile.Contains("/"))
                            {
                                string tenFileHienThi = Path.GetFileName(tenFile);
                                txtDeThi.Text = tenFileHienThi;
                            }

                            tenFileHienTai = "";
                            //duongDanLuuHienTai = "";
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Lỗi khi lưu file: {ex.Message}");
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

                //MessageBox.Show($"Đã gửi điểm danh cho {sinhVienDaChon.HoTen} ({mssv})!");
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
            if (!DaKetNoi)
            {
                MessageBox.Show("Chưa kết nối đến server", "Lỗi Copy", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string thuMucNguon = duongDanLuuHienTai;

            if (string.IsNullOrEmpty(thuMucNguon) || !Directory.Exists(thuMucNguon))
            {
                MessageBox.Show($"Thư mục phát đề không tồn tại: {thuMucNguon}", "Lỗi Copy", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Gửi thông tin sinh viên trước để server thiết lập mapping
                if (sinhVienDaChon != null)
                {
                    string jsonSinhVien = JsonSerializer.Serialize(sinhVienDaChon);
                    socket.SendMessage($"COPY_STUDENT_INFO|{jsonSinhVien}");
                    Thread.Sleep(500); // Đợi server xử lý và thiết lập mapping
                }

                // Gửi tất cả thư mục con sau khi COPY_STUDENT_INFO đã được xử lý (kể cả thư mục rỗng)
                string[] danhSachThuMuc = Directory.GetDirectories(thuMucNguon, "*", SearchOption.AllDirectories);
                foreach (string duongDanThuMuc in danhSachThuMuc)
                {
                    // Tính đường dẫn tương đối từ thư mục gốc
                    string duongDanTuongDoi = duongDanThuMuc;
                    if (duongDanThuMuc.StartsWith(thuMucNguon, StringComparison.OrdinalIgnoreCase))
                    {
                        duongDanTuongDoi = duongDanThuMuc.Substring(thuMucNguon.Length);
                        if (duongDanTuongDoi.StartsWith("\\") || duongDanTuongDoi.StartsWith("/"))
                        {
                            duongDanTuongDoi = duongDanTuongDoi.Substring(1);
                        }
                    }
                    
                    // Gửi thông tin thư mục
                    socket.SendMessage($"DIRECTORY|{duongDanTuongDoi}");
                    Thread.Sleep(200);
                }

                // Gửi các file từ thư mục phát đề (bao gồm cả thư mục con)
                string[] danhSachFile = Directory.GetFiles(thuMucNguon, "*", SearchOption.AllDirectories);
                int soLuongFile = 0;
                int soLuongFileDaGui = 0;

                foreach (string duongDanFile in danhSachFile)
                {
                    // Bỏ qua file ZIP nếu có
                    if (Path.GetExtension(duongDanFile).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Tính đường dẫn tương đối từ thư mục gốc để giữ nguyên cấu trúc thư mục
                    string duongDanTuongDoi = duongDanFile;
                    if (duongDanFile.StartsWith(thuMucNguon, StringComparison.OrdinalIgnoreCase))
                    {
                        duongDanTuongDoi = duongDanFile.Substring(thuMucNguon.Length);
                        if (duongDanTuongDoi.StartsWith("\\") || duongDanTuongDoi.StartsWith("/"))
                        {
                            duongDanTuongDoi = duongDanTuongDoi.Substring(1);
                        }
                    }
                    
                    try
                    {
                        byte[] duLieuFile = File.ReadAllBytes(duongDanFile);

                        // Gửi đường dẫn tương đối (bao gồm cả thư mục con nếu có) trước
                        socket.SendMessage($"FILENAME|{duongDanTuongDoi}");
                        Thread.Sleep(300);
                        socket.SendFile(duLieuFile);
                        Thread.Sleep(500); 

                        soLuongFileDaGui++;
                    }
                    catch
                    {
                        
                    }

                    soLuongFile++;
                }

                // Gửi tín hiệu hoàn thành copy
                socket.SendMessage("COPY_DATA_COMPLETE");
                
                // Đợi lâu hơn để đảm bảo tất cả dữ liệu đã được gửi hoàn toàn
                Thread.Sleep(1000);
                
                // Xóa dữ liệu sau khi đã gửi thành công
                if (soLuongFileDaGui > 0)
                {
                    XoaDuLieuDaCopy();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gửi dữ liệu copy: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Xóa dữ liệu sau khi đã copy sang máy đích
        /// </summary>
        private void XoaDuLieuDaCopy()
        {
            try
            {
                string thuMucDeThi = string.IsNullOrEmpty(duongDanLuuHienTai)
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DefaultDeThi")
                    : duongDanLuuHienTai;

                if (!Directory.Exists(thuMucDeThi))
                {
                    Console.WriteLine($"Thư mục không tồn tại: {thuMucDeThi}");
                    return;
                }

                // Lấy danh sách tất cả file trong thư mục và thư mục con (bao gồm cả file ZIP)
                string[] danhSachFile = Directory.GetFiles(thuMucDeThi, "*", SearchOption.AllDirectories);
                int soLuongFile = danhSachFile.Length;

                if (soLuongFile == 0)
                {
                    Console.WriteLine("Không có file nào để xóa.");
                    return;
                }

                Console.WriteLine($"Bắt đầu xóa {soLuongFile} file từ thư mục: {thuMucDeThi}");

                // Xóa tất cả file (bao gồm cả file ZIP)
                int soFileDaXoa = 0;
                List<string> danhSachFileLoi = new List<string>();
                
                foreach (string duongDanFile in danhSachFile)
                {
                    try
                    {
                        // Kiểm tra file có tồn tại không
                        if (!File.Exists(duongDanFile))
                        {
                            continue;
                        }

                        // Thử xóa file nhiều lần nếu file đang được sử dụng
                        int soLanThu = 0;
                        bool daXoaThanhCong = false;
                        int delayTime = 500; // Tăng thời gian đợi giữa các lần thử
                        
                        while (soLanThu < 5 && !daXoaThanhCong) // Tăng số lần thử lên 5
                        {
                            try
                            {
                                // Đảm bảo file không đang được sử dụng
                                using (FileStream fs = File.Open(duongDanFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                                {
                                    fs.Close();
                                }
                                
                                File.Delete(duongDanFile);
                                daXoaThanhCong = true;
                                soFileDaXoa++;
                                Console.WriteLine($"Đã xóa file: {Path.GetFileName(duongDanFile)}");
                            }
                            catch (IOException)
                            {
                                // File đang được sử dụng, đợi một chút rồi thử lại
                                soLanThu++;
                                if (soLanThu < 5)
                                {
                                    Thread.Sleep(delayTime);
                                    delayTime += 200; // Tăng dần thời gian đợi
                                }
                                Console.WriteLine($"Lần thử {soLanThu}: File đang được sử dụng, đợi {delayTime}ms...");
                            }
                            catch (UnauthorizedAccessException)
                            {
                                // Không có quyền xóa file
                                danhSachFileLoi.Add(Path.GetFileName(duongDanFile));
                                Console.WriteLine($"Không có quyền xóa file: {Path.GetFileName(duongDanFile)}");
                                break;
                            }
                        }
                        
                        if (!daXoaThanhCong)
                        {
                            danhSachFileLoi.Add(Path.GetFileName(duongDanFile));
                            Console.WriteLine($"Không thể xóa file sau {soLanThu} lần thử: {Path.GetFileName(duongDanFile)}");
                        }
                    }
                    catch (Exception ex)
                    {
                        danhSachFileLoi.Add(Path.GetFileName(duongDanFile));
                        Console.WriteLine($"Lỗi khi xóa file {Path.GetFileName(duongDanFile)}: {ex.Message}");
                    }
                }

                // Xóa tất cả thư mục rỗng sau khi xóa file
                // Lấy danh sách tất cả thư mục con (từ sâu nhất lên)
                string[] danhSachThuMuc = Directory.GetDirectories(thuMucDeThi, "*", SearchOption.AllDirectories);
                int soThuMucDaXoa = 0;
                List<string> danhSachThuMucLoi = new List<string>();
                
                // Sắp xếp theo độ sâu (thư mục sâu nhất trước) để xóa từ trong ra ngoài
                Array.Sort(danhSachThuMuc, (a, b) => b.Length.CompareTo(a.Length));
                
                foreach (string duongDanThuMuc in danhSachThuMuc)
                {
                    try
                    {
                        // Kiểm tra thư mục có tồn tại và rỗng không
                        if (Directory.Exists(duongDanThuMuc))
                        {
                            // Kiểm tra thư mục rỗng (không có file và không có thư mục con)
                            string[] fileTrongThuMuc = Directory.GetFiles(duongDanThuMuc);
                            string[] thuMucTrongThuMuc = Directory.GetDirectories(duongDanThuMuc);
                            
                            if (fileTrongThuMuc.Length == 0 && thuMucTrongThuMuc.Length == 0)
                            {
                                Directory.Delete(duongDanThuMuc, false);
                                soThuMucDaXoa++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        danhSachThuMucLoi.Add(Path.GetFileName(duongDanThuMuc));
                        Console.WriteLine($"Lỗi khi xóa thư mục {duongDanThuMuc}: {ex.Message}");
                    }
                }

                // Xóa tên file hiển thị trên form
                txtDeThi.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Lỗi khi xóa dữ liệu đã copy: {ex.Message}",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Console.WriteLine($"Lỗi trong XoaDuLieuDaCopy: {ex.Message}");
            }
        }

        /// <summary>
        /// Thu hồi đề thi: xóa các file đề thi được chỉ định trong danh sách
        /// </summary>
        /// <param name="danhSachTenFile">Danh sách tên file cần xóa. Nếu null, xóa tất cả file đề thi</param>
        private void ThuHoiDeThi(List<string> danhSachTenFile)
        {
            try
            {
                string thuMucDeThi = string.IsNullOrEmpty(duongDanLuuHienTai)
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DefaultDeThi")
                    : duongDanLuuHienTai;

                if (!Directory.Exists(thuMucDeThi))
                {
                    // Thư mục không tồn tại, không cần thông báo
                    return;
                }

                // Lấy danh sách tất cả file trong thư mục
                string[] danhSachFile = Directory.GetFiles(thuMucDeThi);
                
                List<string> danhSachFileCanXoa = new List<string>();

                if (danhSachTenFile != null && danhSachTenFile.Count > 0)
                {
                    // Chỉ xóa các file có tên trong danh sách (so sánh không phân biệt hoa thường)
                    foreach (string duongDanFile in danhSachFile)
                    {
                        string tenFile = Path.GetFileName(duongDanFile);
                        // So sánh không phân biệt hoa thường để tránh lỗi
                        if (danhSachTenFile.Any(x => x.Equals(tenFile, StringComparison.OrdinalIgnoreCase)))
                        {
                            danhSachFileCanXoa.Add(duongDanFile);
                        }
                    }
                }
                else
                {
                    // Nếu không có danh sách cụ thể, xóa tất cả file đề thi (tương thích với code cũ)
                    string[] extensionsDeThi = { ".pdf", ".docx", ".doc", ".txt", ".xlsx", ".xls", ".pptx", ".ppt" };
                    foreach (string duongDanFile in danhSachFile)
                    {
                        string extension = Path.GetExtension(duongDanFile).ToLower();
                        if (extensionsDeThi.Contains(extension))
                        {
                            danhSachFileCanXoa.Add(duongDanFile);
                        }
                    }
                }

                if (danhSachFileCanXoa.Count == 0)
                {
                    return;
                }

                // Xóa các file đã được chỉ định
                int soFileDaXoa = 0;
                List<string> danhSachFileLoi = new List<string>();
                
                foreach (string duongDanFile in danhSachFileCanXoa)
                {
                    try
                    {
                        File.Delete(duongDanFile);
                        soFileDaXoa++;
                    }
                    catch (Exception ex)
                    {
                        string tenFile = Path.GetFileName(duongDanFile);
                        danhSachFileLoi.Add(tenFile);
                        Console.WriteLine($"Không thể xóa file {duongDanFile}: {ex.Message}");
                    }
                }

                // Kiểm tra xem còn file đề thi nào trong thư mục không
                string[] danhSachFileConLai = Directory.GetFiles(thuMucDeThi);
                bool conFileDeThi = false;
                if (danhSachTenFile != null && danhSachTenFile.Count > 0)
                {
                    // Kiểm tra xem còn file nào trong danh sách không
                    conFileDeThi = danhSachFileConLai.Any(f => 
                        danhSachTenFile.Any(x => x.Equals(Path.GetFileName(f), StringComparison.OrdinalIgnoreCase)));
                }
                else
                {
                    // Kiểm tra xem còn file đề thi nào không
                    string[] extensionsDeThi = { ".pdf", ".docx", ".doc", ".txt", ".xlsx", ".xls", ".pptx", ".ppt" };
                    conFileDeThi = danhSachFileConLai.Any(f => 
                        extensionsDeThi.Contains(Path.GetExtension(f).ToLower()));
                }

                // Xóa tên file hiển thị trên form nếu không còn file đề thi nào
                if (!conFileDeThi)
                {
                    txtDeThi.Text = "";
                }

                // Không hiển thị thông báo, chỉ ghi log nếu có lỗi
                if (danhSachFileLoi.Count > 0)
                {
                    Console.WriteLine($"Không thể xóa {danhSachFileLoi.Count} file: {string.Join(", ", danhSachFileLoi)}");
                }
            }
            catch (Exception ex)
            {
                // Chỉ ghi log lỗi, không hiển thị thông báo
                Console.WriteLine($"Lỗi khi thu hồi đề thi: {ex.Message}");
            }
        }

        private void CbTTSV_Format(object sender, ListControlConvertEventArgs e)
        {
            if (e.ListItem is StudentInfo sv)
                e.Value = $"{sv.MSSV} - {sv.HoTen}";
        }
    }
}
