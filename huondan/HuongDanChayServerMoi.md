# 🚀 HƯỚNG DẪN KHỞI CHẠY SERVER VÀ DEPLOY FIREBASE HOSTING TỪ GITHUB

Tài liệu này hướng dẫn cách cấu hình chạy Server Backend và triển khai Frontend lên Firebase Hosting từ một dự án vừa clone mới.

---

## 🛠️ PHẦN 1: KHỞI CHẠY SERVER BACKEND (LOCAL)

### Bước 1: Cài đặt .NET 10 SDK
* Tải và cài đặt **.NET 10.0 SDK** tại trang chủ: [https://dotnet.microsoft.com/download/dotnet/10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
* Kiểm tra phiên bản bằng cách mở Terminal và gõ:
  ```bash
  dotnet --version
  ```
  *(Đảm bảo hiển thị phiên bản `10.0.xxx`)*

### Bước 2: Tải và đặt file Key Firebase
Server sử dụng cơ sở dữ liệu **Firestore**, do đó bạn bắt buộc phải có file Key xác thực:
1. Truy cập **Firebase Console**: [https://console.firebase.google.com/](https://console.firebase.google.com/)
2. Vào mục **Cài đặt dự án** (Project Settings) -> Chọn tab **Tài khoản dịch vụ** (Service Accounts).
3. Nhấp chọn nút **Tạo khóa riêng tư mới** (Generate new private key) để tải file JSON key về máy.
4. Đổi tên file vừa tải về thành: `serviceAccountKey.json`
5. Đặt file này vào thư mục **`RMA.Server`** của dự án:
   * *Đường dẫn:* `SongLinhRMA.System/RMA.Server/serviceAccountKey.json`

### Bước 3: Cấu hình Project ID
* Mở file `RMA.Server/appsettings.json`.
* Kiểm tra mục `"ProjectId"` xem đã khớp với ID dự án Firebase của bạn chưa (mặc định đang là `"onglinh-rma-production"`).

### Bước 4: Khởi chạy Server
Mở Terminal tại thư mục gốc của dự án (`SongLinhRMA.System`) và chạy lệnh sau:
```bash
dotnet run --project RMA.Server/RMA.Server.csproj --launch-profile http
```
*Server API sẽ chạy tại địa chỉ:* `http://localhost:5299`

---

## 🌐 PHẦN 2: TRIỂN KHAI FRONTEND LÊN FIREBASE HOSTING

Để giao diện Frontend chạy trực tuyến trên Firebase Hosting, bạn có 2 cách thực hiện:

### Cách 1: Tự động deploy qua GitHub Actions (Khuyên dùng)
Bạn không cần cài đặt bất kỳ công cụ nào ở máy cá nhân. Chỉ cần thiết lập khóa xác thực một lần trên GitHub:
1. Vào kho chứa GitHub của bạn -> **Settings** -> **Secrets and variables** -> **Actions**.
2. Tạo một **New repository secret**:
   * **Name**: `FIREBASE_SERVICE_ACCOUNT`
   * **Value**: Dán toàn bộ nội dung file JSON key `serviceAccountKey.json` (đã tải ở Bước 2 bên trên).
3. Từ bây giờ, mỗi khi bạn chạy lệnh `git push` lên nhánh `main`, GitHub sẽ tự động build Blazor và deploy trực tiếp lên Firebase Hosting.

### Cách 2: Deploy thủ công từ máy cá nhân
Nếu bạn muốn deploy trực tiếp từ máy tính của mình mà không cần push git:
1. **Cài đặt Node.js** và **Firebase CLI** bằng cách chạy lệnh:
   ```bash
   npm install -g firebase-tools
   ```
2. **Đăng nhập Firebase** trên máy của bạn:
   ```bash
   firebase login
   ```
3. **Build dự án Blazor WASM**:
   ```bash
   dotnet publish RMA.Client/RMA.Client.csproj -c Release -o release
   ```
4. **Deploy lên Firebase**:
   ```bash
   firebase deploy --only hosting --project onglinh-rma-production
   ```
