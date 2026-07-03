# 🔍 Báo cáo Lỗi Kiến Trúc & Đề Xuất Tối Ưu Hóa (dtt.md)

Dưới đây là các điểm nghẽn kiến trúc (friction points) được tìm thấy sau khi rà soát hệ thống bằng kỹ năng `improve-codebase-architecture`.

---

## 📌 Lỗi 1: Trùng Lặp Caching & Mất Đồng Bộ Dữ Liệu (Redundant & Out-of-sync Caching)

### 🔴 Vấn đề hiện tại:
* Caching được triển khai ở hai lớp khác nhau nhưng không kết nối với nhau:
  1. **Lớp Repository (`FirestoreRepository<T>`):** Tự động lưu cache các danh sách (`GetAllAsync`) trong 15 phút và tự động xóa cache (`InvalidateCache`) khi có thao tác ghi dữ liệu (`Add`, `Update`, `Delete`).
  2. **Lớp Controller & Background Service (`RmaTicketsController`, `RmaAlertBackgroundService`):** Tự gọi `IMemoryCache.GetOrCreateAsync` cho các thực thể như `devices_dict`, `customers_dict`, `locations_dict`, `sla_settings_cached` với TTL cố định (5 phút / 10 phút).
* **Lỗi đồng bộ (Data Inconsistency):** Khi Admin chỉnh sửa thông tin Khách hàng (`Customer`) hoặc cấu hình SLA (`SystemSetting`), Repository của thực thể đó tự xóa cache của nó. Tuy nhiên, Cache cục bộ của `RmaTicketsController` và `RmaAlertBackgroundService` **không được thông báo** và tiếp tục trả về dữ liệu cũ (stale) lên tới 5-10 phút.

### 🟢 Đề xuất tối ưu:
* **Hợp nhất Caching về lớp Repository:** Xóa bỏ hoàn toàn lớp cache cục bộ (`_cache.GetOrCreateAsync`) trong Controllers và Background Service.
* **Tận dụng Repository Cache:** Khi cần lấy danh sách, Controller chỉ cần gọi thẳng `_customerRepo.GetAllAsync()` (hàm này đã được Repository cache an toàn). Vì dữ liệu lấy từ RAM rất nhanh (< 1ms), ta có thể convert sang Dictionary `.ToDictionary(...)` trực tiếp trên mỗi Request mà không cần lưu trữ thêm một lớp cache Dictionary riêng biệt.

---

## 📌 Lỗi 2: Trùng Lặp Mã Nguồn Map DTO (DTO Mapping Duplication)

### 🔴 Vấn đề hiện tại:
* Trong [RmaTicketsController.cs](file:///media/tanma/DATA/project/SongLinhRMA.System/RMA.Server/Controllers/RmaTicketsController.cs), đoạn mã chuyển đổi từ thực thể `RmaTicket` sang `RmaTicketDto` (bao gồm lấy thông tin từ các dictionary thiết bị, khách hàng, trạng thái, hãng, lịch sử trạng thái, tệp đính kèm) bị **sao chép y hệt 5 lần** tại các hàm:
  1. `Get()` (Dòng 119)
  2. `GetPaged()` (Dòng 277)
  3. `Get(string id)` (Dòng 385)
  4. `Post()` (Dòng 659)
  5. `Put()` (Dòng 716)
* Việc này làm code bị phình to (shallow module), khó bảo trì và dễ sinh lỗi khi thêm bớt trường trong DTO (phải sửa ở cả 5 nơi).

### 🟢 Đề xuất tối ưu:
* **Trích xuất hàm dùng chung:** Gom toàn bộ logic mapping phức tạp này vào một hàm trợ giúp duy nhất trong controller (ví dụ: `MapToRmaTicketDto(...)`).
* Cả 5 API endpoint trên sẽ gọi chung qua hàm này để xử lý dữ liệu nhất quán.

---

## 📊 Kế hoạch kiểm chứng (Verification Plan)
1. **Kiểm tra biên dịch:** Chạy `dotnet build` toàn hệ thống.
2. **Kiểm tra tính đúng đắn:** Đảm bảo khi sửa đổi khách hàng/cấu hình SLA thì trên trang RMA Tickets và FCM Alerts cập nhật ngay lập tức (không bị trễ 5-10 phút như trước).
3. **Kiểm tra Unit Test:** Chạy lại toàn bộ test cases.
