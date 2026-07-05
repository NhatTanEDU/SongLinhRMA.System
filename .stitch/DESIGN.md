---
name: SongLinhRMA Design System
colors:
  primary: "#1b6ec2"
  primary-hover: "#1861ac"
  secondary: "#0071c1"
  success: "#26b050"
  error: "#f44336"
  error-dark: "#b32121"
  warning: "#FFE500"
typography:
  body-md:
    fontFamily: "'Helvetica Neue', Helvetica, Arial, sans-serif"
    fontSize: 1rem
components:
  button-primary:
    backgroundColor: "{colors.primary}"
    textColor: "#ffffff"
---
## Overview
SongLinhRMA sử dụng phong cách thiết kế hiện đại, tinh gọn, tập trung vào tính hiệu quả của một hệ thống quản lý (RMA - Return Merchandise Authorization). Giao diện được xây dựng trên nền tảng Blazor (kết hợp với thư viện MudBlazor), mang lại trải nghiệm người dùng mượt mà, chuyên nghiệp và nhất quán.

## Colors
Hệ thống màu sắc được thiết kế với độ tương phản cao, giúp người dùng dễ dàng nhận biết các trạng thái của hệ thống:
- **Primary (#1b6ec2):** Màu xanh dương chủ đạo, dùng cho các nút hành động chính (Primary Button) và các thành phần nổi bật.
- **Secondary (#0071c1):** Dùng cho các văn bản liên kết (Links) và các thành phần phụ trợ.
- **Success (#26b050):** Màu xanh lá biểu thị trạng thái thành công hoặc dữ liệu hợp lệ.
- **Error (#f44336 / #b32121):** Màu đỏ dùng để biểu thị lỗi, viền báo lỗi. Dự án còn có hiệu ứng `pulse-red` (nhịp đập) để thu hút sự chú ý vào các cảnh báo nghiêm trọng.

## Typography
Sử dụng bộ phông chữ tiêu chuẩn, an toàn cho web để tối ưu hóa tốc độ tải và đảm bảo tính dễ đọc trên mọi thiết bị làm việc:
- **Font Family:** `'Helvetica Neue', Helvetica, Arial, sans-serif`

## Layout & Tương tác
- **Forms:** Áp dụng thiết kế Floating Labels (nhãn nổi) để tiết kiệm không gian và tạo vẻ ngoài gọn gàng.
- **Accessibility:** Các phần tử có thể tương tác (như nút, ô nhập liệu) đều có viền sáng (focus ring) màu xanh sáng (`#258cfb`) rõ ràng khi được focus, hỗ trợ tốt cho việc điều hướng bằng bàn phím.
