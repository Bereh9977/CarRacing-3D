# Map scene contract

What a map scene needs so the race works.

## Scale
- 1 Unity unit = 1 metre. A car is about **3–4.5 m long and 1.5–2 m wide**.
- Track pieces are 15 m wide.

## Road borders
Build the road from the BEDRILL track prefabs. They already contain invisible walls along both road edges (on the **`Wall`** layer), so cars can't leave the road. Cars bounce off walls.

On a road that isn't a loop, close both ends with fences. The fence prefabs are on `Wall` too. Any other solid object that should count as a border also goes on `Wall`.

## Start and finish
- **Finish:** the `Track_line_type_01_start_finish_15m_free_obs` piece. It contains the `FinishLine` trigger, so nothing extra is needed. Use exactly one per map. Its **blue (Z) arrow must point in the driving direction**, because crossings the other way take a lap away. The piece looks the same both ways round, so rotate it 180° if needed.
- **Start:** drop one `Assets/Prefabs/Race.prefab` into the map. It holds the spawn point and the race logic. Its **blue arrow points in the driving direction**, and the car spawns facing that way.
  - Loop maps: just past the finish line, so the first lap is a full lap.
  - Maps that aren't loops: at the beginning of the road.
- Don't place cars in the map and don't add a camera script. The race spawns the car and sets up Main Camera.

## Organizing a map
**Tools → Car Racing → Organize Open Scene** puts the top-level objects into `Lighting`, `Environment`, `Track`, `Decorations`, `Coins` and `Obstacles`. Main Camera and `Race` stay at the top. Run it again after adding objects.

## Maps
| Scene | Map | Laps | Type | Obstacles |
|---|---|---|---|---|
| `Map2` | Meadow Rush | 2 | loop | sand piles |
| `Map3` | Tidal Heatwave | 2 | loop | banana peels |
| `Map_Beach` | Palm Paradise | 1 | not a loop | banana peels |
| `Map_Winter` | Frozen Tides | 1 | not a loop | ice cubes |
| `Map_Summer` | Evergreen Escape | 1 | not a loop | none |
| `Map_Champions_Field` | Champions Field | 2 | loop | none |

## Checking a scene
Open the scene and run **Tools → Car Racing → Validate Open Scene**. The Console lists anything that's wrong.
