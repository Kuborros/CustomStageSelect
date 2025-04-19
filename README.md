# Custom Stage Select

![image](https://github.com/user-attachments/assets/a7997ecc-ade9-41e9-9db0-a05c56c0ecef)

## Features:

- A menu listing all the available custom stages in one place!
- Lightweight and simple.
- Does **not** come with custom stages by itself.

## Usage (for Players):

- Install this mod.
- Install mods that add custom maps.
- The Stage Select Tile is added next to Battlesphere and unlocked after completing it, both in Classic and Adventure mode.
- Pick a stage and play! :3

![DebugRomScanline](https://github.com/user-attachments/assets/39c2ffe4-a896-4dfb-b070-7b9a7e861ebc)

# Usage (for Modders):

- Register your custom stage in [FP2Lib](https://github.com/Kuborros/FP2Lib) and set ``showInCustomStageLoaders`` to ``true``.
- If you wish your stages to be only shown in specific scenarios, the easiest approach that also does not require you adding this mod as an Assembly Reference is checking the scene name - and if the just loaded scene is ``ZaosArcadeDebug`` run a prefix to ``FPStage.Start()`` which sets the ``showInCustomLoaders`` for your level depending on your need.

