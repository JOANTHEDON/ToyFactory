# Toy Factory update (2026-10-22)

## First thing to do in the editor
The editor was open while these files were written, so:

1. **Do not save GameScene from the editor before reloading it.** If Unity asks whether to reload the
   scene from disk, choose **Reload**. If it does not ask, open `Assets/Scenes/StartMenu.unity` then
   re-open `Assets/Scenes/GameScene.unity` (discard changes if prompted).
2. Wait for scripts to compile (no errors expected; verified with MSBuild and a batch-mode Unity run).
3. Press Play from **StartMenu** (it is now scene 0 in Build Settings) or directly from GameScene.

## What changed

### Recipe Details modal (`RecipeDetailsUI`, `ToyRecipeDatabase`)
- Recipes are no longer guessed from the toy's name. They are built from the real recipe assets on the
  counters in the scene (cutting -> melting -> molding/shaping -> painting -> delivery), so every toy
  shows its true steps, timings, MELT hit count and the correct icon per stage.
- The modal is rebuilt in code: toy icon + title + subtitle, scrollable numbered step cards with the
  station chip and key hint, "GOT IT" button, dim backdrop, ESC / tap-outside to close.
  The game is paused while it is open.
- The old `StepTemplate` / `StepsContainer` objects are hidden at runtime (safe to delete).

### Customers at the delivery window (`OrderManager`, `CustomerManager`, `Customer`, `CustomerOrderBubbleUI`)
- Orders now arrive one at a time (3 s, then every 9-16 s, max 4) with a patience timer (110 s).
- Each order spawns a customer who walks from `CustomerManager/SpawnPoint` to a queue slot outside the
  delivery window and shows a bubble with the toy, name, queue number and a patience bar.
- The top-left order bar shows the same list in the same order with `#n` badges and timer bars.
- Delivering a painted toy completes the **earliest order for that toy, wherever it is in the line**;
  that customer thanks you and leaves, the rest shuffle forward. Expired orders leave angry and count
  as "Missed". Score = 100 + speed bonus; best score is saved.
- Tweak counts/timers on the `OrderManager` object; tweak queue positions under `CustomerManager`.

### Factory walls and layout (`FactoryWalls` object)
- Cube walls around the floor with colliders (back/left/right full height, front wall low so the
  camera can see over it), a serving gap in the right wall where the Delivery Counter sits, a sidewalk
  and curb outside for the customer line. Materials: `_Assets/Materials/FactoryWall*.mat`, `Sidewalk.mat`.
- Counters were re-laid out as production lines:
  - Back wall: Melter, Metal / Plastic / Wood containers, Table
  - Left wall: Mold Maker, Melter (1), Wood Cutter
  - Front wall: Wood Shaper, Paint Station, Trash
  - Middle island: two Tables; Delivery Counter in the right wall gap.
  Positions are plain transforms, move them freely.

### Player interaction
- Floating prompt above the counter you face: `[E] Pick up Wood`, `[F] Melt (3/10)`, `Molding...`.
  Text comes from new `GetInteractHint` / `GetInteractAlternateHint` / `GetBusyHint` overrides on each
  counter, and the mobile buttons use the same logic (only shown when the action does something).
- Delivery feedback popups and SFX (pickup, drop, trash, melt hit, success, fail, customer lost).
- Pause menu (ESC or the II button): Resume / Settings / Restart / Main Menu.
- Toys can no longer be yanked out of the Wood Cutter or Paint Station mid-process.

### Start Menu scene (`Assets/Scenes/StartMenu.unity`, `StartMenuUI`)
- Logo, PLAY, HOW TO PLAY, SETTINGS, QUIT, best score. Settings (shared with the pause menu):
  music volume, SFX volume, touch controls Auto/On/Off, graphics quality. Saved in PlayerPrefs.
- Music (`_Assets/Sounds/Music/Music.wav`) persists across scenes via `MusicManager`.

### New scripts
`Scripts/Core`: GameSettings, GamePause, Loader, SoundManager, MusicManager
`Scripts/UI`: UIFactory, UIResources, ButtonScaleFeedback, InteractionPromptUI, GameHUD, PauseMenuUI,
SettingsPanelUI, StartMenuUI
`Scripts/Customers`: CustomerManager, Customer, CustomerOrderBubbleUI

`UIResources` (on each Canvas) holds the sprites and fonts the code-built UI uses; swap them there to
restyle everything at once. Notes from testing: the ornate cream panel sprite only looks right on panels
of at least ~180 px, so smaller boxes (order cards, score box) use a flat outlined style, and buttons are
drawn flat (gold face on a dark base) because the button sprites in `Assets/Sprites` have baked-in text.

## Verified
Compiled with MSBuild, then run in a batch-mode copy of the project: both scenes load with no missing
scripts; orders spawn, customers queue and leave, out-of-order delivery removes the right order, unpainted
toys are refused, walls block the player, recipe modal / pause menu pause and resume the game, Start Menu
loads and music persists. Screenshots from that run are in
`C:\Users\saadn\AppData\Local\Temp\kc_verify\shots`.
