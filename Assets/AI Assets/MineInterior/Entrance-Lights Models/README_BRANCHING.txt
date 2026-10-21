BRANCHING LEVEL KIT (add-on)
Copy the .obj files into MineInterior/Models, replace Mine_Materials.mtl and Editor/MineSetup.cs, then run
Mine > 1, then Mine > 2 again (they are safe to re-run).
Convention: camera/player side is -z, rock mass is +z (same as Mine_Exit). If one faces the wrong way, rotate it 180 on Y.

FRONT WALLS (block the player from walking toward the camera, add depth)
  Mine_FrontWall_A / B / C / Crystals   4 m long, 1.5 m tall, ~1.1 m thick. Same end profile: tile along x every 4 m.
  Place at the front edge of the floor (z at the edge you don't want crossed). Mesh collider is added automatically.
  Mine_FrontRock_Big   big foreground rock to frame the shot (tuck a few behind the front wall line).
DOORS (face -z, pivot = floor centre of the opening)
  Mine_Door_Shaft     open timber shaft with rail stub and lantern   -> normal route
  Mine_Door_Crawl     small low crawl hole with crystals             -> secret / side route
  Mine_Door_Boarded   planks + warning sign                          -> blocked for now
  Mine_Door_Gate      iron gate with padlock                         -> needs a key
  Mine_Door_Crystal   glowing crystal arch                           -> special / boss area
  Select a door in the scene and run  Mine > 7 - Add Door Trigger To Selected  (adds a trigger + your SceneTransition).
  Then type the scene name on its DoorTrigger.
OTHER
  Mine_Shaft_Down     floor shaft with winch (way down to a lower level)
  Mine_Ladder         3 m ladder
  Mine_Rubble_Block   cave-in that blocks a passage (5.9 m wide)
  Mine_Sign_Arrows    direction sign post

THEMED ENTRANCES (all face -z, pivot = floor centre of the opening; each has its own surround so it reads as a different area)
  Mine_Door_Shaft          classic timber mine shaft in rock lumps          -> normal mine area           (open)
  Mine_Door_Tunnel_Wide    wide cart tunnel with an ore hopper gantry         -> ore / transport area       (open)
  Mine_Door_Wood_Open      timber storehouse with a gabled roof, lit window   -> workshop / storage         (open)
  Mine_Door_Stone_Arch     ruined temple: columns, entablature, fallen drum   -> ancient ruins              (open)
  Mine_Door_Crystal        mouth ringed with huge crystals                    -> crystal cavern             (open)
  Mine_Door_Mushroom       grassy mound with glowing mushrooms, low door      -> fungus / secret area       (open)
  Mine_Door_Forge          stone furnace with chimney, glowing fire           -> smithy / hot area          (open)
  Mine_Door_Flooded        low tunnel half under glowing water, plank landing -> lake / water area          (open)
  Mine_Door_Crawl          tiny rock crawl hole                               -> secret passage             (open)
  Mine_Trapdoor_Open       floor hatch                                        -> cellar / lower level       (open)
  Mine_Door_Wood_Closed    same storehouse, doors shut                        (closed, swap for Wood_Open later)
  Mine_Door_Iron           steel bulkhead with pipes, hazard stripes, wheel    -> facility / vault           (closed)
  Mine_Door_Gate           stone gatehouse, portcullis, barred windows        -> dungeon / prison           (locked, needs key)
  Mine_Door_Stone_Sealed   temple with glowing runes and sealed slab          -> sealed temple              (locked)
  Mine_Trapdoor_Closed     floor hatch, shut                                  (closed)
  Mine_Door_Bricked        old city wall, arch walled up                      -> abandoned district         (blocked)
  Mine_Door_Boarded        planks and warning sign                            (blocked)
  Mine_Door_Collapsed      rockfall filling a shaft                           (blocked)
  Mine_Rubble_Block        cave-in that blocks a passage                      (blocked)
  Unlocking later: delete the blocked/closed door and put the matching open one in the same spot.
  Run Mine > 7 on the open doors to add the scene trigger.
LIGHTS (each one gets a point light from Mine > 2; torches/lanterns flicker)
  Mine_Lantern_Wall / Mine_Torch_Wall / Mine_Sconce_Crystal : pivot at the wall mount point, wall at z=0, they stick out toward -z.
  Mine_Lantern_Hanging / Mine_Chandelier : pivot at the top, hang from the ceiling (y=4 in the tunnel).
  Mine_Lantern_Floor, Mine_Brazier, Mine_Lamp_Crystal, Mine_Candles : stand on the floor.
  Tip: a light every 8-10 m along a tunnel, and one at every door. Keep the number of real-time lights low
  (turn off shadows on the small ones). Bloom makes the glow materials look good.
