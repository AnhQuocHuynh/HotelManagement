# PROJECT GUIDE AND STANDARDS

## Mục lục
1. Hướng dẫn cài đặt (Setup)
2. Hướng dẫn sử dụng & vận hành
3. Coding Style & UI Style Guide
4. Dependency Injection Best Practices

---

## 1. Hướng dẫn cài đặt (Tóm tắt)
- Yêu cầu: Windows 10/11, .NET 8, SQL Server, Visual Studio 2022/VS Code.
- Clone repo, chạy script `setup-database-config.bat` hoặc copy file cấu hình DB thủ công.
- Sửa `Config/DatabaseConfig.cs` cho đúng server name.
- Chạy `dotnet restore`, `dotnet build`, `dotnet run` để khởi động ứng dụng.
- Tài khoản mặc định: admin1, manager1, cleaner1, technician1, receptionist1 (mật khẩu giống username).
- Lưu ý: Không commit file DatabaseConfig.cs, khi chuyển nhánh cần chạy lại script nếu file này bị mất.

--- 

---

## 2. Hướng dẫn sử dụng & vận hành (Tóm tắt)
- Ứng dụng WPF .NET 8, kiến trúc MVVM, Material Design UI.
- Chạy `dotnet run --project HotelManager.csproj` để khởi động.
- Hệ thống có 4 role: Admin, Manager, Staff (Receptionist, Cleaner, Technician).
- Mỗi role có giao diện riêng, chức năng quản lý khác nhau.
- Database tự động tạo và seed data khi chạy lần đầu.
- Troubleshooting: Kiểm tra SQL Server, server name, Windows Authentication.

---

## 3. Coding Style & UI Style Guide (Tóm tắt)
- Áp dụng Material Design 3: Clean, Consistent, Accessible, Responsive, Intuitive.
- Color System: Primary (Purple), Secondary (Teal), Surface (White), Semantic colors (Success, Warning, Error).
- Typography: Segoe UI font, hierarchy từ Headline đến Label, spacing hệ thống.
- Component Styles: Buttons (Primary, Secondary, Text, Icon), Cards, Form Controls, Data Grid.
- Layout Patterns: Page structure, Card layout, Navigation drawer.
- Accessibility: High contrast, keyboard navigation, screen reader support.

---

## 4. Dependency Injection Best Practices (Tóm tắt)
- Sử dụng Microsoft.Extensions.DependencyInjection cho DI container.
- Service Lifetime: Scoped cho DbContext, Transient cho ViewModels, Singleton cho configuration.
- Interface-based architecture: IUnitOfWork, IRepository<T>, IService interfaces.
- Constructor injection cho tất cả dependencies.
- Registration trong App.xaml.cs, Host.CreateDefaultBuilder pattern.
- Backward compatibility: Giữ legacy constructors cho gradual migration.
- Testing: Easy mocking với interface injection, test isolation.

--- 