# Map scene contract

Every map scene needs the objects below so the race scripts (cars, bot, coins, obstacles, finish) can find them. They are empty GameObjects with exact names (case-sensitive). They can sit at the root or inside a group.

## Scale
- 1 Unity unit = 1 metre. A car is about **5 m long and 2 m wide**.
- Track pieces are 15 m wide, and you can scale them in the scene.

## Road borders
Build the road from the BEDRILL track prefabs. They already contain invisible walls along both road edges (on the **`Wall`** layer), so cars can't leave the road. Cars bounce off walls, and each hit costs race points.

The fence prefabs are on `Wall` too. Any other solid object you put next to the road that should count as a border hit also goes on `Wall`.

## Objects

```
SpawnPoints
├── Player             start of the player (Single) / player 1 (Doubles)
└── Bot                start of the bot (Single) / player 2 (Doubles)
FinishLine             BoxCollider, Is Trigger ON
BotPath
├── WP_00 … WP_NN      route points, in driving order
CoinSpots
├── about 10 points    possible coin positions
ObstacleSpots          only on maps with obstacles
├── about 12 points    possible obstacle positions
```

- **Spawn points:** the car faces the object's **blue (Z) axis**. Place them side by side on the road, pointing in the driving direction.
- **FinishLine:** spans the full road width and is about 4 m tall. Its **blue (Z) axis points in the driving direction**, because crossings the other way don't count. Put it behind the spawn points so the first lap is a full lap.
- **BotPath:** the order of children in the Hierarchy is the route. Put a point every 20–40 m on straights and closer together in corners, in the middle of the road. The last point should be just past the finish line.
- **CoinSpots / ObstacleSpots:** points on the road surface. Each race uses 5 random coin spots and 4 random obstacle spots.

## Maps
| Scene | Map | Laps | Obstacles |
|---|---|---|---|
| `Map2` | Meadow Rush | 2 | sand piles |
| `Map3` | Tidal Heatwave | 2 | banana peels |
| `Map_Beach` | Palm Paradise | 1 | banana peels |
| `Map_Winter` | Frozen Tides | 1 | ice cubes |
| `Map_Summer` | Evergreen Escape | 1 | none |
| `Map_Champions_Field` | Champions Field | 2 | none |

## Checking a scene
Open the scene and run **Tools → Car Racing → Validate Open Scene**. The Console lists anything that's missing.
