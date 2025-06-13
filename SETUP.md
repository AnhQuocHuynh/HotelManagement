# 🛠️ Hướng dẫn Setup Hotel Manager Project

## 📋 Yêu cầu hệ thống
- Windows 10/11
- .NET 8.0 SDK
- SQL Server (Express/LocalDB/Full)
- Visual Studio 2022 hoặc VS Code

## 🚀 Cách setup project

### 1. Clone repository
```bash
git clone <repository-url>
cd HotelManager
```

### 2. Setup Database Configuration

#### **Cách 1: Sử dụng script tự động (Khuyến nghị)**
```bash
# Chạy script setup
setup-database-config.bat
```

#### **Cách 2: Tạo thủ công**
1. Copy file template:
   ```bash
   copy "Config\DatabaseConfig.example.cs" "Config\DatabaseConfig.cs"
   ```

2. Mở file `Config\DatabaseConfig.cs` và sửa:
   ```csharp
   // Thay đổi dòng này:
   public const string ServerName = "YOUR_SERVER_NAME\\SQLEXPRESS";
   
   // Thành server name của bạn, ví dụ:
   public const string ServerName = "LAPTOP-CUA-QUOC\\SQLEXPRESS01";
   ```

### 3. Tìm SQL Server Name của bạn

#### **Cách 1: SQL Server Management Studio (SSMS)**
1. Mở SSMS
2. Trong dialog "Connect to Server", xem tên trong field "Server name"
3. Copy tên đó (ví dụ: `LAPTOP-ABC\\SQLEXPRESS`)

#### **Cách 2: Command Line**
```cmd
# Liệt kê SQL Server instances
sqlcmd -L

# Hoặc sử dụng PowerShell
Get-Service -Name "*SQL*" | Where-Object {$_.Status -eq "Running"}
```

#### **Cách 3: Services Manager**
1. Win + R → `services.msc`
2. Tìm service có tên "SQL Server (INSTANCE_NAME)"
3. Instance name sẽ là phần trong ngoặc

### 4. Chạy ứng dụng
```bash
dotnet restore
dotnet build
dotnet run
```

## 🔐 Tài khoản mặc định

| Username | Password | Role | Mô tả |
|----------|----------|------|-------|
| `admin1` | `admin1` | Admin | Quản trị viên |
| `manager1` | `manager1` | Manager | Quản lý |
| `cleaner1` | `cleaner1` | Staff | Nhân viên dọn phòng |
| `technician1` | `technician1` | Staff | Kỹ thuật viên |
| `receptionist1` | `receptionist1` | Staff | Lễ tân |

## ❗ Lưu ý quan trọng

### 🚫 **KHÔNG commit file DatabaseConfig.cs**
- File này chứa thông tin database cá nhân
- Mỗi developer có server name khác nhau
- File đã được thêm vào `.gitignore`

### 🔄 **Khi chuyển nhánh**
Nếu file `DatabaseConfig.cs` bị mất khi chuyển nhánh:
```bash
# Chạy lại script setup
setup-database-config.bat
```

### 🗄️ **Database Migration**
Project sẽ tự động tạo database và seed data khi chạy lần đầu.

## 🆘 Troubleshooting

### Lỗi kết nối database
1. Kiểm tra SQL Server đã chạy chưa
2. Xác nhận lại server name trong `DatabaseConfig.cs`
3. Kiểm tra Windows Authentication có hoạt động không

### Build errors
1. Chạy `dotnet clean`
2. Chạy `dotnet restore`
3. Rebuild solution

### File DatabaseConfig.cs bị mất
1. Chạy `setup-database-config.bat`
2. Hoặc copy manual từ template file 