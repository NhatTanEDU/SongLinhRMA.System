# HƯỚNG DẪN VẬN HÀNH HỆ THỐNG SONGLINHRMA

Tài liệu này hướng dẫn cách khởi chạy và vận hành hệ thống **SongLinhRMA** theo mô hình kết hợp: **Frontend chạy trên Firebase Hosting** và **Backend chạy cục bộ (Local) kết hợp với đường hầm bảo mật Cloudflare Tunnel**.

---

## 🏗️ TỔNG QUAN KIẾN TRÚC HIỆN TẠI

*   **Frontend (Giao diện người dùng):** Được host trực tuyến và miễn phí trên **Firebase Hosting** (`https://onglinh-rma-production.web.app`).
*   **Backend (Hệ thống API):** Chạy trực tiếp trên máy tính Ubuntu của bạn (`http://localhost:5299`).
*   **Cầu nối (Cloudflare Tunnel):** Tạo một đường dẫn HTTPS bảo mật công khai trên Internet trỏ thẳng về Backend máy bạn để Frontend trên Firebase có thể gọi dữ liệu.

---

## 🚀 QUY TRÌNH KHỞI CHẠY HỆ THỐNG HÀNG NGÀY

Mỗi khi bạn muốn bật hệ thống lên để sử dụng hoặc test, hãy thực hiện theo 3 bước sau:

### BƯỚC 1: Khởi chạy API Backend
1. Mở cửa sổ Terminal thứ nhất trên máy Ubuntu của bạn.
2. Di chuyển đến thư mục dự án và chạy lệnh:
   ```bash
   dotnet run --project RMA.Server/RMA.Server.csproj --launch-profile http
   ```
3. Giữ nguyên Terminal này chạy ẩn (không tắt đi). API hiện đang chạy ở địa chỉ nội bộ `http://localhost:5299`.

### BƯỚC 2: Khởi chạy đường hầm Cloudflare Tunnel
1. Mở cửa sổ Terminal thứ hai (không tắt terminal ở Bước 1).
2. Chạy lệnh tạo đường hầm ngẫu nhiên:
   ```bash
   cloudflared tunnel --url http://localhost:5299
   ```
3. Chờ 3-5 giây, hệ thống sẽ in ra một đường dẫn HTTPS có dạng:
   `https://[tên-ngẫu-nhiên].trycloudflare.com`
4. **Copy đường dẫn này.**

### BƯỚC 3: Cập nhật địa chỉ API cho Frontend (Nếu đường dẫn thay đổi)
Do sử dụng gói đường hầm dùng thử ngẫu nhiên của Cloudflare, mỗi lần bạn tắt đi bật lại lệnh ở Bước 2, đường dẫn sẽ thay đổi. Bạn cần cập nhật lại cho Frontend:
1. Mở file [appsettings.json](file:///media/tanma/40220E4A220E44FE/SongLinhRMA.System/RMA.Client/wwwroot/appsettings.json).
2. Dán đường dẫn mới copy ở Bước 2 vào mục `ApiBaseUrl` (nhớ có dấu `/` ở cuối).
3. Đẩy cấu hình mới lên GitHub bằng các lệnh:
   ```bash
   git add RMA.Client/wwwroot/appsettings.json
   git commit -m "config: update api url for new tunnel session"
   git push
   ```
4. GitHub Actions sẽ tự động deploy giao diện mới lên Firebase Hosting sau khoảng 1 phút. Bạn có thể mở web Firebase lên sử dụng bình thường.

---

## 💡 MẸO TỐI ƯU: CỐ ĐỊNH ĐỊA CHỈ API KHÔNG ĐỔI

Nếu bạn muốn **không phải sửa file cấu hình và push git mỗi ngày**, bạn có thể cấu hình **Cloudflare Tunnel cố định** kèm theo tên miền riêng (Free hoàn toàn):

1. Đăng ký tài khoản miễn phí trên [Cloudflare](https://dash.cloudflare.com/).
2. Trỏ tên miền của bạn về Cloudflare (ví dụ: `songlinhrma.com`).
3. Vào mục **Zero Trust** -> **Access** -> **Tunnels** trên giao diện web Cloudflare để tạo một Tunnel cố định tên là `api` trỏ về `http://localhost:5299`.
4. Cấu hình cố định `ApiBaseUrl` trong `appsettings.json` là `https://api.songlinhrma.com/` duy nhất một lần. Từ đó bạn chỉ việc bật máy chạy lệnh mà không bao giờ cần sửa code hay push git nữa!

---

## 🛠️ PHỤ LỤC: CÁC LỆNH CHẠY THỬ NGHIỆM KHÁC

### 1. Chạy Frontend cục bộ (Local Client) để test nhanh
Nếu bạn muốn chạy thử nghiệm cả giao diện Frontend ở dưới máy cục bộ (không thông qua Firebase Hosting):
* Mở một Terminal khác và chạy lệnh:
  ```bash
  dotnet run --project RMA.Client/RMA.Client.csproj
  ```
* Truy cập địa chỉ hiển thị trên Terminal (thường là `http://localhost:5286` hoặc `https://localhost:7237`) để test giao diện.

### 2. Cài đặt lại các công cụ khi chuyển sang máy khác
* Cài đặt .NET SDK:
  `sudo apt install dotnet-sdk-10.0`
* Cài đặt Cloudflared:
  `curl -L --output cloudflared.deb https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64.deb && sudo dpkg -i cloudflared.deb`
