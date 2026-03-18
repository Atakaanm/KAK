# KAK — KaçAtaKaç

> A fast-paced 2.5D mobile arcade game built with Unity.

---

## Gameplay

You control a character trapped in an arena. Enemy spawners in each corner continuously fire projectiles at you. Dodge everything, survive as long as possible, and beat your high score.

**Controls:** Joystick (mobile) / WASD or Arrow keys (editor)

---

## Features

- 🎮 **8-directional movement & animation** — smooth sprite-based character animation
- 👾 **4 corner spawners** — directional idle + attack animations based on player angle
- ❤️ **Health system** — hit flash, invincibility frames, heart UI
- 🔥 **Endless difficulty** — auto-scaling difficulty via `DifficultyManager` (speed & fire rate)
- 📊 **Score system** — real-time score, persistent best score via `PlayerPrefs`
- 📱 **Mobile-ready** — adaptive camera, screen-orientation aware arena layout
- 🧩 **ScriptableObject architecture** — add new characters, arenas, projectiles and levels without writing code

---

## Project Structure

```
Assets/
├── Scripts/
│   ├── Data/              # ScriptableObject definitions
│   │   ├── PlayerData.cs
│   │   ├── SpawnerData.cs
│   │   ├── ProjectileData.cs
│   │   ├── ArenaData.cs
│   │   ├── DifficultyStageData.cs
│   │   └── LevelData.cs
│   ├── Managers/          # Runtime systems
│   │   ├── LevelManager.cs
│   │   └── DifficultyManager.cs
│   ├── Editor/
│   │   └── KacAtaKacSetup.cs  # One-click project setup tool
│   ├── PlayerMovement2D.cs
│   ├── PlayerHealth.cs
│   ├── PlayerDirectionSprite.cs
│   ├── CornerShoother.cs
│   ├── SpawnerAimAndVisual.cs
│   ├── Projectile.cs
│   ├── ArenaAutoLayout.cs
│   ├── CameraFitWidth.cs
│   ├── GameManager.cs
│   ├── ScoreManager.cs
│   └── HealthUI.cs
├── Sprites/
│   ├── Player/            # 8-directional player sprites
│   ├── Spawner/           # 8-directional spawner idle + attack frames
│   ├── Projectiles/
│   ├── Arena/
│   └── UI/
└── Prefabs/
```

---

## Setup (After Cloning)

1. Open project in **Unity 2022+**
2. In the top menu: **KacAtaKac → Spawner Sprite'larını Yeniden Adlandır**
3. Then: **KacAtaKac → Setup - Tüm Default Dataları Oluştur**
4. All default `ScriptableObject` assets will be generated in `Assets/Data/`
5. Assign sprites and prefab references in each asset via Inspector

---

## Adding a New Character

1. Import sprites to `Assets/Sprites/Player/YourChar/`
2. Right-click in Project → **Create → KacAtaKac → Player Data**
3. Fill in stats (speed, health) and drag sprites into the 8 direction slots
4. Done — set this `PlayerData` as the `currentLevel`'s player in `LevelData`

---

## Roadmap

- [ ] Mobile touch joystick
- [ ] Main menu scene
- [ ] Character selection screen
- [ ] Store / unlock system
- [ ] New arena themes
- [ ] New projectile types
- [ ] Google Play / App Store release

---

## Built With

- [Unity](https://unity.com/) — Game Engine
- C# — Scripting
- TextMeshPro — UI Text

---

*Work in progress — not yet released.*
