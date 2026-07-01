namespace SpendSmart2.Models.Helpers
{
    public static class CurrencyHelper
    {
        public static string FormatCurrency(decimal amount)
        {
            if (amount >= 1000000000)
                return $"₦{amount / 1000000000m:0.#}B";

            if (amount >= 1000000)
                return $"₦{amount / 1000000m:0.#}M";

            if (amount >= 1000)
                return $"₦{amount / 1000m:0.#}K";

            return $"₦{amount:N2}";
        }
    }
}
