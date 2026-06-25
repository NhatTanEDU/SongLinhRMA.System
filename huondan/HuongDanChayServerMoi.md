# 🚀 HƯỚNG DẪN KHỞI CHẠY SERVER TỪ GITHUB

Khi bạn clone dự án từ link `https://github.com/NhatTanEDU/SongLinhRMA.System` về một máy tính mới, hãy làm theo các bước chi tiết sau để chạy được Server Backend:

---

### Bước 1: Cài đặt .NET 10 SDK
* Tải và cài đặt **.NET 10.0 SDK** tại trang chủ: [https://dotnet.microsoft.com/download/dotnet/10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
* Sau khi cài đặt xong, hãy mở Terminal/CMD mới và gõ lệnh sau để kiểm tra:
  ```bash
  dotnet --version
  ```
  *(Đảm bảo hiển thị phiên bản `10.0.xxx`)*

---

### Bước 2: Tải và đặt file Key Firebase (Bắt buộc)
Server sử dụng cơ sở dữ liệu **Firestore**, do đó bạn bắt buộc phải có file Key xác thực:
1. Truy cập **Firebase Console**: [https://console.firebase.google.com/](https://console.firebase.google.com/)
2. Vào mục **Cài đặt dự án** (Project Settings) -> Chọn tab **Tài khoản dịch vụ** (Service Accounts).
3. Nhấp chọn nút **Tạo khóa riêng tư mới** (Generate new private key) để tải file JSON key về máy.
4. Đổi tên file vừa tải về thành: `serviceAccountKey.json`
5. Di chuyển file này và đặt trực tiếp vào thư mục **`RMA.Server`** của dự án:
   * *Đường dẫn:* `SongLinhRMA.System/RMA.Server/serviceAccountKey.json`

---

### Bước 3: Cấu hình Project ID (Nếu cần)
* Mở file `RMA.Server/appsettings.json`.
* Kiểm tra mục `"ProjectId"` xem đã khớp với ID dự án Firebase của bạn chưa (mặc định đang là `"onglinh-rma-production"`).

---

### Bước 4: Khởi chạy Server
Mở Terminal tại thư mục gốc của dự án (`SongLinhRMA.System`) và chạy lệnh sau:
```bash
dotnet run --project RMA.Server/RMA.Server.csproj --launch-profile http
```

Khi chạy thành công, Terminal sẽ hiển thị:
```text
Now listening on: http://localhost:5299
```
*Bạn đã chạy thành công Server Backend!*
