using FluentValidation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.ChatMauiApp.ViewModels
{
    public abstract class ValidatableViewModel<TModel, TValidator> : BaseViewModel
        where TModel : class, new()
        where TValidator : AbstractValidator<TModel>, new()
    {
        protected TValidator Validator { get; } = new();
        private readonly Dictionary<string, string> _validationErrors = new();
        public IReadOnlyDictionary<string, string> ValidationErrors => _validationErrors;

        public bool Validate(TModel model)
        {
            _validationErrors.Clear();

            var result = Validator.Validate(model);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                    _validationErrors.Add(error.PropertyName, error.ErrorMessage);
            }

            RaisePropertyChanged(nameof(ValidationErrors));

            return result.IsValid;
        }

        public string GetErrorForProperty(string propertyName)
            => _validationErrors.ContainsKey(propertyName) ? _validationErrors[propertyName] : string.Empty;
    }
}
