# 🌌 HƯỚNG DẪN CÀI ĐẶT & SỬ DỤNG HỆ THỐNG AGENTIC SKILLS V3

Chào mừng bạn đến với bộ tài liệu hướng dẫn sử dụng thư viện **245+ Agentic Skills** cho AI Assistant (Antigravity IDE, Claude Code, Gemini CLI). Hệ thống kỹ năng này giúp biến AI từ một trợ lý thông thường thành một kỹ sư cấp cao chuyên biệt cho từng tác vụ cụ thể.

---

## 🚀 1. Trạng Thái Cài Đặt (Installation Status)
Chúng tôi đã giải nén tệp `skills.zip` và cài đặt toàn bộ **245 kỹ năng** vào thư mục cấu hình mặc định của agent trong dự án của bạn:
*   **Thư mục đích:** `d:/SongLinhRMA.System/.agents/skills/`

> [!NOTE]
> Tất cả các kỹ năng đều có cấu trúc dạng thư mục chứa tệp `SKILL.md` (ví dụ: `d:/SongLinhRMA.System/.agents/skills/clean-code/SKILL.md`). Các tệp này chứa các chỉ dẫn, khuôn mẫu (templates) và quy trình hoạt động chuẩn (SOPs) để AI tuân theo khi thực hiện tác vụ tương ứng.

---

## 🧠 2. Cách Kích Hoạt & Sử Dụng (How to Use)
AI Assistant của bạn được lập trình để tự động quét thư mục `d:/SongLinhRMA.System/.agents/skills/` mỗi khi chạy. Bạn không cần cài đặt gì thêm. Để yêu cầu AI sử dụng một kỹ năng cụ thể, bạn chỉ cần ra lệnh tự nhiên trong khung chat hoặc terminal:

### Cách viết câu lệnh (Prompting Examples):
1.  **Chỉ định trực tiếp bằng ký tự `@` hoặc tên kỹ năng:**
    *   *“Sử dụng kỹ năng `@clean-code` để tối ưu hóa hàm đăng nhập này.”*
    *   *“Hãy kiểm tra API này bằng quy trình `@api-security-best-practices`.”*
2.  **Yêu cầu theo ngữ cảnh tự nhiên:**
    *   *“Hãy tạo một kế hoạch triển khai dựa trên kỹ năng `@concise-planning`.”*
    *   *“Tái cấu trúc mã nguồn theo hướng `@clean-code`.”*

Khi bạn gọi một kỹ năng, AI sẽ đọc hướng dẫn trong tệp `SKILL.md` của kỹ năng đó và thực hiện chính xác các quy trình và tiêu chuẩn được mô tả trong đó.

---

## 📦 3. Các Nhóm Kỹ Năng & Khi Nào Nên Sử Dụng

Dưới đây là bảng phân loại các nhóm kỹ năng phổ biến nhất cùng liên kết trực tiếp để bạn có thể xem chi tiết chỉ dẫn của từng kỹ năng:

### 🌟 Nhóm 1: Essentials & Workflow (Cơ bản & Quy trình làm việc)
*Dành cho mọi lập trình viên. Nên dùng hàng ngày để giữ cho mã nguồn sạch và quản lý dự án hiệu quả.*

| Tên Kỹ Năng | Liên kết tệp cấu hình | Khi nào nên sử dụng? (Use Cases) |
| :--- | :--- | :--- |
| **clean-code** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/clean-code/SKILL.md) | Khi cần tối ưu hóa code ngắn gọn, dễ đọc, xóa bỏ các chú thích thừa thãi và tuân thủ các nguyên lý SOLID. |
| **concise-planning** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/concise-planning/SKILL.md) | Trước khi bắt đầu một tính năng mới hoặc sửa lỗi phức tạp, yêu cầu AI lập kế hoạch dạng Checklist nguyên tử (Atomic Checklist). |
| **lint-and-validate** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/lint-and-validate/SKILL.md) | Sau khi chỉnh sửa code, yêu cầu chạy kiểm tra lỗi cú pháp, định dạng và phân tích tĩnh tự động. |
| **git-pushing** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/git-pushing/SKILL.md) | Khi bạn muốn AI tự động gom nhóm thay đổi (stage), tạo commit message chuẩn (Conventional Commits) và đẩy (push) lên GitHub. |
| **kaizen** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/kaizen/SKILL.md) | Khi muốn cải tiến liên tục mã nguồn, tìm kiếm các lỗi tiềm ẩn tiềm tàng và chuẩn hóa quy trình. |
| **executing-plans** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/executing-plans/SKILL.md) | Sử dụng khi đã có một kế hoạch chi tiết cần AI tự động thực thi từng bước và báo cáo tiến độ tại mỗi chặng. |
| **writing-plans** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/writing-plans/SKILL.md) | Khi cần viết các tài liệu kế hoạch triển khai chi tiết cho các hệ thống lớn. |

---

### 🌐 Nhóm 2: Lập Trình Chuyên Sâu (Development)
*Dành cho việc xây dựng kiến trúc phần mềm, viết backend, frontend hoặc tối ưu hóa database.*

| Tên Kỹ Năng | Liên kết tệp cấu hình | Khi nào nên sử dụng? (Use Cases) |
| :--- | :--- | :--- |
| **typescript-expert** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/typescript-expert/SKILL.md) | Khi viết mã nguồn TypeScript, tối ưu hóa các kiểu dữ liệu nâng cao (Generics, Mapped Types) và sửa lỗi trình biên dịch. |
| **dotnet-backend** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/dotnet-backend/SKILL.md) | Khi phát triển các ứng dụng backend C# ASP.NET Core, làm việc với Entity Framework Core hoặc API Controller. |
| **backend-dev-guidelines** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/backend-dev-guidelines/SKILL.md) | Khi cần áp dụng mô hình Layered Architecture, BaseController, Prisma Repositories, hoặc Zod validation cho Node.js + Express. |
| **frontend-dev-guidelines** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/frontend-dev-guidelines/SKILL.md) | Khi lập trình React/Next.js với cấu trúc Feature-based, tối ưu hóa Suspense-first data fetching hoặc quản lý hiệu năng. |
| **database-design** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/database-design/SKILL.md) | Khi thiết kế lược đồ cơ sở dữ liệu (Schema Design), lập chỉ mục (indexing) hoặc tối ưu hóa các truy vấn SQL phức tạp. |
| **react-best-practices** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/react-best-practices/SKILL.md) | Khi cần viết các React Component chuẩn mực, quản lý state và tối ưu render. |

---

### 🛡️ Nhóm 3: Bảo Mật & Đánh Giá An Toàn (Security & Pentesting)
*Dành cho việc rà soát mã nguồn, kiểm tra lỗ hổng bảo mật và phòng thủ hệ thống.*

| Tên Kỹ Năng | Liên kết tệp cấu hình | Khi nào nên sử dụng? (Use Cases) |
| :--- | :--- | :--- |
| **ethical-hacking-methodology** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/ethical-hacking-methodology/SKILL.md) | Quy trình chuẩn 5 bước kiểm thử xâm nhập (Reconnaissance, Scanning, Exploitation, Reporting). |
| **api-security-best-practices** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/api-security-best-practices/SKILL.md) | Khi muốn AI kiểm tra bảo mật các API endpoints, chống các lỗi phân quyền, rate limit, và lộ dữ liệu. |
| **sql-injection-testing** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/sql-injection-testing/SKILL.md) | Khi cần rà soát mã nguồn xem các câu lệnh truy vấn SQL có bị dính lỗi chèn ép dữ liệu SQL Injection hay không. |
| **broken-authentication** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/broken-authentication/SKILL.md) | Khi thiết kế hệ thống đăng nhập, quản lý phiên làm việc (Session/JWT) để tránh bị bypass hoặc rò rỉ token. |
| **cloud-penetration-testing** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/cloud-penetration-testing/SKILL.md) | Khi muốn đánh giá an toàn của hạ tầng AWS, Azure hoặc GCP (S3 buckets, IAM Roles, Cognito...). |

---

### 🧪 Nhóm 4: Kiểm Thử & Sửa Lỗi (Testing & QA)
*Giúp đảm bảo code hoạt động hoàn hảo và giải quyết nhanh các bug khó.*

| Tên Kỹ Năng | Liên kết tệp cấu hình | Khi nào nên sử dụng? (Use Cases) |
| :--- | :--- | :--- |
| **test-driven-development** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/test-driven-development/SKILL.md) | Khi bạn muốn AI viết Unit Test trước, sau đó phát triển tính năng khớp với test (quy trình Đỏ - Xanh - Tái cấu trúc). |
| **systematic-debugging** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/systematic-debugging/SKILL.md) | Khi bạn gặp một lỗi cực kỳ khó tìm ra nguyên nhân, AI sẽ đóng vai Sherlock Holmes để phân tích hành vi và cô lập lỗi một cách khoa học. |
| **browser-automation** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/browser-automation/SKILL.md) | Khi cần viết các kịch bản kiểm thử tự động End-to-End (E2E) bằng Playwright hoặc Puppeteer. |
| **webapp-testing** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/webapp-testing/SKILL.md) | Khi kiểm thử tổng thể các tính năng của một ứng dụng web (functional, integration testing). |

---

### 🎨 Nhóm 5: UI/UX & Thiết Kế (Creative & Design)
*Tạo giao diện người dùng đẹp mắt, chuyên nghiệp và có chiều sâu.*

| Tên Kỹ Năng | Liên kết tệp cấu hình | Khi nào nên sử dụng? (Use Cases) |
| :--- | :--- | :--- |
| **frontend-design** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/frontend-design/SKILL.md) | Khi cần thiết kế giao diện UI mang tính cao cấp (Premium), màu sắc hài hòa HSL, Typography và Micro-animations sống động. |
| **ui-ux-pro-max** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/ui-ux-pro-max/SKILL.md) | Thiết kế hệ thống Design System chuẩn, tokens, tỷ lệ bố cục và trải nghiệm người dùng tối ưu. |
| **canvas-design** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/canvas-design/SKILL.md) | Tạo ra các tác phẩm trực quan, poster, hoặc sơ đồ phức tạp dạng ảnh PNG hoặc PDF. |
| **3d-web-experience** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/3d-web-experience/SKILL.md) | Khi tích hợp trải nghiệm 3D (Three.js, React Three Fiber, WebGL) vào trang web. |

---

### 📈 Nhóm 6: Product & Marketing (Sản phẩm & Tăng trưởng)
*Dành cho quản lý sản phẩm, viết nội dung tiếp thị, tối ưu tỷ lệ chuyển đổi (CRO).*

| Tên Kỹ Năng | Liên kết tệp cấu hình | Khi nào nên sử dụng? (Use Cases) |
| :--- | :--- | :--- |
| **product-manager-toolkit** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/product-manager-toolkit/SKILL.md) | Khi cần viết tài liệu PRD, phân tích bài toán nghiệp vụ, phác thảo User Persona và User Story. |
| **seo-audit** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/seo-audit/SKILL.md) | Rà soát mã nguồn trang web để đảm bảo chuẩn SEO, cấu trúc tiêu đề H1-H6, thẻ meta và tốc độ tải trang. |
| **copywriting** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/copywriting/SKILL.md) | Khi viết nội dung cho trang Landing Page, trang chủ hoặc mô tả tính năng để tăng khả năng bán hàng. |
| **form-cro** | [SKILL.md](file:///d:/SongLinhRMA.System/.agents/skills/form-cro/SKILL.md) | Tối ưu hóa các biểu mẫu (form liên hệ, đăng ký demo) để giảm tỷ lệ bỏ qua và tăng chuyển đổi. |

---

## ⚡ 4. Gợi Ý Thực Hành
Để làm quen ngay lập tức, bạn hãy gửi các tin nhắn thử nghiệm sau cho tôi:

*   **Thử nghiệm 1:** *“Hãy sử dụng kỹ năng `@concise-planning` viết cho tôi kế hoạch xây dựng tính năng đăng ký người dùng mới.”*
*   **Thử nghiệm 2:** *“Hãy áp dụng kỹ năng `@clean-code` để xem qua và refactor lại tệp `d:/SongLinhRMA.System/RMA.Server/Controllers/CustomersController.cs`.”*

Nếu bạn có bất cứ câu hỏi nào hoặc muốn tùy chỉnh bất kỳ kỹ năng nào, chỉ cần nhắn cho tôi biết nhé!
