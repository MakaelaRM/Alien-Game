# Alien Wild West: Medic, Space Suit Shop, Library and "Water" Fountain

Four more models for the Alien Wild West pack, in the same detailed style as the town hall, the final palette and the Deep Purple lighting.

| Model | Triangles | Size (W x H x D) | What makes it silly |
|---|---|---|---|
| `Bldg_Medic` | 6,368 | 17.0 x 10.0 x 11.5 m (the clinic is 8 x 7 m; the rest is the ramp, gurney and clothesline) | Giant glowing pill on the roof, bandaged cactus, an eyeball floating in a specimen jar, bandages on the washing line |
| `Bldg_SuitShop` | 8,948 | 10.0 x 10.4 x 12.7 m | Giant fishbowl helmet wearing a cowboy hat, airlock front door, a suit on a turntable waving at passers-by |
| `Bldg_Library` | 9,484 | 11.7 x 12.3 x 13.2 m | The roof is a giant open book, with a bookworm in glasses and a tiny hat chewing out through the cover, and a telescope on the back |
| `Bldg_Fountain` | 4,686 | 8.5 x 6.1 x 8.5 m | Glowing green "water", frog-head spouts, a "don't drink" skull sign by the tin cup, and a statue of a saucer beaming up a cow |

Like the town hall, these use far more triangles than the original buildings (held to 2,500). That's fine on PC for buildings you only have one of each.

## Install (copy files, do not replace the whole folder)

Copy each file into your existing `AlienWildWest` folder in Unity. **Do not drag this whole `AlienWildWest` folder over your current one**: on a Mac, Finder's "Replace" deletes everything in the old folder.

| File in this zip | Goes into your project at |
|---|---|
| `AlienWildWest/Models/Buildings/*.fbx` (4 files) | `AlienWildWest/Models/Buildings/` |
| `AlienWildWest/Editor/AlienWildWestSetup.cs` | `AlienWildWest/Editor/` (replace the old one) |
| `AlienWildWest/Scripts/PartMotion.cs` | `AlienWildWest/Scripts/` (next to `TownHallClock.cs`, if you have it) |
| `AlienWildWest/Previews/Buildings/*.png` | `AlienWildWest/Previews/Buildings/` (optional) |

After Unity finishes importing, the updated setup script notices the new colors and runs by itself. If anything shows up white or grey, choose **Tools > Alien Wild West > Set Up Materials** once.

New materials it creates:

| Material | Color | Used for |
|---|---|---|
| `Mat_WoodMid` | #9A7650 | lighter boards on porches, the boardwalk and the library walls |
| `Mat_AlienWater` | #7EF24A, glowing | the fountain's pool and streams |

`Mat_AlienWater` is its own material, separate from the slime, so you can turn its glow down or swap in a water shader without changing the slime anywhere else. If you skipped the town hall, the script also creates `Mat_AdobeDark` and `Mat_PaintedWoodDark`, which these models use too.

## Moving parts

Each model is split into parts. The pivot of each part sits where it turns.

| Model | Parts (besides the Body) | Pivot |
|---|---|---|
| Medic | `Door_L`, `Door_R` | hinge edge of each front door |
| | `Sign` | top of the hanging sign, so it swings |
| | `Eyeball` | centre of the eye in the jar |
| | `Gurney` | ground under the hover gurney |
| Space Suit Shop | `Door` | hinge edge of the airlock door |
| | `Turntable` | centre of the turntable, under the waving suit |
| | `Fan` | centre of the vent fan on the left side |
| Library | `Door_L`, `Door_R` | hinge edge of each front door |
| | `Bookworm` | where the worm comes out of the cover |
| | `Telescope` | top of its mount |
| Fountain | `Liquid` | building origin; the pool and bowl surfaces with foam rings |
| | `Streams` | building origin; all the arcs and drips of "water" |
| | `Saucer` | centre of the saucer |
| | `Cow` | centre of the cow |

`Liquid` and `Streams` are separate so you can give them their own shader, or switch `Streams` off and use particle effects instead.

## Making the parts move (optional)

`PartMotion.cs` is a small component that loops one motion. Select a part in your scene (for example `Bldg_Fountain_Saucer`), choose **Add Component > Part Motion**, set it as below, and press Play.

| Part | Motion | Axis | Amount | Period |
|---|---|---|---|---|
| `Bldg_Fountain_Saucer` | Spin | 0, 1, 0 | 30 | (not used) |
| `Bldg_Fountain_Cow` | Bob | 0, 1, 0 | 0.15 | 3 |
| `Bldg_SuitShop_Turntable` | Spin | 0, 1, 0 | 20 | (not used) |
| `Bldg_SuitShop_Fan` | Spin | 1, 0, 0 | 360 | (not used) |
| `Bldg_Medic_Eyeball` | Look Around | (not used) | 40 | 1.5 |
| `Bldg_Medic_Sign` | Swing | 1, 0, 0 | 6 | 2.5 |
| `Bldg_Medic_Gurney` | Bob | 0, 1, 0 | 0.05 | 2 |
| `Bldg_Library_Bookworm` | Swing | 0, 0, 1 | 8 | 2 |
| `Bldg_Library_Telescope` | Swing | 0, 1, 0 | 35 | 8 |

**Amount** means degrees per second for Spin, the largest angle for Swing and Look Around, and metres for Bob. The saucer is tilted, so spinning it gives it a wobble. **Phase** offsets the timing so two copies of a model don't move in step.

## Not yet tested in Unity

Like the rest of the pack, these were built and checked outside Unity. Each FBX was read back and compared with its source model (triangle counts, normals, materials, part names and pivots all match), and each model was rendered from 24 directions with back faces hidden to check for holes. The scripts have not been compiled in Unity. If `PartMotion.cs` shows an error in the Console, delete it and send me the message; the models work without it.

`OBJ_Fallback/Buildings/` holds OBJ copies of all four models with their material file, in case an FBX gives trouble.
