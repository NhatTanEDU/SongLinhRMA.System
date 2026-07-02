# 🌌 HƯỚNG DẪN SỬ DỤNG HỆ THỐNG AGENTIC SKILLS (Dành cho SongLinhRMA.System)

Chào mừng bạn đến với tài liệu hướng dẫn sử dụng thư viện **Agentic Skills** trong dự án của bạn trên môi trường Linux. Hệ thống kỹ năng này giúp AI (Gemini/Antigravity) hoạt động như các chuyên gia có chuyên môn sâu về bảo mật, kiến trúc, kiểm thử hoặc tối ưu hóa code.

---

## 🚀 1. Tổng Quan Vị Trí Cài Đặt (Local & Global)

Toàn bộ thư viện kỹ năng hiện đang được lưu trữ ở:
*   **Thư mục dự án (Local):** `[Dự án]/.agents/skills/` (Tương ứng: `/media/tanma/DATA/project/SongLinhRMA.System/.agents/skills/`)
*   **Thư mục cấu hình (Global):** `/home/tanma/.gemini/config/skills/`

Mỗi kỹ năng được đặt trong một thư mục con chứa tệp `SKILL.md` (chứa quy trình chuẩn và các ràng buộc).

---

## 🧠 2. Cách AI Tự Động Hoặc Thủ Công Sử Dụng Skill

### A. Tự động nhận diện (Không cần chỉ định)
Khi bạn đưa ra yêu cầu (Ví dụ: *"Hãy viết Unit Test cho hàm này"*), AI sẽ tự động phân tích và khớp với mô tả của các skill hiện có. AI sẽ tự động mở file [test-driven-development](file:///media/tanma/DATA/project/SongLinhRMA.System/.agents/skills/test-driven-development/SKILL.md) để đọc hướng dẫn trước khi thực hiện.

### B. Thủ công chọn lựa (Khuyên dùng để có độ chính xác cao nhất)
Bạn có thể gọi trực tiếp tên của skill bằng cách sử dụng ký tự `@` kèm theo tên của skill trong câu lệnh của mình.

**Cú pháp:**
`[Yêu cầu của bạn] + @tên-skill`

---

## 📋 3. Danh Sách Các Skill Phổ Biến & Cách Dùng Thủ Công

Dưới đây là bảng tổng hợp các skill được sử dụng nhiều nhất, trường hợp áp dụng và câu lệnh mẫu để bạn copy/chỉnh sửa nhanh:

### 🌟 Nhóm 1: Tối ưu hóa code & Kiến trúc (Development & Clean Code)

| Tên Skill | Khi nào nên dùng? (Use Cases) | Ví dụ câu lệnh mẫu để gọi thủ công |
| :--- | :--- | :--- |
| **`clean-code`** | Cần dọn dẹp code, xóa bỏ comment thừa, tách hàm dài, áp dụng nguyên lý SOLID, làm code dễ đọc và bảo trì hơn. | **`"Sử dụng kỹ năng @clean-code để tối ưu hóa hàm đăng nhập này."`** |
| **`dotnet-backend`** | Khi phát triển backend C# ASP.NET Core API, Firestore Repository, Dependency Injection hoặc tối ưu Middleware. | **`"Áp dụng @dotnet-backend để tối ưu hóa việc quản lý kết nối Firestore trong RmaAlertBackgroundService."`** |
| **`react-best-practices`** | Khi phát triển các Component giao diện, quản lý State, tối ưu render và đảm bảo cấu trúc sạch trong React/Blazor. | **`"Hãy tái cấu trúc giao diện danh sách RMA bằng @react-best-practices."`** |
| **`database-design`** | Thiết kế sơ đồ cơ sở dữ liệu Firestore, tối ưu hóa các câu truy vấn tài liệu, đánh chỉ mục (Index) dữ liệu. | **`"Thiết kế cấu trúc document cho collection RMA_TICKETS bằng @database-design."`** |

---

### 🛡️ Nhóm 2: Rà soát bảo mật & An toàn thông tin (Security & Pentesting)

| Tên Skill | Khi nào nên dùng? (Use Cases) | Ví dụ câu lệnh mẫu để gọi thủ công |
| :--- | :--- | :--- |
| **`api-security-best-practices`** | Kiểm tra quyền truy cập API endpoints, rà soát dữ liệu đầu vào (Input Validation), chống lộ dữ liệu nhạy cảm. | **`"Hãy kiểm tra API này bằng quy trình @api-security-best-practices."`** |
| **`broken-authentication`** | Rà soát lỗ hổng đăng nhập, quy trình cấp/làm mới JWT Token, bảo mật Firebase Auth. | **`"Dùng @broken-authentication để rà soát cơ chế xác thực Token JWT trong Program.cs."`** |
| **`sql-injection-testing`** | Kiểm tra xem các câu lệnh truy vấn database có an toàn trước lỗ hổng chèn ép mã độc SQL Injection hay không. | **`"Kiểm tra xem các API tìm kiếm RMA ticket có bị lỗ hổng @sql-injection-testing hay không."`** |

---

### 🧪 Nhóm 3: Kiểm thử & Khắc phục lỗi (Testing & Debugging)

| Tên Skill | Khi nào nên dùng? (Use Cases) | Ví dụ câu lệnh mẫu để gọi thủ công |
| :--- | :--- | :--- |
| **`systematic-debugging`** | Gặp các bug khó, lỗi chạy ngầm, lỗi kết nối hoặc crash không rõ nguyên nhân. AI sẽ đóng vai Sherlock Holmes để cô lập lỗi. | **`"Dùng kỹ năng @systematic-debugging để phân tích và sửa lỗi không nhận được FCM notification này."`** |
| **`test-driven-development`** | Khi bắt đầu viết một tính năng, muốn AI viết mã kiểm thử (Unit Test) trước, sau đó phát triển code khớp với test. | **`"Áp dụng @test-driven-development để viết test case và cài đặt logic cho hàm tính SLA này."`** |
| **`browser-automation`** | Viết các kịch bản kiểm thử tự động giả lập người dùng trên trình duyệt (Playwright/Puppeteer). | **`"Sử dụng @browser-automation viết script Playwright kiểm thử luồng đăng ký RMA từ trang Client."`** |

---

### 🎨 Nhóm 4: Giao diện & Trải nghiệm người dùng (UI/UX & Design)

| Tên Skill | Khi nào nên dùng? (Use Cases) | Ví dụ câu lệnh mẫu để gọi thủ công |
| :--- | :--- | :--- |
| **`frontend-design`** | Thiết kế CSS đẹp, sang trọng, tinh tế. Căn chỉnh màu sắc, khoảng cách, font chữ và tạo hiệu ứng mượt mà. | **`"Dùng @frontend-design để cải tiến UI/UX cho trang danh sách RMA Tickets thêm cao cấp."`** |
| **`ui-ux-pro-max`** | Xây dựng Design System, quản lý các Design Token, đảm bảo bố cục trực quan và dễ sử dụng cho người dùng cuối. | **`"Tối ưu biểu mẫu tạo phiếu RMA bằng quy trình @ui-ux-pro-max."`** |

---

### ⚡ Nhóm 5: Quy trình & Kế hoạch (Workflow & Management)

| Tên Skill | Khi nào nên dùng? (Use Cases) | Ví dụ câu lệnh mẫu để gọi thủ công |
| :--- | :--- | :--- |
| **`concise-planning`** | Trước khi bắt đầu làm một tính năng phức tạp, yêu cầu AI lập một Checklist chi tiết và chia nhỏ các bước thực hiện. | **`"Hãy lập kế hoạch triển khai tính năng gửi báo cáo tuần bằng @concise-planning."`** |
| **`git-pushing`** | Tự động kiểm tra thay đổi, tạo commit message chuẩn (Conventional Commits) và đẩy lên kho lưu trữ từ xa. | **`"Sử dụng kỹ năng @git-pushing để commit và push các thay đổi hiện tại lên Github."`** |

---

## 🎯 4. Gợi ý Cách Đọc File Hướng Dẫn Của Skill Để Chỉnh Sửa

Nếu bạn muốn xem chi tiết quy trình của một skill cụ thể (ví dụ: `clean-code`), bạn có thể nhấp chuột vào liên kết tương ứng hoặc yêu cầu mình hiển thị bằng lệnh:
> *"Cho tôi xem nội dung hướng dẫn của `@clean-code`"*

Nếu muốn chỉnh sửa luật hoạt động của một skill, bạn chỉ cần sửa nội dung trong file `SKILL.md` nằm tại thư mục của skill đó. AI sẽ áp dụng cấu trúc mới của bạn trong những cuộc trò chuyện sau.
