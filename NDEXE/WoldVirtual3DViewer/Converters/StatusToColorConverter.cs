using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WoldVirtual3DViewer.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? status = value as string;

            return status?.ToLower() switch
            {
                "available" => new SolidColorBrush(Color.FromRgb(72, 187, 120)), // Verde
                "development" => new SolidColorBrush(Color.FromRgb(246, 173, 85)), // Amarillo
                "coming_soon" => new SolidColorBrush(Color.FromRgb(160, 174, 192)), // Gris
                _ => new SolidColorBrush(Color.FromRgb(245, 101, 101)) // Rojo por defecto
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
