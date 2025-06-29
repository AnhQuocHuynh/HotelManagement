using System;

namespace HotelManager.Exceptions
{
    /// <summary>
    /// Base exception class for all business logic exceptions in Hotel Manager
    /// </summary>
    public class BusinessException : Exception
    {
        public string UserMessage { get; }
        public string ErrorCode { get; }

        public BusinessException(string message, string userMessage = null, string errorCode = null)
            : base(message)
        {
            UserMessage = userMessage ?? message;
            ErrorCode = errorCode;
        }

        public BusinessException(string message, Exception innerException, string userMessage = null, string errorCode = null)
            : base(message, innerException)
        {
            UserMessage = userMessage ?? message;
            ErrorCode = errorCode;
        }
    }
} 