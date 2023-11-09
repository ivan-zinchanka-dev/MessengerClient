using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MessengerClient.Converters;

public class NicknameToHorizontalAlignment : IValueConverter
{
    // TODO переделать через триггеры
    
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string nickname)
        {
            return nickname == App.Instance.CurrentUser.Nickname ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        }
        else
        {
            throw new InvalidOperationException("Value must be a System.String");
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is HorizontalAlignment textAlignment && textAlignment == HorizontalAlignment.Right)
        {
            return App.Instance.CurrentUser.Nickname;
        }
        else
        {
            throw new Exception();
        }
    }
}