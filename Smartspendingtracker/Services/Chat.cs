using Smartspendingtracker.Models;
using System.Text.RegularExpressions;

namespace SmartSpendingTracker.Services
{
    /// <summary>
    /// Service for parsing natural language chat input into expense data
    /// Supports Arabic and English
    /// Uses regex-based pattern matching
    /// </summary>
    public class ChatParsingService
    {
        private readonly ILogger<ChatParsingService> _logger;

        public ChatParsingService(ILogger<ChatParsingService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Parsed result from chat input
        /// </summary>
        public class ParsedExpense
        {
            public decimal Amount { get; set; }
            public Currency Currency { get; set; }
            public string? DetectedCategory { get; set; }
            public string OriginalText { get; set; } = string.Empty;
            public bool IsValid { get; set; }
            public string? ErrorMessage { get; set; }
        }

        /// <summary>
        /// Parse natural language text into expense data
        /// Examples:
        ///   "5 ريال أكل" -> Amount=5, Currency=SAR, Category=أكل
        ///   "10 sar food" -> Amount=10, Currency=SAR, Category=food
        ///   "100 bills" -> Amount=100, Currency=EGP (default), Category=bills
        /// </summary>
        public async Task<ParsedExpense> ParseAsync(string input)
        {
            var result = new ParsedExpense
            {
                OriginalText = input?.Trim() ?? string.Empty
            };

            if (string.IsNullOrWhiteSpace(input))
            {
                result.ErrorMessage = "Input cannot be empty";
                return result;
            }

            input = input.Trim();
            _logger.LogInformation("Parsing input: {Input}", input);

            try
            {
                // Step 1: Extract amount (required)
                var amount = ExtractAmount(input);
                if (amount == null || amount <= 0)
                {
                    result.ErrorMessage = "Could not detect a valid amount";
                    return result;
                }
                result.Amount = amount.Value;

                // Step 2: Extract currency (optional, defaults to EGP)
                result.Currency = ExtractCurrency(input);

                // Step 3: Extract category keywords (optional)
                result.DetectedCategory = ExtractCategoryKeyword(input);

                result.IsValid = true;
                _logger.LogInformation("Parsed successfully: Amount={Amount}, Currency={Currency}, Category={Category}",
                    result.Amount, result.Currency, result.DetectedCategory ?? "Not detected");

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing input: {Input}", input);
                result.ErrorMessage = "Failed to parse input";
                return result;
            }
        }

        /// <summary>
        /// Extract numeric amount from text
        /// Supports: "5", "10.5", "٥", "١٠.٥" (Arabic numerals)
        /// </summary>
        private decimal? ExtractAmount(string input)
        {
            // Convert Arabic numerals to Western numerals
            input = ConvertArabicNumerals(input);

            // Pattern: match decimal numbers (including optional decimal point)
            var patterns = new[]
            {
                @"\b(\d+\.?\d*)\b",  // Matches: 5, 10.5, 100
                @"(\d+\.?\d*)"       // Fallback: any number
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(input, pattern);
                if (match.Success && decimal.TryParse(match.Groups[1].Value, out var amount))
                {
                    return amount;
                }
            }

            return null;
        }

        /// <summary>
        /// Extract currency from text
        /// Detects: ريال, SAR (Saudi Riyal) and جنيه, EGP (Egyptian Pound)
        /// Defaults to EGP if not found
        /// </summary>
        private Currency ExtractCurrency(string input)
        {
            var inputLower = input.ToLowerInvariant();

            // Check for SAR indicators
            var sarPatterns = new[] { "ريال", "sar", "riyal", "sr" };
            if (sarPatterns.Any(p => inputLower.Contains(p)))
            {
                return Currency.SAR;
            }

            // Check for EGP indicators
            var egpPatterns = new[] { "جنيه", "جنية", "egp", "pound", "le", "egyptian" };
            if (egpPatterns.Any(p => inputLower.Contains(p)))
            {
                return Currency.EGP;
            }

            // Default to EGP
            return Currency.EGP;
        }

        /// <summary>
        /// Extract category keyword from text
        /// Returns the detected keyword (not the category ID)
        /// The calling service will match this to a category
        /// </summary>
        private string? ExtractCategoryKeyword(string input)
        {
            var inputLower = input.ToLowerInvariant();

            // Define category keyword groups
            // These match the keywords in Category seed data
            var categoryKeywords = new Dictionary<string, string[]>
            {
                ["food"] = new[] { "أكل", "مطعم", "قهوة", "فطار", "غدا", "عشا", "طعام", "food", "restaurant", "coffee", "lunch", "dinner", "breakfast" },
                ["transport"] = new[] { "مواصلات", "بنزين", "تاكسي", "سيارة", "transport", "taxi", "gas", "car", "fuel", "uber" },
                ["bills"] = new[] { "فواتير", "كهرباء", "ماء", "نت", "انترنت", "bills", "electricity", "water", "internet", "utilities" },
                ["rent"] = new[] { "إيجار", "ايجار", "سكن", "rent", "housing" },
                ["shopping"] = new[] { "تسوق", "ملابس", "شراء", "shopping", "clothes", "purchase", "buy" },
            };

            // Search for keywords in input
            foreach (var group in categoryKeywords)
            {
                foreach (var keyword in group.Value)
                {
                    // Use word boundary or exact match to avoid false positives
                    if (inputLower.Contains(keyword.ToLowerInvariant()))
                    {
                        _logger.LogInformation("Detected category keyword: {Keyword} -> {Group}", keyword, group.Key);
                        return keyword; // Return the actual keyword found
                    }
                }
            }

            return null; // No category detected
        }

        /// <summary>
        /// Convert Arabic numerals (٠-٩) to Western numerals (0-9)
        /// </summary>
        private string ConvertArabicNumerals(string input)
        {
            var arabicNumerals = new Dictionary<char, char>
            {
                ['٠'] = '0',
                ['١'] = '1',
                ['٢'] = '2',
                ['٣'] = '3',
                ['٤'] = '4',
                ['٥'] = '5',
                ['٦'] = '6',
                ['٧'] = '7',
                ['٨'] = '8',
                ['٩'] = '9',
                ['٫'] = '.'  // Arabic decimal separator
            };

            var result = input.ToCharArray();
            for (int i = 0; i < result.Length; i++)
            {
                if (arabicNumerals.TryGetValue(result[i], out var westernNumeral))
                {
                    result[i] = westernNumeral;
                }
            }

            return new string(result);
        }

        /// <summary>
        /// Generate a friendly confirmation message in the appropriate language
        /// </summary>
        public string GenerateConfirmationMessage(decimal amount, Currency currency, string categoryName, bool isArabic)
        {
            var currencySymbol = currency == Currency.SAR ? "ريال" : "جنيه";
            var currencyCode = currency == Currency.SAR ? "SAR" : "EGP";

            if (isArabic)
            {
                return $"✔ تم تسجيل {amount} {currencySymbol} في فئة {categoryName}";
            }
            else
            {
                return $"✔ Added {amount} {currencyCode} to {categoryName} category";
            }
        }

        /// <summary>
        /// Detect if the input is primarily in Arabic
        /// </summary>
        public bool IsArabicInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            var arabicChars = input.Count(c => c >= 0x0600 && c <= 0x06FF);
            var totalLetters = input.Count(char.IsLetter);

            return totalLetters > 0 && arabicChars > totalLetters / 2;
        }
    }
}