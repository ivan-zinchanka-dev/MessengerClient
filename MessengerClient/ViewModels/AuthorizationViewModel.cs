using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using MessengerClient.Commands;
using MessengerClient.Validation;
using MessengerClient.Views;
using MessengerCoreLibrary.Models;

namespace MessengerClient.ViewModels;

public class AuthorizationViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
{
    private const string NoSpacesRegexPattern = @"^\S*$";
    private const string PasswordRegexPattern = @"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?!.*\s).{8,}$";
    
    private string _nickname;
    private string _password;
    private string _passwordConfirm;
    private string _errorMessage;
    
    private RelayCommand _signInCommand;
    private RelayCommand _signUpCommand;
    private RelayCommand _backCommand;

    private readonly App _appInstance;
    private readonly SignInWindow _signInWindow;
    private readonly SignUpWindow _signUpWindow;
    private Window _currentView;
    
    private readonly IEnumerable<string> _validatablePropertyNames;
    private readonly ValidationErrorCollection _errorCollection = new ValidationErrorCollection();
    
    public event PropertyChangedEventHandler PropertyChanged;
    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
    
    [Required(AllowEmptyStrings = false, ErrorMessage = Messages.NicknameErrorMessage)]
    [RegularExpression(NoSpacesRegexPattern, ErrorMessage = Messages.NicknameErrorMessage)]
    public string Nickname
    {
        get => _nickname;
        set
        {
            _nickname = value;
            OnPropertyChanged();
            UpdatePropertyValidationState();
        }
    }
    
    [Required(ErrorMessage = Messages.PasswordErrorMessage)] 
    [RegularExpression(PasswordRegexPattern, ErrorMessage = Messages.PasswordErrorMessage)]
    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged();
            UpdatePropertyValidationState();
        }
    }
    
    [Compare(nameof(Password), ErrorMessage = Messages.PasswordConfirmErrorMessage)]
    public string PasswordConfirm
    {
        get => _passwordConfirm;
        set
        {
            _passwordConfirm = value;
            OnPropertyChanged();
            UpdatePropertyValidationState();
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
                if (!_appInstance.IsClientConnected)
                {
                    ErrorMessage = Messages.ConnectionErrorMessage;
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

    private static class Messages
    {
        public const string ConnectionErrorMessage = "A connection error occurred.";
        public const string NicknameErrorMessage = "The nickname must not be empty and must not contain spaces.";
        public const string PasswordErrorMessage = "The password must be at least 8 characters long and contain numbers,\n" + 
                                                   "uppercase and lowercase Latin letters, and must not contain spaces.";
        public const string PasswordConfirmErrorMessage = "Passwords do not match.";
    }

    public AuthorizationViewModel(App appInstance)
    {
        _appInstance = appInstance;
        
        _signInWindow = new SignInWindow();
        _signInWindow.DataContext = this;
         
        _signUpWindow = new SignUpWindow();
        _signUpWindow.DataContext = this;

        _validatablePropertyNames = ValidationUtility.GetValidatablePropertyNames(this);
        
        _signInWindow.Closed += OnWindowClosed;
        _signUpWindow.Closed += OnWindowClosed;

        _currentView = _signInWindow;
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
    
    public bool HasErrors => _errorCollection.HasErrors;
    public IEnumerable GetErrors(string propertyName) => _errorCollection.GetErrors(propertyName);
    
    private bool TryGetValidatedUser(out User user)
    {
        user = null;
        
        if (!_appInstance.IsClientConnected)
        {
            ErrorMessage = Messages.ConnectionErrorMessage;
            return false;
        }

        if (!Validate())
        {
            return false;
        }
        
        user = new User(Nickname, Password);

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
                    ErrorMessage = _appInstance.IsClientConnected ? 
                        "This user does not exist or the password is incorrect." : 
                        Messages.ConnectionErrorMessage;
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
                    ErrorMessage = _appInstance.IsClientConnected ? 
                        "This nickname is already taken." : 
                        Messages.ConnectionErrorMessage;
                }
                
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
    
    private void SwitchToSignUpWindow()
    {
        _signInWindow.Hide();
        
        UpdateErrorMessage();
        
        _currentView = _signUpWindow;
        _signUpWindow.Show();
    }
    
    private void SwitchToSignInWindow()
    {
        _signUpWindow.Hide();
        
        _errorCollection.ClearAllErrors();
        UpdateErrorMessage();
        
        _currentView = _signInWindow;
        _signInWindow.Show();
    }
    
    private void OnWindowClosed(object sender, EventArgs e)
    {
        _appInstance.Shutdown();
    }

    private void UpdatePropertyValidationState([CallerMemberName] string propertyName = null)
    {
        if (_errorCollection.HasErrorsOf(propertyName))
        {
            List<ValidationResult> results = new List<ValidationResult>();
            ValidationContext context = new ValidationContext(this);

            if (!Validator.TryValidateObject(this, context, results, true))
            {
                foreach (ValidationResult result in results)
                {
                    foreach (string resultPropertyName in result.MemberNames)
                    {
                        if (resultPropertyName == propertyName)
                        {
                            return;
                        }

                    }
                }
            }
        }

        _errorCollection.TryClearErrors(propertyName);
        
        OnErrorsChanged(propertyName);
    }

    private bool Validate()
    {
        if (_currentView is SignInWindow)
        {
            return true;
        }
        
        List<ValidationResult> results = new List<ValidationResult>();
        ValidationContext context = new ValidationContext(this);

        foreach (string propertyName in _validatablePropertyNames)
        {
            if (_errorCollection.TryClearErrors(propertyName)){
            
                OnErrorsChanged(propertyName); 
            }
        }
        
        if (!Validator.TryValidateObject(this, context, results, true))
        {
            foreach (ValidationResult result in results)
            {
                foreach (string propertyName in result.MemberNames)
                {
                    if (_errorCollection.TryAddError(propertyName, result.ErrorMessage))
                    {
                        OnErrorsChanged(propertyName);
                    }
                }
            }

            UpdateErrorMessage();
            return false;
        }
        else
        {
            UpdateErrorMessage();
            return true;
        }
    }
    
    private void UpdateErrorMessage()
    {
        foreach (string propertyName in _validatablePropertyNames)
        {
            if (_errorCollection.HasErrorsOf(propertyName))
            {
                string error = _errorCollection.GetErrors(propertyName).FirstOrDefault();

                if (!string.IsNullOrEmpty(error))
                {
                    ErrorMessage = error;
                    return;
                }
            }
        }

        ErrorMessage = string.Empty;
    } 
    
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    private void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }
    
}