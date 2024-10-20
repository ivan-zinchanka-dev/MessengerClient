using System;
using System.Windows;

namespace MessengerClient.Views
{
    public partial class SignInWindow : Window
    {
        public event Action OnHidden;
        
        public SignInWindow()
        {
            InitializeComponent();
        }

        public void SetControlsEnabled(bool controlsEnabled)
        {
            _signInButton.IsEnabled = _signUpButton.IsEnabled = controlsEnabled;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            OnHidden?.Invoke();
        }
    }
}