# 📘 Cẩm Nang Kỹ Năng & Cú Pháp Antigravity

Trong hệ thống **Antigravity**, bạn không cần sử dụng các cú pháp phím tắt phức tạp (như `/dtt`). Thay vào đó, bạn chỉ cần **gọi trực tiếp tên của kỹ năng (skills)** trong câu lệnh giao tiếp tự nhiên của mình.

---

## 🛠️ 1. Nhóm Kỹ Năng Kỹ Thuật (Engineering Skills)
*Dành riêng cho việc xử lý mã nguồn, sửa lỗi, thiết kế giao diện và tối ưu hóa cấu trúc dự án.*

| Kỹ Năng (Folder Name) | Mô Tả & Công Dụng | Khi Nào Nên Sử Dụng? |
| :--- | :--- | :--- |
| `debugger` | Tìm kiếm nguyên nhân gốc rễ (Root Cause) của một lỗi phức tạp. | Khi gặp lỗi không mong muốn lúc chạy dự án (như lỗi kết nối Client - Server, lỗi logic) mà không rõ lý do. |
| `triage` | Phân loại, đánh giá mức độ nghiêm trọng và lập kế hoạch xử lý lỗi. | Khi có danh sách dài các lỗi (như lỗi build sau khi nâng cấp SDK) và cần hướng dẫn thứ tự sửa lỗi ưu tiên. |
| `prototype` | Dựng nhanh giao diện (UI) hoặc các khối logic nghiệp vụ cơ bản. | Khi muốn tạo nhanh các khung giao diện mới (ví dụ: màn hình Đăng nhập, bảng danh sách thiết bị RMA). |
| `tdd` | Viết kiểm thử trước khi viết mã nguồn chính (Test Driven Development). | Khi viết các khối xử lý nghiệp vụ quan trọng (như AuthService, PaymentService) để đảm bảo độ chính xác ngay từ đầu. |
| `improve-codebase-architecture` | Rà soát, tái cấu trúc (Refactor) và tối ưu hóa kiến trúc toàn bộ dự án. | Khi mã nguồn bắt đầu lộn xộn, phức tạp và bạn muốn tối ưu chuẩn kiến trúc (ví dụ: Clean Architecture). |

---

## 🚀 2. Nhóm Kỹ Năng Hiệu Suất (Productivity Skills)
*Giúp bạn quản lý luồng công việc, làm rõ yêu cầu nghiệp vụ và lưu vết tiến độ.*

### 💬 `grill-me` (AI Phản Biện)
* **Công dụng:** Kích hoạt chế độ AI đặt câu hỏi ngược lại cho bạn để làm rõ yêu cầu trước khi code.
* **Khi nào dùng:** Khi chuẩn bị xây dựng một tính năng mới (như quản lý Ticket) nhưng chưa rõ cụ thể cần những trường dữ liệu nào, AI sẽ phỏng vấn bạn để làm rõ chi tiết.

### 📝 `handoff` (Bàn Giao Công Việc)
* **Công dụng:** Tạo ra một bản tóm tắt chi tiết về trạng thái công việc hiện tại.
* **Khi nào dùng:** Khi kết thúc buổi làm việc, giúp tạo file ghi chú lưu tiến trình để lần tới khi mở Antigravity lên, AI hoặc các thành viên khác có thể tiếp tục công việc ngay lập tức.

---

## 💡 Cú Pháp Ra Lệnh Mẫu
Để kích hoạt một kỹ năng, bạn chỉ cần lồng ghép tên kỹ năng đó vào câu chat tự nhiên. Ví dụ:

> 🔍 **Sửa lỗi:**
> *"Sử dụng **debugger**, hãy phân tích lỗi NETSDK1045 tôi đang gặp và đưa ra các bước sửa lỗi chi tiết."*

> 🎨 **Thiết kế UI:**
> *"Sử dụng **prototype**, hãy giúp tôi thiết kế giao diện hiển thị danh sách thiết bị RMA chuyên nghiệp."*

> 📋 **Báo cáo tiến độ:**
> *"Sử dụng **handoff**, hãy tóm tắt tiến độ code của tôi ngày hôm nay."*