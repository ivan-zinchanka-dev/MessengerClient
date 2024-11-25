using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MessengerClient.Validation;

public class ValidationErrorCollection : IEnumerable<IReadOnlyList<string>>
{
    private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();
    
    public bool HasErrors => _errors.Any(error => error.Value.Any());
    
    public IEnumerable<string> GetErrors(string propertyName)
    {
        if (_errors.TryGetValue(propertyName, out List<string> errors))
        {
            return errors;
        }
        else
        {
            return new List<string>();
        }
    }

    public bool TryAddError(string propertyName, string errorMessage)
    {
        if (!_errors.ContainsKey(propertyName))
        {
            _errors[propertyName] = new List<string>();
        }
        
        if (!_errors[propertyName].Contains(errorMessage))
        {
            _errors[propertyName].Add(errorMessage);
            return true;
        }

        return false;
    }
    
    public bool TryClearErrors(string propertyName)
    {
        if (_errors.ContainsKey(propertyName) && _errors[propertyName].Any())
        {
            _errors[propertyName].Clear();
            return true;
        }

        return false;
    }

    public void ClearAllErrors()
    {
        _errors.Clear();
    }

    public IEnumerator<IReadOnlyList<string>> GetEnumerator()
    {
        foreach (var errorList in _errors.Values)
        {
            yield return errorList;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}