using System;

namespace HotelManager.Exceptions
{
    public class DuplicateEntityException : BusinessException
    {
        public string EntityType { get; }
        public string DuplicateField { get; }
        public object DuplicateValue { get; }

        public DuplicateEntityException(string entityType, string duplicateField, object duplicateValue)
            : base($"{entityType} with {duplicateField} '{duplicateValue}' already exists", $"{entityType} với {duplicateField} '{duplicateValue}' đã tồn tại", "DUPLICATE_ENTITY")
        {
            EntityType = entityType;
            DuplicateField = duplicateField;
            DuplicateValue = duplicateValue;
        }
    }
} 