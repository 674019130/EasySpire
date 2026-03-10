using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace EasySpire;

[ModInitializer(nameof(Initialize))]
public static class ModEntry
{
    public static Harmony? HarmonyInstance { get; private set; }

    public static void Initialize()
    {
        SettingsManager.Initialize();
        HarmonyInstance = new Harmony("com.susu.easyspire");
        HarmonyInstance.PatchAll();
    }
}
