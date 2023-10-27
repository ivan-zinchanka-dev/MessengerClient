using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MessengerClient.Commands;
using MessengerClient.Views;

namespace MessengerClient.ViewModels;

public class AuthorizationViewModel : INotifyPropertyChanged
{
    private string _nickname = "Nick";
    private string _password;
    private RelayCommand _signInCommand;

    public LoginWindow Window { get; private set; }

    public event PropertyChangedEventHandler PropertyChanged;
    public event Action OnSignedIn;
    
    public string Nickname
    {
        get => _nickname;
        set
        {
            _nickname = value;
            OnPropertyChanged(nameof(Nickname));
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged(nameof(Password));
        }
    }
    
    
    public RelayCommand SignInCommand
    {
        get
        {
            return _signInCommand ??= new RelayCommand(obj =>
            {
                Console.WriteLine("Sign in");
                OnSignedIn?.Invoke();
            });
        }
    }

    public AuthorizationViewModel()
    {
        Window = new LoginWindow();
        Window.DataContext = this;
    }
    
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}