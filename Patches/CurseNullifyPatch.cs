using HarmonyLib;

namespace EasySpire.Patches;

/// <summary>
/// Nullifies curse and harmful status cards (Burn, Wound, Dazed, etc.)
/// by preventing their negative effects from triggering.
/// </summary>
[HarmonyPatch]
internal static class CurseNullifyPatch
{
    // Known harmful status/curse card type names
    private static readonly HashSet<string> HarmfulCards =
    [
        "Burn", "Wound", "Dazed", "Slimed", "Void", "Doubt", "Shame",
        "Regret", "Decay", "Pain", "Parasite", "Normality", "Injury",
        "Clumsy", "CurseOfTheBell", "Necronomicurse", "Writhe", "AscendersBane"
    ];

    [HarmonyTargetMethods]
    static IEnumerable<System.Reflection.MethodBase> TargetMethods()
    {
        // Find card OnPlay and OnTurnEndInHand methods for harmful cards
        var modelsNamespace = "MegaCrit.Sts2.Core.Models.Cards";

        foreach (var cardName in HarmfulCards)
        {
            var cardType = AccessTools.TypeByName($"{modelsNamespace}.{cardName}");
            if (cardType == null) continue;

            foreach (var method in cardType.GetMethods())
            {
                if (method.Name is "OnPlay" or "OnTurnEndInHand" or "OnTurnEndInDraw"
                    or "OnTurnEndInDiscard" or "OnDraw" or "OnExhaust"
                    && method.DeclaringType == cardType)
                {
                    yield return method;
                }
            }
        }

        // Also try to block the Afflict command that adds status cards to the deck
        var cardCmdType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Commands.CardCmd");
        if (cardCmdType != null)
        {
            foreach (var method in cardCmdType.GetMethods())
            {
                if (method.Name == "Afflict")
                    yield return method;
            }
        }
    }

    [HarmonyPrefix]
    static bool Prefix(System.Reflection.MethodBase __originalMethod)
    {
        var settings = SettingsManager.Current;
        if (!settings.NullifyCurses.Enabled) return true; // execute original

        // If it's an Afflict call, skip it entirely (don't add curses)
        if (__originalMethod.Name == "Afflict")
            return false; // skip original

        // For OnPlay/OnTurnEnd of curse cards, skip the negative effect
        return false; // skip original
    }
}
