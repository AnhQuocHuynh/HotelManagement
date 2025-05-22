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
using System.Drawing;
using Microsoft.VisualBasic;
using System.Windows.Media;
using System.Diagnostics;

namespace HotelManager.ViewModels.Common
{
    class LoginViewModel : BaseViewModel
    {
        // khai bao bien
        private string _userName;
        private string _password;
        private string _passwordVisibility = "Hidden";
        private bool _ischecked = false;
        private EmployeePosition _position;
        private bool _loginButtonEnabled = true; // Button state

        //position button background để biết hiện tại đang  chọn chức vụ nào
        private Brush _managerBtnBackground;
        private Brush _receptionistBtnBackground;
        private Brush _attendantBtnBackground;

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

        public EmployeePosition Position
        {
            get { return _position; }
            set { _position = value; OnPropertyChanged(); }
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

        //position button background;
        public Brush ManagerBtnBackground
        {
            get { return _managerBtnBackground; }
            set
            {
                _managerBtnBackground = value;
                OnPropertyChanged();
            }
        }
        public Brush ReceptionistBtnBackground
        {
            get { return _receptionistBtnBackground; }
            set
            {
                _receptionistBtnBackground = value;
                OnPropertyChanged();
            }
        }
        public Brush AttendantBtnBackground
        {
            get { return _attendantBtnBackground; }
            set
            {
                _attendantBtnBackground = value;
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
            // test xem constructor đc gọi bao nhiêu lần
            Debug.WriteLine("LoginViewModel created");

            // coommand
            LoginCommand = new RelayCommand(_ => Login());
            IsManagerCommand = new RelayCommand(_ => IsManager());
            IsReceptionistCommand = new RelayCommand(_ => IsReceptionist());
            IsAttendentCommand = new RelayCommand(_ => IsAttendent());

            // gán chức vụ là manager khi vừa chạy
            IsManager();
        }



        // fuctions

        // check if username or password text box empty
        private bool isEmpty()
        {
            return string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password);
        }

        // check if username & password is correct
        //private async Task<bool> isCorrect()
        //{
        //    ILoginProcess loginProcess = new LoginProcess();
        //    UserAccount account = await loginProcess.Login(UserName, Password, Role);
        //    if (account == null)
        //        return false;
        //    return true;
        //}

        // goi event cho mainview biet doi current view
        public event Action<EmployeePosition>? LoginSucceeded;

        // login
        private async Task Login()
        {
            // chờ có kết quả login thì mới cho phép button hoạt động
            // nếu ko, khi nhấn button liên tục thì yêu cầu trả về liên tục --> trả về nhiều kết quả
            LoginButtonEnabled = false; // Disable the button

            try
            {
                if (isEmpty())
                {
                    MessageBox.Show("empty usernamee or password");
                    return;
                }

                ILoginProcess loginProcess = new LoginProcess();
                UserAccount account = await loginProcess.Login(UserName, Password, Position);

                if (account != null)
                {
                    MessageBox.Show("login successful");
                    AppSession.SetCurrentUserAccount(account);
                    LoginSucceeded?.Invoke(account.Employee.Position);
                }
                else
                {
                    MessageBox.Show("login unsuccessful");
                }

                //bool isLoginSuccessful = await isCorrect();
                //if (isLoginSuccessful)
                //{
                //    MessageBox.Show("login successful");
                //    UserAccount account = await new UserAccountRepository().FindAccountAsync(UserName, Password, Role);
                //    AppSession.SetCurrentUserAccount(account); // Store the logged-in user in the session
                //    MainViewModel _mainVM = new MainViewModel();
                //    _mainVM.BaseViewLocator(account.Role);
                //}
                //else
                //{
                //    MessageBox.Show("login unsuccessful");
                //}
            }
            finally
            {
                LoginButtonEnabled = true; // Re-enable the button
            }
        }

        // change possition
        private void IsManager()
        {
            Position = EmployeePosition.Manager;
            MessageBox.Show("You are " + Position);
            ManagerBtnBackground = (Brush)Application.Current.Resources["AccentBrush"];
            ReceptionistBtnBackground = AttendantBtnBackground = (Brush)Application.Current.Resources["PrimaryBrush"];

        }
        private void IsReceptionist()
        {
            Position = EmployeePosition.Receptionist;
            MessageBox.Show("You are " + Position);
            ReceptionistBtnBackground = (Brush)Application.Current.Resources["AccentBrush"];
            ManagerBtnBackground = AttendantBtnBackground = (Brush)Application.Current.Resources["PrimaryBrush"];
        }
        private void IsAttendent()
        {
            Position = EmployeePosition.Cleaner;
            MessageBox.Show("You are " + Position);
            AttendantBtnBackground = (Brush)Application.Current.Resources["AccentBrush"];
            ManagerBtnBackground = ReceptionistBtnBackground = (Brush)Application.Current.Resources["PrimaryBrush"];
        }

    }
}
