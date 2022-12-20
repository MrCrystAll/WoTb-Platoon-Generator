using System;
using System.Globalization;
using System.Windows.Data;

namespace GénérateurWot.Converters
{
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) throw new Exception("Waiting for boolean, got null");
            return (bool)value ? "ON": "OFF";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string)value)!.ToUpper() switch
            {
                "ON" => true,
                "OFF" => false,
                _ => throw new Exception($"Expected \"ON\" or \"OFF\", got {value}")
            };
        }
    }
}