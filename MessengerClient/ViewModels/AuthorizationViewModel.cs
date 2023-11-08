using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using MessengerClient.Commands;
using MessengerClient.Core.Models;
using MessengerClient.Views;

namespace MessengerClient.ViewModels;

public class AuthorizationViewModel : INotifyPropertyChanged
{
    private string _nickname = "Jan Zinch";
    private string _password = "111111";
    private string _passwordConfirm = "111111";
    private string _errorMessage;
    private RelayCommand _signInCommand;
    private RelayCommand _signUpCommand;

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
                User user = new User()
                {
                    Nickname = Nickname,
                    Password = Password
                };
                
                _appClient.TryLoginAsync(user, success =>
                {
                    if (success)
                    {
                        OnSignedIn?.Invoke(user);
                    }
                    else
                    {
                        ErrorMessage = "User not exist";
                    }
                });
                
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
                    Console.WriteLine("SwitchToSignUpWindow");
                    SwitchToSignUpWindow();
                }
                else if (_currentView is SignUpWindow)
                {
                    Console.WriteLine("SignUpIfPossible");
                    SignUpIfPossible();
                }
            });
        }
    }

    private void SwitchToSignUpWindow()
    {
        //_signInWindow.Closed -= ShutDownApp;
        _signInWindow.Hide();
        _currentView = _signUpWindow;
        _signUpWindow.Show();
        //_signInWindow.Closed += ShutDownApp;
    }

    private void SignUpIfPossible()
    {
        if (Password == PasswordConfirm)
        {
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
        else
        {
            ErrorMessage = "Passwords mismatch";
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
        
        _signInWindow.Closed += ShutDownApp;
        _signUpWindow.Closed += ShutDownApp;
    }

    public void ShowSignInWindow()
    {
        _signInWindow.Show();
    }

    public void CloseAllWindows()
    {
        /*_signInWindow.Closed -= ShutDownApp;
        _signUpWindow.Closed -= ShutDownApp;*/
        
        _signInWindow.Hide();
        _signUpWindow.Hide();
    }

    private void OnAppClientErrorCaptured()
    {
        ErrorMessage = "Connection error";
    }

    private static void ShutDownApp(object sender, EventArgs e)
    {
        App.Instance.Shutdown();
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}