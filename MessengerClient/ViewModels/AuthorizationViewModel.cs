using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using MessengerClient.Commands;
using MessengerClient.Core.Models;
using MessengerClient.Validation;
using MessengerClient.Views;

namespace MessengerClient.ViewModels;

public class AuthorizationViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
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

    private readonly App _appInstance;
    private readonly SignInWindow _signInWindow;
    private readonly SignUpWindow _signUpWindow;
    private Window _currentView;

    private readonly ValidationErrorCollection _errorCollection = new ValidationErrorCollection();
    
    public event PropertyChangedEventHandler PropertyChanged;
    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
    
    public string Nickname
    {
        get => _nickname;
        set
        {
            _nickname = value;
            OnPropertyChanged();

            ValidateProperty(propertyName =>
            {
                if (string.IsNullOrEmpty(Nickname))
                {
                    return _errorCollection.TryAddError(propertyName, "Nickname should not be empty");
                }

                return false;
            });
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged();
            
            ValidateProperty(propertyName =>
            {
                Regex regex = new Regex(PasswordRegexPattern);
            
                if (!regex.IsMatch(Password))
                {
                    return _errorCollection.TryAddError(propertyName,
                        "Password should has at least 8 characters and contains " +
                        "numbers, uppercase and lowercase latin letters");
                }
                
                return false;
            });
        }
    }
    
    public string PasswordConfirm
    {
        get => _passwordConfirm;
        set
        {
            _passwordConfirm = value;
            OnPropertyChanged();
            
            ValidateProperty(propertyName =>
            {
                if (Password != PasswordConfirm)
                {
                    return _errorCollection.TryAddError(propertyName, "Passwords mismatch");
                }
                
                return false;
            });
        }
    }
    
    public string ErrorMessage      // TODO ValidationSummary 
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
                if (!_appInstance.IsClientConnected)
                {
                    ErrorMessage = ConnectionErrorMessage;
                }
                else
                {
                    if (_currentView is SignInWindow)
                    {
                        SwitchToSignUpWindow();
                    }
                    else if (_currentView is SignUpWindow)
                    {
                        SignUpIfPossible();
                    }
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
    
    public AuthorizationViewModel(App appInstance)
    {
        _appInstance = appInstance;
        
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

    
    
    private bool TryGetValidatedUser(out User user)
    {
        user = null;
        
        if (!_appInstance.IsClientConnected)
        {
            ErrorMessage = ConnectionErrorMessage;
            return false;
        }

        if (_errorCollection.HasErrors)
        {
            return false;
        }

        /*if (!Validate())
        {
            return false;
        }*/
        
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
            _appInstance.TrySignInAsync(user).ContinueWith(task =>
            {
                if (task.Result)
                {
                    ErrorMessage = string.Empty;
                }
                else
                {
                    ErrorMessage = _appInstance.IsClientConnected ? "User not exist" : ConnectionErrorMessage;
                }
                
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }

    private void SignUpIfPossible()
    { 
        if (TryGetValidatedUser(out User user))
        {
            _appInstance.TrySignUpAsync(user).ContinueWith(task =>
            {
                if (task.Result)
                {
                    ErrorMessage = string.Empty;
                }
                else
                {
                    ErrorMessage = _appInstance.IsClientConnected ? "This nickname is already taken" : ConnectionErrorMessage;
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
    
    private void OnWindowClosed(object sender, EventArgs e)
    {
        _appInstance.Shutdown();
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    private void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }
    

    private void ValidateProperty(Func<string, bool> validation = null, [CallerMemberName] string propertyName = null)
    {
        if (_errorCollection.TryClearErrors(propertyName)){
            
           OnErrorsChanged(propertyName); 
        }

        if (validation?.Invoke(propertyName) == true)
        {
            OnErrorsChanged(propertyName);
        }
        
        UpdateErrorMessage();
    }

    private void UpdateErrorMessage()
    {
        // TODO foreach

        string errorMessage = string.Join('\n',
            string.Join('\n', _errorCollection.GetErrors(nameof(Nickname))),
            string.Join('\n', _errorCollection.GetErrors(nameof(Password))),
            string.Join('\n', _errorCollection.GetErrors(nameof(PasswordConfirm))));

        ErrorMessage = errorMessage;
    }

    private bool Validate()
    {
        // TODO Use data annotations or validation rules
        
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
    
    /*private void UpdateValidationSummary()
    {
        var errors = new List<string>();

        // Collect errors for each property
        foreach (var propertyName in new[] { nameof(Name), nameof(Age) })
        {
            var error = this[propertyName];
            if (!string.IsNullOrEmpty(error))
                errors.Add(error);
        }

        // Combine errors into a single string
        ValidationSummary = string.Join(Environment.NewLine, errors);
    }*/
    public IEnumerable GetErrors(string propertyName) => _errorCollection.GetErrors(propertyName);
    public bool HasErrors => _errorCollection.HasErrors;
    
}