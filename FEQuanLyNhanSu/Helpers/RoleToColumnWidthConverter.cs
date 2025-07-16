using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace FEQuanLyNhanSu.Helpers
{
    public class RoleToColumnWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var role = value as string;

            // Nếu là SystemAdmin thì Width=0 để ẩn hoàn toàn
            if (role == "SystemAdmin")
                return new DataGridLength(0);

            // Ngược lại lấy Width từ ConverterParameter (vd: 200)
            if (parameter != null && double.TryParse(parameter.ToString(), out double defaultWidth))
                return new DataGridLength(defaultWidth);

            return new DataGridLength(200); // fallback width
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
