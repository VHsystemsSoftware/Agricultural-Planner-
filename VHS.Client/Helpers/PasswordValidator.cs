using System.Text.RegularExpressions;

namespace VHS.Client.Helpers
{
    public static class PasswordValidator
    {
        public static class Requirements
        {
            public const int MinimumLength = 8;
            public const bool RequireDigit = true;
            public const bool RequireLowercase = true;
            public const bool RequireUppercase = false;
            public const bool RequireNonAlphanumeric = false;
        }

        public static (bool isValid, string errorMessage) ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return (false, "Password is required.");
            }

            var errors = new List<string>();

            // Check minimum length
            if (password.Length < Requirements.MinimumLength)
            {
                errors.Add($"Password must be at least {Requirements.MinimumLength} characters.");
            }

            // Check for digit
            if (Requirements.RequireDigit && !Regex.IsMatch(password, @"\d"))
            {
                errors.Add("Password must have at least one digit ('0'-'9').");
            }

            // Check for lowercase
            if (Requirements.RequireLowercase && !Regex.IsMatch(password, @"[a-z]"))
            {
                errors.Add("Password must have at least one lowercase ('a'-'z').");
            }

            // Check for uppercase
            if (Requirements.RequireUppercase && !Regex.IsMatch(password, @"[A-Z]"))
            {
                errors.Add("Password must have at least one uppercase ('A'-'Z').");
            }

            // Check for non-alphanumeric
            if (Requirements.RequireNonAlphanumeric && !Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
            {
                errors.Add("Password must have at least one non-alphanumeric character.");
            }

            if (errors.Any())
            {
                return (false, string.Join(" ", errors));
            }

            return (true, string.Empty);
        }

        public static string GetPasswordRequirementsMessage()
        {
            var requirements = new List<string>
            {
                $"Minimum {Requirements.MinimumLength} characters"
            };

            if (Requirements.RequireDigit)
                requirements.Add("at least one digit (0-9)");

            if (Requirements.RequireLowercase)
                requirements.Add("at least one lowercase letter (a-z)");

            if (Requirements.RequireUppercase)
                requirements.Add("at least one uppercase letter (A-Z)");

            if (Requirements.RequireNonAlphanumeric)
                requirements.Add("at least one special character");

            return string.Join(", ", requirements);
        }
    }
}