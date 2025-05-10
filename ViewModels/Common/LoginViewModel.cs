using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HotelManager.Utilities;

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
            ChangePasswordVisibilityCommand = new RelayCommand(_ => ChangePasswordVisibility());
        }



        // fuctions

        // login
        private void Login()
        {
            if (isEmpty())
            {
                Console.WriteLine("Empty name/pass");
            }

        }

        // check if username or password text box empty
        private bool isEmpty()
        {
            return string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password);
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

        // chage password visibility
        private void ChangePasswordVisibility()
        {
            PasswordVisibility = !PasswordVisibility;
        }

    }
}
