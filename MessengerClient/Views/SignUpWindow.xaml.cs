using System;
using System.Windows;

namespace MessengerClient.Views;

public partial class SignUpWindow : Window
{
    public event Action OnHidden;
        
    public SignUpWindow()
    {
        InitializeComponent();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        OnHidden?.Invoke();
    }
}