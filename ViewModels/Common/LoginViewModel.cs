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
using HotelManager.Services.Common.Interfaces;
using HotelManager.Models.Enums;
using System.Windows;

namespace HotelManager.ViewModels.Common
{
    class LoginViewModel : BaseViewModel
    {
        // khai bao bien
        private string _userName;
        private string _password;
        private string _passwordVisibility = "Hidden";
        private bool _ischecked = false;
        private UserRole _role;
        private bool _loginButtonEnabled = true; // Button state

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

        public string PasswordVisibility
        {
            get { return _passwordVisibility; }
            set { _passwordVisibility = value; OnPropertyChanged(); }
        }

        public bool IsChecked
        {
            get { return _ischecked; }
            set
            {
                _ischecked = value;
                if (_ischecked)
                {
                    PasswordVisibility = "Visible";
                }
                else
                {
                    PasswordVisibility = "Hidden";
                }
                OnPropertyChanged();
            }
        }

        public UserRole Role
        {
            get { return _role; }
            set { _role = value; OnPropertyChanged(); }
        }

        public bool LoginButtonEnabled
        {
            get { return _loginButtonEnabled; }
            set
            {
                _loginButtonEnabled = value;
                OnPropertyChanged();
            }
        }

        // ICommnd
        public ICommand LoginCommand { get; set; }
        public ICommand IsManagerCommand { get; set; }
        public ICommand IsReceptionistCommand { get; set; }
        public ICommand IsAttendentCommand { get; set; }



        // **** Constructor
        public LoginViewModel()
        {
            // coommand
            LoginCommand = new RelayCommand(_ => Login());
            IsManagerCommand = new RelayCommand(_ => IsManager());
            IsReceptionistCommand = new RelayCommand(_ => IsReceptionist());
            IsAttendentCommand = new RelayCommand(_ => IsAttendent());
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
            ILoginProcess loginProcess = new LoginProcess();
            UserAccount account = await loginProcess.Login(UserName, Password, Role);
            if (account == null)
                return false;
            else
                return true;
        }

        // login
        private async Task Login()
        {
            // chờ có kết quả login thì mới cho phép button hoạt động
            // nếu ko nhấn button liên tục thì yêu cầu trả về liên tục --> trả về nhiều kết quả
            LoginButtonEnabled = false; // Disable the button

            try
            {
                if (isEmpty())
                {
                    MessageBox.Show("empty usernamee or password");
                    return;
                }

                bool correction = await isCorrect();
                if (correction)
                {
                    MessageBox.Show("login successful");
                }
                else
                {
                    MessageBox.Show("login unsuccessful");
                }
            }
            finally
            {
                LoginButtonEnabled = true; // Re-enable the button
            }


        }

        // change possition
        private void IsManager()
        {
            Role = UserRole.Manager;
            MessageBox.Show("You are " + Role, ToString());
        }
        private void IsReceptionist()
        {
            Role = UserRole.Staff;
            MessageBox.Show("You are " + Role.ToString());
        }
        private void IsAttendent()
        {
            Role = UserRole.Staff;
            MessageBox.Show("You are " + Role.ToString());
        }

    }
}
