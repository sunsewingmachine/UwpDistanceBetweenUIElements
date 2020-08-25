using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace SharedLibrary.Converters
{
    public class ZeroToInVisibilityConverter : IValueConverter
    {
        private int val;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            //int intParam = (int)parameter;
            val = (int)value;

            return val == 0 ? Visibility.Collapsed : Visibility.Visible;

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }


    }
}
