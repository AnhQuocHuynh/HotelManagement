using HotelManager.Config;
using HotelManager.Models.Enums;
using HotelManager.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using HotelManager.Data;
using System.Data;
using HotelManager.Utilities;

namespace HotelManager.ViewModels.StaffViewModels
{
    public class ReceptionistViewModel : BaseViewModel
    {
        private Booking _selectedBooking;
        private Room _selectedRoom;
        private RoomType? _roomTypeFilter;
        private ObservableCollection<Room> _availableRooms = new ObservableCollection<Room>();
        private ObservableCollection<Room> _filteredRooms = new ObservableCollection<Room>();
        private ObservableCollection<Booking> _bookings;
        private Booking _searchCriteria;

        public ObservableCollection<Booking> Bookings
        {
            get => _bookings;
            set
            {
                _bookings = value;
                OnPropertyChanged();
            }
        }

        public RoomType? RoomTypeFilter
        {
            get => _roomTypeFilter;
            set
            {
                _roomTypeFilter = value;
                OnPropertyChanged();
                UpdateFilteredRooms();
            }
        }

        public ObservableCollection<Room> AvailableRooms
        {
            get => _availableRooms;
            set
            {
                _availableRooms = value ?? new ObservableCollection<Room>();
                OnPropertyChanged();
                UpdateFilteredRooms();
            }
        }

        public ObservableCollection<Room> FilteredRooms
        {
            get => _filteredRooms;
            set
            {
                _filteredRooms = value ?? new ObservableCollection<Room>();
                OnPropertyChanged();
            }
        }

        public Booking SelectedBooking
        {
            get => _selectedBooking;
            set
            {
                _selectedBooking = value;
                OnPropertyChanged();
                if (value != null)
                {
                    // Cập nhật form với thông tin booking, không chọn RoomNumber trong ComboBox
                    SearchCriteria = new Booking
                    {
                        Customer = new Customer
                        {
                            FullName = value.Customer?.FullName,
                            CCCD = value.Customer?.CCCD,
                            PhoneNumber = value.Customer?.PhoneNumber,
                            Type = value.Customer?.Type ?? default(CustomerType)
                        },
                        CheckInDate = value.CheckInDate,
                        CheckOutDate = value.CheckOutDate,
                        Status = value.Status,
                        RoomType = value.RoomType,
                        RoomNumber = value.RoomNumber
                    };
                }
                else
                {
                    SearchCriteria = new Booking
                    {
                        Customer = new Customer(),
                        CheckInDate = DateTime.Now,
                        CheckOutDate = DateTime.Now.AddDays(1),
                        Status = BookingStatus.Pending,
                        RoomType = default(RoomType)
                    };
                    SelectedRoom = null;
                    LoadAvailableRooms();
                }
            }
        }

        public Room SelectedRoom
        {
            get => _selectedRoom;
            set
            {
                _selectedRoom = value;
                OnPropertyChanged();
                if (_selectedBooking != null && _selectedRoom != null)
                {
                    _selectedBooking.RoomNumber = _selectedRoom.RoomNumber;
                    SearchCriteria.RoomNumber = _selectedRoom.RoomNumber;
                }
            }
        }

        public Booking SearchCriteria
        {
            get => _searchCriteria;
            set
            {
                _searchCriteria = value;
                OnPropertyChanged();
                UpdateFilteredRooms();
            }
        }

        public ICommand AddNewCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteCommand { get; }

        public ReceptionistViewModel()
        {
            Bookings = new ObservableCollection<Booking>();
            SearchCriteria = new Booking
            {
                Customer = new Customer(),
                CheckInDate = DateTime.Now,
                CheckOutDate = DateTime.Now.AddDays(1),
                Status = BookingStatus.Pending,
                RoomType = default(RoomType)
            };

            AddNewCommand = new RelayCommand(AddNewBooking);
            SaveCommand = new RelayCommand(SaveBooking);
            CancelCommand = new RelayCommand(CancelEditing);
            DeleteCommand = new RelayCommand(DeleteBooking, CanDeleteBooking);
            RoomTypeFilter = null;
            
            LoadBookings();
            LoadAvailableRooms();
        }

        private void UpdateFilteredRooms()
        {
            _filteredRooms.Clear();
            if (AvailableRooms == null || !AvailableRooms.Any())
                return;

            // Lọc phòng theo RoomTypeFilter
            var filtered = AvailableRooms
                .Where(r => RoomTypeFilter == null || r.RoomType == RoomTypeFilter)
                .ToList();

            // Thêm phòng của booking hiện tại nếu phù hợp
            if (SelectedBooking != null && !string.IsNullOrEmpty(SelectedBooking.RoomNumber))
            {
                var currentRoom = AvailableRooms.FirstOrDefault(r => r.RoomNumber == SelectedBooking.RoomNumber);
                if (currentRoom != null && (RoomTypeFilter == null || currentRoom.RoomType == RoomTypeFilter) &&
                    !filtered.Any(r => r.RoomNumber == currentRoom.RoomNumber))
                {
                    filtered.Add(currentRoom);
                }
            }

            foreach (var room in filtered)
            {
                _filteredRooms.Add(room);
            }

            // Không tự động chọn phòng
            SelectedRoom = null;
        }

        private void LoadBookings()
        {
            try
            {
                using var connection = new SqlConnection(DatabaseConfig.GetConnectionString());
                connection.Open();
                var query = @"
                    SELECT b.Id, b.CheckInDate, b.CheckOutDate, b.Status, b.CustomerId, b.RoomNumber,
                    c.FullName, c.PhoneNumber, c.CCCD, c.Type, r.RoomType
                    FROM Bookings b
                    LEFT JOIN Customers c ON b.CustomerId = c.Id
                    LEFT JOIN Rooms r ON b.RoomNumber = r.RoomNumber";
                using var command = new SqlCommand(query, connection);
                using var reader = command.ExecuteReader();
                Bookings.Clear();
                while (reader.Read())
                {
                    var booking = new Booking
                    {
                        Id = reader.GetInt32(0),
                        CheckInDate = reader.GetDateTime(1),
                        CheckOutDate = reader.GetDateTime(2),
                        Status = (BookingStatus)reader.GetInt32(3),
                        CustomerId = reader.GetInt32(4),
                        RoomNumber = reader.IsDBNull(5) ? null : reader.GetString(5),
                        Customer = new Customer
                        {
                            FullName = reader.IsDBNull(6) ? null : reader.GetString(6),
                            PhoneNumber = reader.IsDBNull(7) ? null : reader.GetString(7),
                            CCCD = reader.IsDBNull(8) ? null : reader.GetString(8),
                            Type = reader.IsDBNull(9) ? default(CustomerType) : (CustomerType)reader.GetInt32(9)
                        },
                        RoomType = reader.IsDBNull(10) ? default(RoomType) : (RoomType)reader.GetInt32(10)
                    };
                    Bookings.Add(booking);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách đặt phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAvailableRooms()
        {
            try
            {
                using var connection = new SqlConnection(DatabaseConfig.GetConnectionString());
                connection.Open();
                var query = @"SELECT RoomNumber, RoomType, PricePerNight, IsAvailable 
                              FROM Rooms 
                              WHERE IsAvailable = 1 AND RoomNumber IS NOT NULL 
                              ORDER BY RoomNumber";
                using var command = new SqlCommand(query, connection);
                using var reader = command.ExecuteReader();
                AvailableRooms.Clear();
                while (reader.Read())
                {
                    _availableRooms.Add(new Room
                    {
                        RoomNumber = reader.GetString(0),
                        RoomType = reader.IsDBNull(1) ? default(RoomType) : (RoomType)reader.GetInt32(1),
                        PricePerNight = reader.GetDecimal(2),
                        IsAvailable = reader.GetBoolean(3)
                    });
                }
                UpdateFilteredRooms();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddNewBooking(object parameter)
        {
            if (SearchCriteria.Customer == null)
                SearchCriteria.Customer = new Customer();

            if (string.IsNullOrEmpty(SearchCriteria.Customer.FullName) ||
                string.IsNullOrEmpty(SearchCriteria.Customer.CCCD) ||
                SelectedRoom == null || string.IsNullOrEmpty(SelectedRoom.RoomNumber) ||
                SearchCriteria.CheckOutDate <= SearchCriteria.CheckInDate)
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Họ Tên, CCCD, chọn Số Phòng hợp lệ, và đảm bảo ngày Check-Out sau Check-In.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!IsRoomAvailable(SelectedRoom.RoomNumber))
            {
                MessageBox.Show($"Phòng {SelectedRoom.RoomNumber} không còn khả dụng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                LoadAvailableRooms();
                return;
            }

            SelectedBooking = new Booking
            {
                Id = 0,
                CheckInDate = SearchCriteria.CheckInDate,
                CheckOutDate = SearchCriteria.CheckOutDate,
                Status = SearchCriteria.Status != default(BookingStatus) ? SearchCriteria.Status : BookingStatus.Pending,
                CustomerId = 0,
                RoomNumber = SelectedRoom.RoomNumber,
                RoomType = SearchCriteria.RoomType != default ? SearchCriteria.RoomType : SelectedRoom.RoomType,
                Customer = new Customer
                {
                    FullName = SearchCriteria.Customer.FullName,
                    CCCD = SearchCriteria.Customer.CCCD,
                    PhoneNumber = SearchCriteria.Customer.PhoneNumber,
                    Type = SearchCriteria.Customer.Type
                }
            };

            if (CanSaveBooking())
            {
                SaveBooking(true);
                LoadAvailableRooms();
                LoadBookings();
                OnPropertyChanged(nameof(Bookings));
                OnPropertyChanged(nameof(SelectedBooking));
                MessageBox.Show("Thêm mới đặt phòng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                SearchCriteria = new Booking
                {
                    Customer = new Customer(),
                    CheckInDate = DateTime.Now,
                    CheckOutDate = DateTime.Now.AddDays(1),
                    Status = BookingStatus.Pending,
                    RoomType = default(RoomType)
                };
                SelectedRoom = null;
                SelectedBooking = null;
            }
            else
            {
                MessageBox.Show("Dữ liệu không hợp lệ. Vui lòng kiểm tra thông tin.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveBooking(object parameter)
        {
            bool showSuccess = parameter is not bool || !(bool)parameter;

            if (!CanSaveBooking())
            {
                MessageBox.Show("Không thể lưu vì thông tin không hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (SelectedBooking == null || SelectedBooking.Customer == null || string.IsNullOrEmpty(SelectedBooking.RoomNumber))
            {
                MessageBox.Show("Thông tin đặt phòng không hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Cập nhật thông tin booking từ SearchCriteria
                if (SearchCriteria?.Customer != null)
                {
                    SelectedBooking.Customer.FullName = SearchCriteria.Customer.FullName;
                    SelectedBooking.Customer.CCCD = SearchCriteria.Customer.CCCD;
                    SelectedBooking.Customer.PhoneNumber = SearchCriteria.Customer.PhoneNumber;
                    SelectedBooking.Customer.Type = SearchCriteria.Customer.Type;
                    SelectedBooking.CheckInDate = SearchCriteria.CheckInDate;
                    SelectedBooking.CheckOutDate = SearchCriteria.CheckOutDate;
                    SelectedBooking.Status = SearchCriteria.Status;
                    SelectedBooking.RoomType = SearchCriteria.RoomType;
                }
                if (SelectedRoom != null)
                    SelectedBooking.RoomNumber = SelectedRoom.RoomNumber;

                using var connection = new SqlConnection(DatabaseConfig.GetConnectionString());
                connection.Open();
                using var transaction = connection.BeginTransaction();

                try
                {
                    // Lấy RoomNumber cũ nếu sửa booking
                    string oldRoomNumber = null;
                    if (SelectedBooking.Id > 0)
                    {
                        var getOldRoomQuery = "SELECT RoomNumber FROM Bookings WHERE Id = @Id";
                        using var command = new SqlCommand(getOldRoomQuery, connection, transaction);
                        command.Parameters.AddWithValue("@Id", SelectedBooking.Id);
                        var result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                            oldRoomNumber = result.ToString();
                    }

                    if (!IsRoomExists(SelectedBooking.RoomNumber, connection, transaction))
                    {
                        MessageBox.Show($"Phòng {SelectedBooking.RoomNumber} không tồn tại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        transaction.Rollback();
                        return;
                    }

                    int customerId = SaveOrUpdateCustomer(connection, transaction);
                    if (customerId == 0)
                    {
                        MessageBox.Show("Không thể lưu thông tin khách hàng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        transaction.Rollback();
                        return;
                    }

                    if (SelectedBooking.Id == 0)
                    {
                        var insertQuery = @"INSERT INTO Bookings (CheckInDate, CheckOutDate, Status, CustomerId, RoomNumber)
                                            VALUES (@CheckInDate, @CheckOutDate, @Status, @CustomerId, @RoomNumber)";
                        using var command = new SqlCommand(insertQuery, connection, transaction);
                        command.Parameters.AddWithValue("@CheckInDate", SelectedBooking.CheckInDate);
                        command.Parameters.AddWithValue("@CheckOutDate", SelectedBooking.CheckOutDate);
                        command.Parameters.AddWithValue("@Status", (int)SelectedBooking.Status);
                        command.Parameters.AddWithValue("@CustomerId", customerId);
                        command.Parameters.AddWithValue("@RoomNumber", SelectedBooking.RoomNumber);
                        command.ExecuteNonQuery();

                        // Phòng mới được đánh dấu không khả dụng
                        UpdateRoomAvailability(SelectedBooking.RoomNumber, false, connection, transaction);
                    }
                    else
                    {
                        var updateQuery = @"UPDATE Bookings 
                                            SET CheckInDate = @CheckInDate, 
                                                CheckOutDate = @CheckOutDate, 
                                                Status = @Status, 
                                                CustomerId = @CustomerId, 
                                                RoomNumber = @RoomNumber
                                            WHERE Id = @Id";
                        using var command = new SqlCommand(updateQuery, connection, transaction);
                        command.Parameters.AddWithValue("@Id", SelectedBooking.Id);
                        command.Parameters.AddWithValue("@CheckInDate", SelectedBooking.CheckInDate);
                        command.Parameters.AddWithValue("@CheckOutDate", SelectedBooking.CheckOutDate);
                        command.Parameters.AddWithValue("@Status", (int)SelectedBooking.Status);
                        command.Parameters.AddWithValue("@CustomerId", customerId);
                        command.Parameters.AddWithValue("@RoomNumber", SelectedBooking.RoomNumber);
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            MessageBox.Show($"Không tìm thấy đặt phòng với Id={SelectedBooking.Id}.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            transaction.Rollback();
                            return;
                        }

                        // Cập nhật trạng thái phòng
                        if (!string.IsNullOrEmpty(oldRoomNumber) && oldRoomNumber != SelectedBooking.RoomNumber)
                        {
                            UpdateRoomAvailability(oldRoomNumber, true, connection, transaction);
                            UpdateRoomAvailability(SelectedBooking.RoomNumber, false, connection, transaction);
                        }
                        else
                        {
                            bool isRoomAvailable = SelectedBooking.Status == BookingStatus.Cancelled || SelectedBooking.Status == BookingStatus.CheckedOut;
                            UpdateRoomAvailability(SelectedBooking.RoomNumber, isRoomAvailable, connection, transaction);
                        }
                    }

                    transaction.Commit();
                    if (showSuccess)
                        MessageBox.Show("Lưu đặt phòng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"Lỗi khi lưu đặt phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                LoadBookings();
                LoadAvailableRooms();
                OnPropertyChanged(nameof(Bookings));
                SearchCriteria = new Booking
                {
                    Customer = new Customer(),
                    CheckInDate = DateTime.Now,
                    CheckOutDate = DateTime.Now.AddDays(1),
                    Status = BookingStatus.Pending,
                    RoomType = default(RoomType)
                };
                SelectedRoom = null;
                SelectedBooking = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu đặt phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsRoomExists(string roomNumber, SqlConnection connection, SqlTransaction transaction)
        {
            var query = @"SELECT COUNT(*) FROM Rooms WHERE RoomNumber = @RoomNumber";
            using var command = new SqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@RoomNumber", roomNumber);
            return (int)command.ExecuteScalar() > 0;
        }

        private int SaveOrUpdateCustomer(SqlConnection connection, SqlTransaction transaction)
        {
            if (SelectedBooking?.Customer == null)
            {
                MessageBox.Show("Thông tin khách hàng không hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }

            try
            {
                var checkQuery = @"SELECT Id FROM Customers WHERE CCCD = @CCCD";
                using var checkCommand = new SqlCommand(checkQuery, connection, transaction);
                checkCommand.Parameters.AddWithValue("@CCCD", SelectedBooking.Customer.CCCD ?? string.Empty);
                var existingId = checkCommand.ExecuteScalar();
                if (existingId != null)
                {
                    SelectedBooking.Customer.Id = Convert.ToInt32(existingId);
                    SelectedBooking.CustomerId = SelectedBooking.Customer.Id;
                }

                string query;
                if (SelectedBooking.Customer.Id == 0)
                {
                    query = @"INSERT INTO Customers (FullName, PhoneNumber, CCCD, Type)
                              VALUES (@FullName, @PhoneNumber, @CCCD, @Type);
                              SELECT SCOPE_IDENTITY();";
                    using var command = new SqlCommand(query, connection, transaction);
                    command.Parameters.AddWithValue("@FullName", SelectedBooking.Customer.FullName ?? string.Empty);
                    command.Parameters.AddWithValue("@PhoneNumber", SelectedBooking.Customer.PhoneNumber ?? string.Empty);
                    command.Parameters.AddWithValue("@CCCD", SelectedBooking.Customer.CCCD ?? string.Empty);
                    command.Parameters.AddWithValue("@Type", (int)SelectedBooking.Customer.Type);
                    var newId = Convert.ToInt32(command.ExecuteScalar());
                    SelectedBooking.Customer.Id = newId;
                    SelectedBooking.CustomerId = newId;
                    return newId;
                }
                else
                {
                    query = @"UPDATE Customers 
                              SET FullName = @FullName, 
                                  PhoneNumber = @PhoneNumber, 
                                  CCCD = @CCCD, 
                                  Type = @Type
                              WHERE Id = @Id";
                    using var command = new SqlCommand(query, connection, transaction);
                    command.Parameters.AddWithValue("@Id", SelectedBooking.Customer.Id);
                    command.Parameters.AddWithValue("@FullName", SelectedBooking.Customer.FullName ?? string.Empty);
                    command.Parameters.AddWithValue("@PhoneNumber", SelectedBooking.Customer.PhoneNumber ?? string.Empty);
                    command.Parameters.AddWithValue("@CCCD", SelectedBooking.Customer.CCCD ?? string.Empty);
                    command.Parameters.AddWithValue("@Type", (int)SelectedBooking.Customer.Type);
                    command.ExecuteNonQuery();
                    SelectedBooking.CustomerId = SelectedBooking.Customer.Id;
                    return SelectedBooking.Customer.Id;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu khách hàng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }
        }

        private bool CanSaveBooking()
        {
            return SelectedBooking != null &&
                   SelectedBooking.Customer != null &&
                   !string.IsNullOrWhiteSpace(SelectedBooking.Customer.FullName) &&
                   !string.IsNullOrWhiteSpace(SelectedBooking.Customer.CCCD) &&
                   !string.IsNullOrEmpty(SelectedBooking.RoomNumber) &&
                   SelectedBooking.CheckOutDate > SelectedBooking.CheckInDate &&
                   (SelectedBooking.Id > 0 || IsRoomAvailable(SelectedBooking.RoomNumber));
        }

        private bool IsRoomAvailable(string roomNumber)
        {
            if (string.IsNullOrEmpty(roomNumber))
                return false;

            try
            {
                using var connection = new SqlConnection(DatabaseConfig.GetConnectionString());
                connection.Open();
                var query = @"SELECT COUNT(*) FROM Rooms WHERE RoomNumber = @RoomNumber AND IsAvailable = 1";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomNumber", roomNumber);
                return (int)command.ExecuteScalar() > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void DeleteBooking(object parameter)
        {
            if (SelectedBooking == null)
            {
                MessageBox.Show("Vui lòng chọn đặt phòng để xóa.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using var connection = new SqlConnection(DatabaseConfig.GetConnectionString());
                connection.Open();
                using var transaction = connection.BeginTransaction();
                try
                {
                    var query = "DELETE FROM Bookings WHERE Id = @Id";
                    using var command = new SqlCommand(query, connection, transaction);
                    command.Parameters.AddWithValue("@Id", SelectedBooking.Id);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        MessageBox.Show("Không tìm thấy đặt phòng để xóa.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        transaction.Rollback();
                        return;
                    }

                    if (!string.IsNullOrEmpty(SelectedBooking.RoomNumber))
                    {
                        UpdateRoomAvailability(SelectedBooking.RoomNumber, true, connection, transaction);
                    }

                    transaction.Commit();
                    Bookings.Remove(SelectedBooking);
                    LoadAvailableRooms();
                    OnPropertyChanged(nameof(Bookings));
                    MessageBox.Show("Xóa đặt phòng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    SelectedBooking = null;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"Lỗi khi xóa đặt phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa đặt phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanDeleteBooking(object parameter)
        {
            return SelectedBooking != null && SelectedBooking.Id > 0;
        }

        private void CancelEditing(object parameter)
        {
            SelectedBooking = null;
            SearchCriteria = new Booking
            {
                Customer = new Customer(),
                CheckInDate = DateTime.Now,
                CheckOutDate = DateTime.Now.AddDays(1),
                Status = BookingStatus.Pending,
                RoomType = default(RoomType),
                RoomNumber = null
            };
            SelectedRoom = null;
            RoomTypeFilter = null;
            LoadAvailableRooms();
            LoadBookings();
        }

        private void UpdateRoomAvailability(string roomNumber, bool isAvailable, SqlConnection connection = null, SqlTransaction transaction = null)
        {
            if (string.IsNullOrEmpty(roomNumber))
                return;

            SqlConnection localConnection = connection;
            bool newConnection = localConnection == null || localConnection.State != ConnectionState.Open;

            try
            {
                if (newConnection)
                {
                    localConnection = new SqlConnection(DatabaseConfig.GetConnectionString());
                    localConnection.Open();
                }

                var query = "UPDATE Rooms SET IsAvailable = @IsAvailable WHERE RoomNumber = @RoomNumber";
                using var command = new SqlCommand(query, localConnection);
                if (transaction != null && !newConnection)
                    command.Transaction = transaction;

                command.Parameters.AddWithValue("@IsAvailable", isAvailable ? 1 : 0);
                command.Parameters.AddWithValue("@RoomNumber", roomNumber);
                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    MessageBox.Show($"Không tìm thấy phòng {roomNumber} để cập nhật trạng thái.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (newConnection && localConnection != null && localConnection.State == ConnectionState.Open)
                    localConnection.Close();
            }
        }

        
    }


}
