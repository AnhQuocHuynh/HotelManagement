using HotelManager.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;
using CommunityToolkit.Mvvm.Input;
using HotelManager.Data;
using HotelManager.Interfaces;
using HotelManager.Models;
using HotelManager.Services;
using RelayCommand = HotelManager.Utilities.RelayCommand;
using HotelManager.Helpers;
using HotelManager.Models.Enums;

namespace HotelManager.ViewModels
{
    //Tuấn
    //Todo: 1. Hiển thị danh sách tài khoản, 2. Tạo tài khoản mới, 3. Sửa tài khoản, 4. Xóa tài khoản, 5. Ràng buộc phân quyền
    internal class AdminViewModel : BaseViewModel
    {
        private readonly UserAccountService _userService;
        public ICommand AddCommand { get; set; }
        public ICommand UpdateCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand AddNewAccountCommand { get; set; }

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
        public AdminViewModel()
        {
            _userService = new UserAccountService(new HotelDbContext());
            AddCommand = new RelayCommand(async param => await AddAsync(SelectedUser));
            UpdateCommand = new RelayCommand(async param => await UpdateAsync(SelectedUser));
            DeleteCommand = new RelayCommand(async param => await DeleteAsync(SelectedUser));
            AddNewAccountCommand = new RelayCommand(param => AddNewAccount());

            LoadAccounts();
        }
        public AdminViewModel(IService<UserAccount> userService)
        {
            _userService = (UserAccountService)userService;

            AddCommand = new RelayCommand(async param => await AddAsync(SelectedUser));
            UpdateCommand = new RelayCommand(async param => await UpdateAsync(SelectedUser));
            DeleteCommand = new RelayCommand(async param => await DeleteAsync(SelectedUser));
            AddNewAccountCommand = new RelayCommand(param => AddNewAccount());

            LoadAccounts();
        }
        public async void LoadAccounts()
        {

            // Fix for CS1593: Ensure the RelayCommand constructor matches the expected delegate signature.  
            
            var accounts = await _userService.GetAllAsync();
            Accounts.Clear();
            foreach (var account in accounts)
            {
                Accounts.Add(account);
            }
        }
        private async Task AddAsync(UserAccount account)
        {
            var added = await _userService.CreateAsync(account);
            Accounts.Add(added);
        }

        private async Task UpdateAsync(UserAccount account)
        {
            await _userService.UpdateAsync(account);
            // Refresh list or raise OnPropertyChanged
            OnPropertyChanged(nameof(Accounts));
        }

        private async Task DeleteAsync(UserAccount account)
        {
            if (await _userService.DeleteAsync(account.Id))
            {
                // DO NOT USE THIS RIGHT NOW
                //Accounts.Remove(account);
            }
        }

        private void AddNewAccount()
        {

        }
        
    }

}
