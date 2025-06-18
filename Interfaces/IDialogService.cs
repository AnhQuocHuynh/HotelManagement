using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManager.Interfaces
{
    public interface IDialogService
    {
        bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : class;
    }
}
