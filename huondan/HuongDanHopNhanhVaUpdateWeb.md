# 🔄 HƯỚNG DẪN HỢP NHÁNH (MERGE) & CẬP NHẬT TOÀN BỘ HỆ THỐNG
*(Tài liệu hướng dẫn triển khai sau khi tích hợp nhánh `codex/featuresales-delivery-design` vào `main`)*

Dự án đã được Antigravity AI thực hiện **hợp nhánh thành công ở môi trường local** của bạn, giải quyết triệt để tất cả xung đột (conflict) giữa mã nguồn mới nhất trên `main` và các tính năng nghiệp vụ của nhánh `codex/featuresales-delivery-design`.

Dưới đây là hướng dẫn cách đẩy mã nguồn đã hợp nhất lên GitHub và cập nhật hệ thống trên **Firebase Hosting** và **Render.com**.

---

## 🏛️ TỔNG QUAN CÁC ĐÃ THAY ĐỔI
1. **Phân tách Hãng (Brand) & Đối tác sửa chữa (Vendor):** Hệ thống NoSQL chuẩn hóa quản lý Master Data.
2. **Hợp nhất Tra cứu Vòng đời (Lifecycle) & Đơn hàng:** Tra cứu S/N toàn diện trực tiếp trong popup và hiển thị banner cảnh báo nếu thiết bị đang có ticket mở nhằm tránh trùng lặp.
3. **Bộ lọc nâng cao & Xuất dữ liệu (CSV):** Cho phép phòng Sales lọc nâng cao và xuất báo cáo trực tiếp từ Sales Dashboard.
4. **Trình chỉnh sửa thông tin đa vai trò:** Hợp nhất giao diện `SalesOrderDetailViewDialog` cho phép cập nhật đồng thời thông tin Mã đơn hàng (OrderCode), Ghi chú Sale (SalesNote) và Ghi chú nghiệp vụ (Note).

---

## ⚡ PHẦN 1: ĐẨY CODE ĐÃ MERGE LÊN GITHUB (Mã nguồn chính)
Do môi trường dòng lệnh của AI không lưu thông tin bảo mật (Username/Password/Token) Github của bạn, bạn cần chạy lệnh đẩy code bằng terminal cá nhân đã đăng nhập của mình:

1. Mở **Terminal** của bạn trên máy tính tại thư mục dự án: `/media/tanma/DATA/project/SongLinhRMA.System`
2. Thực hiện lệnh đẩy (push) code lên nhánh `main` trên GitHub:
   ```bash
   git push origin main
   ```
   *(Thao tác này sẽ đồng bộ toàn bộ lịch sử commits và merge sạch sẽ lên repository trên đám mây).*

---

## ❄️ PHẦN 2: CẬP NHẬT WEB LÊN GOOGLE FIREBASE HOSTING
Hiện tại dự án có thiết lập Github Actions tự động deploy Frontend khi có code push lên `main`. Tuy nhiên, nếu bạn muốn cập nhật lập tức và kiểm soát thủ công, hãy thực hiện theo 2 bước sau:

1. **Build lại giao diện Client (Blazor WebAssembly) sang chế độ Release:**
   ```bash
   dotnet publish RMA.Client/RMA.Client.csproj -c Release -o release
   ```
   *Lệnh này sẽ biên dịch toàn bộ giao diện C# thành định dạng Web tĩnh (HTML/JS/WASM) tối ưu nằm trong thư mục `release/wwwroot`.*

2. **Tải và deploy trực tiếp lên Firebase Hosting:**
   ```bash
   npx -y firebase-tools deploy --only hosting --project onglinh-rma-production
   ```
   *Firebase Hosting sẽ tải các tập tin trong thư mục `release/wwwroot` lên máy chủ và cập nhật URL chạy chính thức: [https://onglinh-rma-production.web.app](https://onglinh-rma-production.web.app).*

---

## 🌐 PHẦN 3: CẬP NHẬT SERVER API LÊN RENDER.COM
Trang API Backend được cài đặt chế độ **Auto Deploy** kết nối trực tiếp với GitHub Repository của bạn:

1. **Tự động kích hoạt:**
   Ngay sau khi bạn chạy lệnh `git push origin main` ở **Phần 1**, máy chủ Render.com sẽ tự động nhận biết có commit mới trên nhánh `main` và bắt đầu tải mã nguồn về để build lại Docker Container mới.

2. **Cách kiểm tra & Kích hoạt thủ công (nếu cần thiết):**
   - Truy cập trang quản trị Render: [https://dashboard.render.com](https://dashboard.render.com)
   - Chọn ứng dụng `songlinhrma-system`.
   - Xem tab **Events** hoặc **Logs** để theo dõi quá trình build Docker.
   - Nếu muốn deploy lại ngay lập tức mà không push code, nhấp vào nút **Manual Deploy** ở góc trên bên phải trang Dashboard Render → chọn **Deploy latest commit**.

---

## 🛠️ PHẦN 4: THAO TÁC MERGE NHÁNH CHO CÁC LẦN SAU (Tự thực hiện)
Nếu sau này bạn có thêm nhánh tính năng mới (ví dụ: `features/new-feature`) và muốn tự hợp nhất vào `main`:

```bash
# 1. Chuyển về nhánh main và cập nhật code mới nhất từ internet
git checkout main
git pull origin main

# 2. Hợp nhất nhánh tính năng vào main
git merge features/new-feature

# 3. Nếu xảy ra xung đột (conflict):
# - Sử dụng VS Code hoặc trình soạn thảo mở các file bị báo xung đột (chứa dấu <<<<<<< và >>>>>>>).
# - Lựa chọn giữ code hiện tại (Accept Current), giữ code mới (Accept Incoming) hoặc kết hợp cả hai.
# - Sau khi giải quyết xong, lưu file và thêm vào git:
git add <tên_file_đã_sửa>

# 4. Tạo commit kết luận merge
git commit -m "Merge branch features/new-feature into main"

# 5. Đẩy code lên GitHub để tự động deploy
git push origin main
```

---
*Tài liệu được tạo bởi Antigravity AI — Ngày 07/07/2026*
