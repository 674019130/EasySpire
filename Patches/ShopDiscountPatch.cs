using HarmonyLib;

namespace EasySpire.Patches;

/// <summary>
/// Reduces shop prices by applying a discount multiplier.
/// </summary>
[HarmonyPatch]
internal static class ShopDiscountPatch
{
    [HarmonyTargetMethods]
    static IEnumerable<System.Reflection.MethodBase> TargetMethods()
    {
        // Target merchant entry price methods
        var merchantEntryTypes = new[]
        {
            "MegaCrit.Sts2.Core.Entities.Merchant.MerchantCardEntry",
            "MegaCrit.Sts2.Core.Entities.Merchant.MerchantRelicEntry",
            "MegaCrit.Sts2.Core.Entities.Merchant.MerchantPotionEntry",
            "MegaCrit.Sts2.Core.Entities.Merchant.MerchantCardRemovalEntry",
            "MegaCrit.Sts2.Core.Entities.Merchant.MerchantEntry",
        };

        foreach (var typeName in merchantEntryTypes)
        {
            var type = AccessTools.TypeByName(typeName);
            if (type == null) continue;

            // Look for price-related properties or methods
            foreach (var method in type.GetMethods())
            {
                if (method.Name is "get_Cost" or "GetCost" or "CalcCost"
                    && method.DeclaringType == type)
                {
                    yield return method;
                }
            }
        }
    }

    [HarmonyPostfix]
    static void Postfix(ref int __result)
    {
        var settings = SettingsManager.Current;
        if (!settings.ShopDiscount.Enabled) return;

        var original = __result;
        __result = Math.Max(1, (int)(__result * settings.ShopDiscount.Value));
        Logger.LogPatchResult("ShopDiscount", true, $"Price {original} -> {__result}");
    }
}
