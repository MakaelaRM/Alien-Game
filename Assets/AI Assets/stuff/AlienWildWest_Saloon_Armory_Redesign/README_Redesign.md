# Alien Wild West: New Saloon and Weapon Store

Two brand-new designs that replace the old ones. They keep the old file names, `Bldg_Saloon` and `Bldg_Armory`, so they drop straight into the places the old ones were.

| Model | Triangles | Size (W x H x D) | The idea |
|---|---|---|---|
| `Bldg_Saloon` | 5,130 | 16.8 x 14.4 x 9.2 m | The whole saloon is a giant cowboy boot. Batwing doors in the side of the foot, glowing stitching, a chrome toe cap, a spinning spur on the heel, and a rooftop bar deck inside the boot top under a neon sign with an eyeball-olive cocktail. |
| `Bldg_Armory` | 6,178 | 17.8 x 11.9 x 7.0 m | The weapon store is a giant upright laser cartridge with a glowing energy band and a vault door, plus a fortified wing with a walk-up gun counter, a spinning crossed-blasters sign, and a target range in the side yard. |

**These are bigger than the old versions** (the old saloon was 10.5 x 9.2 x 8.6 m and the old armory 9.7 x 8.4 x 7.5 m), so check the gaps to the buildings next to them. The boot's toe points along +X, and the armory's target range sits on its +X side.

## Install (copy files, do not replace the whole folder)

| File in this zip | Goes into your project at |
|---|---|
| `AlienWildWest/Models/Buildings/Bldg_Saloon.fbx`, `Bldg_Armory.fbx` | `AlienWildWest/Models/Buildings/` (replace the old ones) |
| `AlienWildWest/Previews/Buildings/*.png` | `AlienWildWest/Previews/Buildings/` (optional, replace) |
| `AlienWildWest/Editor/AlienWildWestSetup.cs` | `AlienWildWest/Editor/` (same as the last delivery; replace if you skipped it) |
| `AlienWildWest/Scripts/PartMotion.cs` | `AlienWildWest/Scripts/` (same as the last delivery; only needed if you don't have it) |

Copy the files in one at a time. **Don't drag this whole `AlienWildWest` folder over your current one**: on a Mac, Finder's "Replace" deletes the other models.

Replacing the FBX files updates every copy already in your scenes. If something shows up white or grey, choose **Tools > Alien Wild West > Set Up Materials** once. No new materials are needed.

The parts have changed. The old saloon's mug, liquid and sign, and the old armory's revolver, gun racks and double doors, no longer exist. Anything you attached to those parts in a scene (scripts, colliders) will need moving to the new parts.

## Moving parts

| Model | Part | Pivot |
|---|---|---|
| Saloon | `Door_L`, `Door_R` | hinge edge of each batwing door |
| | `Spur` | centre of the spur's rowel (the star wheel) |
| Weapon Store | `Door` | hinge edge of the vault door |
| | `Sign` | top of its pole, so the crossed blasters spin |
| | `Targets` | middle of the range, on the ground (bullseyes, bandit cutout, barrel and tin cans together) |

Suggested **Part Motion** settings (Add Component > Part Motion on the part):

| Part | Motion | Axis | Amount | Period |
|---|---|---|---|---|
| `Bldg_Saloon_Spur` | Spin | 0, 0, 1 | 90 | (not used) |
| `Bldg_Armory_Sign` | Spin | 0, 1, 0 | 45 | (not used) |

## Not yet tested in Unity

Both FBX files were read back and compared with their source models (triangle counts, normals, materials, part names and pivots all match), and each model was rendered from 24 directions with back faces hidden to check for holes. `OBJ_Fallback/Buildings/` holds OBJ copies in case an FBX gives trouble.
