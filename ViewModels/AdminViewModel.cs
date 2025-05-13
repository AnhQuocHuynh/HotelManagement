using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HotelManager.Data;
using HotelManager.Models;

namespace HotelManager.ViewModels
{
    //Tuấn
    //Todo: 1. Hiển thị danh sách tài khoản, 2. Tạo tài khoản mới, 3. Sửa tài khoản, 4. Xóa tài khoản, 5. Ràng buộc phân quyền
    internal class AdminViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public ICommand AddCommand { get; set; }
        public ICommand UpdateCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public AdminViewModel()
        {
            AddCommand = new RelayCommand(Add);
            UpdateCommand = new RelayCommand(Update);
            DeleteCommand = new RelayCommand(Delete);
        }
        //Values
        private readonly HotelDbContext _dbContext = new HotelDbContext();
        public ObservableCollection<UserAccount> Accounts { get; set; } = new();
        private UserAccount _selectedUser;
        public UserAccount SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged(nameof(SelectedUser));
            }
        }

        private void Add(object obj)
        {
            // Implement the logic to add a new room
            // For example, show a dialog to enter room details and save it to the database
        }
        private void Update(object obj)
        {
            // Implement the logic to update an existing room
            // For example, show a dialog to edit room details and save the changes to the database
        }
        private void Delete(object obj)
        {
            // Implement the logic to delete a room
            // For example, show a confirmation dialog and delete the room from the database
        }

        //1. Hiển thị danh sách tài khoản

    }
}
