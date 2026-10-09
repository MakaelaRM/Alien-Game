# Alien Wild West: Alien Characters (final set)

All 13 final alien models: the player, Blip Jr, and the 12 Gumdrop townsfolk. This replaces every earlier character zip.

## Player

| Model | Look | Height |
|---|---|---|
| `Char_Player_BlipJr` | one-eyed green alien (#5CCB3E) in a white spacesuit with red trim, round bubble helmet, amber antenna tip, twin air tanks | 1.82 m |

## Gumdrops (native townsfolk)

| Model | Eyes | Antennae | Colour | Look |
|---|---|---|---|---|
| `Char_Native_Sheriff` | 2 | 2 | coral | white hat, vest, star badge, two holsters |
| `Char_Native_Bartender` | 3 | 1 | butter yellow | derby, bow tie, vest, apron |
| `Char_Native_Outlaw` | 4 | 0 | mint | black hat, neckerchief, bandolier, holster |
| `Char_Native_Shopkeeper` | 2 (big and small) | 3 | peach | apron with a pencil, sleeve garters |
| `Char_Native_Prospector` | 5 | 2 | apricot | battered hat, overalls, backpack with pan and pickaxe |
| `Char_Native_Banker` | 3 | 2 | lilac | green teller's visor, coat, bow tie, watch chain |
| `Char_Native_Kid` | 1 | 2 | tomato red | small, hat pushed back, glowing toy on a stick |
| `Char_Native_Elder` | 2 | 1 | sand | spectacles, cardigan, walking cane |
| `Char_Native_Poncho` | 1 | 3 | sage | striped fringed poncho, battered hat |
| `Char_Native_Dandy` | 4 | 2 | plum | long coat, glowing bolo tie, black hat with a feather |
| `Char_Native_Farmer` | 3 | 0 | lime | straw hat, denim vest, glowing alien vegetable |
| `Char_Native_Townie` | 2 | 1 | honey | leather vest, blue neckerchief, battered hat |

Gumdrops are 1.27 to 1.69 m tall with their hats.

## Parts

Every model faces +Z with its feet at the origin. Each one is split into `Body`, `Head`, `Arm_L`, `Arm_R`, `Leg_L` and `Leg_R`, with pivots at the neck, shoulders and hips. Held items (cane, toy, vegetable) belong to the arm that holds them. The models aren't rigged yet.

## Install

1. Copy everything in `AlienWildWest/Models/Characters/` into your project's `AlienWildWest/Models/Characters/` folder. Delete any older character models there first (the earlier option sets and Blip Jr colourways).
2. Replace `AlienWildWest/Editor/AlienWildWestSetup.cs` with the one in this zip. It holds every colour in the pack, including all the skin colours.
3. If anything shows up white or grey, choose **Tools > Alien Wild West > Set Up Materials** once.

Each FBX was read back and checked against its source model, and each was rendered from 24 directions with back faces hidden. `OBJ_Fallback/Characters/` has OBJ copies of all 13 with one shared material file.
