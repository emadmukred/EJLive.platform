using System;
using System.Text.RegularExpressions;

namespace EJLive.Core.Engine
{
    /// <summary>
    /// Redacts sensitive data from log messages and strings before they are persisted
    /// or transmitted. Supports card numbers, account numbers, passwords, and generic secrets.
    /// </summary>
    public static class LogRedactionEngine
    {
        // Card numbers: 13-19 consecutive digits, optionally with dashes or spaces.
        private static readonly Regex CardNumberPattern = new Regex(
            @"\b(?:\d[ -]*?){13,19}\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Account numbers: 8-20 consecutive digits, often prefixed with labels.
        private static readonly Regex AccountNumberPattern = new Regex(
            @"(?<=(?:account|acct|accountnumber|account\s*number)[:\s=]*)\b\d{8,20}\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Passwords: common patterns like Password=xxx, pwd:xxx, etc.
        private static readonly Regex PasswordPattern = new Regex(
            @"(?<=(?:password|passwd|pwd|pass|secret)[:\s=]*)[^\s&;,]+",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Generic secret keys: API keys, tokens, connection strings with secrets.
        private static readonly Regex SecretPattern = new Regex(
            @"(?<=(?:api[_-]?key|token|secret|connectionstring|connection[_-]?string)[:\s=]*)[^\s&;,]+",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Redacts sensitive patterns from the input text.
        /// </summary>
        /// <param name="input">The text to redact. May be null.</param>
        /// <returns>The redacted text, or null if <paramref name="input"/> is null.</returns>
        public static string? Redact(string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            string result = CardNumberPattern.Replace(input, "[REDACTED-CARD]");
            result = AccountNumberPattern.Replace(result, "[REDACTED-ACCOUNT]");
            result = PasswordPattern.Replace(result, "[REDACTED]");
            result = SecretPattern.Replace(result, "[REDACTED]");

            return result;
        }

        /// <summary>
        /// Redacts sensitive patterns and also masks a custom pattern with the specified replacement.
        /// </summary>
        /// <param name="input">The text to redact. May be null.</param>
        /// <param name="customPattern">A custom regex pattern to match and redact.</param>
        /// <param name="replacement">The replacement string for the custom pattern.</param>
        /// <returns>The redacted text, or null if <paramref name="input"/> is null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="customPattern"/> is null.</exception>
        public static string? Redact(string? input, Regex customPattern, string replacement)
        {
            if (customPattern == null)
            {
                throw new ArgumentNullException(nameof(customPattern));
            }

            string? result = Redact(input);
            if (result == null)
            {
                return null;
            }

            return customPattern.Replace(result, replacement);
        }
    }
}
