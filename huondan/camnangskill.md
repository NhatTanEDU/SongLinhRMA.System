# Cẩm nang Kỹ năng và Cú pháp Antigravity

Trong hệ thống Antigravity, bạn không dùng các cú pháp phím tắt (như `/dtt`) mà sẽ gọi trực tiếp tên của các kỹ năng (skills) thông qua câu lệnh tự nhiên [1, 2].

Dưới đây là danh sách đầy đủ các skills đã được tích hợp cho Antigravity để bạn sử dụng:

## 1. Nhóm Kỹ năng Kỹ thuật (Engineering Skills)
Đây là các kỹ năng chuyên dùng để xử lý mã nguồn, giao diện và cấu trúc dự án:

- **`engineering/diagnose`**: Dùng để tìm nguyên nhân gốc rễ của một lỗi phức tạp [3].
  - **Khi nào dùng:** Khi bạn gặp lỗi không mong muốn lúc chạy dự án (như lỗi kết nối Client - Server) mà không rõ lý do [3].

- **`engineering/triage`**: Dùng để phân loại, đánh giá mức độ nghiêm trọng và lập kế hoạch xử lý lỗi [3].
  - **Khi nào dùng:** Khi bạn có một danh sách dài các lỗi (ví dụ như lỗi build do sai phiên bản SDK) và cần AI hướng dẫn nên sửa lỗi nào trước, sửa như thế nào [3].

- **`engineering/prototype`**: Dùng để dựng nhanh các giao diện (UI) hoặc logic nghiệp vụ cơ bản [4].
  - **Khi nào dùng:** Khi bạn muốn dựng khung giao diện nhanh, ví dụ như tạo form Login hoặc bảng danh sách thiết bị bằng framework giao diện [4].

- **`engineering/tdd` (Test Driven Development)**: Quy trình viết kiểm thử trước khi viết mã nguồn chính [4].
  - **Khi nào dùng:** Khi viết các hàm xử lý logic quan trọng (như AuthService hoặc DeviceService) và muốn đảm bảo code chạy đúng ngay từ đầu [2, 4].

- **`engineering/improve-codebase-architecture`**: Dùng để rà soát và tối ưu hóa cấu trúc toàn bộ dự án [5].
  - **Khi nào dùng:** Khi code bắt đầu lộn xộn, bạn muốn AI kiểm tra xem dự án đã chuẩn mô hình Clean Architecture hay chưa [5].

## 2. Nhóm Kỹ năng Hiệu suất (Productivity Skills)
Đây là các kỹ năng giúp bạn quản lý luồng công việc và làm rõ yêu cầu nghiệp vụ:

- **`productivity/grill-me`**: Kích hoạt chế độ AI đặt các câu hỏi ngược lại cho bạn [5].
  - **Khi nào dùng:** Khi bạn chuẩn bị làm một tính năng mới (như quản lý Ticket RMA) nhưng chưa rõ cụ thể cần những trường dữ liệu nào, AI sẽ hỏi để giúp bạn làm rõ yêu cầu [5].

- **`productivity/handoff`**: Dùng để tạo ra một bản tóm tắt công việc đang làm [5].
  - **Khi nào dùng:** Khi bạn kết thúc buổi làm việc, skill này sẽ tạo bản ghi chú để ngày mai khi mở Antigravity lên, AI (hoặc thành viên khác) biết ngay dự án đang dừng ở bước nào và có thể code tiếp tục ngay lập tức [1].

## 💡 Cú pháp ra lệnh mẫu:
Để kích hoạt, bạn chỉ cần đưa tên kỹ năng vào câu lệnh trò chuyện với Antigravity. Ví dụ [1, 2]:

> *"Sử dụng `engineering/diagnose`, hãy phân tích lỗi NETSDK1045 tôi đang gặp và đưa ra các bước sửa lỗi chi tiết."* [1]

> *"Hãy gọi `engineering/prototype` giúp tôi thiết kế giao diện hiển thị danh sách thiết bị RMA chuyên nghiệp."* [1]

> *"Dùng `productivity/handoff`, hãy tóm tắt tiến độ code của tôi ngày hôm nay."* [1, 5]
