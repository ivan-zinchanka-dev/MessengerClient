using System;
using System.Windows;

namespace MessengerClient
{
    public partial class LoginWindow : Window
    {
        public event Action OnHidden;
        
        public LoginWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            OnHidden?.Invoke();
        }
    }
}