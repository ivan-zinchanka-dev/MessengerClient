using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MessengerClient.Commands;
using MessengerClient.Core.Models;
using MessengerClient.Views;

namespace MessengerClient.ViewModels;

public class AuthorizationViewModel : INotifyPropertyChanged
{
    private string _nickname = "Nick";
    private string _password;
    private string _errorMessage;
    private RelayCommand _signInCommand;

    private AppClient _appClient;
    
    public LoginWindow Window { get; private set; }

    public event PropertyChangedEventHandler PropertyChanged;
    public event Action OnSignedIn;
    
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
                    Console.WriteLine("Res: " + success.ToString());
                    
                    if (success)
                    {
                        Console.WriteLine("Try event");
                        OnSignedIn?.Invoke();
                    }
                    else
                    {
                        ErrorMessage = "User not exist";
                    }

                });
                
            });
        }
    }

    public AuthorizationViewModel(AppClient appClient)
    {
        _appClient = appClient;
        _appClient.ErrorCaptured += OnAppClientErrorCaptured;
        
        Window = new LoginWindow();
        Window.DataContext = this;
    }

    private void OnAppClientErrorCaptured()
    {
        ErrorMessage = "Connection error";
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}