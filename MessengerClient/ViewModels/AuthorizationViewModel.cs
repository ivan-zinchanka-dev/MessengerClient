using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using MessengerClient.Commands;
using MessengerClient.Core.Models;
using MessengerClient.Views;

namespace MessengerClient.ViewModels;

public class AuthorizationViewModel : INotifyPropertyChanged
{
    private const string PasswordRegexPattern = "^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9]).{8,}$";
    
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
                if (_currentView is SignInWindow)
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
    
    private void SignInIfPossible()
    {
        if (!Validate())
        {
            return;
        }

        User user = new User()
        {
            Nickname = Nickname,
            Password = Password
        };
                
        _appClient.TryLoginAsync(user, success =>
        {
            if (success)
            {
                ErrorMessage = string.Empty;
                OnSignedIn?.Invoke(user);
            }
            else
            {
                ErrorMessage = "User not exist";
            }
        });
        
    }

    private void SignUpIfPossible()
    {
        if (!Validate())
        {
            return;
        }
        
        User user = new User()
        {
            Nickname = Nickname,
            Password = Password
        };
                    
        _appClient.TrySignUpAsync(user, success =>
        {
            if (success)
            {
                ErrorMessage = string.Empty;
                OnSignedUp?.Invoke(user);
            }
            else
            {
                ErrorMessage = "This nickname is already taken";
            }
        });
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
        ErrorMessage = "Connection error";
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