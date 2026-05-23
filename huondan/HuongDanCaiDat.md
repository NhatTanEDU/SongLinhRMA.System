# Hướng Dẫn Cài Đặt và Chạy Dự Án SongLinhRMA.System

Dự án này sử dụng kiến trúc:
- **Frontend:** Blazor WebAssembly (.NET 10)
- **Backend:** ASP.NET Core Web API (.NET 10)
- **Database:** SQL Server qua Entity Framework Core

Dưới đây là các bước để cài đặt và chạy trên một máy tính mới từ đầu.

---

## Bước 1: Cài đặt công cụ và môi trường
1. **Cài đặt .NET SDK:**
   - Tải về và cài đặt **.NET 10.0 SDK** (hoặc mới nhất) tại: `https://dotnet.microsoft.com/download`
   - Hoặc cài đặt qua dòng lệnh (nếu có winget): `winget install --id Microsoft.DotNet.SDK.10`
   - **Lưu ý Cực Kỳ Quan Trọng:** Sau khi cài đặt, nếu bạn đang mở sẵn Terminal / Command Prompt / VS Code, bạn **phải tắt hẳn VS Code đi và mở lại** để máy tính nhận diện được lệnh `dotnet`. Việc chỉ mở tab Terminal mới là không đủ vì nó vẫn ăn theo cấu hình cũ của VS Code.

2. **Cài đặt Database (Chỉ cần nếu bạn chạy Backend):**
   - Tải và cài đặt **SQL Server Express** và **SQL Server Management Studio (SSMS)**.
   - Cài đặt công cụ Entity Framework CLI: mở terminal chạy `dotnet tool install --global dotnet-ef`

---

## Bước 2: Khởi chạy dự án

### Tùy chọn A: Chỉ phát triển Frontend (Đã được Mock Data)
Nếu bạn chỉ muốn code giao diện và không muốn thiết lập Server/Database, dự án đã được tôi chỉnh sửa tạm thời để dùng dữ liệu giả lập trên RAM.
1. Mở Terminal (trong VS Code hoặc PowerShell).
2. Di chuyển vào thư mục Frontend:
   ```bash
   cd RMA.Client
   ```
3. Chạy lệnh:
   ```bash
   dotnet run
   ```
4. Mở trình duyệt và truy cập vào đường dẫn mà Terminal cung cấp (ví dụ: `http://localhost:5286`).

**🔑 Tính năng đăng nhập (Mock Authentication):**
Hệ thống Frontend đã được tích hợp giao diện đăng nhập xịn xò với JWT (Mock). Bạn có thể gõ bất kỳ Username/Password nào để đăng nhập thử nghiệm giao diện. Khi bạn muốn tích hợp Backend JWT thật, bạn chỉ cần thay đổi class `MockAuthStateProvider` thành gọi API thực tế.

### Tùy chọn B: Chạy Toàn Bộ Dự Án (Frontend + Backend)
Nếu bạn muốn dùng Database thực tế:
1. **Hoàn tác Mock Data:** Phục hồi các file `CustomerService.cs`, `DeviceService.cs`, `RmaTicketService.cs` trong `RMA.Client/Services` về lại trạng thái gọi `HttpClient.GetFromJsonAsync`.
2. **Cấu hình Connection String:** Mở `RMA.Server/appsettings.json`, sửa lại `DefaultConnection` cho phù hợp với SQL Server của bạn.
3. **Cập nhật Database:**
   ```bash
   cd RMA.Server
   dotnet ef database update
   ```
4. **Chạy Backend:**
   ```bash
   cd RMA.Server
   dotnet run
   ```
5. **Chạy Frontend (Mở Terminal khác):**
   ```bash
   cd RMA.Client
   dotnet run
   ```

---
*Chúc bạn code vui vẻ!*
