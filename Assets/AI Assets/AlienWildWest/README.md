# Alien Wild West Asset Pack

Low-poly Wild West town on an alien planet, for Unity 6 with URP. Built 7 October 2026.

- **118 models** as FBX, 33,453 triangles in total, every one under its triangle budget
- **28 shared materials**, solid colors only
- **3 ground textures** and **1 sky panorama**
- A preview image of every model, and a setup script for Unity

Final look: Sundown Western colors, Deep Purple sky, teal lasers and crystals, royal purple paint.

## Read this first: not yet tested in Unity

These files were built and checked outside Unity. No copy of Unity was available, so **nothing here has been imported into a Unity project yet**.

What was checked:

- Every FBX file was read back and compared with its source model: triangle counts, normals, material assignments, part names and pivots all match.
- Every model was rendered from up to 24 directions with back faces hidden, the way Unity draws them, to confirm no surface shows a hole.
- Every model is under its triangle budget.

What was not checked: that Unity's importer accepts the files, the import scale, and that the setup script compiles. Test one model before relying on the rest (see Quick start). If something is off, the fixes are in Troubleshooting below.

## Quick start

1. Drag the whole `AlienWildWest` folder into your project's `Assets` folder and wait for the import to finish.
2. Drag `Models/Props/Prop_Barrel_Juice` into an empty scene. It should be about 1 m tall, upright, brown with a glowing green top.
3. The setup script runs by itself after the first import and fills the `Materials` folder. If the `Materials` folder holds only a README, choose **Tools > Alien Wild West > Set Up Materials**.
4. Choose **Tools > Alien Wild West > Apply Sky, Fog and Lights to Open Scene**.
5. Add the post-processing listed under Lighting below. Bloom is what makes the neon glow.

## Troubleshooting

| Problem | Fix |
| --- | --- |
| The FBX files do not import, or import empty | Use `AlienWildWest_OBJ_Fallback.zip`, delivered with this pack. Same models as OBJ files. Parts keep their names but lose their own pivots. |
| Models are 100 times too big or too small | Select the models, and in the Inspector's Model tab set Scale Factor to 0.01 or 100, then Apply. A door should be 2.4 m tall. |
| Red errors mention `AlienWildWestSetup.cs` | Delete `AlienWildWest/Editor`. The models still work with the materials Unity imports from each FBX file; you lose only the shared materials and the menu. |
| Models are white or grey | Run Tools > Alien Wild West > Set Up Materials. |
| Models are pink | The materials were made for a different render pipeline. Delete the `.mat` files in `Materials` and run Set Up Materials again. |
| Neon does not glow | Glow needs HDR on the URP Asset and a Bloom override on a Global Volume. |
| Sky looks blurry | Select `Textures/Tex_Sky_DeepPurple` and set Max Size to 4096. |

## Folder layout

```
AlienWildWest/
  Models/
    Buildings/    12   saloon, sheriff, store, bank, slime tower, armory, 5 homes, mine shaft
    Props/        30   street props and the ten extra props
    Vegetation/   20   cacti, shrubs, succulents, glowing plants, tumbleweeds
    Rocks/        27   crystals, rocks, arches, spires, craters, pools, bones, geysers, meteorites
    Terrain/       9   modular mountain wall kit and backdrop peaks
    Ships/         7   3 hero ships, each intact and crashed, plus the tractor beam
    Weapons/      13   8 blasters and 5 melee weapons
  Textures/            3 ground textures, 1 sky panorama
  Materials/           filled in by the setup script
  Editor/              AlienWildWestSetup.cs
  Previews/            one PNG per model, contact sheets, two town renders
  README.md
```

## Conventions

- **Scale:** 1 unit = 1 m. Buildings are sized for a 2 m tall character; doors are 2.4 m tall.
- **Facing:** the front of every building and prop faces Unity's forward axis, +Z.
- **Pivot:** at ground level under the model, where it was designed to sit. For mountain pieces the pivot is the center of the 20 m grid cell, so pieces snap together when placed 20 m apart.
- **Hovering models** such as the hover wagon and floating tumbleweeds keep their pivot on the ground, so they float at the right height when placed at ground level.
- **Weapons:** the pivot is at the grip. Blasters point along +Z with up along +Y. Melee weapons point up along +Y. `Wpn_Blaster_SixShooter` is the same model as `Prop_LaserRevolver`, set up as a held weapon.
- **Separate parts:** anything meant to move is its own child object, named `<Model>_<Part>`. Doors and gates pivot on their hinge edge, hanging things at their top, things that sit on a surface at their base, and everything else at its center.
- **Shading:** flat shaded. Normals are stored in the files, so leave Normals set to Import.
- **No UVs:** models use solid-color materials and carry no texture coordinates. To bake lightmaps, turn on Generate Lightmap UVs in the Model tab.
- **No colliders:** add your own. One Box Collider per mountain wall piece is enough to stop the player.
- **Mine shaft:** the tunnel is open and about 5 m deep, ending in a dark wall. Put your dungeon trigger there.
- **Crashed ships:** parts of the wreck sit below ground level on purpose, so place them on flat ground and let the terrain hide the buried part. Smoke is not modeled; use a particle effect.
- **Tractor beam:** `Prop_UFO_Beam` is 6 m tall. Raise `Ship_Saucer_Classic` so its base sits about 5 m up and stand the beam under it.

## Lighting

These are starting values taken from the preview renders, not tuned in Unity. The menu command applies the first table.

| Setting | Value |
| --- | --- |
| Skybox | Skybox/Panoramic material using `Tex_Sky_DeepPurple` |
| Fog | Linear, color #5A3A9A, start 70, end 260 |
| Ambient | Gradient: sky #544C6C, equator #3E354C, ground #281F2B |
| Sun | Directional, color #FFE0B4, intensity 1.0, rotation (35, 211, 0), soft shadows |
| Second sun | Directional, color #A07CFF, intensity 0.35, rotation (19, 59, 0), no shadows |

Set these by hand:

| Where | Setting | Value |
| --- | --- | --- |
| URP Asset | HDR | On |
| URP Asset | Anti Aliasing (MSAA) | 4x |
| Global Volume | Bloom | Threshold 1.0, Intensity 1.0, Scatter 0.7 |
| Global Volume | Tonemapping | Neutral |
| Global Volume | Color Adjustments | Post Exposure 0.2, Contrast 15, Saturation 15 |

## Ground and sky

| File | Size | Use |
| --- | --- | --- |
| Tex_Ground_Desert.png | 1024 x 1024 | Default ground. Tiles seamlessly; one tile covers 4 x 4 m. |
| Tex_Ground_Cracked.png | 1024 x 1024 | Dry cracked flats and crater surrounds. |
| Tex_Ground_DirtRoad.png | 1024 x 1024 | Town road with wheel ruts and hoof prints. Tiles along the road. |
| Tex_Sky_DeepPurple.png | 4096 x 2048 | Sky panorama with stars, three moons and cloud bands. |

On a Unity Terrain, add each ground texture as a Terrain Layer with Size 4 x 4.

## Materials

Every model draws from these 28 materials. After setup they live in `Materials/`, so changing one recolors the whole pack. Glow strength is the HDR emission multiplier; anything above 1 picks up bloom.

Five materials are named for what they color, because your final palette moved them away from their original color names: Mat_Cactus, Mat_Foliage, Mat_NeonLaser, Mat_NeonCrystal and Mat_PaintedWood.

| Material | Color | Glow | Opacity | Used for |
| --- | --- | --- | --- | --- |
| Mat_Sand | #E3B56F | none | 100% | Adobe walls, ground mounds, crater rims, mesa caps |
| Mat_Terracotta | #D2602F | none | 100% | Rock tops, strata bands |
| Mat_RustRock | #A6402B | none | 100% | Rock sides, strata bands |
| Mat_ShadowRock | #5A2E3A | none | 100% | Rock undersides, mountain bases |
| Mat_FadedWood | #B08A5E | none | 100% | Planks, porches, posts, barrels, gun stocks |
| Mat_DarkWood | #6B4A35 | none | 100% | Roofs, beams, twigs |
| Mat_PaintedWood | #6A3FB0 | none | 100% | Painted walls, doors, shutters, trim |
| Mat_Cactus | #5E9E4A | none | 100% | Cacti, succulents, mushroom roof, tentacle |
| Mat_Foliage | #9AA34F | none | 100% | Shrubs, plant stalks and leaves, poker felt |
| Mat_Bone | #F2E6CF | none | 100% | Skulls, bones, posters, canvas, foam |
| Mat_Metal | #8D99A6 | none | 100% | Straps, hoops, plates, frames |
| Mat_DarkMetal | #3A3F4B | none | 100% | Vault door, gun bodies, emitters, meteorites |
| Mat_Chrome | #D5DCE4 | none | 100% | Saucer hull, hover rings, pod roofs, blades |
| Mat_Brass | #D9A441 | none | 100% | Star badge, dial lock, gauges, trim |
| Mat_SolarBlue | #23406E | none | 100% | Solar panels |
| Mat_Glass | #BFE9F2 | none | 35% | Mug, display cases, domes, jars |
| Mat_Void | #221A2B | none | 100% | Interiors, holes, glyph paint |
| Mat_NeonLaser | #19D8C4 | 3 | 100% | Laser bars, fence rails, sign tubing, target rings |
| Mat_NeonGreen | #5CFF3B | 3 | 100% | Sign tubing, glyphs, plasma coils |
| Mat_NeonCyan | #2EF2FF | 3 | 100% | Hover pads, screens, bulbs, portholes |
| Mat_NeonCrystal | #3DE0D0 | 2.5 | 100% | Crystals, mushroom glow, meteorite cracks |
| Mat_NeonAmber | #FFB02E | 1.5 | 100% | Window light, lanterns, engine glow |
| Mat_SlimeGreen | #A6FF2E | 2 | 100% | Slime, juice |
| Mat_TractorBeam | #9BFFF0 | 1.5 | 40% | Saucer tractor beam only |
| Mat_HeroWhite | #F1ECE0 | none | 100% | Ship hulls, ray guns |
| Mat_HeroRed | #E2473B | none | 100% | Ship trim and fins, ray gun grips |
| Mat_HeroBlue | #2D6CDF | none | 100% | Disintegrator ray gun |
| Mat_HeroYellow | #FFC83D | none | 100% | Shuttle hull, ray gun tanks |

Mat_NeonLaser (#19D8C4), Mat_NeonCrystal (#3DE0D0) and Mat_NeonCyan (#2EF2FF) are close to each other by your choice. They are separate materials, so you can pull them apart later.

## Model list

Size is width x depth x height in meters. Budget is the triangle limit set for that kind of model.

### Buildings (12 models, 10,037 triangles)

| Model | Triangles | Budget | Size | Separate parts |
| --- | --- | --- | --- | --- |
| Bldg_Saloon | 914 | 2,500 | 10.5 x 8.62 x 9.22 | Door_L, Door_R, Sign, Mug, Liquid |
| Bldg_Sheriff | 548 | 2,500 | 9.8 x 6.04 x 6.49 | Door, Star, Laser_Bars, Posters |
| Bldg_GeneralStore | 1,502 | 2,500 | 9.82 x 8.2 x 5.29 | Door, Vending_A, Vending_B, Sign |
| Bldg_Bank | 1,060 | 2,500 | 7.02 x 7.7 x 5.8 | Vault_Door, Dial, Pet_Flap |
| Bldg_SlimeTower | 930 | 2,500 | 4.69 x 4.05 x 10.17 | Valve_Wheel, Gauges, Drips, Puddle |
| Bldg_Armory | 1,406 | 2,500 | 9.73 x 7.48 x 8.44 | Revolver, Targets, Gun_Racks, Door_L, Door_R |
| Bldg_Home_PodCabin | 516 | 2,500 | 7.71 x 5.8 x 5.07 | Door |
| Bldg_Home_MushroomAdobe | 516 | 2,500 | 6.2 x 5.9 x 6.1 | Door_Small, Door_Medium, Door_Tall |
| Bldg_Home_HoverWagon | 760 | 2,500 | 7.59 x 4.85 x 4.71 | Door |
| Bldg_Home_BarrelHouse | 608 | 2,500 | 5 x 5.3 x 5.4 | Door |
| Bldg_Home_CrateShack | 481 | 2,500 | 6.46 x 5.76 x 3.85 | Parachute, Door |
| Bldg_MineShaft | 796 | 2,500 | 13.55 x 9.88 x 6.84 | Boards, Lantern, Eyes |

### Props (30 models, 5,993 triangles)

| Model | Triangles | Budget | Size | Separate parts |
| --- | --- | --- | --- | --- |
| Prop_HitchingPost_A | 208 | 300 | 0.87 x 0.58 x 1.67 | Ring |
| Prop_HitchingPost_B | 216 | 300 | 2.14 x 0.66 x 1.71 | Rings |
| Prop_Sign_Arrow_A | 102 | 300 | 1.48 x 0.24 x 2.38 | none |
| Prop_Sign_Arrow_B | 252 | 300 | 1.7 x 0.24 x 3.5 | none |
| Prop_Sign_Glyph_A | 196 | 300 | 1.17 x 0.15 x 1.25 | Board |
| Prop_Sign_Glyph_B | 240 | 300 | 1.17 x 0.16 x 1.25 | Board |
| Prop_Sign_Glyph_C | 288 | 300 | 1.17 x 0.15 x 1.25 | Board |
| Prop_Sign_NeonTumbleweed | 230 | 300 | 1.24 x 1.17 x 2.08 | Neon |
| Prop_CyberHat_A | 176 | 300 | 0.47 x 0.39 x 0.35 | none |
| Prop_CyberHat_B | 184 | 300 | 0.47 x 0.39 x 0.23 | Propeller |
| Prop_LaserRevolver | 260 | 300 | 0.4 x 0.09 x 0.23 | none |
| Prop_HoverPokerTable | 206 | 300 | 1.15 x 1.15 x 0.67 | Card |
| Prop_HoverStool | 84 | 300 | 0.42 x 0.42 x 0.18 | none |
| Prop_RoboCowSkull | 220 | 300 | 0.94 x 0.89 x 0.51 | none |
| Prop_Barrel_Juice | 164 | 300 | 0.73 x 0.73 x 1.29 | none |
| Prop_Crate_Juice | 244 | 300 | 0.71 x 0.71 x 0.57 | none |
| Prop_Fence_Laser | 126 | 300 | 2.13 x 0.41 x 1.35 | Rails |
| Prop_Fence_Laser_Corner | 114 | 300 | 0.69 x 0.69 x 1.35 | Rail_Stubs |
| Prop_Fence_Laser_Gate | 168 | 300 | 2.18 x 0.82 x 1.35 | Gate |
| Prop_Lamp_Street | 148 | 300 | 0.96 x 0.23 x 3.52 | Jar |
| Prop_HoverWagon | 326 | 500 | 4.21 x 2.22 x 1.83 | none |
| Prop_Trough_Slime | 181 | 300 | 1.89 x 0.74 x 1.18 | none |
| Prop_Outhouse_Rocket | 194 | 300 | 1.85 x 1.64 x 3.2 | Door |
| Prop_Piano_Robot | 464 | 500 | 1.66 x 0.85 x 1.41 | Arm_L, Arm_R |
| Prop_WindPump | 304 | 500 | 2.1 x 2.25 x 7.28 | Blades |
| Prop_Mailbox_Teleporter | 160 | 300 | 0.84 x 1.42 x 1.32 | Parcel |
| Prop_GraveMarker_A | 44 | 300 | 0.69 x 1.52 x 1.02 | none |
| Prop_GraveMarker_B | 228 | 300 | 0.84 x 1.68 x 1.02 | Ray_Gun |
| Prop_GraveMarker_C | 112 | 300 | 0.69 x 1.75 x 1.06 | Hologram |
| Prop_Spittoon_Hover | 154 | 300 | 0.34 x 0.55 x 0.33 | none |

### Vegetation (20 models, 4,059 triangles)

| Model | Triangles | Budget | Size | Separate parts |
| --- | --- | --- | --- | --- |
| Veg_Cactus_A | 144 | 500 | 1.77 x 0.52 x 3.21 | none |
| Veg_Cactus_B | 154 | 500 | 1 x 1 x 1.19 | none |
| Veg_Cactus_C | 148 | 500 | 2.15 x 0.52 x 4.11 | none |
| Veg_CactusBio_A | 456 | 500 | 1.83 x 1.21 x 3.24 | none |
| Veg_CactusBio_B | 272 | 500 | 0.72 x 0.75 x 1.76 | none |
| Veg_Shrub_A | 84 | 500 | 0.39 x 0.58 x 0.69 | none |
| Veg_Shrub_B | 140 | 500 | 1.13 x 1.29 x 1.43 | none |
| Veg_Shrub_C | 130 | 500 | 0.76 x 0.68 x 0.8 | none |
| Veg_Succulent_A | 116 | 500 | 0.39 x 0.39 x 0.12 | none |
| Veg_Succulent_B | 149 | 500 | 0.9 x 0.88 x 0.33 | none |
| Veg_LanternLily_A | 127 | 500 | 1.41 x 0.55 x 1.55 | Bulb |
| Veg_LanternLily_B | 127 | 500 | 2.16 x 0.71 x 2.56 | Bulb |
| Veg_ZapBloom_A | 164 | 500 | 0.68 x 0.79 x 0.84 | Orb |
| Veg_ZapBloom_B | 204 | 500 | 0.7 x 0.79 x 1.03 | Orb |
| Veg_GlowCap_A | 114 | 500 | 0.97 x 0.73 x 0.83 | none |
| Veg_GlowCap_B | 190 | 500 | 1.54 x 1.35 x 1.17 | none |
| Veg_Tumbleweed_A | 306 | 500 | 0.84 x 0.86 x 0.82 | none |
| Veg_Tumbleweed_B | 336 | 500 | 0.92 x 0.77 x 0.79 | none |
| Veg_TumbleweedFloat_A | 334 | 500 | 0.84 x 0.86 x 0.97 | Glow |
| Veg_TumbleweedFloat_B | 364 | 500 | 0.92 x 0.77 x 0.94 | Glow |

### Rocks and natural elements (27 models, 3,433 triangles)

| Model | Triangles | Budget | Size | Separate parts |
| --- | --- | --- | --- | --- |
| Rock_Crystal_Shard_A | 48 | 500 | 0.37 x 0.19 x 0.52 | none |
| Rock_Crystal_Shard_B | 48 | 500 | 0.66 x 0.49 x 1.03 | none |
| Rock_Crystal_Shard_C | 72 | 500 | 1.27 x 0.67 x 2.07 | none |
| Rock_Crystal_Cluster_A | 170 | 500 | 1.62 x 1.46 x 1.39 | none |
| Rock_Crystal_Cluster_B | 242 | 500 | 3.09 x 2.82 x 2.76 | none |
| Rock_Scatter_A | 20 | 500 | 0.24 x 0.24 x 0.17 | none |
| Rock_Scatter_B | 20 | 500 | 0.36 x 0.35 x 0.24 | none |
| Rock_Scatter_C | 20 | 500 | 0.58 x 0.41 x 0.23 | none |
| Rock_Scatter_D | 20 | 500 | 0.59 x 0.54 x 0.41 | none |
| Rock_Boulder_A | 80 | 500 | 1.07 x 1.03 x 0.74 | none |
| Rock_Boulder_B | 80 | 500 | 2.19 x 2.16 x 1.45 | none |
| Rock_Boulder_C | 100 | 500 | 3.29 x 2.67 x 2.07 | none |
| Rock_Arch_A | 136 | 500 | 13.86 x 3.26 x 6.89 | none |
| Rock_Arch_B | 148 | 500 | 12.94 x 4.64 x 6.37 | none |
| Rock_Spire_A | 100 | 500 | 2.42 x 2.12 x 6.08 | none |
| Rock_Spire_B | 120 | 500 | 3.56 x 2.97 x 9.23 | none |
| Rock_Spire_C | 216 | 500 | 4.62 x 3.93 x 12.14 | none |
| Nat_Crater_A | 70 | 500 | 3.38 x 3.48 x 0.43 | none |
| Nat_Crater_B | 164 | 500 | 7.14 x 6.63 x 0.82 | Meteorite |
| Nat_SlimePool_A | 130 | 500 | 2.47 x 2.35 x 0.27 | Bubbles |
| Nat_SlimePool_B | 209 | 500 | 5.16 x 5.39 x 0.57 | Bubbles |
| Nat_AlienBones_A | 382 | 500 | 5.47 x 10.23 x 5.06 | none |
| Nat_AlienBones_B | 286 | 500 | 4.51 x 3.11 x 3.31 | none |
| Nat_Geyser_A | 166 | 500 | 2.21 x 2.4 x 3.98 | Plume |
| Nat_Geyser_B | 146 | 500 | 2.91 x 2.29 x 1.45 | none |
| Nat_Meteorite_A | 100 | 500 | 1.11 x 2.34 x 0.71 | none |
| Nat_Meteorite_B | 140 | 500 | 3.6 x 5.65 x 2.02 | none |

### Terrain (9 models, 1,915 triangles)

| Model | Triangles | Budget | Size | Separate parts |
| --- | --- | --- | --- | --- |
| Mtn_Wall_A | 204 | 1,500 | 20 x 13.91 x 18.35 | none |
| Mtn_Wall_B | 478 | 1,500 | 20 x 18.05 x 22.24 | none |
| Mtn_Wall_C | 404 | 1,500 | 20 x 14.2 x 25.23 | Crystal_Vein |
| Mtn_Corner_In | 164 | 1,500 | 16.06 x 16.07 x 20.74 | none |
| Mtn_Corner_Out | 164 | 1,500 | 18.71 x 18.38 x 20.11 | none |
| Mtn_End | 177 | 1,500 | 20.89 x 12.22 x 18 | none |
| Mtn_End_Mirrored | 177 | 1,500 | 20.89 x 12.22 x 18 | none |
| Mtn_Peak_A | 49 | 1,500 | 26.64 x 30.22 x 35 | none |
| Mtn_Peak_B | 98 | 1,500 | 36 x 32.14 x 45 | none |

### Ships (7 models, 4,518 triangles)

| Model | Triangles | Budget | Size | Separate parts |
| --- | --- | --- | --- | --- |
| Ship_Saucer_Classic | 1,010 | 2,000 | 6.54 x 6.54 x 4.81 | Dome, Landing_Gear |
| Ship_Saucer_Classic_Crashed | 1,084 | 2,000 | 11.58 x 7.63 x 4.28 | Loose_Leg, Crater, Debris |
| Prop_UFO_Beam | 24 | 300 | 3.86 x 3.86 x 5.85 | none |
| Ship_Rocket_Classic | 568 | 2,000 | 4.83 x 4.77 x 10.06 | Fin_A, Fin_B, Fin_C |
| Ship_Rocket_Classic_Crashed | 700 | 2,000 | 14.14 x 9.27 x 8.73 | Loose_Fin, Crater, Debris |
| Ship_Rocket_Shuttle | 464 | 2,000 | 6.89 x 6.52 x 3.34 | Canopy, Wing_L, Wing_R |
| Ship_Rocket_Shuttle_Crashed | 668 | 2,000 | 10.11 x 14.76 x 3.76 | Loose_Wing, Crater, Debris |

### Weapons (13 models, 3,498 triangles)

| Model | Triangles | Budget | Size | Separate parts |
| --- | --- | --- | --- | --- |
| Wpn_Blaster_Scattergun | 364 | 500 | 0.15 x 1 x 0.31 | Cork |
| Wpn_Blaster_Repeater | 308 | 500 | 0.06 x 1.21 x 0.26 | Lever, Scope_Jar |
| Wpn_Blaster_SixShooter | 260 | 500 | 0.09 x 0.4 x 0.23 | none |
| Wpn_Blaster_Derringer | 136 | 500 | 0.05 x 0.19 x 0.25 | Bulb |
| Wpn_Blaster_CrankGatling | 448 | 500 | 0.3 x 0.86 x 0.45 | Barrels, Crank |
| Wpn_Blaster_Atomizer | 284 | 500 | 0.17 x 0.48 x 0.37 | Discs, Tip |
| Wpn_Blaster_Disintegrator | 300 | 500 | 0.2 x 0.56 x 0.35 | Dome, Core |
| Wpn_Blaster_HeatRay | 436 | 500 | 0.18 x 1.16 x 0.33 | Ray, Hoops |
| Wpn_Melee_PlasmaBowie | 116 | 500 | 0.09 x 0.03 x 0.44 | Edge |
| Wpn_Melee_CrystalPickaxe | 122 | 500 | 0.73 x 0.08 x 0.86 | Crystals |
| Wpn_Melee_BrandingIron | 196 | 500 | 0.22 x 0.04 x 1.01 | Head |
| Wpn_Melee_CactusClub | 240 | 500 | 0.31 x 0.25 x 0.88 | Bulb |
| Wpn_Melee_LaserLasso | 288 | 500 | 0.37 x 0.2 x 0.72 | Rope |
