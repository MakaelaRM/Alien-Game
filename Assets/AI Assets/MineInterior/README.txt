CRYSTAL MINE INTERIOR - generated assets (v2)
==============================================

INSTALL
1. Drag the whole MineInterior folder into your Unity project's Assets folder
   (keep the subfolders: Models, Textures, Editor, Runtime).
2. Wait for Unity to finish importing.
3. In the menu bar use the new "Mine" menu, in order:
     Mine > 1 - Create Materials and Fix Models
     Mine > 2 - Create Prefabs
     Mine > 3 - Build Test Tunnel In Scene   (try this in a NEW empty scene)
   Step 1 needs the Universal Render Pipeline (your project already uses it).

FLOOR = TEXTURE ONLY
  Textures/Mine_Ground.png is a tileable faceted violet gravel texture (one tile = 4 m).
  Mine_Ground.mat is the material. To put it on a plane (like the Ground plane in the town):
    - Select your plane(s) and use  Mine > Apply Ground Texture To Selected Planes
      (it creates a material with the right tiling for each plane's size), or
    - Mine > 4 - Create Floor Plane (40 x 40) makes a ready-made one.
  By hand: Tiling = size in metres / 4.

WALLS AND CEILING = CHUNKY ROCK MODELS (same flat-shaded, faceted style as the town rocks)
  Mine_Wall_Rock_A / B / C   wall slabs, 4 m long, 4.4 m tall. All share the same edge profile so they
                             can be mixed in any order along z.
  Mine_Wall_Rock_Vein        wall slab with glowing crystals growing out of it.
  Mine_Ceiling_Rock_A / B    ceiling slabs with a lumpy underside and stalactites. Covers x -2.4..2.4.
  Mine_Ceiling_Rock_Crystals ceiling slab with hanging crystals.
  Layout: tunnel is 4 m wide. Left wall at x = -2, right wall at x = +2, ceiling pieces at y = 4,
  every piece is placed every 4 m along z. The rock mass of a wall must be on the outside: if a wall
  piece faces the wrong way, rotate it 180 degrees on Y and move it +4 m in z
  (the test tunnel script does this automatically).
  No textures are used on these. Colour comes from 4 solid materials (top / side / side alt / underside)
  assigned per face, so the light does the rest, like the town rocks.

TERRAIN ROCKS (pivot at the base)
  Mine_Rock_Spire_A / B, Mine_Rock_Boulder_A / B / C, Mine_Rock_Pillar (floor to ceiling).
  Use these to break up the walls, build the crystal cavern and block off side tunnels.

TRACK
  Mine_Track_Straight_4m   4 m long, gauge 1 m.
  Mine_Track_Curve_90      90 degree turn, 4 m radius, snaps to the straight piece.
  Rail tops are at y = 0.22.

PROPS (pivot at the base)
  Mine_Cart, Mine_Cart_Loaded (sit on the rails at y = 0),
  Mine_Support_Frame (place across the tunnel every 8 m),
  Mine_Lantern_Post (amber point light, flickers), Mine_Crystal_Cluster_Large, Mine_Crystal_Shards_Small
  (cyan lights), Mine_Crate, Mine_Barrel, Mine_Ore_Sack, Mine_Ore_Pile, Mine_Tool_Rack (hang on a wall),
  Mine_Rail_EndStop (with 1.7 m of rail), Mine_Warning_Sign.

NOTES
  - Crystals and the lantern glass are emissive. For a bloom glow, add a Global Volume with Bloom.
  - To make the mine enterable, put the Mine scene's spawn point just inside the entrance and keep the
    SceneTransition trigger you already have on the town side.
