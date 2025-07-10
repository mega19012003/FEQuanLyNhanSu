using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FEQuanLyNhanSu.Helpers
{
    public static class AppState
    {
        public static string get_CurrentRole()
        {
            return Application.Current.Properties["UserRole"]?.ToString();
        }
    }

}
