using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace MessengerClient.Management;

public class WindowManager
{
    private readonly IServiceProvider _serviceProvider;
    private Window _currentWindow;

    public Window CurrentWindow => _currentWindow;
    public event Action<Window> OnWindowSwitched;
    
    public WindowManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public bool SwitchTo<TWindow>() where TWindow : Window
    {
        Type windowType = typeof(TWindow);

        if (windowType == _currentWindow?.GetType())
        {
            return false; 
        }

        if (_serviceProvider.GetService<TWindow>() is Window window)
        {
            Window previousWindow = _currentWindow;
            
            _currentWindow = window;
            _currentWindow.Show();
            
            if (previousWindow != null)
            {
                previousWindow.Close();
            }
            
            OnWindowSwitched?.Invoke(_currentWindow);
            return true;
        }
        else
        {
            return false;
        }
    }
}