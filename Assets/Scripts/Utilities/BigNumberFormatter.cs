// BigNumberFormatter.cs
using System;
using System.Globalization;

public static class BigNumberFormatter
{
    // K=1e3, M=1e6, B=1e9, T=1e12, Qa=1e15, Qi=1e18
    private static readonly string[] s_suffixes = { "", "K", "M", "B", "T", "Qa", "Qi" };

    public static string Format(long value, int maxSignificantDigits = 3, bool useThousandSeparatorsUnderK = true)
        => Format((double)value, maxSignificantDigits, useThousandSeparatorsUnderK);

    public static string Format(double value, int maxSignificantDigits = 3, bool useThousandSeparatorsUnderK = true)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            return "—";

        bool negative = value < 0;
        double abs = Math.Abs(value);

        // < 1000: optionally use thousand separators
        if (abs < 1000d)
        {
            string small = useThousandSeparatorsUnderK
                ? abs.ToString("#,0", CultureInfo.InvariantCulture)
                : abs.ToString("0", CultureInfo.InvariantCulture);
            return (negative ? "-" : "") + small;
        }

        // group 1=>K, 2=>M, ...
        int group = (int)Math.Floor(Math.Log(abs, 1000d));
        group = Math.Min(group, s_suffixes.Length - 1);

        double scaled = abs / Math.Pow(1000d, group);

        // integer digit count of scaled
        int intDigits = (int)Math.Floor(scaled) >= 1
            ? ((int)Math.Floor(Math.Log10(Math.Floor(scaled))) + 1)
            : 1;
        intDigits = Math.Max(intDigits, 1);

        int decimals = Math.Max(0, maxSignificantDigits - intDigits);

        // tiny bias to avoid 999.999 -> 1000 with rounding
        double rounded = Math.Round(scaled - 1e-12, decimals, MidpointRounding.AwayFromZero);

        // carry to next suffix if needed
        if (rounded >= 1000d && group < s_suffixes.Length - 1)
        {
            group++;
            rounded = rounded / 1000d;

            intDigits = (int)Math.Floor(rounded) >= 1 
                ? ((int)Math.Floor(Math.Log10(Math.Floor(rounded))) + 1)
                : 1;
            decimals = Math.Max(0, maxSignificantDigits - intDigits);
            rounded = Math.Round(rounded, decimals, MidpointRounding.AwayFromZero);
        }

        string format = decimals > 0 ? "0." + new string('0', decimals) : "0";
        string text = rounded.ToString(format, CultureInfo.InvariantCulture);

        return (negative ? "-" : "") + text + s_suffixes[group];
    }
}
