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

namespace HotelManager.ViewModels.Common
{
    class LoginViewModel : BaseViewModel
    {
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
        public ICommand IsManagerCommand { get; set; }
        public ICommand IsReceptionistCommand { get; set; }
        public ICommand IsAttendentCommand { get; set; }
        public ICommand ChangePasswordVisibilityCommand { get; set; }



// **** Constructor
        public LoginViewModel()
        {
            // coommand
            LoginCommand = new RelayCommand(_ => Login());
            IsManagerCommand = new RelayCommand(_ => IsManager());
            IsReceptionistCommand = new RelayCommand(_ => IsReceptionist());
            IsAttendentCommand = new RelayCommand(_ => IsAttendent());

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
            var loginProcess = new LoginProcess(new UserAuthenticationService(new UserRepository(new Data.HotelDbContext())));
            UserAccount account = await loginProcess.Login(UserName, Password);
            return account != null;
        }

        // login
        private async Task Login()
        {
            if (isEmpty())
            {
                Console.WriteLine("Empty username or password");
                return;
            }

            bool correction = await isCorrect();
            if (correction)
            {
                Console.WriteLine("Login successful");
            }
            else
            {
                Console.WriteLine("Invalid username or password");
            }

        }

        // change possition
        private void IsManager()
        {
            Position = "Manager";
        }
        private void IsReceptionist()
        {
            Position = "Receptionist";
        }
        private void IsAttendent()
        {
            Position = "Attendent";
        }

    }
}
