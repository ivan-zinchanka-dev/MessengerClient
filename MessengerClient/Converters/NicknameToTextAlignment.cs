using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MessengerClient.Converters;

public class NicknameToTextAlignment : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string nickname)
        {
            return nickname == App.Instance.CurrentUser.Nickname ? TextAlignment.Right : TextAlignment.Left;
        }
        else
        {
            throw new InvalidOperationException("Value must be a System.String");
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TextAlignment textAlignment && textAlignment == TextAlignment.Right)
        {
            return App.Instance.CurrentUser.Nickname;
        }
        else
        {
            throw new Exception();
        }
    }
}