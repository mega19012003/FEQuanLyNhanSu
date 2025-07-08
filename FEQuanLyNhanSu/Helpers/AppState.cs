using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEQuanLyNhanSu.Helpers
{
    public class AppState
    {
        private static string _currentRole = "Employee";
        public static string CurrentRole
        {
            get => _currentRole;
            set
            {
                if (_currentRole != value)
                {
                    _currentRole = value;
                    StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(CurrentRole)));
                }
            }
        }

        public static event PropertyChangedEventHandler StaticPropertyChanged;
    }

}
