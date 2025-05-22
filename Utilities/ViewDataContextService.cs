using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HotelManager.Utilities
{
    public static class ViewDataContextService
    {
        public static TView CreateViewWithViewModel<TViewModel, TView>()
            where TViewModel : class, new()
            where TView : FrameworkElement, new()
        {
            TView view = new TView();
            TViewModel vm = new TViewModel();
            view.DataContext = vm;
            return view;
        }
    }

}
