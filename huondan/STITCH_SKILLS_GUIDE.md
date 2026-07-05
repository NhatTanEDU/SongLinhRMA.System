# Hướng Dẫn Sử Dụng Stitch Skills và DESIGN.md

Tài liệu này hướng dẫn bạn cách sử dụng các skill từ `stitch-skills` và chuẩn `DESIGN.md` của Google Labs mà chúng ta vừa cài đặt vào dự án.

## 1. DESIGN.md là gì?
`DESIGN.md` là một chuẩn định dạng (format specification) giúp các Agent AI (như tôi) hiểu được hệ thống thiết kế (Design System) của dự án.
Nó kết hợp giữa **Design Tokens** (dưới dạng YAML) để máy có thể đọc các giá trị màu sắc, typography,... và **Văn bản giải thích** (Markdown) để con người và AI hiểu được ngữ cảnh sử dụng.

### Cách sử dụng DESIGN.md
1. **Tạo file `.stitch/DESIGN.md`** trong thư mục gốc của dự án.
2. Bạn có thể nhờ tôi (Agent) tạo file này hoặc tự định nghĩa theo cấu trúc:
   ```md
   ---
   name: Tên Design System
   colors:
     primary: "#1A1C1E"
     secondary: "#6C7278"
   typography:
     body-md:
       fontFamily: Public Sans
       fontSize: 1rem
   ---
   ## Overview
   Mô tả về phong cách thiết kế của dự án...
   ```
3. **Sử dụng CLI để kiểm tra (Linting):**
   Bạn có thể cài đặt công cụ CLI để kiểm tra lỗi của file DESIGN.md:
   ```bash
   npm install @google/design.md
   npx @google/design.md lint .stitch/DESIGN.md
   ```

## 2. Stitch Skills
**Stitch Skills** là tập hợp các kỹ năng (skills) giúp Agent AI tương tác với nền tảng thiết kế [Google Stitch](https://stitch.withgoogle.com). Các skill này đã được sao chép vào thư mục `.agents/skills` của dự án.

Các skill được chia làm 3 nhóm chính:

### A. Nhóm Design (`stitch-design`)
Giúp tạo, quản lý và tối ưu thiết kế trên Stitch:
- **`code-to-design`**: Chuyển đổi mã nguồn Frontend (React, Vue, HTML) thành bản thiết kế trên Stitch.
  - *Câu lệnh mẫu:* "Hãy upload mã nguồn frontend ở `/path/to/folder` lên dự án Stitch có tên là 'My-Project'."
- **`generate-design`**: Tạo màn hình mới, chỉnh sửa màn hình hiện có hoặc tạo các biến thể (variants) thiết kế bằng văn bản.
  - *Câu lệnh mẫu:* "Hãy tạo màn hình đăng nhập cho ứng dụng di động với màu xanh chủ đạo."
- **`manage-design-system`**: Đẩy file `DESIGN.md` lên Stitch và áp dụng cho các màn hình.
  - *Câu lệnh mẫu:* "Hãy upload design system từ `.stitch/DESIGN.md` và áp dụng cho tất cả màn hình."
- **`extract-design-md`**: Quét mã nguồn hiện tại và tự động trích xuất ra file `DESIGN.md`.

### B. Nhóm Build (`stitch-build`)
Giúp sinh code từ thiết kế:
- **`react-components`**: Chuyển đổi màn hình Stitch thành các component React chuẩn.
  - *Câu lệnh mẫu:* "Hãy chuyển đổi tất cả màn hình trong dự án Stitch `projects/123` thành component React."
- **`react-native`**: Chuyển đổi thiết kế sang React Native.
- **`remotion`**: Tạo video walkthrough (trình diễn) từ thiết kế Stitch.

### C. Nhóm Utilities (`stitch-utilities`)
Các công cụ hỗ trợ cải thiện prompt và tiêu chuẩn thiết kế:
- **`enhance-prompt`**: Giúp bạn trau chuốt lại ý tưởng UI/UX thành một prompt chi tiết cho Stitch.
- **`stitch-loop`**: Tự động hóa quá trình xây dựng toàn bộ website nhiều trang từ một prompt duy nhất.
- **`taste-design`**: Sinh ra file `DESIGN.md` cao cấp, chống thiết kế rập khuôn, mang tính thẩm mỹ cao.
- **`design-md`**: Phân tích dự án Stitch và sinh ra file `DESIGN.md`.

---

## 3. Cách Yêu Cầu Tôi Thực Hiện
Do các skill này đã được cài đặt vào hệ thống bộ nhớ của tôi thông qua `.agents/skills`, bạn chỉ cần ra lệnh bằng ngôn ngữ tự nhiên. 

**Ví dụ, bạn có thể nói với tôi:**
- *"Hãy phân tích dự án này và tạo cho tôi file `.stitch/DESIGN.md`."*
- *"Dùng skill `stitch-loop` để tạo cho tôi một landing page giới thiệu sản phẩm RMA."*
- *"Hãy dùng `enhance-prompt` để làm rõ ý tưởng thiết kế dashboard admin của tôi."*

> **Lưu ý:** Để tôi có thể kết nối hoàn toàn với các thao tác trên Google Stitch (tạo/sửa dự án trên cloud), bạn cần đảm bảo rằng **Stitch MCP** server đã được cài đặt và cấp quyền hợp lệ trong môi trường của bạn.
