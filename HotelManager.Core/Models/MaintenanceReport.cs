using HotelManager.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HotelManager.Models
{
    public class MaintenanceReport : INotifyPropertyChanged
    {
        public int Id { get; set; }
        
        private string _roomNumber = string.Empty;
        public string RoomNumber 
        { 
            get => _roomNumber;
            set { _roomNumber = value; OnPropertyChanged(); }
        }
        
        public Room Room { get; set; }
        
        private string _description = string.Empty;
        public string Description 
        { 
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }
        
        public DateTime ReportedDate { get; set; }
        
        private string _imagePath = string.Empty;
        public string ImagePath 
        { 
            get => _imagePath;
            set { _imagePath = value; OnPropertyChanged(); }
        }

        private bool _isResolved = false;
        public bool IsResolved 
        { 
            get => _isResolved;
            set { _isResolved = value; OnPropertyChanged(); }
        }
        
        private DateTime? _completedDate = null;
        public DateTime? CompletedDate 
        { 
            get => _completedDate;
            set { _completedDate = value; OnPropertyChanged(); }
        }
        
        private string _completionImagePath = string.Empty;
        public string CompletionImagePath 
        { 
            get => _completionImagePath;
            set { _completionImagePath = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICollection<Maintenance> Maintenances { get; set; }

    }
}
