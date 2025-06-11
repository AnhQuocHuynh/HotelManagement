using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HotelManager.Models;
using HotelManager.Services.Common.Implements;
using HotelManager.Utilities;
using HotelManager.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.ViewModels.Common
{
    class LoginViewModel : BaseViewModel
    {
        // Event to notify main view model about successful login
        public static event Action OnLoginSuccess;
        // khai bao bien
        private string _userName;
        private string _password;
        private bool _passwordVisibility;
        private string _position;

        public string UserName
        {
            get { return _userName; }
            set { _userName = value; OnPropertyChanged(); }
        }
        public string Password
        {
            get { return _password; }
            set { _password = value; OnPropertyChanged(); }
        }

        public bool PasswordVisibility
        {
            get { return _passwordVisibility; }
            set { _passwordVisibility = value; OnPropertyChanged(); }
        }
        public string Position
        {
            get { return _position; }
            set { _position = value; OnPropertyChanged(); }
        }

        // ICommnd
        public ICommand LoginCommand { get; set; }
        public ICommand ChangePasswordVisibilityCommand { get; set; }



// **** Constructor
        public LoginViewModel()
        {
            // coommand
            LoginCommand = new RelayCommand(_ => Login());

            PasswordVisibility = false;
        }



        // fuctions

        // check if username or password text box empty
        private bool isEmpty()
        {
            return string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password);
        }

        // check if username & password is correct
        private async Task<bool> isCorrect()
        {
            try
            {
                // Direct authentication without complex layers
                using (var context = new Data.HotelDbContext())
                {
                    var hashedPassword = Helpers.HashHelper.HashPassword(Password);
                    var user = context.UserAccounts
                        .Include(u => u.Employee) // Include Employee data for role-based navigation
                        .FirstOrDefault(u => u.Username == UserName && u.PasswordHash == hashedPassword);
                    
                    if (user != null)
                    {
                        // Set current user session
                        Utilities.AppSession.SetCurrentUserAccount(user);
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Authentication error: {ex.Message}");
                return false;
            }
        }

        // login
        private async void Login()
        {
            if (isEmpty())
            {
                System.Windows.MessageBox.Show("Username or password is empty!");
                return;
            }

            try
            {
            bool correction = await isCorrect();
            if (correction)
            {
                    System.Windows.MessageBox.Show("Login successful!");
                    // Trigger navigation to appropriate view
                    OnLoginSuccess?.Invoke();
            }
            else
            {
                    System.Windows.MessageBox.Show("Invalid username or password!");
            }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Login error: {ex.Message}!");
            }
        }
    }
}
