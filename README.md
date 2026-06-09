# SongLinhRMA.System

Hệ thống quản lý quy trình bảo hành (RMA - Return Merchandise Authorization) chuyên nghiệp cho doanh nghiệp **SongLinh**. Dự án được xây dựng dựa trên kiến trúc phân tách Client - Server hiện đại, sử dụng cơ sở dữ liệu **Google Cloud Firestore** tốc độ cao, hỗ trợ tính toán thời hạn SLA, quét OCR mã vạch/văn bản và gửi cảnh báo quá hạn.

---

## 1. Công nghệ sử dụng (Tech Stack)

### Backend (RMA.Server)
- **Framework:** ASP.NET Core Web API (.NET 10.0.0)
- **Database:** Google Cloud Firestore (NoSQL Document DB)
- **Authentication:** Firebase Auth & JWT Bearer Token (hỗ trợ cả tài khoản Local phục vụ kiểm thử)
- **Dịch vụ hỗ trợ:**
  - **OCR & Barcode Reader:** Google Cloud Vision OCR kết hợp Tesseract OCR.
  - **Thông báo đẩy:** Firebase Cloud Messaging (FCM) gửi cảnh báo.
  - **Xuất file:** Sinh biên nhận RMA dưới dạng file PDF (`QuestPDF` hoặc tương tự).
  - **Hệ thống cảnh báo:** Background Service tính toán quá hạn SLA và cập nhật mức độ khẩn cấp (14 ngày).

### Frontend (RMA.Client)
- **Framework:** Blazor WebAssembly (WASM) Single Page Application (SPA).
- **Styling:** CSS hiện đại, thiết kế Responsive.

---

## 2. Sơ đồ cơ sở dữ liệu ERD (Mermaid)

Khi đẩy dự án này lên **GitHub / GitLab / Azure DevOps**, khối mã Mermaid dưới đây sẽ tự động hiển thị dưới dạng **sơ đồ quan hệ thực thể tương tác trực quan**:

```mermaid
erDiagram
    customers ||--o{ devices : "sở hữu"
    customers ||--o{ rma_tickets : "yêu cầu"
    categories ||--o{ models : "phân nhóm"
    models ||--o{ devices : "định danh mẫu"
    devices ||--o{ rma_tickets : "lịch sử bảo hành"
    status_masters ||--o{ rma_tickets : "trạng thái hiện tại"
    status_masters ||--o{ status_histories : "ghi nhận trạng thái"
    vendors |o--o{ rma_tickets : "hãng tiếp nhận"
    locations |o--o{ status_histories : "địa điểm xử lý"
    rma_tickets ||--|{ status_histories : "nhật ký thay đổi"
    rma_tickets ||--o{ attachments : "tệp đính kèm"

    customers {
        string Id PK
        string Name "Tên khách hàng/đại lý"
        string ContactPerson "Người liên hệ"
        string Phone "Số điện thoại"
        string Email "Thư điện tử"
        string Address "Địa chỉ nhận trả"
        string AvatarUrl "Ảnh đại diện"
        datetime CreatedAt "Thời gian tạo"
    }

    devices {
        string Id PK
        string SerialNumber "Số sê-ri máy (S/N)"
        string CustomerId FK "Khách hàng sở hữu"
        string ModelId FK "Dòng sản phẩm"
        datetime PurchaseDate "Ngày mua"
        datetime WarrantyExpiry "Ngày hết hạn BH"
    }

    models {
        string Id PK
        string CategoryId FK "Danh mục lớn"
        string Brand "Thương hiệu"
        string ModelName "Tên dòng máy"
    }

    categories {
        string Id PK
        string Name "Laptop, PC, UPS..."
    }

    vendors {
        string Id PK
        string Name "Dell Service, HP Service..."
        string ContactInfo "Hotline/Địa chỉ"
        string WarrantyLink "Link tra cứu S/N"
    }

    status_masters {
        string Id PK
        string StatusName "Chờ duyệt, Đã nhận sửa..."
        string ColorCode "Mã màu Hex"
    }

    locations {
        string Id PK
        string Name "Tại Cty, Tại Hãng, Kho..."
    }

    rma_tickets {
        string Id PK
        string DeviceId FK "Thiết bị bảo hành"
        string CustomerId FK "Khách hàng yêu cầu"
        string StatusId FK "Trạng thái hiện tại"
        string VendorId FK "Hãng bảo hành gửi đi (tùy chọn)"
        string ProblemDescription "Mô tả lỗi gặp phải"
        string ServiceMode "Chế độ (Warranty/Repair)"
        datetime ReceivedDate "Ngày tiếp nhận"
        datetime SentDate "Ngày gửi đi hãng"
        bool IsUrgent "Yêu cầu xử lý gấp"
        string WarningColor "Cảnh báo SLA (Yellow/Red)"
        string StaffNote "Ghi chú kỹ thuật"
        string EndUserName "Tên khách hàng cuối"
    }

    status_histories {
        string Id PK
        string RmaTicketId FK "Thuộc phiếu RMA"
        string LocationId FK "Vị trí chuyển tới"
        string StatusId FK "Trạng thái mới"
        datetime UpdateTime "Thời gian cập nhật"
        string Note "Ghi chú chi tiết cập nhật"
    }

    attachments {
        string Id PK
        string RmaTicketId FK "Thuộc phiếu RMA"
        string FileUrl "Đường dẫn ảnh/tệp"
        string FileType "Loại ảnh (SN_PHOTO/CONDITION_PHOTO)"
        datetime UploadedAt "Thời gian đăng tải"
    }
```

---

## 3. Cấu trúc thư mục dự án

```text
SongLinhRMA/
├── RMA.Client/         # Dự án Blazor WebAssembly frontend
│   ├── Pages/          # Các trang nghiệp vụ (Dashboard, Tickets, Devices, Customers)
│   ├── Services/       # Service kết nối API backend
│   └── Layout/         # Giao diện chính (MainLayout, NavMenu)
│
├── RMA.Server/         # Dự án Web API backend
│   ├── Controllers/    # Các API Endpoint (RMA, Khách hàng, Thiết bị, Tham chiếu)
│   ├── Entities/       # Khai báo cấu trúc dữ liệu Firestore (Models)
│   ├── Services/       # Logic xử lý (Firestore, OCR, FCM, PDF, SLA)
│   └── appsettings.json# Cấu hình dự án (Firebase, JWT keys)
│
├── RMA.Shared/         # Thư viện dùng chung chứa DTOs (Data Transfer Objects)
└── RMA.Server.Tests/   # Bộ mã kiểm thử tự động xUnit (SLA, Background Service, API)
```

---

## 4. Hướng dẫn khởi chạy dự án

### Cấu hình Firebase Credentials
Hệ thống kết nối trực tiếp với Firestore qua tài khoản dịch vụ (Service Account).
1. Tải file cấu hình JSON từ **Firebase Console** -> **Project Settings** -> **Service Accounts** -> **Generate new private key**.
2. Đổi tên tệp tải về thành `serviceAccountKey.json` và lưu vào thư mục `RMA.Server/`.

### Khởi chạy Backend và Client đồng thời
Mở terminal tại thư mục gốc của dự án và chạy các lệnh tương ứng:

```bash
# Chạy dự án Backend Web API
dotnet watch --project RMA.Server

# Chạy dự án Blazor WASM Client
dotnet watch --project RMA.Client
```

- Địa chỉ mặc định của API Swagger: `https://localhost:7136/swagger/index.html` hoặc cổng HTTP tương ứng.
- Địa chỉ mặc định của Client: `http://localhost:5286` hoặc `https://localhost:7237`.