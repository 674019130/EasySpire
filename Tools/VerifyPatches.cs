// Standalone verification script
// Run with: dotnet script Tools/VerifyPatches.cs
// Or build as a console app to check all patch targets exist in sts2.dll

using System.Reflection;

// Path to the game DLL
var dllPath = args.Length > 0
    ? args[0]
    : Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "Library/Application Support/Steam/steamapps/common/Slay the Spire 2",
        "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.dll");

if (!File.Exists(dllPath))
{
    Console.WriteLine($"[ERROR] sts2.dll not found at: {dllPath}");
    Console.WriteLine("Usage: dotnet run -- <path-to-sts2.dll>");
    return 1;
}

Console.WriteLine($"Loading: {dllPath}\n");

var asm = Assembly.LoadFrom(dllPath);
var allTypes = asm.GetTypes();

int passed = 0, failed = 0, warnings = 0;

void Check(string description, string typeName, string? memberName = null, MemberTypes memberType = MemberTypes.Method)
{
    var type = allTypes.FirstOrDefault(t => t.FullName == typeName);
    if (type == null)
    {
        Console.WriteLine($"  [FAIL] {description}");
        Console.WriteLine($"         Type not found: {typeName}");
        failed++;
        return;
    }

    if (memberName == null)
    {
        Console.WriteLine($"  [OK]   {description}  ->  {typeName}");
        passed++;
        return;
    }

    var members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic |
                                   BindingFlags.Instance | BindingFlags.Static)
        .Where(m => m.Name == memberName && (m.MemberType & memberType) != 0)
        .ToArray();

    if (members.Length > 0)
    {
        var info = memberType == MemberTypes.Method
            ? $"({members.Length} overload{(members.Length > 1 ? "s" : "")})"
            : "";
        Console.WriteLine($"  [OK]   {description}  ->  {typeName}.{memberName} {info}");
        passed++;
    }
    else
    {
        Console.WriteLine($"  [FAIL] {description}");
        Console.WriteLine($"         Member '{memberName}' not found on {typeName}");
        // List available members with similar names
        var similar = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic |
                                       BindingFlags.Instance | BindingFlags.Static)
            .Where(m => m.Name.Contains(memberName, StringComparison.OrdinalIgnoreCase)
                        || memberName.Contains(m.Name, StringComparison.OrdinalIgnoreCase))
            .Select(m => m.Name)
            .Distinct()
            .Take(5);
        var similarList = string.Join(", ", similar);
        if (!string.IsNullOrEmpty(similarList))
            Console.WriteLine($"         Similar: {similarList}");
        failed++;
    }
}

void Warn(string description, string typeName, string memberName)
{
    var type = allTypes.FirstOrDefault(t => t.FullName == typeName);
    if (type == null)
    {
        Console.WriteLine($"  [WARN] {description} - type not found, patch will be skipped gracefully");
        warnings++;
        return;
    }
    var members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic |
                                   BindingFlags.Instance | BindingFlags.Static)
        .Where(m => m.Name == memberName)
        .ToArray();
    if (members.Length > 0)
    {
        Console.WriteLine($"  [OK]   {description}  ->  {typeName}.{memberName}");
        passed++;
    }
    else
    {
        Console.WriteLine($"  [WARN] {description} - member not found, patch will be skipped gracefully");
        warnings++;
    }
}

// ========================================
Console.WriteLine("=== 1. PlayerHpPatch ===");
Check("Player class exists",
    "MegaCrit.Sts2.Core.Entities.Players.Player");
Check("Player.CreateForNewRun method",
    "MegaCrit.Sts2.Core.Entities.Players.Player", "CreateForNewRun");

// Check HP properties on Creature (base class)
Console.WriteLine("\n=== HP Properties (on Creature base class) ===");
Check("Creature class exists",
    "MegaCrit.Sts2.Core.Entities.Creatures.Creature");
Check("Creature.MaxHp property",
    "MegaCrit.Sts2.Core.Entities.Creatures.Creature", "MaxHp", MemberTypes.Property);
Check("Creature.CurrentHp property",
    "MegaCrit.Sts2.Core.Entities.Creatures.Creature", "CurrentHp", MemberTypes.Property);
// Also check if Player inherits these
var playerType = allTypes.FirstOrDefault(t => t.FullName == "MegaCrit.Sts2.Core.Entities.Players.Player");
if (playerType != null)
{
    var maxHp = playerType.GetProperty("MaxHp", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    var currentHp = playerType.GetProperty("CurrentHp", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    Console.WriteLine($"  [INFO] Player inherits MaxHp: {maxHp != null} (declaring type: {maxHp?.DeclaringType?.Name ?? "N/A"})");
    Console.WriteLine($"  [INFO] Player inherits CurrentHp: {currentHp != null} (declaring type: {currentHp?.DeclaringType?.Name ?? "N/A"})");
    if (maxHp != null) Console.WriteLine($"  [INFO] MaxHp can write: {maxHp.CanWrite}, type: {maxHp.PropertyType.Name}");
    if (currentHp != null) Console.WriteLine($"  [INFO] CurrentHp can write: {currentHp.CanWrite}, type: {currentHp.PropertyType.Name}");
}

// ========================================
Console.WriteLine("\n=== 2. EnemyDamagePatch ===");
Check("CreatureCmd class exists",
    "MegaCrit.Sts2.Core.Commands.CreatureCmd");
Check("CreatureCmd.Damage method",
    "MegaCrit.Sts2.Core.Commands.CreatureCmd", "Damage");

// ========================================
Console.WriteLine("\n=== 3. EnemyHpPatch ===");
Check("CreatureCmd.SetMaxHp method",
    "MegaCrit.Sts2.Core.Commands.CreatureCmd", "SetMaxHp");

// ========================================
Console.WriteLine("\n=== 4. PostCombatHealPatch ===");
Check("CombatManager class exists",
    "MegaCrit.Sts2.Core.Combat.CombatManager");
// Check possible victory method names
var cmType = allTypes.FirstOrDefault(t => t.FullName == "MegaCrit.Sts2.Core.Combat.CombatManager");
if (cmType != null)
{
    var victoryMethods = cmType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                            BindingFlags.Instance | BindingFlags.Static)
        .Where(m => m.Name.Contains("Victory") || m.Name.Contains("End") || m.Name.Contains("Win"))
        .Select(m => m.Name)
        .Distinct()
        .ToArray();
    Console.WriteLine($"  [INFO] Victory/End related methods: {string.Join(", ", victoryMethods)}");
}

// ========================================
Console.WriteLine("\n=== 5. CurseNullifyPatch ===");
Check("CardCmd class exists",
    "MegaCrit.Sts2.Core.Commands.CardCmd");
Warn("CardCmd.Afflict method",
    "MegaCrit.Sts2.Core.Commands.CardCmd", "Afflict");
// Check for some curse card types
foreach (var cardName in new[] { "Burn", "Wound", "Dazed", "Slimed", "Void" })
{
    Warn($"Curse card: {cardName}",
        $"MegaCrit.Sts2.Core.Models.Cards.{cardName}", "OnPlay");
}

// ========================================
Console.WriteLine("\n=== 6. EnergyPatch ===");
Check("PlayerCmd class exists",
    "MegaCrit.Sts2.Core.Commands.PlayerCmd");
Check("PlayerCmd.SetEnergy method",
    "MegaCrit.Sts2.Core.Commands.PlayerCmd", "SetEnergy");

// ========================================
Console.WriteLine("\n=== 7. GoldBoostPatch ===");
Check("PlayerCmd.GainGold method",
    "MegaCrit.Sts2.Core.Commands.PlayerCmd", "GainGold");

// ========================================
Console.WriteLine("\n=== 8. ShopDiscountPatch ===");
foreach (var merchant in new[] { "MerchantCardEntry", "MerchantRelicEntry",
    "MerchantPotionEntry", "MerchantCardRemovalEntry", "MerchantEntry" })
{
    var fullName = $"MegaCrit.Sts2.Core.Entities.Merchant.{merchant}";
    var mType = allTypes.FirstOrDefault(t => t.FullName == fullName);
    if (mType != null)
    {
        var priceMethods = mType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic |
                                             BindingFlags.Instance | BindingFlags.Static)
            .Where(m => m.Name.Contains("Price") || m.Name.Contains("Cost") || m.Name.Contains("Gold"))
            .Select(m => $"{m.Name}({m.MemberType})")
            .Distinct();
        Console.WriteLine($"  [INFO] {merchant}: {string.Join(", ", priceMethods)}");
    }
    else
    {
        Console.WriteLine($"  [WARN] {merchant} type not found");
        warnings++;
    }
}

// ========================================
Console.WriteLine("\n=== 9. DeathRevivePatch ===");
Check("Hook class exists",
    "MegaCrit.Sts2.Core.Hooks.Hook");
Check("Hook.BeforeDeath method",
    "MegaCrit.Sts2.Core.Hooks.Hook", "BeforeDeath");

// ========================================
Console.WriteLine("\n=== 10. RestSiteBoostPatch ===");
Check("HealRestSiteOption class exists",
    "MegaCrit.Sts2.Core.Entities.RestSite.HealRestSiteOption");
Check("HealRestSiteOption.ExecuteRestSiteHeal method",
    "MegaCrit.Sts2.Core.Entities.RestSite.HealRestSiteOption", "ExecuteRestSiteHeal");

// ========================================
Console.WriteLine("\n=== 11. InGameSettingsUi ===");
Check("NModInfoContainer class exists",
    "MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen.NModInfoContainer");
Check("NModInfoContainer.Fill method",
    "MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen.NModInfoContainer", "Fill");
Check("Mod class exists",
    "MegaCrit.Sts2.Core.Modding.Mod");

// ========================================
Console.WriteLine("\n=== 12. ModEntry ===");
Check("ModInitializerAttribute exists",
    "MegaCrit.Sts2.Core.Modding.ModInitializerAttribute");

// ========================================
Console.WriteLine("\n=== Run Manager (for healing patches) ===");
var rmType = allTypes.FirstOrDefault(t => t.FullName == "MegaCrit.Sts2.Core.Runs.RunManager");
if (rmType != null)
{
    Console.WriteLine($"  [OK]   RunManager found");
    var props = rmType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic |
                                      BindingFlags.Instance | BindingFlags.Static)
        .Select(p => p.Name)
        .ToArray();
    Console.WriteLine($"  [INFO] RunManager properties: {string.Join(", ", props)}");
    passed++;
}
else
{
    Console.WriteLine($"  [FAIL] RunManager not found");
    failed++;
}

// ========================================
Console.WriteLine("\n" + new string('=', 50));
Console.WriteLine($"Results: {passed} passed, {failed} failed, {warnings} warnings");
Console.WriteLine(new string('=', 50));

if (failed > 0)
    Console.WriteLine("\n[!] Some patches target non-existent methods and WILL NOT WORK.");
if (warnings > 0)
    Console.WriteLine("[i] Warnings indicate optional patches that will be skipped gracefully.");
if (failed == 0)
    Console.WriteLine("\n[*] All critical patches verified! Mod should work correctly.");

return failed;
