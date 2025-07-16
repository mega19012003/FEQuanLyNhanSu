using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FEQuanLyNhanSu.Helpers
{
    public class RoleToHeaderTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var role = value as string;
            if (role == "SystemAdmin")
                return "";  // header trống để không hiển thị chữ H

            // Lấy header mặc định từ ConverterParameter
            return parameter?.ToString() ?? "Hành động";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
