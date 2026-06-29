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

## 🔗 PHẦN 2: TẠO ĐƯỜNG HẦM (TUNNEL) ĐỂ KẾT NỐI CLIENT VỚI SERVER

> ⚠️ **BƯỚC QUAN TRỌNG NHẤT!** Nếu bỏ qua phần này, Client đã deploy lên Firebase sẽ **không thể kết nối** tới Server đang chạy trên máy cá nhân của bạn, và bạn sẽ gặp lỗi **"Lỗi kết nối tới Máy chủ"**.

### Tại sao cần bước này?
Kiến trúc hệ thống của bạn gồm 2 phần:
- **RMA.Client** (Blazor WASM) → Deploy lên Firebase Hosting (Internet công khai)
- **RMA.Server** (ASP.NET Core API) → Chạy trên máy cá nhân (`localhost:5299`)

Trình duyệt người dùng tải Client từ Firebase, sau đó Client sẽ gọi API tới Server. Vì Server chạy trên `localhost`, không thể truy cập từ Internet → cần tạo **đường hầm (tunnel)** để "lộ" Server ra ngoài.

### Bước 1: Cài đặt Cloudflared

**Trên Ubuntu/Debian:**
```bash
curl -sL https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64.deb -o cloudflared.deb
sudo dpkg -i cloudflared.deb
```

**Trên Windows:**
- Tải tại: [https://github.com/cloudflare/cloudflared/releases/latest](https://github.com/cloudflare/cloudflared/releases/latest)
- Tải file `cloudflared-windows-amd64.exe`, đổi tên thành `cloudflared.exe` và thêm vào PATH.

**Trên macOS:**
```bash
brew install cloudflared
```

Kiểm tra cài đặt thành công:
```bash
cloudflared --version
```

### Bước 2: Tạo Tunnel (Đường hầm)

> **Lưu ý:** Đảm bảo Server đang chạy (Bước 4, Phần 1) trước khi tạo tunnel!

Mở một Terminal **mới** (giữ nguyên Terminal chạy Server) và gõ:
```bash
cloudflared tunnel --url http://localhost:5299
```

Sau vài giây, bạn sẽ thấy dòng log chứa URL tương tự:
```
INF |  https://abc-xyz-123-something.trycloudflare.com
```

📋 **Sao chép URL này** (ví dụ: `https://abc-xyz-123-something.trycloudflare.com`) — bạn sẽ cần nó ở bước tiếp theo.

### Bước 3: Cập nhật địa chỉ API trong Client

Mở file `RMA.Client/wwwroot/appsettings.json` và cập nhật `ApiBaseUrl` với URL tunnel vừa tạo:
```json
{
  "ApiBaseUrl": "https://abc-xyz-123-something.trycloudflare.com/"
}
```
> ⚠️ **Nhớ thêm dấu `/` ở cuối URL!**

### ⚡ Lưu ý quan trọng về Cloudflare Quick Tunnel
| Đặc điểm | Chi tiết |
|:---|:---|
| **Tạm thời** | URL sẽ thay đổi mỗi lần bạn tạo tunnel mới |
| **Phụ thuộc Terminal** | Tắt Terminal chạy `cloudflared` = tunnel ngừng hoạt động |
| **Phụ thuộc máy tính** | Tắt máy = tunnel ngừng hoạt động |
| **Phù hợp cho** | Demo, testing, phát triển |
| **Không phù hợp cho** | Production (nên deploy Server lên Cloud) |

**Mỗi lần tạo tunnel mới, bạn phải:**
1. Sao chép URL mới
2. Cập nhật lại `RMA.Client/wwwroot/appsettings.json`
3. Build lại Client (`dotnet publish ...`)
4. Deploy lại lên Firebase (`firebase deploy ...`)

---

## 🌐 PHẦN 3: TRIỂN KHAI FRONTEND LÊN FIREBASE HOSTING

Để giao diện Frontend chạy trực tuyến trên Firebase Hosting, bạn có 2 cách thực hiện:

### Cách 1: Tự động deploy qua GitHub Actions (Khuyên dùng)
Bạn không cần cài đặt bất kỳ công cụ nào ở máy cá nhân. Chỉ cần thiết lập khóa xác thực một lần trên GitHub:
1. Vào kho chứa GitHub của bạn -> **Settings** -> **Secrets and variables** -> **Actions**.
2. Tạo một **New repository secret**:
   * **Name**: `FIREBASE_SERVICE_ACCOUNT`
   * **Value**: Dán toàn bộ nội dung file JSON key `serviceAccountKey.json` (đã tải ở Bước 2 bên trên).
3. Từ bây giờ, mỗi khi bạn chạy lệnh `git push` lên nhánh `main`, GitHub sẽ tự động build Blazor và deploy trực tiếp lên Firebase Hosting.

> ⚠️ **Lưu ý:** Trước khi push, hãy đảm bảo bạn đã cập nhật `ApiBaseUrl` trong file `RMA.Client/wwwroot/appsettings.json` với URL tunnel đang hoạt động (xem Phần 2, Bước 3).

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
3. **Đảm bảo `ApiBaseUrl` đã được cập nhật** trong file `RMA.Client/wwwroot/appsettings.json` với URL tunnel hiện tại (xem Phần 2, Bước 3).
4. **Build dự án Blazor WASM**:
   ```bash
   dotnet publish RMA.Client/RMA.Client.csproj -c Release -o release
   ```
5. **Deploy lên Firebase**:
   ```bash
   firebase deploy --only hosting --project onglinh-rma-production
   ```

---

## 🛑 PHẦN 4: XỬ LÝ LỖI THƯỜNG GẶP

### Lỗi 1: "Lỗi kết nối tới Máy chủ" trên trang web
**Nguyên nhân:** Client không kết nối được tới Server API.

**Kiểm tra theo thứ tự:**
1. ✅ Server có đang chạy không? (`dotnet run --project RMA.Server/RMA.Server.csproj --launch-profile http`)
2. ✅ Cloudflare tunnel có đang chạy không? (Terminal chạy `cloudflared` còn mở?)
3. ✅ URL tunnel trong `RMA.Client/wwwroot/appsettings.json` có khớp với URL tunnel hiện tại?
4. ✅ Đã build lại Client và deploy lại Firebase sau khi đổi URL chưa?

**Cách sửa nhanh:**
```bash
# 1. Đảm bảo Server đang chạy (Terminal 1)
dotnet run --project RMA.Server/RMA.Server.csproj --launch-profile http

# 2. Tạo tunnel mới (Terminal 2)
cloudflared tunnel --url http://localhost:5299
# → Sao chép URL mới (ví dụ: https://xxx.trycloudflare.com)

# 3. Cập nhật file RMA.Client/wwwroot/appsettings.json với URL mới

# 4. Build lại và deploy (Terminal 3)
dotnet publish RMA.Client/RMA.Client.csproj -c Release -o release
firebase deploy --only hosting --project onglinh-rma-production
```

### Lỗi 2: "Không tìm thấy file credentials 'serviceAccountKey.json'"
**Nguyên nhân:** Thiếu file key Firebase.
**Cách sửa:** Xem lại Phần 1, Bước 2 để tải và đặt file key đúng vị trí.

### Lỗi 3: `firebase: command not found`
**Nguyên nhân:** Chưa cài Firebase CLI.
**Cách sửa:**
```bash
npm install -g firebase-tools
```
Hoặc dùng trực tiếp qua npx mà không cần cài:
```bash
npx -y firebase-tools deploy --only hosting --project onglinh-rma-production
```

---

## 📋 TÓM TẮT QUY TRÌNH CHẠY ĐẦY ĐỦ

Mỗi khi bạn muốn chạy hệ thống, hãy thực hiện theo thứ tự:

```
┌─────────────────────────────────────────────────────────┐
│  Terminal 1: Chạy Server                                │
│  $ dotnet run --project RMA.Server/RMA.Server.csproj    │
│    --launch-profile http                                │
├─────────────────────────────────────────────────────────┤
│  Terminal 2: Tạo Tunnel                                 │
│  $ cloudflared tunnel --url http://localhost:5299       │
│  → Sao chép URL tunnel mới                             │
├─────────────────────────────────────────────────────────┤
│  Cập nhật file: RMA.Client/wwwroot/appsettings.json    │
│  { "ApiBaseUrl": "https://URL-MỚI.trycloudflare.com/" }│
├─────────────────────────────────────────────────────────┤
│  Terminal 3: Build & Deploy Client                      │
│  $ dotnet publish RMA.Client/RMA.Client.csproj          │
│    -c Release -o release                                │
│  $ firebase deploy --only hosting                       │
│    --project onglinh-rma-production                     │
└─────────────────────────────────────────────────────────┘
```

