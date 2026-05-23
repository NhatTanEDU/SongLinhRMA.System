# 📕 Hướng dẫn sử dụng & Kiểm tra (Firebase JWT & Skills)

Dự án đã được dọn dẹp sạch bóng SQL Server và chuyển sang **Firebase Auth**. Dưới đây là cách để bạn bắt đầu sử dụng.

---

## 🔐 1. Kiểm tra xác thực Firebase JWT

Tôi đã tạo sẵn một API để kiểm tra tại: `/api/auth/test`. API này yêu cầu phải có Token hợp lệ.

### Cách lấy Token để kiểm tra:
1. Bạn có thể lấy **ID Token** từ phía Frontend (sau khi người dùng đăng nhập bằng Firebase SDK).
2. Hoặc dùng **Firebase CLI** để lấy token nhanh: `gcloud auth print-identity-token` (nếu đã cài).
3. Sử dụng file `test-api.http` tôi đã tạo sẵn trong thư mục gốc nếu bạn cài extension **REST Client** trong VS Code.

### Cấu hình Project ID:
Tôi đã cập nhật `ProjectId: "onglinh-rma-production"` vào cả hai thư mục `RMA.Server` để đảm bảo đồng bộ.

---

## 🛠️ 2. Cách sử dụng bộ kỹ năng (Skills)

Bạn vừa cài đặt xong 1,400+ kỹ năng vào thư mục `.agents/skills`. Đây không phải là file để bạn chạy trực tiếp, mà là **"Sách kỹ năng"** dành cho tôi (AI Assistant).

### Cách kích hoạt:
Bạn chỉ cần gõ các lệnh sau vào khung chat với tôi:

*   **`/tdd`**: Khi bạn muốn tôi viết Test trước rồi mới viết Code (Test-Driven Development).
*   **`/diagnose`**: Khi bạn gặp lỗi build hoặc lỗi runtime phức tạp cần chẩn đoán sâu.
*   **`/grill-me`**: Khi bạn muốn tôi "chất vấn" ngược lại bạn để làm rõ yêu cầu dự án.
*   **`/triage`**: Khi bạn muốn tôi phân loại và xử lý các lỗi/tính năng mới.

---

## 🚀 3. Lệnh chạy ứng dụng

Bây giờ bạn có thể dừng lệnh `dotnet run` cũ và chạy lại để áp dụng code mới:

```powershell
cd RMA.Server
dotnet build
dotnet run
```

Nếu có bất kỳ lỗi nào, hãy dùng lệnh `/diagnose` để tôi hỗ trợ bạn ngay lập tức!
