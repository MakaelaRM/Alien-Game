# Alien Wild West: General Store (new version)

A classic two-story western mercantile in the same detailed style as the new sheriff and bank. It replaces the tin-can version and keeps the file name `Bldg_GeneralStore`.

- **7,940 triangles**, 12.3 x 10.7 x 12.2 m (W x H x D), including the loading dock on its +X side
- Sage-green false front with bone trim, a brass-framed glyph name board, and the original planet-and-basket neon sign on top
- Deep covered boardwalk with two big lit display windows of goods, fruit crates, sacks, barrels, a bench and the vending machine in a cowboy hat
- Solar panels on the roof, a side loading dock with a hoist beam and a hanging sack

## Install

Copy `AlienWildWest/Models/Buildings/Bldg_GeneralStore.fbx` into `AlienWildWest/Models/Buildings/`, replacing the tin-can version. The preview image is optional.

`AlienWildWest/Editor/AlienWildWestSetup.cs` is the same as in the sheriff and bank delivery; you only need it if you skipped that one. No other new materials. If anything shows up white or grey, choose **Tools > Alien Wild West > Set Up Materials** once.

## Moving parts

| Part | Pivot | Suggested Part Motion |
|---|---|---|
| `Bldg_GeneralStore_Door` | hinge edge of the front door | (open it from your own script) |
| `Bldg_GeneralStore_Sign` | base of the neon sign | Spin, axis 0, 1, 0, amount 30 |
| `Bldg_GeneralStore_Hook` | where the rope meets the hoist beam | Swing, axis 1, 0, 0, amount 5, period 3 |

## Not yet tested in Unity

The FBX was read back and compared with the source model (triangle counts, normals, materials, part names and pivots all match), and the model was rendered from 24 directions with back faces hidden. `OBJ_Fallback/Buildings/` has an OBJ copy.
