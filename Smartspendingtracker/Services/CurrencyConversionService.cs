using Microsoft.EntityFrameworkCore;
using Smartspendingtracker;
using Smartspendingtracker.Models;

namespace SmartSpendingTracker.Services
{
    /// <summary>
    /// Service for handling currency conversion with historical exchange rates
    /// </summary>
    public class CurrencyConversionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CurrencyConversionService> _logger;

        // UTC+3 timezone offset
        private const int TIME_ZONE_OFFSET_HOURS = 3;

        public CurrencyConversionService(
            ApplicationDbContext context,
            ILogger<CurrencyConversionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get current date/time in UTC+3
        /// </summary>
        public DateTime GetCurrentDateTime()
        {
            return DateTime.UtcNow.AddHours(TIME_ZONE_OFFSET_HOURS);
        }

        /// <summary>
        /// Convert amount to EGP using the exchange rate for the given month
        /// </summary>
        public async Task<(decimal convertedAmount, decimal? exchangeRate)> ConvertToEGPAsync(
            decimal amount,
            Currency fromCurrency,
            int year,
            int month)
        {
            // If already in EGP, no conversion needed
            if (fromCurrency == Currency.EGP)
            {
                return (amount, null);
            }

            // Get exchange rate for the specified month
            var rate = await GetExchangeRateAsync(fromCurrency, Currency.EGP, year, month);

            if (rate == null)
            {
                _logger.LogWarning(
                    "No exchange rate found for {From} to {To} for {Year}-{Month}. Using default rate.",
                    fromCurrency, Currency.EGP, year, month);

                // Use a default rate as fallback (should be configured)
                rate = GetDefaultRate(fromCurrency);
            }

            var converted = amount * rate.Value;
            return (converted, rate);
        }

        /// <summary>
        /// Get exchange rate for a specific month
        /// </summary>
        public async Task<decimal?> GetExchangeRateAsync(
            Currency fromCurrency,
            Currency toCurrency,
            int year,
            int month)
        {
            var exchangeRate = await _context.ExchangeRates
                .Where(er => er.FromCurrency == fromCurrency
                    && er.ToCurrency == toCurrency
                    && er.Year == year
                    && er.Month == month)
                .OrderByDescending(er => er.UpdatedAt)
                .FirstOrDefaultAsync();

            return exchangeRate?.Rate;
        }

        /// <summary>
        /// Set exchange rate for a specific month
        /// </summary>
        public async Task<bool> SetExchangeRateAsync(
            Currency fromCurrency,
            Currency toCurrency,
            int year,
            int month,
            decimal rate)
        {
            try
            {
                var existingRate = await _context.ExchangeRates
                    .FirstOrDefaultAsync(er => er.FromCurrency == fromCurrency
                        && er.ToCurrency == toCurrency
                        && er.Year == year
                        && er.Month == month);

                if (existingRate != null)
                {
                    // Update existing rate
                    existingRate.Rate = rate;
                    existingRate.UpdatedAt = GetCurrentDateTime();
                }
                else
                {
                    // Create new rate
                    var newRate = new ExchangeRate
                    {
                        FromCurrency = fromCurrency,
                        ToCurrency = toCurrency,
                        Rate = rate,
                        Year = year,
                        Month = month,
                        UpdatedAt = GetCurrentDateTime()
                    };
                    _context.ExchangeRates.Add(newRate);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting exchange rate");
                return false;
            }
        }

        /// <summary>
        /// Get default exchange rate if no rate is configured
        /// This is a fallback mechanism
        /// </summary>
        private decimal GetDefaultRate(Currency fromCurrency)
        {
            return fromCurrency switch
            {
                Currency.SAR => 13.5m, // 1 SAR = 13.5 EGP (example)
                _ => 1.0m
            };
        }

        /// <summary>
        /// Get all exchange rates for a specific month
        /// </summary>
        public async Task<List<ExchangeRate>> GetAllRatesForMonthAsync(int year, int month)
        {
            return await _context.ExchangeRates
                .Where(er => er.Year == year && er.Month == month)
                .OrderBy(er => er.FromCurrency)
                .ToListAsync();
        }

        /// <summary>
        /// Initialize exchange rates for current month if they don't exist
        /// </summary>
        public async Task EnsureCurrentMonthRatesExistAsync()
        {
            var now = GetCurrentDateTime();
            var year = now.Year;
            var month = now.Month;

            var existingRates = await GetAllRatesForMonthAsync(year, month);

            // Check if SAR to EGP rate exists
            if (!existingRates.Any(r => r.FromCurrency == Currency.SAR && r.ToCurrency == Currency.EGP))
            {
                await SetExchangeRateAsync(Currency.SAR, Currency.EGP, year, month, 13.5m);
                _logger.LogInformation("Created default SAR to EGP rate for {Year}-{Month}", year, month);
            }
        }
    }
}