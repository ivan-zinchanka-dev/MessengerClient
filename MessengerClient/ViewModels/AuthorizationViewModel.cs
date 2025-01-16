using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MessengerClient.Commands;
using MessengerClient.Management;
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
    private readonly WindowManager _windowManager;
    /*private readonly SignInWindow _signInWindow;
    private readonly SignUpWindow _signUpWindow;
    private Window _currentView;*/
    
    private readonly ValidationComponent _validationComponent;
    
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
                    if (_windowManager.ActiveWindow is SignInWindow)
                    {
                        SwitchToSignUpWindow();
                    }
                    else if (_windowManager.ActiveWindow is SignUpWindow)
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

    public bool HasErrors => _validationComponent.ErrorCollection.HasErrors;
    
    private static class Messages
    {
        public const string ConnectionErrorMessage = "A connection error occurred.";
        public const string NicknameErrorMessage = "The nickname must not be empty and must not contain spaces.";
        public const string PasswordErrorMessage = "The password must be at least 8 characters long and contain numbers,\n" + 
                                                   "uppercase and lowercase Latin letters, and must not contain spaces.";
        public const string PasswordConfirmErrorMessage = "Passwords do not match.";

        public const string SignInFail = "This user does not exist or the password is incorrect.";
        public const string SignUpFail = "This nickname is already taken.";
    }

    public AuthorizationViewModel(App appInstance, WindowManager windowManager)
    {
        _appInstance = appInstance;
        _windowManager = windowManager;
        
        _validationComponent = new ValidationComponent(this);
        _validationComponent.OnErrorsChanged += OnErrorsChanged;
    }
    
    public IEnumerable GetErrors(string propertyName) => _validationComponent.ErrorCollection.GetErrors(propertyName);
    
    private bool TryGetValidatedUser(out User user)
    {
        user = null;
        
        if (!_appInstance.IsClientConnected)
        {
            ErrorMessage = Messages.ConnectionErrorMessage;
            return false;
        }

        if (!ValidateViewModel())
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
                    ErrorMessage = _appInstance.IsClientConnected ? Messages.SignInFail : Messages.ConnectionErrorMessage;
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
                    ErrorMessage = _appInstance.IsClientConnected ? Messages.SignUpFail : Messages.ConnectionErrorMessage;
                }
                
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
    
    private void SwitchToSignUpWindow()
    {
        UpdateErrorMessage();
        _windowManager.SwitchTo<SignUpWindow>();
    }
    
    private void SwitchToSignInWindow()
    {
        _validationComponent.ErrorCollection.ClearAllErrors();
        UpdateErrorMessage();
        _windowManager.SwitchTo<SignInWindow>();
    }
    
    private void OnWindowClosed(object sender, EventArgs e)
    {
        _appInstance.Shutdown();
    }

    private void UpdatePropertyValidationState([CallerMemberName] string propertyName = null)
    {
        _validationComponent.UpdatePropertyValidationState(propertyName);
        UpdateErrorMessage();
    }

    private bool ValidateViewModel()
    {
        if (_windowManager.ActiveWindow is SignInWindow)
        {
            return true;
        }
        
        bool result = _validationComponent.ValidateModel();
        UpdateErrorMessage();
        return result;
    }
    
    private void UpdateErrorMessage()
    {
        foreach (string propertyName in _validationComponent.PropertyNames)
        {
            if (_validationComponent.ErrorCollection.HasErrorsOf(propertyName))
            {
                string error = _validationComponent.ErrorCollection.GetErrors(propertyName).FirstOrDefault();

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