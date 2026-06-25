# ⚡ HƯỚNG DẪN KHỞI CHẠY NHANH SONGLINHRMA

Khi bạn tắt hết Terminal hoặc khởi động lại máy, hãy làm đúng theo **5 bước ngắn gọn** sau để chạy lại hệ thống:

---

### Bước 1: Chạy API Backend
Mở **Terminal 1** và chạy lệnh:
```bash
dotnet run --project RMA.Server/RMA.Server.csproj --launch-profile http
```
*(Giữ nguyên cửa sổ này, không tắt)*

---

### Bước 2: Chạy Cloudflare Tunnel
Mở **Terminal 2** và chạy lệnh:
```bash
cloudflared tunnel --url http://localhost:5299
```
* Chờ 3 giây, tìm và **Copy đường dẫn HTTPS** dạng `https://xxx.trycloudflare.com` được in ra trên màn hình.

---

### Bước 3: Cập nhật URL API mới vào Code
* Mở file [appsettings.json](file:///media/tanma/40220E4A220E44FE/SongLinhRMA.System/RMA.Client/wwwroot/appsettings.json).
* Dán link vừa copy vào mục `"ApiBaseUrl"`.
* *Ví dụ:* `"ApiBaseUrl": "https://xxxx.trycloudflare.com/"` (Lưu ý: Phải giữ dấu `/` ở cuối đường dẫn).

---

### Bước 4: Đẩy thay đổi lên GitHub để cập nhật Firebase
Mở **Terminal 3** và chạy chuỗi lệnh:
```bash
git add RMA.Client/wwwroot/appsettings.json
git commit -m "config: update api url"
git push
```

---

### Bước 5: Truy cập hệ thống
Chờ **1 - 2 phút** để GitHub Actions hoàn tất việc deploy tự động, sau đó truy cập:
👉 **[https://onglinh-rma-production.web.app/](https://onglinh-rma-production.web.app/)**
*(Đường dẫn trang quản trị: [https://onglinh-rma-production.web.app/admin/rma](https://onglinh-rma-production.web.app/admin/rma))*
