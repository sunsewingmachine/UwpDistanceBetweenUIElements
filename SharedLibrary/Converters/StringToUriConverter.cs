using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedLibrary.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace SharedLibrary.Converters
{
	public class StringToUriConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value != null)
            {
                //if (string.IsNullOrWhiteSpace(value.ToString())) return string.Empty;
				return new BitmapImage(new Uri(value.ToString()));
			}
			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}

	public class ObjectToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value != null && value is Picture)
				return Visibility.Visible;
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return null;
		}
	}

	//public class BoolToVisibilityConverter : IValueConverter
	//{
	//	public object Convert(object value, Type targetType, object parameter, string language)
	//	{
	//		if ((value is bool) && (bool)value)
	//			return Visibility.Visible;
	//		return Visibility.Collapsed;
	//	}

	//	public object ConvertBack(object value, Type targetType, object parameter, string language)
	//	{
	//		return null;
	//	}
	//}
}
