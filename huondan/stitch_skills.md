# 🎨 HƯỚNG DẪN SỬ DỤNG STITCH SKILLS & DESIGN.MD

Bộ kỹ năng **Stitch Skills** (bao gồm `design-md`, `design-spells`, `design-system`...) từ thư viện [google-labs-code/stitch-skills](https://github.com/google-labs-code/stitch-skills) đã được cài đặt và tích hợp sẵn trong dự án của bạn tại thư mục `.agents/skills/`.

Tài liệu dưới đây sẽ hướng dẫn bạn cách sử dụng bộ kỹ năng này kết hợp với tệp `DESIGN.md` để tối ưu hóa quy trình phát triển giao diện (UI/UX) thông qua AI Agent.

---

## 🚀 1. Trạng Thái Cài Đặt (Installation Status)
*   **Thư mục Kỹ năng:** Toàn bộ mã nguồn kỹ năng từ `stitch-skills` đã được giải nén sẵn trong thư mục dự án của bạn tại `[workspace]/.agents/skills/design-md/`.
*   **Công cụ Hỗ trợ:** CLI `@google/design.md` đã được cài đặt toàn cục (global) trên máy của bạn và chạy qua lệnh `npx @google/design.md`.
*   **Tệp nguồn thiết kế:** Đã khởi tạo tệp cấu hình thiết kế gốc [DESIGN.md](file:///media/tanma/DATA/project/SongLinhRMA.System/DESIGN.md) tại thư mục gốc của dự án.

---

## 🧠 2. Cách Sử Dụng Lệnh CLI `design.md`
Bạn có thể sử dụng các lệnh dưới đây trong Terminal để làm việc với tệp cấu hình thiết kế `DESIGN.md`:

### 🔍 Kiểm tra lỗi cấu trúc thiết kế (Lint)
Lệnh này giúp bạn kiểm tra xem file `DESIGN.md` của mình có viết đúng cú pháp YAML và các định dạng chuẩn hay chưa:
```bash
npx @google/design.md lint DESIGN.md
```

### 📤 Xuất cấu hình thiết kế (Export)
Khi bạn cập nhật màu sắc hoặc font chữ mới trong `DESIGN.md`, bạn có thể dùng lệnh sau để tự động chuyển đổi sang mã cấu hình CSS/JSON:
*   **Xuất ra Tailwind CSS v4 `@theme`:**
    ```bash
    npx @google/design.md export DESIGN.md --format css-tailwind
    ```
*   **Xuất ra Tailwind CSS v3 (định dạng JSON):**
    ```bash
    npx @google/design.md export DESIGN.md --format json-tailwind
    ```

---

## 🤖 3. Cách Ra Lệnh Cho AI Agent Sử Dụng Stitch Skills

Khi bạn nói chuyện với AI (Antigravity hoặc Claude Code), bạn có thể gọi trực tiếp kỹ năng `@design-md` để AI thực thi các quy trình chuyên sâu về thiết kế.

### 💡 Các câu lệnh mẫu (Prompt Templates):

1.  **Phân tích mã nguồn và cập nhật tài liệu thiết kế:**
    *   *“Hãy sử dụng kỹ năng `@design-md` để phân tích các component giao diện hiện tại của client và cập nhật lại file `DESIGN.md` cho chuẩn xác.”*
2.  **Tạo trang giao diện mới đồng bộ với thiết kế có sẵn:**
    *   *“Hãy tạo một trang Razor component mới trong dự án `RMA.Client` để quản lý Ticket. Nhớ đọc file `DESIGN.md` để áp dụng đúng mã màu SOLICOM Blue (`#0072BC`) cho Button và AppBar.”*
3.  **Tối ưu trải nghiệm người dùng (UX) và hiệu ứng chuyển động:**
    *   *“Sử dụng kỹ năng `@design-spells` để gợi ý thêm hiệu ứng hover và micro-animation cho các nút bấm trên màn hình Dashboard.”*

---

## 📝 4. Cấu Trúc Của Tệp `DESIGN.md`
Tệp [DESIGN.md](file:///media/tanma/DATA/project/SongLinhRMA.System/DESIGN.md) của bạn hoạt động như một **"Hợp đồng Thiết kế" (Design Contract)** giữa bạn và AI. Nó gồm hai phần chính:

1.  **YAML Front Matter (Phần đầu tệp - giữa `---`):** Chứa các dữ liệu dạng key-value để các công cụ tự động đọc (như mã hex màu, font chữ, spacing, border radius).
2.  **Markdown Body (Phần thân tệp):** Chứa các mô tả bằng ngôn ngữ tự nhiên để AI hiểu được ngữ cảnh sử dụng (Ví dụ: *"Nút nguy hiểm thì dùng màu Đỏ, thanh tiêu đề thì dùng màu Xanh"*).

*Để chỉnh sửa hoặc xem trực tiếp tệp thiết kế của hệ thống, hãy mở tệp [DESIGN.md](file:///media/tanma/DATA/project/SongLinhRMA.System/DESIGN.md).*
