using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace FEQuanLyNhanSu.Helpers
{
    public class RoleToDoneButtonVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var role = value as string;

            if (role == "SystemAdmin")
                return Visibility.Collapsed; // Ẩn nút hoàn thành

            return Visibility.Visible; // Các role khác vẫn hiện
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
