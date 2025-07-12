using HotelManager.ViewModels.ManagerViewModels;
using HotelManager.ViewModels.ManagerViewModels.Reports;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace HotelManager.Views.ManagerViews
{
    /// <summary>
    /// Interaction logic for WorkScheduleManagementView.xaml
    /// TODO (Bảo): Thêm event handlers nếu cần, nhưng tốt nhất là keep logic trong ViewModel
    /// </summary>
    public partial class WorkScheduleManagementView : UserControl
    {
        public WorkScheduleManagementView()
        {
            InitializeComponent();

            // TODO (Bảo): Set DataContext nếu cần
            // Hoặc để ViewModelLocator handle việc này
            if (App.ServiceProvider != null)
            {
                DataContext = App.ServiceProvider.GetRequiredService<WorkScheduleManagementViewModel>();
            }
        }
    }
} 