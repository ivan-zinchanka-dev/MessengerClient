using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MessengerClient.Network;
using Microsoft.Extensions.DependencyInjection;

namespace MessengerClient.Converters;

public class TextAlignmentByNickname : IValueConverter
{
    private readonly App _appInstance = Application.Current as App;
    private readonly AppClient _appClient;
    
    public TextAlignmentByNickname()
    {
        _appClient = _appInstance.Services.GetRequiredService<AppSharedOptions>().AppClient;
    }
    
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string nickname)
        {
            return nickname == _appClient.CurrentUser.Nickname ? TextAlignment.Right : TextAlignment.Left;
        }
        else
        {
            throw new InvalidOperationException("The value must be of type System.String.");
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TextAlignment textAlignment && textAlignment == TextAlignment.Right)
        {
            return _appClient.CurrentUser.Nickname;
        }
        else
        {
            throw new InvalidOperationException("Getting another user's name is not supported.");
        }
    }
}