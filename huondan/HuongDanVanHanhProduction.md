# 🚀 HƯỚNG DẪN VẬN HÀNH HỆ THỐNG PRODUCTION
## (Firebase Hosting + Render.com)

> **Tổng quan kiến trúc:**
> - **Frontend (Giao diện):** Firebase Hosting → `https://onglinh-rma-production.web.app`
> - **Backend (Máy chủ API):** Render.com → `https://songlinhrma-system.onrender.com`
> - **Database:** Google Firestore (luôn luôn hoạt động, không cần quản lý)

---

## ⚡ PHẦN 1: HIỂU VỀ GÓI FREE CỦA RENDER.COM

### 🔴 Vấn đề "Spin Down" (Ngủ Đông)
Gói miễn phí của Render có giới hạn quan trọng:
- **Sau 15 phút không có request**, Backend sẽ tự động **tắt hoàn toàn**.
- Lần gọi API **đầu tiên** sau khi bị tắt sẽ cần **chờ 30-60 giây** để khởi động lại.
- Đây là hành vi **bình thường** với gói Free — không phải lỗi.

### ✅ Cách "Gọi Dậy" Backend khi bị Spin Down

**Cách 1 — Chờ tự động (Đơn giản nhất):**
1. Mở trang web `https://onglinh-rma-production.web.app`
2. Đăng nhập bình thường
3. Nếu thấy màn hình loading kéo dài (30-60 giây) → **Bình thường!** Backend đang khởi động lại.
4. Sau khi load xong lần đầu, mọi request tiếp theo sẽ **nhanh như bình thường**.

**Cách 2 — Gọi dậy thủ công (Nhanh hơn):**
Mở trình duyệt, truy cập trực tiếp URL sau để ping Backend:
```
https://songlinhrma-system.onrender.com/api/customers
```
Nếu trình duyệt hiển thị JSON hoặc báo lỗi 401 (Unauthorized) → Backend đã **thức dậy** thành công!

**Cách 3 — Xem trạng thái trên Dashboard Render:**
1. Truy cập `https://dashboard.render.com`
2. Đăng nhập bằng GitHub
3. Click vào service `songlinhrma-system`
4. Nhìn vào mục **Logs** — nếu thấy dòng `Now listening on: http://[::]:8080` → đang hoạt động ✅

---

## 🔄 PHẦN 2: QUY TRÌNH CẬP NHẬT MÃ NGUỒN (Deploy mới)

### Bước 1: Cập nhật Backend (Tự động — Chỉ cần git push)
```bash
# Thực hiện ở thư mục gốc của dự án
git add .
git commit -m "mô tả thay đổi của bạn"
git push
```
> ✅ Render.com **tự động phát hiện** khi có code mới trên GitHub và tự động build + deploy lại Backend. Không cần làm thêm gì!

### Bước 2: Cập nhật Frontend (Thủ công — 2 lệnh)
```bash
# Chạy ở thư mục gốc: /media/tanma/DATA/project/SongLinhRMA.System

# 1. Build lại giao diện Blazor
dotnet publish RMA.Client/RMA.Client.csproj -c Release -o release

# 2. Deploy lên Firebase Hosting
npx -y firebase-tools deploy --only hosting --project onglinh-rma-production
```

---

## 🌐 PHẦN 3: QUẢN LÝ FIREBASE HOSTING

### Xem trang web đang chạy:
```
https://onglinh-rma-production.web.app
```

### Khôi phục về phiên bản cũ (Rollback):
1. Vào Firebase Console → `https://console.firebase.google.com/project/onglinh-rma-production`
2. Chọn **Hosting** ở menu trái
3. Tìm phiên bản cũ trong danh sách
4. Click vào **⋮** → **Rollback to this version**

---

## 🔧 PHẦN 4: QUẢN LÝ RENDER.COM (Backend)

### Truy cập Dashboard:
```
https://dashboard.render.com
```

### Khởi động lại thủ công (nếu bị lỗi):
1. Vào Dashboard → Click vào service `songlinhrma-system`
2. Nhấn nút **Manual Deploy** ở góc trên bên phải
3. Chọn **Deploy latest commit**

### Thay đổi biến môi trường:
1. Vào Dashboard → Click service → Click tab **Environment**
2. Thay đổi giá trị → Nhấn **Save Changes**
3. Render tự động restart để áp dụng

---

## 🚨 PHẦN 5: XỬ LÝ SỰ CỐ THƯỜNG GẶP

### ❌ Lỗi: "Lỗi kết nối máy chủ" khi đăng nhập
**Kiểm tra nhanh:** Mở trình duyệt → Truy cập:
```
https://songlinhrma-system.onrender.com/api/customers
```
- Hiện ra JSON hoặc lỗi 401 → Backend **đang sống** ✅ (chờ vài giây)
- Hiện "Service Unavailable" → Backend đang **khởi động lại** (chờ 60 giây)
- Sau 5 phút vẫn lỗi → Kiểm tra Logs trên Render Dashboard

### ❌ Lỗi: Backend deploy thất bại trên Render
1. Vào Render Dashboard → Logs → Tìm dòng màu đỏ (ERROR)
2. Lỗi phổ biến:
   - `Cannot find firebase.json` → Kiểm tra lại Secret File trên Render
   - `Build failed` → Chạy `dotnet build RMA.Server/RMA.Server.csproj` trên máy trước để kiểm tra

---

## 📋 PHẦN 6: BẢNG TÓM TẮT LỆNH THƯỜNG DÙNG

| Mục đích | Lệnh |
|---|---|
| Cập nhật Backend (push code) | `git add . && git commit -m "msg" && git push` |
| Build Frontend | `dotnet publish RMA.Client/RMA.Client.csproj -c Release -o release` |
| Deploy Frontend lên Firebase | `npx -y firebase-tools deploy --only hosting --project onglinh-rma-production` |

---

## 🔗 PHẦN 7: ĐƯỜNG DẪN QUAN TRỌNG

| Tên | Đường dẫn |
|---|---|
| 🌐 Trang web chính | https://onglinh-rma-production.web.app |
| ⚙️ API Backend | https://songlinhrma-system.onrender.com |
| 📊 Render Dashboard | https://dashboard.render.com |
| 🔥 Firebase Console | https://console.firebase.google.com/project/onglinh-rma-production |
| 💾 GitHub Repository | https://github.com/NhatTanEDU/SongLinhRMA.System |

---

*Được tạo tự động bởi Antigravity AI — Ngày 29/06/2026*
