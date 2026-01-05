using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WoldVirtual3DViewer.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;
            string? param = parameter as string;

            if (boolValue)
            {
                // Verde para éxito
                return new SolidColorBrush(Color.FromRgb(72, 187, 120)); // #FF48BB78
            }
            else
            {
                // Rojo para error
                return new SolidColorBrush(Color.FromRgb(245, 101, 101)); // #FFF56565
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
