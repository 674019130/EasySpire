# Easy Spire - 轻松爬塔

A configurable difficulty reduction mod for **Slay the Spire 2**.

可配置的杀戮尖塔 2 降难度 mod，适合休闲玩家和联机游玩。

## Features / 功能

| Feature | Default | Description |
|---------|---------|-------------|
| Player HP Boost | ON, ×1.5 | 玩家初始血量提升 |
| Enemy Damage Reduce | ON, ×0.7 | 敌人伤害降低 |
| Enemy HP Reduce | ON, ×0.75 | 敌人血量降低 |
| Post-Combat Heal | ON, 15% | 战斗后自动回血 |
| Nullify Curses | ON | 诅咒/状态卡无效化 |
| Extra Energy | OFF, +1 | 每回合额外能量 |
| Gold Boost | ON, ×2.0 | 金币奖励翻倍 |
| Shop Discount | ON, ×0.5 | 商店半价 |

All features can be toggled on/off and values adjusted in-game or via config file.

所有功能都可以在游戏内或配置文件中开关和调节数值。

## Installation / 安装

1. Download `EasySpire.dll` and `mod_manifest.json` from [Releases](../../releases)
2. Create folder: `<Game Directory>/mods/EasySpire/`
3. Place both files in that folder
4. Launch the game → Settings → Mods → Enable **Easy Spire**

```
Steam\steamapps\common\Slay the Spire 2\mods\EasySpire\
├── EasySpire.dll
└── mod_manifest.json
```

## Configuration / 配置

On first launch, `EasySpire.config.json` is auto-generated in the mod folder. Edit it to customize:

```json
{
  "playerHpBoost":     { "enabled": true,  "value": 1.5  },
  "enemyDamageReduce": { "enabled": true,  "value": 0.7  },
  "enemyHpReduce":     { "enabled": true,  "value": 0.75 },
  "postCombatHeal":    { "enabled": true,  "value": 0.15 },
  "nullifyCurses":     { "enabled": true,  "value": 1.0  },
  "extraEnergy":       { "enabled": false, "value": 1.0  },
  "goldBoost":         { "enabled": true,  "value": 2.0  },
  "shopDiscount":      { "enabled": true,  "value": 0.5  }
}
```

Changes are hot-reloaded — no restart needed.

## Co-op / 联机

Both players need to install the mod with the same config for best results.

## Build from Source / 从源码构建

Requirements:
- [.NET 9+ SDK](https://dotnet.microsoft.com/download)
- Slay the Spire 2 installed via Steam

```bash
cp local.props.example local.props
# Edit local.props with your game path
dotnet build
```

Output: `bin/Debug/net9.0/EasySpire.dll`

## Tech Stack

- **Engine**: Godot 4.5 + C# (.NET 9)
- **Patching**: [HarmonyLib](https://github.com/pardeike/Harmony) runtime method hooking
- **Config**: System.Text.Json with hot-reload

## License

MIT
