using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace FEQuanLyNhanSu.Helpers
{
    public class ImagePathToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string path = value as string;
                if (string.IsNullOrWhiteSpace(path))
                    path = "pack://application:,,,/Assets/none.jpg";

                return new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
            }
            catch
            {
                return new BitmapImage(new Uri("pack://application:,,,/Assets/none.jpg"));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
