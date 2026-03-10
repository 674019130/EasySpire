# Easy Spire - 轻松爬塔

A configurable difficulty reduction mod for **Slay the Spire 2**.

可配置的杀戮尖塔 2 降难度 mod，适合休闲玩家和联机游玩。

## Features / 功能

| Feature | Default | Description |
|---------|---------|-------------|
| Player HP Boost | ON, ×1.5 | 玩家初始血量提升 |
| Enemy Damage Reduce | ON, ×0.5 | 敌人伤害减半 |
| Enemy HP Reduce | ON, ×0.5 | 敌人血量减半 |
| Post-Combat Heal | ON, 30% | 战斗后回复 30% 最大生命 |
| Nullify Curses | ON | 诅咒/状态卡无效化 |
| Extra Energy | OFF, +1 | 每回合额外能量 |
| Gold Boost | ON, ×2.0 | 金币奖励翻倍 |
| Shop Discount | ON, ×0.5 | 商店半价 |
| Death Revive | ON, 50% | 死亡时复活，恢复 50% 血量（每场战斗限一次） |
| More Card Choices | OFF, +2 | 战斗奖励增加可选卡牌数（开发中） |
| Rest Site Boost | ON, ×2.0 | 营火休息回血量翻倍 |

All features can be toggled on/off and values adjusted in-game or via config file.

所有功能都可以在游戏内或配置文件中开关和调节数值。

## Installation / 安装

1. Download `EasySpire.dll` and `mod_manifest.json` from [Releases](../../releases/latest)
2. Find your game directory:
   - **Windows**: `C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\`
   - **Mac**: `~/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/`
   - Or: Steam → Right-click game → Manage → Browse local files
3. Create `mods/EasySpire/` folder inside the game directory
4. Place both files in that folder
5. Launch the game — the mod loads automatically

```
Slay the Spire 2/
└── mods/
    └── EasySpire/
        ├── EasySpire.dll
        └── mod_manifest.json
```

## Configuration / 配置

On first launch, `EasySpire.config.json` is auto-generated in the mod folder. Edit it to customize:

```json
{
  "playerHpBoost":     { "enabled": true,  "value": 1.5  },
  "enemyDamageReduce": { "enabled": true,  "value": 0.5  },
  "enemyHpReduce":     { "enabled": true,  "value": 0.5  },
  "postCombatHeal":    { "enabled": true,  "value": 0.3  },
  "nullifyCurses":     { "enabled": true,  "value": 1.0  },
  "extraEnergy":       { "enabled": false, "value": 1.0  },
  "goldBoost":         { "enabled": true,  "value": 2.0  },
  "shopDiscount":      { "enabled": true,  "value": 0.5  },
  "deathRevive":       { "enabled": true,  "value": 0.5  },
  "moreCardChoices":   { "enabled": false, "value": 2.0  },
  "restSiteBoost":     { "enabled": true,  "value": 2.0  }
}
```

Changes are hot-reloaded — no restart needed.

## Troubleshooting / 排查问题

The mod writes a log file to `mods/EasySpire/EasySpire.log` with:
- All patched methods listed on startup
- Every feature activation with before/after values
- Error details with stack traces

If something isn't working, check the log or [open an issue](../../issues) with the log contents.

如果 mod 不工作，查看 `mods/EasySpire/EasySpire.log` 日志文件，或在 GitHub Issues 中附上日志内容。

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
