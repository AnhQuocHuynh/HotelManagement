using CommunityToolkit.Mvvm.Input;
using HotelManager.Models;
using HotelManager.Models.Enums;
using HotelManager.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HotelManager.ViewModels
{
    public class BookingViewModel : INotifyPropertyChanged
    {
        private readonly BookingService _bookingService;
        private readonly RoomService _roomService;
        private readonly CustomerService _customerService;
        private readonly ILogger<BookingViewModel> _logger;

        public ObservableCollection<Booking> Bookings { get; set; } = new();
        public Booking? SelectedBooking { get; set; }
        public string? CustomerName { get; set; }
        public RoomType? SelectedRoomType { get; set; }
        public string? SelectedRoomNumber { get; set; }
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public BookingStatus? SelectedStatus { get; set; }
        public string? SearchText { get; set; }
        public ICommand AddBookingCommand { get; }
        public ICommand EditBookingCommand { get; }
        public ICommand DeleteBookingCommand { get; }
        public ICommand CheckInCommand { get; }
        public ICommand CheckOutCommand { get; }
        public ICommand FilterCommand { get; }
        public event PropertyChangedEventHandler? PropertyChanged;
        // ... (implement logic as previously discussed)
    }
} 