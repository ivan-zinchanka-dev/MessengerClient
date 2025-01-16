using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace MessengerClient.Management;

public class WindowManager
{
    private readonly IServiceProvider _serviceProvider;
    private Window _activeWindow;

    public Window ActiveWindow => _activeWindow;
    public event Action<Window> OnWindowSwitched;
    public event Action OnAllWindowsClosed;
    
    public WindowManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public bool SwitchTo<TWindow>() where TWindow : Window
    {
        Type windowType = typeof(TWindow);

        if (windowType == _activeWindow?.GetType())
        {
            return false; 
        }

        if (_serviceProvider.GetRequiredService<TWindow>() is Window window)
        {
            Window previousWindow = _activeWindow;
            
            _activeWindow = window;
            _activeWindow.Show();
            _activeWindow.Closed += OnActiveWindowClosed;
            
            if (previousWindow != null)
            {
                previousWindow.Closed -= OnActiveWindowClosed;
                previousWindow.Close();
            }
            
            OnWindowSwitched?.Invoke(_activeWindow);
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnActiveWindowClosed(object sender, EventArgs eventArgs)
    {
        _activeWindow = null;
        OnAllWindowsClosed?.Invoke();
    }
}