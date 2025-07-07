﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FEQuanLyNhanSu.Helpers
{
    public class RoleToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string role)
            {
                // Example logic: Adjust based on your application's roles  
                return role == "Admin" ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
