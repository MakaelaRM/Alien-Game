# Alien Wild West: Town Hall

An add-on for the Alien Wild West asset pack. Adobe town hall with a clock tower, a 13-hour glyph clock on three sides, an open belfry with a swinging bell, and a flying-saucer weathervane.

- **Bldg_TownHall.fbx**: 11,236 triangles, about 13.7 m wide, 11.5 m deep and 23.2 m tall (to the top of the weathervane)
- Same palette and lighting as the rest of the pack, plus 2 new colors
- An optional clock script that runs the clock and rings the bell on the hour

This is the detailed building style (like the saloon concept), so it uses far more triangles than the original buildings, which were held to 2,500. On a PC game this is fine for a building you only have one of.

## Install (copy files, do not replace the whole folder)

Copy each file into your existing `AlienWildWest` folder in Unity. **Do not drag this whole `AlienWildWest` folder over your current one**: on a Mac, Finder's "Replace" deletes everything in the old folder, including the other 118 models.

| File in this zip | Goes into your project at |
|---|---|
| `AlienWildWest/Models/Buildings/Bldg_TownHall.fbx` | `AlienWildWest/Models/Buildings/` |
| `AlienWildWest/Editor/AlienWildWestSetup.cs` | `AlienWildWest/Editor/` (replace the old one) |
| `AlienWildWest/Scripts/TownHallClock.cs` | `AlienWildWest/Scripts/` (new folder) |
| `AlienWildWest/Previews/Buildings/Bldg_TownHall.png` | `AlienWildWest/Previews/Buildings/` (optional) |

After Unity finishes importing, the updated setup script notices the 2 new colors and runs by itself. If the town hall still shows white or grey patches, choose **Tools > Alien Wild West > Set Up Materials** once.

New materials it creates:

| Material | Color | Used for |
|---|---|---|
| `Mat_AdobeDark` | #C99A58 | plinth, lower wall band, tower base, front steps |
| `Mat_PaintedWoodDark` | #56329A | darker purple slats on the shutters and doors |

Then drag `Models/Buildings/Bldg_TownHall` into the scene in place of your temporary town hall.

## Parts

The model is split into parts so you can animate them. Each pivot sits where the part turns.

| Part | Pivot |
|---|---|
| `Bldg_TownHall_Body` | ground, centre of the building |
| `Bldg_TownHall_Door_L`, `Bldg_TownHall_Door_R` | hinge edge of each front door, so they swing open |
| `Bldg_TownHall_Bell` | the bell's axle, so it swings |
| `Bldg_TownHall_Weathervane` | base of the pole, so it turns |
| `Bldg_TownHall_Clock_Hour_*`, `_Minute_*`, `_Third_*` | centre of each clock face (Front, Right, Left) |

## Making the clock run (optional)

1. Select the town hall in your scene and choose **Add Component > Town Hall Clock**.
2. Press Play. The hands move, the bell swings once for each hour struck (13 times at the thirteenth hour), and the weathervane wobbles in the wind.

Settings on the component:

- **Hours**: the starting time, 0 to 13.
- **Game Minutes Per Second**: 1 means one game hour passes per real minute.
- **Third Hand Runs Backwards**: the thin teal hand spins backwards once per game minute. Untick it for a normal seconds hand.
- **Bell Audio / Bell Clip**: optional. Add an Audio Source and a bell sound to hear it ring.
- **On Hour Struck**: an event you can hook up in the Inspector, for example to spawn enemies at the thirteenth hour.

Other scripts can call `SetTime(hours)` to jump the clock and `RingBell(times)` to ring it.

## Not yet tested in Unity

Like the rest of the pack, this was built and checked outside Unity. The FBX was read back and compared with the source model (triangle counts, normals, materials, part names and pivots all match), and the model was rendered from 24 directions with back faces hidden to check for holes. The clock script has not been compiled in Unity. If it shows an error in the Console, delete `TownHallClock.cs` and send me the error message; the model works without it.

`OBJ_Fallback/Buildings/` holds an OBJ copy of the model with its material file, in case the FBX gives trouble.
