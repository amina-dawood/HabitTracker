using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace HabitTracker.Utils
{
    public static class InputSanitizer
    {
        // Basic XSS prevention - remove potentially dangerous HTML/script tags
        private static readonly Regex DangerousTagsRegex = new Regex(
            @"<(script|object|embed|form|input|meta|link|iframe|frame|frameset|applet|style)\b[^<]*(?:(?!<\/\1>)<[^<]*)*<\/\1>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex DangerousAttributesRegex = new Regex(
            @"\b(on\w+|javascript:|vbscript:|data:text\/html|data:text\/javascript)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly HtmlEncoder _htmlEncoder = HtmlEncoder.Default;

        /// <summary>
        /// Sanitizes input by removing potentially dangerous HTML/script content
        /// </summary>
        public static string SanitizeHtml(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // HTML encode the input first
            string sanitized = _htmlEncoder.Encode(input);

            // Remove dangerous tags (though they should be encoded now)
            sanitized = DangerousTagsRegex.Replace(sanitized, "");

            // Remove dangerous attributes
            sanitized = DangerousAttributesRegex.Replace(sanitized, "");

            return sanitized;
        }

        /// <summary>
        /// Validates and sanitizes habit name input
        /// </summary>
        public static string SanitizeHabitName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            // Trim whitespace
            name = name.Trim();

            // Limit length
            if (name.Length > 100)
                name = name.Substring(0, 100);

            // Remove any HTML/script content
            name = SanitizeHtml(name);

            return name;
        }

        /// <summary>
        /// Validates and sanitizes habit description input
        /// </summary>
        public static string SanitizeHabitDescription(string description)
        {
            if (string.IsNullOrEmpty(description))
                return description;

            // Trim whitespace
            description = description.Trim();

            // Limit length
            if (description.Length > 500)
                description = description.Substring(0, 500);

            // Remove any HTML/script content
            description = SanitizeHtml(description);

            return description;
        }

        /// <summary>
        /// Validates frequency input
        /// </summary>
        public static string SanitizeFrequency(string frequency)
        {
            if (string.IsNullOrEmpty(frequency))
                return frequency;

            // Only allow specific frequency values
            var allowedFrequencies = new[] { "Daily", "Weekly", "Monthly", "Bi-weekly", "Yearly" };
            var sanitized = SanitizeHtml(frequency.Trim());

            return allowedFrequencies.Contains(sanitized) ? sanitized : "Daily";
        }

        /// <summary>
        /// Validates category input
        /// </summary>
        public static string SanitizeCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
                return "Other";

            // Only allow specific category values
            var allowedCategories = new[] { "Health", "Fitness", "Learning", "Productivity", "Finance", "Social", "Other" };
            var sanitized = SanitizeHtml(category.Trim());

            return allowedCategories.Contains(sanitized) ? sanitized : "Other";
        }

        /// <summary>
        /// Validates progress value
        /// </summary>
        public static int SanitizeProgress(int progress)
        {
            // Ensure progress is between 0 and 100
            return Math.Clamp(progress, 0, 100);
        }

        /// <summary>
        /// Validates target days
        /// </summary>
        public static int SanitizeTargetDays(int targetDays)
        {
            // Ensure target days is reasonable (1-365)
            return Math.Clamp(targetDays, 1, 365);
        }
    }
}
