using System;

namespace MessengerClient.Validation;

public struct PropertyValidationStep
{
    public string PropertyName { get; private set; }
    public Func<bool> ErrorCondition { get; private set; }
    public string ErrorMessage { get; private set; }

    public PropertyValidationStep(string propertyName, Func<bool> errorCondition, string errorMessage)
    {
        PropertyName = propertyName;
        ErrorCondition = errorCondition;
        ErrorMessage = errorMessage;
    }
}