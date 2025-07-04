using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HotelManager.Data;
using HotelManager.Helpers;
using HotelManager.Models;
using HotelManager.Models.Enums;
using HotelManager.Services;
using HotelManager.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace HotelManager.ViewModels
{
    internal class RoomInfoEditViewModel : BaseViewModel
    {
        private readonly RoomService _roomService;
        public Room EditableRoom { get; set; }
        private readonly Room _originalRoom;

        public IEnumerable<KeyValuePair<RoomStatus, string>> LocalizedRoomStatuses { get; }
      
        public IEnumerable<RoomStatus> RoomStatuses { get; } = EnumHelper.RoomStatuses;

        public IEnumerable<RoomType> RoomTypes { get; } = EnumHelper.RoomTypes;
        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public bool? DialogResult { get;  set; }
        public Action? CloseAction { get; set; } // Action to close the dialog if needed

        public RoomInfoEditViewModel(Room room, RoomService roomService)
        {
            _originalRoom = room;
            EditableRoom = new Room
            {
                RoomNumber = room.RoomNumber,
                RoomType = room.RoomType,
                RoomStatus = room.RoomStatus,
                PricePerNight = room.PricePerNight
            };

            _roomService = roomService;
            SaveCommand = new AsyncRelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Cancel()
        {
            DialogResult = false;
            CloseAction?.Invoke();
        }

        private async Task Save()
        {
            if (EditableRoom.PricePerNight < 0)
            {
                MessageBox.Show("Giá mỗi đêm không hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                //Áp dụng thay đổi vào original room
                _originalRoom.RoomNumber = EditableRoom.RoomNumber;
                _originalRoom.RoomType = EditableRoom.RoomType;
                _originalRoom.RoomStatus = EditableRoom.RoomStatus;
                _originalRoom.PricePerNight = EditableRoom.PricePerNight;
                //Update csdl
                await _roomService.UpdateAsync(EditableRoom);
                MessageBox.Show("Thông tin phòng đã được cập nhật.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
