using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using MessengerClient.Commands;
using MessengerClient.Core.Models;
using MessengerClient.Network;
using MessengerClient.Views;

namespace MessengerClient.ViewModels;

public class AuthorizationViewModel : INotifyPropertyChanged
{
    private const string PasswordRegexPattern = "^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9]).{8,}$";
    private const string ConnectionErrorMessage = "Connection error";
    
    private string _nickname = "Jan Zinch";           // "Jan Zinch"
    private string _password = "1111aBll";           // "1111aBll"
    private string _passwordConfirm;
    private string _errorMessage;
    
    private RelayCommand _signInCommand;
    private RelayCommand _signUpCommand;
    private RelayCommand _backCommand;
    
    private AppClient _appClient;
    private Window _currentView;

    private SignInWindow _signInWindow;
    private SignUpWindow _signUpWindow;
    
    public event PropertyChangedEventHandler PropertyChanged;
    public event Action<User> OnSignedIn;
    public event Action<User> OnSignedUp;
    
    public string Nickname
    {
        get => _nickname;
        set
        {
            _nickname = value;
            OnPropertyChanged();
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged();
        }
    }
    
    public string PasswordConfirm
    {
        get => _passwordConfirm;
        set
        {
            _passwordConfirm = value;
            OnPropertyChanged();
        }
    }
    
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }
    
    public RelayCommand SignInCommand
    {
        get
        {
            return _signInCommand ??= new RelayCommand(obj =>
            {
                SignInIfPossible();
            });
        }
    }
    
    public RelayCommand SignUpCommand
    {
        get
        {
            return _signUpCommand ??= new RelayCommand(obj =>
            {
                if (_currentView is SignInWindow && _appClient.IsConnected)
                {
                    SwitchToSignUpWindow();
                }
                else if (_currentView is SignUpWindow)
                {
                    SignUpIfPossible();
                }
            });
        }
    }
    
    public RelayCommand BackCommand
    {
        get
        {
            return _backCommand ??= new RelayCommand(obj =>
            {
                SwitchToSignInWindow();
            });
        }
    }
    
    public AuthorizationViewModel(AppClient appClient)
    {
        _appClient = appClient;
        _appClient.ErrorCaptured += OnAppClientErrorCaptured;
        
        _signInWindow = new SignInWindow();
        _signInWindow.DataContext = this;
        _currentView = _signInWindow;
        
        _signUpWindow = new SignUpWindow();
        _signUpWindow.DataContext = this;
        
        _signInWindow.Closed += OnWindowClosed;
        _signUpWindow.Closed += OnWindowClosed;
    }

    public void ShowSignInWindow()
    {
        _signInWindow.Show();
    }

    public void HideAllWindows()
    {
        _signInWindow.Hide();
        _signUpWindow.Hide();
    }

    private bool Validate()
    {
        if (string.IsNullOrEmpty(Nickname))
        {
            ErrorMessage = "Nickname should not be empty";
            return false;
        }

        if (_currentView is SignUpWindow)
        {
            Regex regex = new Regex(PasswordRegexPattern);
            
            if (!regex.IsMatch(Password))
            {
                ErrorMessage = "Password should has at least 8 characters " +
                               "and contains numbers, uppercase and lowercase latin letters";
                return false;
            }
            
            if (Password != PasswordConfirm)
            {
                ErrorMessage = "Passwords mismatch";
                return false;
            }
        }
        
        ErrorMessage = string.Empty;
        return true;
    }
    
    private bool TryGetValidatedUser(out User user)
    {
        user = null;
        
        if (!_appClient.IsConnected)
        {
            ErrorMessage = ConnectionErrorMessage;
            return false;
        }

        if (!Validate())
        {
            return false;
        }
        
        user = new User()
        {
            Nickname = Nickname,
            Password = Password
        };

        return true;
    }
    
    private void SignInIfPossible()
    {
        if (TryGetValidatedUser(out User user))
        {
            _appClient.TryLoginAsync(user).ContinueWith(task =>
            {
                if (task.Result)
                {
                    ErrorMessage = string.Empty;
                    OnSignedIn?.Invoke(user);
                }
                else
                {
                    ErrorMessage = _appClient.IsConnected ? "User not exist" : ConnectionErrorMessage;
                }
                
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }

    private void SignUpIfPossible()
    {
        if (TryGetValidatedUser(out User user))
        {
            _appClient.TrySignUpAsync(user).ContinueWith(task =>
            {
                if (task.Result)
                {
                    ErrorMessage = string.Empty;
                    OnSignedUp?.Invoke(user);
                }
                else
                {
                    ErrorMessage = _appClient.IsConnected ? "This nickname is already taken" : ConnectionErrorMessage;
                }
                
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
    
    private void SwitchToSignUpWindow()
    {
        _signInWindow.Hide();
        _currentView = _signUpWindow;
        _signUpWindow.Show();
    }
    
    private void SwitchToSignInWindow()
    {
        _signUpWindow.Hide();
        _currentView = _signInWindow;
        _signInWindow.Show();
    }
    
    private void OnAppClientErrorCaptured()
    {
        ErrorMessage = ConnectionErrorMessage;
    }

    private static void OnWindowClosed(object sender, EventArgs e)
    {
        App.Instance.Shutdown();
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}