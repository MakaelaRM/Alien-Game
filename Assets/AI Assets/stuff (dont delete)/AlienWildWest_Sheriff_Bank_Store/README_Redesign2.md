# Alien Wild West: New Sheriff, Bank and General Store

Three rebuilt buildings in the detailed style. They keep their old file names (`Bldg_Sheriff`, `Bldg_Bank`, `Bldg_GeneralStore`), so they replace the old models wherever those are used.

| Model | Triangles | Size (W x H x D) | What it is |
|---|---|---|---|
| `Bldg_Sheriff` | 5,384 | 15.7 x 10.4 x 10.4 m | Same idea as before, rebuilt: purple false front with a giant crooked brass star badge, porch, a stone jail wing with an open cell behind teal laser bars, a red-and-blue siren, a wanted board (1, 3 and 5 eyes), and the escape-tunnel mound with its spoon. |
| `Bldg_Bank` | 6,312 | 10.4 x 11.2 x 9.6 m | Same idea as before, rebuilt: squat strongbox with inward-sloping planked walls, riveted metal bands and corner straps, a giant round vault door with a big dial and the tiny pet flap, money bags, a teller window, and a stack of giant coins on the roof topped by a three-eyed coin. |
| `Bldg_GeneralStore` | 6,488 | 10.5 x 14.7 x 12.4 m | New design: a giant opened tin of space beans. The peeled lid stands up behind as the roof sign, glowing beans fill the top, and a wooden storefront with a striped awning is built into the front, with fruit crates, a pickle barrel, the vending machine in a cowboy hat, and a pyramid of normal-sized cans. |

All three are bigger than the old versions (old sheriff 9.8 x 6.5 x 6.0 m, old bank 7.0 x 5.8 x 7.7 m, old store 9.8 x 5.3 x 8.2 m), so check the gaps to their neighbours. The sheriff's jail wing is on its +X side and the wanted board on its -X side.

## Install (copy files, do not replace the whole folder)

| File in this zip | Goes into your project at |
|---|---|
| `AlienWildWest/Models/Buildings/*.fbx` (3 files) | `AlienWildWest/Models/Buildings/` (replace the old ones) |
| `AlienWildWest/Editor/AlienWildWestSetup.cs` | `AlienWildWest/Editor/` (replace: adds one new color) |
| `AlienWildWest/Scripts/PartMotion.cs` | `AlienWildWest/Scripts/` (only if you don't have it yet) |
| `AlienWildWest/Previews/Buildings/*.png` | `AlienWildWest/Previews/Buildings/` (optional) |

Copy the files in one at a time. **Don't drag this whole `AlienWildWest` folder over your current one**: on a Mac, Finder's "Replace" deletes the other models.

New material: `Mat_RoofDark` (#4E3626), the dark ribs on the sheriff's roofs. The updated setup script creates it by itself after import. If anything shows up white or grey, choose **Tools > Alien Wild West > Set Up Materials** once.

The general store's old vending machines and neon sign parts no longer exist as separate parts. The sheriff and bank keep similar part names, but they are new models, so anything you attached to the old parts needs checking.

## Moving parts

| Model | Part | Pivot |
|---|---|---|
| Sheriff | `Door` | hinge edge of the front door |
| | `Star` | centre of the badge |
| | `Laser_Bars` | bottom middle of the cell bars (switch them off to open the cell) |
| | `Posters` | base of the wanted board |
| Bank | `Vault_Door` | its hinge on the left, so it swings open |
| | `Dial` | centre of the dial |
| | `Pet_Flap` | top edge of the flap |
| | `Coin` | centre of the three-eyed coin on the roof |
| General Store | `Door` | hinge edge of the front door |
| | `Lid` | the lid's hinge at the back of the can |

Suggested **Part Motion** settings (Add Component > Part Motion on the part):

| Part | Motion | Axis | Amount | Period |
|---|---|---|---|---|
| `Bldg_Sheriff_Star` | Swing | 0, 0, 1 | 6 | 3 |
| `Bldg_Bank_Coin` | Spin | 0, 1, 0 | 60 | (not used) |
| `Bldg_Bank_Dial` | Swing | 0, 0, 1 | 90 | 4 |
| `Bldg_Bank_Pet_Flap` | Swing | 1, 0, 0 | 20 | 1.5 |
| `Bldg_GeneralStore_Lid` | Swing | 1, 0, 0 | 4 | 3 |

## Not yet tested in Unity

All three FBX files were read back and compared with their source models (triangle counts, normals, materials, part names and pivots all match), and each was rendered from 24 directions with back faces hidden to check for holes. `OBJ_Fallback/Buildings/` holds OBJ copies in case an FBX gives trouble.
