using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MessengerClient.Validation;

public class ValidationComponent
{
    private readonly object _validatableObject;
    private readonly ValidationContext _context;
    
    public IEnumerable<string> PropertyNames { get; }
    public ValidationErrorCollection ErrorCollection { get; }
    
    public event Action<string> OnErrorsChanged; 
    
    public ValidationComponent(object validatableObject)
    {
        _validatableObject = validatableObject;
        _context = new ValidationContext(_validatableObject);
        
        PropertyNames = GetValidatablePropertyNames(validatableObject);
        ErrorCollection = new ValidationErrorCollection();
    }
    
    public void UpdatePropertyValidationState(string propertyName)
    {
        if (ErrorCollection.HasErrorsOf(propertyName))
        {
            List<ValidationResult> results = new List<ValidationResult>();
            
            if (!Validator.TryValidateObject(_validatableObject, _context, results, true))
            {
                bool errorExist = results
                    .SelectMany(result => result.MemberNames)
                    .Any(resultPropertyName => resultPropertyName == propertyName);
                
                if (errorExist)
                {
                    return;
                }
            }
        }

        ErrorCollection.TryClearErrors(propertyName);
        OnErrorsChanged?.Invoke(propertyName);
    }
    
    public bool ValidateModel()
    {
        List<ValidationResult> results = new List<ValidationResult>();
        
        foreach (string propertyName in PropertyNames)
        {
            if (ErrorCollection.TryClearErrors(propertyName)){
            
                OnErrorsChanged?.Invoke(propertyName); 
            }
        }
        
        if (!Validator.TryValidateObject(_validatableObject, _context, results, true))
        {
            foreach (ValidationResult result in results)
            {
                foreach (string propertyName in result.MemberNames)
                {
                    if (ErrorCollection.TryAddError(propertyName, result.ErrorMessage))
                    {
                        OnErrorsChanged?.Invoke(propertyName);
                    }
                }
            }
            
            return false;
        }
        else
        {
            return true;
        }
    }
    
    private static IEnumerable<string> GetValidatablePropertyNames(object validatableObject)
    {
        return validatableObject.GetType()
            .GetProperties()
            .Where(property => property.GetCustomAttributes(typeof(ValidationAttribute), true).Any())
            .Select(property => property.Name);
    }

}