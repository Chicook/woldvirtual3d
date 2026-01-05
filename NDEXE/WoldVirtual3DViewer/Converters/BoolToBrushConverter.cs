using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WoldVirtual3DViewer.Converters
{
    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;

            if (boolValue)
            {
                return new SolidColorBrush(Color.FromRgb(99, 179, 237)); // Azul para seleccionado
            }
            else
            {
                return new SolidColorBrush(Color.FromRgb(45, 55, 72)); // Gris oscuro para no seleccionado
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
