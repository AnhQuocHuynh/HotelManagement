using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using HotelManager.Data;
using HotelManager.Services;
using HotelManager.ViewModels.StaffViewModels;

namespace HotelManager.Views.StaffViews
{
    /// <summary>
    /// Interaction logic for CleanerView.xaml
    /// </summary>
    public partial class CleanerView : UserControl
    {
        public CleanerView()
        {
            InitializeComponent();
            var context = new HotelDbContext(); // hoặc DI context nếu bạn dùng
            var service = new CleanRoomService(context);
            DataContext = new CleanerViewModel(service);
        }
    }
}
