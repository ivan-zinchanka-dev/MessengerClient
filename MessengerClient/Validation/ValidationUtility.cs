using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MessengerClient.Validation;

public static class ValidationUtility
{
    public static IEnumerable<string> GetValidatablePropertyNames(object validatableObject)
    {
        return validatableObject.GetType()
            .GetProperties()
            .Where(property => property.GetCustomAttributes(typeof(ValidationAttribute), true).Any())
            .Select(property => property.Name);
    }
}