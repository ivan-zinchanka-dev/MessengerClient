﻿using System;
using System.Collections;
using System.Collections.Generic;
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

    private List<PropertyValidationStep> _validationSteps;
    
    public event PropertyChangedEventHandler PropertyChanged;
    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
    
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
        
        _signUpWindow = new SignUpWindow();
        _signUpWindow.DataContext = this;
        
        _signInWindow.Closed += OnWindowClosed;
        _signUpWindow.Closed += OnWindowClosed;

        _currentView = _signInWindow;
        
        InitializeValidationSteps();
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

        ValidateAndUpdateViewModel();
        
        if (_errorCollection.HasErrors)
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
    
    private void InitializeValidationSteps()
    {
        _validationSteps = new List<PropertyValidationStep>()
        {
            new PropertyValidationStep(nameof(Nickname), 
                () => string.IsNullOrEmpty(Nickname) || string.IsNullOrWhiteSpace(Nickname), 
                "Nickname should not be empty"),
            
            new PropertyValidationStep(nameof(Password), 
                () => !new Regex(PasswordRegexPattern).IsMatch(Password), 
                "Password should has at least 8 characters and contains numbers, " +
                "uppercase and lowercase latin letters"),
            
            new PropertyValidationStep(nameof(PasswordConfirm), 
                () => Password != PasswordConfirm, 
                "Passwords mismatch"),
        };
    }

    private void ValidateAndUpdateViewModel()
    {
        foreach (PropertyValidationStep validationStep in _validationSteps)
        {
            ValidateProperty(validationStep);
        }
        
        UpdateErrorMessage();
    }

    private void ValidateProperty(PropertyValidationStep validationStep)
    {
        if (_errorCollection.TryClearErrors(validationStep.PropertyName)){
            
            OnErrorsChanged(validationStep.PropertyName); 
        }

        if (validationStep.ErrorCondition() && 
            _errorCollection.TryAddError(validationStep.PropertyName, validationStep.ErrorMessage))
        {
            OnErrorsChanged(validationStep.PropertyName);
        }
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
    
    public IEnumerable GetErrors(string propertyName) => _errorCollection.GetErrors(propertyName);
    public bool HasErrors => _errorCollection.HasErrors;
}