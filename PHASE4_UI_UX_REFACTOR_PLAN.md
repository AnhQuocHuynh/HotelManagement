# Phase 4: UI/UX & Maintainability Refactor Plan

## Mục tiêu
- Chuẩn hóa kiến trúc MVVM, loại bỏ code-behind khỏi View.
- Nâng cấp toàn bộ UI theo phong cách Material Design hiện đại, nổi khối, sang trọng.
- Đảm bảo UI nhất quán, dễ bảo trì, dễ mở rộng, tăng khả năng tái sử dụng.
- Tối ưu trải nghiệm người dùng (UX) chuyên nghiệp.

---

## 1. Chuẩn bị
### 1.1. Cài đặt thư viện
- [x] **MaterialDesignThemes** (v5+, .NET 8)
- [x] **MaterialDesignColors**
- [x] **CommunityToolkit.Mvvm** (hoặc tự triển khai RelayCommand, ObservableValidator)

### 1.2. Tạo ResourceDictionary
- `UI/Themes/MaterialDesignTheme.xaml` — cấu hình màu chủ đạo, light/dark, accent.
- `UI/Themes/Typography.xaml` — font, cỡ chữ, weight.
- `UI/Themes/Controls.xaml` — style chung: radius, elevation, padding, shadow.
- Merge các dictionary này vào `App.xaml`:
  ```xml
  <Application.Resources>
    <ResourceDictionary>
      <ResourceDictionary.MergedDictionaries>
        <MaterialDesign:BundledTheme ... />
        <ResourceDictionary Source="UI/Themes/MaterialDesignTheme.xaml" />
        <ResourceDictionary Source="UI/Themes/Typography.xaml" />
        <ResourceDictionary Source="UI/Themes/Controls.xaml" />
      </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
  </Application.Resources>
  ```

---

## 2. Chuẩn hóa kiến trúc MVVM
### 2.1. ViewModel
- Đảm bảo **tất cả ViewModel kế thừa `BaseViewModel`** (có INotifyPropertyChanged, logging, audit).
- Tạo `ValidatableBase` (kế thừa BaseViewModel, triển khai IDataErrorInfo/INotifyDataErrorInfo hoặc ObservableValidator).
- Sử dụng **RelayCommand/AsyncRelayCommand** cho mọi thao tác UI.

### 2.2. Helper/Service
- Tạo `INotificationService` (hiện Snackbar, Dialog, xác nhận, báo lỗi).
- Tạo `INavigationService` (điều hướng giữa các màn hình qua ViewModel).
- Đăng ký các service này vào DI container.

### 2.3. Extension/Helper
- `SnackbarExtensions` — hiện Snackbar từ ViewModel.
- `DialogHostExtensions` — mở dialog xác nhận.
- `GridExtensions.Spacing` — attached property cho spacing đồng bộ.

---

## 3. Thiết kế lại UI bằng Material Design
### 3.1. Thay thế control mặc định
- `<TextBox>` → `<TextBox Style="{StaticResource MaterialDesignOutlinedTextBox}" materialDesign:HintAssist.Hint="..." />`
- `<Button>` → `<Button Style="{StaticResource MaterialDesignContainedButton}" ... />`
- Dùng `<materialDesign:Card>` để nhóm nội dung.
- Hiện thông báo qua `<materialDesign:Snackbar>`.
- Dialog xác nhận qua `<materialDesign:DialogHost>`.

### 3.2. Bố cục
- Sử dụng **Grid, StackPanel** với spacing, margin, padding hợp lý.
- Áp dụng **CornerRadius, ShadowAssist.Depth** cho Card, Button, Dialog.
- Đảm bảo responsive, dễ nhìn, nổi khối.

### 3.3. Style đồng bộ
- Định nghĩa màu chủ đạo, font, padding, radius, elevation trong ResourceDictionary.
- Tạo Style Guide (`STYLE_GUIDE.md`) mô tả quy tắc sử dụng màu, font, icon, naming resource.

---

## 4. Tái cấu trúc bố cục giao diện
- Mỗi màn hình chức năng là một **UserControl** riêng biệt.
- Điều hướng qua `MainViewModel.CurrentView` (kiểu BaseViewModel) và binding vào `<ContentControl Content="{Binding CurrentView}" />` trong MainWindow.
- Không code logic trong XAML, **mọi thao tác đều binding qua Command/Property**.

---

## 5. Validation & Notification
- Sử dụng `ValidatesOnDataErrors=True` trong binding.
- Hiện lỗi qua template Material (`materialDesign:TextFieldAssist.HasErrorsTemplate`).
- Notification/Confirm/Dialog đều gọi qua service, không dùng MessageBox trực tiếp.

---

## 6. Lộ trình triển khai (gợi ý)
**Week 1**
- Ngày 1-2: Cài đặt thư viện, tạo theme, cấu hình App.xaml.
- Ngày 3-4: Chuẩn hóa BaseViewModel, Validation, Notification, NavigationService.
- Ngày 5: Refactor MainWindow (DrawerHost, Snackbar, ContentControl).

**Week 2**
- Ngày 6-8: Refactor các màn hình chính (Login, Booking, Payment).
- Ngày 9-10: Refactor Staff/Admin Views, chạy Unit-Test, cập nhật Style Guide, tài liệu.

---

## 7. Kết quả kỳ vọng
- UI hiện đại, nổi khối, sang trọng, nhất quán Material Design.
- 100% interaction qua Binding/Command, không còn code-behind.
- Navigation tập trung, dễ thêm màn hình mới.
- Validation & Notification có helper tái sử dụng.
- Kiến trúc MVVM thuần, dễ test, dễ bảo trì, sẵn sàng mở rộng.

---

## 8. Tài liệu tham khảo
- [MaterialDesignInXAML Docs](https://materialdesigninxaml.net/)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [Google Material Design](https://m3.material.io/)

---

# Checklist Triển Khai Phase 4

## Module 1: Chuẩn hóa kiến trúc MVVM & DI
- [ ] Tất cả ViewModel kế thừa BaseViewModel (INotifyPropertyChanged, logging, audit)
- [ ] Xóa toàn bộ event handler (Click, Loaded, ...) khỏi code-behind
- [ ] Chuyển toàn bộ logic UI sang Command trong ViewModel
- [ ] Đăng ký ViewModel, Service vào DI container
- [ ] Đảm bảo không còn logic xử lý trong XAML/code-behind
- [ ] Viết unit test xác nhận binding, DI hoạt động đúng

---

## Module 2: Xây dựng Helper & Service tái sử dụng
- [ ] Tạo ValidatableBase (hoặc ObservableValidator) cho validation
- [ ] Xây dựng INotificationService (Snackbar, Dialog, Confirm)
- [ ] Xây dựng INavigationService (điều hướng ViewModel)
- [ ] Viết extension cho Snackbar, DialogHost, GridSpacing
- [ ] Đăng ký các helper/service vào DI
- [ ] Viết unit test cho các helper/service

---

## Module 3: Thiết lập ResourceDictionary & Style Guide
- [ ] Tạo UI/Themes/MaterialDesignTheme.xaml (màu chủ đạo, light/dark, accent)
- [ ] Tạo UI/Themes/Typography.xaml (font, cỡ chữ, weight)
- [ ] Tạo UI/Themes/Controls.xaml (radius, elevation, padding, shadow)
- [ ] Merge ResourceDictionary vào App.xaml
- [ ] Viết STYLE_GUIDE.md mô tả quy tắc style, naming resource, màu chủ đạo
- [ ] Review UI hiện tại, xác định các điểm cần chuẩn hóa style

---

## Module 4: Refactor MainWindow & Navigation
- [ ] Refactor MainWindow.xaml dùng MaterialDesign DrawerHost, Snackbar, ContentControl
- [ ] Cài đặt NavigationService, binding CurrentView
- [ ] Đảm bảo mọi màn hình là UserControl, không còn Window riêng lẻ
- [ ] Test navigation, notification toàn cục
- [ ] Đảm bảo MainWindow không chứa logic xử lý UI ngoài ViewModel

---

## Module 5: Refactor từng màn hình chức năng (UserControl)
### (Lặp lại cho từng màn hình: Login, Booking, Payment, Room, Staff/Admin, ...)
- [ ] Thay control mặc định bằng control Material (TextField, ContainedButton, Card, DialogHost, Snackbar)
- [ ] Bố cục lại bằng Grid/StackPanel, spacing, corner radius, shadow
- [ ] Áp dụng validation, notification qua service
- [ ] Đảm bảo mọi thao tác qua Command, không code logic trong XAML
- [ ] Review UI/UX, test chức năng, kiểm tra responsive

---

## Module 6: Hoàn thiện, kiểm thử, tài liệu hóa
- [ ] Chạy unit test, UI test, kiểm tra lại toàn bộ flow
- [ ] Review lại style, theme, resource, đảm bảo nhất quán
- [ ] Cập nhật README, STYLE_GUIDE, hướng dẫn tạo màn hình mới
- [ ] Lấy feedback người dùng, tinh chỉnh UI/UX nếu cần
- [ ] Tổng kết, nghiệm thu Phase 4

---

**Lưu ý:**
- Không chuyển sang module tiếp theo nếu module hiện tại chưa hoàn thành và test ổn định.
- Checklist này cần được review và tick đầy đủ trước khi kết thúc Phase 4. 