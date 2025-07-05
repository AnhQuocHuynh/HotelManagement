@echo off
echo ===========================================
echo   Hotel Manager - Database Setup
echo ===========================================
echo.

REM Kiểm tra xem file DatabaseConfig.cs đã tồn tại chưa
if exist "HotelManager.Core\Config\DatabaseConfig.cs" (
    echo [INFO] DatabaseConfig.cs đã tồn tại!
    echo [SKIP] Bỏ qua việc tạo file mới.
    echo.
    echo [HINT] Nếu cần cập nhật server name, hãy sửa file HotelManager.Core\Config\DatabaseConfig.cs
    pause
    exit /b 0
)

REM Copy template file
echo [INFO] Đang tạo DatabaseConfig.cs từ template...
copy "HotelManager.Core\Config\DatabaseConfig.example.cs" "HotelManager.Core\Config\DatabaseConfig.cs" >nul

echo [SUCCESS] Đã tạo file HotelManager.Core\Config\DatabaseConfig.cs
echo.
echo [TODO] Vui lòng:
echo   1. Mở file HotelManager.Core\Config\DatabaseConfig.cs
echo   2. Thay đổi "YOUR_SERVER_NAME\\SQLEXPRESS" thành SQL Server của bạn
echo   3. Ví dụ: "LAPTOP-CUA-QUOC\\SQLEXPRESS01"
echo.
echo [HINT] Để tìm tên server:
echo   - Mở SQL Server Management Studio (SSMS)
echo   - Xem tên server trong connection dialog
echo.
pause 