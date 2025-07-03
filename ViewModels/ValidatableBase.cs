using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using HotelManager.Interfaces;

namespace HotelManager.ViewModels
{
    public abstract class ValidatableBase : BaseViewModel, INotifyDataErrorInfo
    {
        private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

        public ValidatableBase() : base()
        {
        }

        public ValidatableBase(ILogger logger, IAuditService auditService = null) : base(logger, auditService)
        {
        }

        #region INotifyDataErrorInfo Implementation

        public bool HasErrors => _errors.Any();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return _errors.SelectMany(x => x.Value);

            return _errors.ContainsKey(propertyName) ? _errors[propertyName] : null;
        }

        #endregion

        #region Validation Methods

        /// <summary>
        /// Validates a property using data annotations and custom validation logic
        /// </summary>
        protected bool ValidateProperty<T>(T value, [CallerMemberName] string propertyName = null)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(this) { MemberName = propertyName };
            bool isValid = Validator.TryValidateProperty(value, context, results);

            if (results.Any())
            {
                SetErrors(propertyName, results.Select(r => r.ErrorMessage));
            }
            else
            {
                ClearErrors(propertyName);
            }

            return isValid;
        }

        /// <summary>
        /// Validates the entire model
        /// </summary>
        protected bool ValidateModel()
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(this);
            bool isValid = Validator.TryValidateObject(this, context, results, true);

            // Clear all existing errors
            var propertiesToClear = _errors.Keys.ToList();
            foreach (var property in propertiesToClear)
            {
                ClearErrors(property);
            }

            // Set new errors
            foreach (var result in results)
            {
                var propertyName = result.MemberNames.FirstOrDefault() ?? string.Empty;
                SetErrors(propertyName, new[] { result.ErrorMessage });
            }

            return isValid;
        }

        /// <summary>
        /// Sets errors for a property
        /// </summary>
        protected void SetErrors(string propertyName, IEnumerable<string> errors)
        {
            if (errors?.Any() == true)
            {
                _errors[propertyName] = errors.ToList();
            }
            else
            {
                _errors.Remove(propertyName);
            }

            OnErrorsChanged(propertyName);
        }

        /// <summary>
        /// Clears errors for a property
        /// </summary>
        protected void ClearErrors(string propertyName)
        {
            if (_errors.Remove(propertyName))
            {
                OnErrorsChanged(propertyName);
            }
        }

        /// <summary>
        /// Clears all errors
        /// </summary>
        protected void ClearAllErrors()
        {
            var propertiesToClear = _errors.Keys.ToList();
            _errors.Clear();

            foreach (var property in propertiesToClear)
            {
                OnErrorsChanged(property);
            }
        }

        private void OnErrorsChanged([CallerMemberName] string propertyName = null)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            OnPropertyChanged(nameof(HasErrors));
        }

        #endregion

        #region Enhanced Property Setting with Validation

        /// <summary>
        /// Sets property value with automatic validation
        /// </summary>
        protected bool SetValidatedProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            ValidateProperty(value, propertyName);
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
} 