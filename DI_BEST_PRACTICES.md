# 📚 HƯỚNG DẪN THỰC HÀNH DEPENDENCY INJECTION (DI) CHO DỰ ÁN HOTEL MANAGER

## 1. **Dependency Injection (DI) là gì?**
- **Dependency Injection (DI)** là kỹ thuật giúp "bơm" các phụ thuộc (service, repository, logger,...) vào các class (ViewModel, Service, Controller,...) thay vì tự khởi tạo bên trong class đó.
- **Lợi ích:**
  - Dễ dàng mở rộng, bảo trì, kiểm thử (unit test dễ mock các phụ thuộc)
  - Giảm sự phụ thuộc cứng (tight coupling), tăng tính module hóa
  - Quản lý vòng đời đối tượng hiệu quả (lifetime management)

## 2. **Lợi ích khi áp dụng DI đúng chuẩn**
- Tất cả ViewModel, Service đều nhận đúng phụ thuộc cần thiết
- Không còn khởi tạo thủ công (new ...) trong code
- Dễ dàng thay thế, mở rộng, kiểm thử
- Code sạch, dễ đọc, dễ bảo trì

## 3. **Các loại vòng đời (Lifetime) trong DI**
- **Singleton:** Chỉ tạo một instance duy nhất cho toàn bộ ứng dụng (ví dụ: NotificationService, NavigationService)
- **Scoped:** Tạo một instance cho mỗi scope (thường dùng cho mỗi request hoặc mỗi lần mở một transaction, ví dụ: UnitOfWork, DbContext)
- **Transient:** Tạo mới mỗi lần được yêu cầu (thường dùng cho ViewModel)

## 4. **Checklist thực hành chuẩn DI cho team**
- [x] Xóa hết DataContext hardcode trong XAML (không dùng <UserControl.DataContext> với ViewModel trực tiếp)
- [x] Đăng ký tất cả ViewModel, Service trong DI container (App.xaml.cs)
- [x] Constructor của ViewModel/Service chỉ nhận các dependency đã đăng ký
- [x] Set DataContext trong code-behind bằng DI
- [x] Luôn kiểm tra binding, command, data flow
- [x] Viết unit test cho các ViewModel/Service quan trọng
- [x] Có log và exception handler ở các điểm nhạy cảm

## 5. **Ví dụ thực tế**
```csharp
// Đăng ký DI trong App.xaml.cs
services.AddTransient<ReceptionistViewModel>();
services.AddScoped<BookingService>();

// Inject ViewModel vào View
public ReceptionistView()
{
    InitializeComponent();
    if (App.ServiceProvider != null)
        DataContext = App.ServiceProvider.GetRequiredService<ReceptionistViewModel>();
}

// Constructor injection trong Service
public BookingService(IUnitOfWork unitOfWork, CustomerService customerService, ...)
{
    _unitOfWork = unitOfWork;
    _customerService = customerService;
    ...
}
```

## 6. **Anti-pattern cần tránh**
- ❌ Không khởi tạo ViewModel bằng XAML:
```xml
<UserControl.DataContext>
    <vm:ReceptionistViewModel/>
</UserControl.DataContext>
```
- ❌ Không khởi tạo service bằng từ khóa `new` trong ViewModel/Service
- ❌ Không sử dụng Service Locator (App.ServiceProvider.GetService<...>()) trong constructor (chỉ dùng cho fallback hoặc design-time)

## 7. **Hướng dẫn kiểm tra và bảo trì**
- Luôn kiểm tra lại file App.xaml.cs để đảm bảo đã đăng ký đủ các ViewModel/Service cần thiết
- Khi thêm ViewModel/Service mới, phải đăng ký vào DI container
- Khi sửa constructor, đảm bảo các dependency đều đã đăng ký
- Khi gặp lỗi không inject được, kiểm tra lại thứ tự đăng ký và vòng đời (lifetime)
- Viết unit test cho các logic quan trọng, mock các service/phụ thuộc khi test

## 8. **Lợi ích khi làm đúng DI**
- Dễ dàng mở rộng tính năng, bảo trì code
- Dễ kiểm thử tự động (unit test)
- Giảm lỗi runtime do thiếu phụ thuộc
- Code rõ ràng, dễ đọc, dễ bàn giao cho thành viên mới

---

**Tài liệu này dành cho toàn bộ team phát triển dự án Hotel Manager. Nếu có thắc mắc hoặc cần bổ sung ví dụ thực tế, hãy liên hệ leader hoặc người phụ trách kiến trúc dự án.** 