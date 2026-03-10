using System.Text.Json.Serialization;

namespace EasySpire;

public sealed record FeatureToggle
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; init; } = true;

    [JsonPropertyName("value")]
    public double Value { get; init; } = 1.0;
}

public sealed record EasySpireSettings
{
    [JsonPropertyName("playerHpBoost")]
    public FeatureToggle PlayerHpBoost { get; init; } = new() { Enabled = true, Value = 1.5 };

    [JsonPropertyName("enemyDamageReduce")]
    public FeatureToggle EnemyDamageReduce { get; init; } = new() { Enabled = true, Value = 0.7 };

    [JsonPropertyName("enemyHpReduce")]
    public FeatureToggle EnemyHpReduce { get; init; } = new() { Enabled = true, Value = 0.75 };

    [JsonPropertyName("postCombatHeal")]
    public FeatureToggle PostCombatHeal { get; init; } = new() { Enabled = true, Value = 0.15 };

    [JsonPropertyName("nullifyCurses")]
    public FeatureToggle NullifyCurses { get; init; } = new() { Enabled = true, Value = 1.0 };

    [JsonPropertyName("extraEnergy")]
    public FeatureToggle ExtraEnergy { get; init; } = new() { Enabled = false, Value = 1.0 };

    [JsonPropertyName("goldBoost")]
    public FeatureToggle GoldBoost { get; init; } = new() { Enabled = true, Value = 2.0 };

    [JsonPropertyName("shopDiscount")]
    public FeatureToggle ShopDiscount { get; init; } = new() { Enabled = true, Value = 0.5 };

    [JsonPropertyName("deathRevive")]
    public FeatureToggle DeathRevive { get; init; } = new() { Enabled = true, Value = 0.5 };

    [JsonPropertyName("moreCardChoices")]
    public FeatureToggle MoreCardChoices { get; init; } = new() { Enabled = true, Value = 2.0 };

    [JsonPropertyName("restSiteBoost")]
    public FeatureToggle RestSiteBoost { get; init; } = new() { Enabled = true, Value = 1.5 };

    public static EasySpireSettings Defaults { get; } = new();
}
