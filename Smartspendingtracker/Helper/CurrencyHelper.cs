using Smartspendingtracker.Models;
using SpendingTracker.Models;

namespace SpendingTracker.Helpers
{
    public static class CurrencyHelper
    {
        public static string FormatAmount(decimal amountInSAR, UserSettings settings)
        {
            if (settings == null || settings.PreferredCurrency == "SAR")
            {
                return $"{amountInSAR:N2} ريال";
            }
            else // EGP
            {
                decimal amountInEGP = amountInSAR * settings.ExchangeRate;
                return $"{amountInEGP:N2} ج.م";
            }
        }

        public static string GetCurrencySymbol(UserSettings settings)
        {
            if (settings == null || settings.PreferredCurrency == "SAR")
            {
                return "ريال";
            }
            else
            {
                return "ج.م";
            }
        }

        public static decimal ConvertToPreferred(decimal amountInSAR, UserSettings settings)
        {
            if (settings == null || settings.PreferredCurrency == "SAR")
            {
                return amountInSAR;
            }
            else
            {
                return amountInSAR * settings.ExchangeRate;
            }
        }
    }
}