# Hướng Dẫn Kéo Code Từ Nhánh (Pull Code) Và Ghi Đè Local

Tài liệu này hướng dẫn cách kéo code mới nhất từ một nhánh trên GitHub về máy tính và ép (force) ghi đè hoàn toàn lên code cũ ở máy (loại bỏ mọi thay đổi chưa được lưu lại ở máy của bạn). Đây là giải pháp rất hữu ích khi bạn chuyển đổi giữa nhiều máy (như máy ở công ty và máy ở nhà) và chỉ muốn làm việc tiếp trên phiên bản chuẩn nhất từ GitHub.

---

## 1. Mở Terminal (Command Prompt / PowerShell / VS Code Terminal)
Hãy đảm bảo bạn đã mở Terminal và trỏ đường dẫn (`cd`) đến đúng thư mục gốc của dự án. 
Ví dụ: `d:\Official_Project\Winform\drive`

---

## 2. Các Lệnh Thực Hiện (Theo thứ tự)

### Bước 1: Lấy thông tin cập nhật mới nhất từ GitHub
```bash
git fetch --all
```
**Giải thích:** Lệnh này giúp Git trên máy tính của bạn "đi hỏi" GitHub xem có sự thay đổi nào mới không (bao gồm các nhánh mới tạo, các commit mới). Nó chỉ tải thông tin về chứ chưa áp dụng ngay vào source code trên máy của bạn.

### Bước 2: Chuyển sang nhánh bạn muốn làm việc
```bash
git checkout ten-nhanh-cua-ban
```
*(Ví dụ ở đây là: `git checkout feat/migrate-to-firebase`)*

**Giải thích:** Lệnh này sẽ chuyển môi trường làm việc của bạn sang nhánh có tên tương ứng. Nếu nhánh này đã có trên GitHub nhưng chưa có ở máy bạn, Git sẽ tự động tạo một nhánh local và liên kết nó với nhánh trên GitHub.

### Bước 3: Ép ghi đè code (Loại bỏ code thừa/sửa đổi ở máy)
```bash
git reset --hard origin/ten-nhanh-cua-ban
```
*(Ví dụ: `git reset --hard origin/feat/migrate-to-firebase`)*

**Giải thích:** 
- `origin` là tên mặc định của server GitHub.
- Lệnh này mang ý nghĩa: "Bất kể ở máy của tôi đang sửa cái gì (chưa commit), hãy xóa hết đi và thay thế bằng phiên bản chính xác 100% của nhánh này đang nằm trên GitHub".

### Bước 4: Xóa sạch các file dư thừa không thuộc về Git
```bash
git clean -fd
```
**Giải thích:** 
Trong quá trình code, bạn có thể đã tạo ra một số file/thư mục mới nhưng chưa đưa vào Git (gọi là untracked files). Lệnh `git reset --hard` ở trên không xóa được các file untracked này. Do đó, ta cần thêm lệnh `git clean -fd` để dọn dẹp sạch sẽ:
- `-f`: Force (bắt buộc xóa)
- `-d`: Xóa luôn cả các thư mục (directories) rác chưa được track.

---

## 💡 Mẹo: Chạy gộp tất cả trong một lần
Nếu bạn đang dùng **PowerShell** hoặc **Bash**, bạn có thể kết hợp các lệnh này lại trên cùng một dòng bằng dấu chấm phẩy `;` hoặc `&&`. 

Ví dụ (sau khi đã thực hiện fetch và checkout thành công):
```bash
git reset --hard origin/feat/migrate-to-firebase; git clean -fd
```

*Lưu ý Cực Kỳ Quan Trọng:* Các lệnh trên sẽ **XÓA VĨNH VIỄN** những phần code bạn vừa gõ ở máy tính mà chưa kịp commit và push lên mạng. Hãy chắc chắn bạn không còn cần giữ lại những file code dang dở đó trước khi chạy lệnh nhé!
