using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Frontend.ViewModel
{
    public class ColorCode : IValueConverter
    {
        internal enum ColorCodes
        {
            ForestGreen = 0,
            Black,
            DarkViolet,
            Chocolate,
            YellowGreen,
            DeepPink,
            Maroon,
            CadetBlue,
            Goldenrod,
            PaleVioletRed
        }

        public static string colorPicker(string username)
        {
            int sum = 0;
            if (string.IsNullOrEmpty(username)) return returnColor(0);
            foreach (char c in username)
                sum += (int)c;
            return returnColor(sum);
        }

        public static string returnColor(int asciiSum)
        {
            int length = Enum.GetNames(typeof(ColorCodes)).Length;
            ColorCodes colorCode = (ColorCodes)(asciiSum % length);
            return colorCode.ToString();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string name)
            {
                try
                {
                    string colorName = colorPicker(name.Split('@')[0]);

                    return (Brush)new BrushConverter().ConvertFromString(colorName);
                }
                catch
                {
                    return Brushes.LightGray;
                }
            }
            return Brushes.LightGray;
        }
        // We dont need to convert a Brush back into a name.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}