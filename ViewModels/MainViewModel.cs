using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HotelManager.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _welcomeMessage = "Welcome to Hotel Manager!";
        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set
            {
                _welcomeMessage = value;
                OnPropertyChanged();
            }
        }
        public ICommand ShowMessageCommand { get; }

        public MainViewModel()
        {
            ShowMessageCommand = new RelayCommand(_ => WelcomeMessage = "Let's build a great hotel app!");
        }
    }
}