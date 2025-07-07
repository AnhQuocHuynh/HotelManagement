using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HotelManager.Models;

namespace HotelManager.Views.StaffViews
{
    /// <summary>
    /// Interaction logic for ReceptionistRoomView.xaml
    /// </summary>
    public partial class ReceptionistRoomView : UserControl
    {
        public ReceptionistRoomView()
        {
            InitializeComponent();
        }

        private void RoomCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is Room room)
            {
                // Hiển thị thông tin chi tiết phòng
                var message = $"Room Details:\n\n" +
                             $"Room Number: {room.RoomNumber}\n" +
                             $"Room Type: {room.RoomType}\n" +
                             $"Status: {room.RoomStatus}\n" +
                             $"Price per Night: {room.PricePerNight:N0} ₫\n\n" +
                             $"This room is currently {(room.RoomStatus == Models.Enums.RoomStatus.Available ? "available for booking" : 
                                 room.RoomStatus == Models.Enums.RoomStatus.Pending ? "pending cleaning" : 
                                 "under maintenance")}.";

                MessageBox.Show(message, $"Room {room.RoomNumber} Details", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
} 