using ToadCapture.Core.Models;

namespace ToadCapture.Core.Rules;

public static class ValidationRules
{
    public static bool IsValidWeight(double? weight) => weight is >= 5 and <= 5000;

    public static bool IsValidLength(double? length) => length is >= 3 and <= 40;

    public static bool IsPartnerValid(string? chipId, string? partnerChipId)
    {
        if (string.IsNullOrWhiteSpace(partnerChipId))
        {
            return true;
        }

        return !string.Equals(chipId, partnerChipId, StringComparison.OrdinalIgnoreCase);
    }

    public static bool RequiresMeasure(Individual individual)
    {
        if (individual.Sex == "f")
        {
            return true;
        }

        if (individual.Sex == "m")
        {
            return !individual.MaleMeasuredOnce;
        }

        return false;
    }
}
