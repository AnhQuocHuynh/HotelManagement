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
    public class RoomInfoEditViewModel : BaseViewModel
    {
        private readonly RoomService _roomService;
        private Room _editableRoom;
        private readonly Room _originalRoom;
        private bool? _dialogResult;

        public Room EditableRoom 
        { 
            get => _editableRoom;
            set
            {
                _editableRoom = value;
                OnPropertyChanged(nameof(EditableRoom));
            }
        }

        public bool? DialogResult 
        { 
            get => _dialogResult;
            set
            {
                _dialogResult = value;
                OnPropertyChanged(nameof(DialogResult));
            }
        }
      
        public IEnumerable<KeyValuePair<RoomStatus, string>> LocalizedRoomStatuses { get; }

        public IEnumerable<RoomStatus> RoomStatuses { get; } = EnumHelper.RoomStatuses;

        public IEnumerable<RoomType> RoomTypes { get; } = EnumHelper.RoomTypes;
        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public Action<bool?>? CloseAction { get; set; } // Action to close the dialog with result

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
            // Close the dialog with false result
            CloseAction?.Invoke(false);
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
                _originalRoom.RoomType = EditableRoom.RoomType;
                _originalRoom.RoomStatus = EditableRoom.RoomStatus;
                _originalRoom.PricePerNight = EditableRoom.PricePerNight;
                //Update csdl
                await _roomService.UpdateAsync(_originalRoom);
                MessageBox.Show("Thông tin phòng đã được cập nhật thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Close the dialog with true result
                CloseAction?.Invoke(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật thông tin phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
