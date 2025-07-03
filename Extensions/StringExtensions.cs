using System;
using System.Text.RegularExpressions;

namespace HotelManager.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Checks if the string is a valid email address
        /// </summary>
        public static bool IsValidEmail(this string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
                return emailRegex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the string is a valid phone number (Vietnamese format)
        /// </summary>
        public static bool IsValidPhoneNumber(this string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            try
            {
                // Remove spaces and special characters
                var cleanPhone = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
                
                // Vietnamese phone number format: 10-11 digits starting with 0
                var phoneRegex = new Regex(@"^0[0-9]{8,10}$");
                return phoneRegex.IsMatch(cleanPhone);
            }
            catch
            {
                return false;
            }
        }

        public static string ToTitleCase(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var words = text.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
                }
            }
            return string.Join(" ", words);
        }
    }
} 