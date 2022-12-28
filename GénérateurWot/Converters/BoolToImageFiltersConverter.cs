using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace GénérateurWot.Converters
{
    public class BoolToImageFiltersConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) throw new Exception("Waiting for boolean, got null");
            return new BitmapImage(new Uri((bool)value ? "icons/down_arrow.png" : "icons/right_arrow.png"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}