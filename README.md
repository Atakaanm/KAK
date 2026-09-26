# KAK — Kaç Ata Kaç

> A fast-paced 2.5D pixel-art arcade dodger for mobile (portrait), built with Unity 6 + URP.

---

## Gameplay

You are trapped in a dungeon arena. Rock throwers in the corners keep hurling rocks at you. Dodge, dash through danger, skim rocks for bonus points and survive as long as you can.

**Controls:** floating joystick + dash button (mobile) · WASD / arrow keys + Space (editor)

## Features

- **Endless Mode** — 6 difficulty stages, 7 rock types (straight, pebble, boulder, bouncing, splitting, homing, meteor with ground warning) and random events every 30–45 s (rock rain, crossfire, calm, rolling boulder)
- **Skill rewards** — near-miss bonus, combo multiplier, dash with i-frames ("super dodge")
- **Fair collision** — separate foot (walls) and body (rocks) colliders sized to the visible sprite
- **Power-ups** — heart, shield (bubble), speed, slow-mo, ghost; active ones shown with timer rings
- **First-run hints** — shown once, never to experienced players
- **Screen composition** — HUD band on top, arena scaled to screen width, control corridor at the bottom (safe-area aware)
- **Juice** — hit-stop, screen shake, particles, squash & stretch, procedural 8-bit SFX and music
- **TR / EN** localization, JSON save system
- **ScriptableObject data** — levels, arenas, characters, rocks, stages, power-ups, world themes

## Project Structure

```
Assets/
├── Scripts/
│   ├── Core/        # GameEvents, KakTime, SaveSystem, Loc, KakPalette, bootstrap
│   ├── Data/        # ScriptableObjects (LevelData, PlayerData, ProjectileData, WorldTheme, ...)
│   ├── Managers/    # LevelManager, DifficultyManager, EndlessEventManager, SceneLoader, ...
│   ├── Gameplay/    # PlayerHitbox, PlayerDash, NearMissTracker, ShieldBubble, PlayerStatus, enemies
│   ├── Levels/      # level mode (goals, stars, world catalog) — in progress
│   ├── World/       # ScreenComposer, DungeonFrame, feedback, camera shake
│   ├── UI/          # HUD, menus, PowerupHud, OnboardingHints, joystick, dash button
│   ├── Dev/         # test bot, perf probe, auto bench (editor / development builds only)
│   └── Editor/      # repeatable setup tools (menu "KacAtaKac"), scene audit, build
├── Editor/ClaudeBridge/  # editor automation bridge (tools/kak_bridge.py)
├── Tests/           # PlayMode + EditMode tests
├── Art/ Sprites/ Audio/ Fonts/ Prefabs/ Data/ Scenes/ (MainMenu, Game)
tools/               # Python helpers: bridge client, palette, art pass, audio & UI generators
Docs/                # store listing, privacy policy, release checklist
```

## Setup

1. Open the project with **Unity 6000.3.8f1**.
2. Open `Assets/Scenes/MainMenu.unity` and press Play. Scenes are already configured.
3. Setup tools are idempotent and can be re-run from the **KacAtaKac** menu (scene managers, screen composition, UI, endless content, fonts, performance settings, player settings).

## Development

- Tests: Unity Test Runner (PlayMode + EditMode). The scene audit test fails on missing scripts, broken references or unexpected empty fields.
- Performance: `KacAtaKac → Yayın → macOS Development Build`, then run the app with `-kakbench 40 out.txt [-kakuncapped]` for an automatic bot-played benchmark with a render breakdown.
- Plan, status and conventions: [`.claude/skills/kacatakac/`](.claude/skills/kacatakac/) (`SKILL.md`, `progress.md`, `denetim.md`, `roadmap.md`, `sanat-rehberi.md`).

## Roadmap

- [x] Endless Mode core, content and juice
- [x] Menus, settings, TR/EN, audio
- [x] Audit & fixes (fair collision, performance, onboarding)
- [ ] Engagement & meta progression: coins, characters with stats, pets, progressive unlocks
- [ ] Level worlds: Ice, Football, Dark
- [ ] Google Play / App Store release

---

*Work in progress — not yet released.*
