using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FEQuanLyNhanSu.Helpers
{
    public class RoleToVisibilityConverter : IValueConverter
    {
        //public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        //{
        //    var role = value as string;
        //    if (role == "Administrator" || role == "Manager")
        //        return Visibility.Visible;
        //    return Visibility.Collapsed;
        //}

        //public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        //{
        //    throw new NotImplementedException();
        //}
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var currentRole = AppState.get_CurrentRole();

            if (parameter != null)
            {
                var rolesToHide = parameter.ToString().Split(',').Select(r => r.Trim());
                if (rolesToHide.Contains(currentRole))
                    return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
