# Coastal Bay map

## Scope

Added a selectable Earth shoreline arena to test land-to-sea combat. The existing Foundry and Canyon map enum values stay unchanged; Coastal Bay is appended as map value `3` and the map button cycles Foundry → Canyon → Coastal Bay.

## Map setup

- A broad dry land side uses the existing Foundry cargo, generator, and barricade models for 15 destructible cover pieces.
- A smooth 18 m seabed descent runs from x=-70 to x=0. Its estimated maximum grade is about 21.1°, under the 22° walkability check. The deep sea floor takes the eastern half of the arena.
- The transparent ocean plane starts at x=-35, so the sloped beach transitions below the waterline and the map reads as a shoreline rather than two flat tiles.
- The sea floor uses a blue-gray material with visual-only reef silhouettes. The non-colliding water surface and submerged fighters use blue fog and reduced sunlight; submersion fades in over four metres.
- Deep water applies the existing 50% gravity scale. Starts place one fighter on the coast and the other on the sea floor.
- Fourteen translucent ripple rings travel across the ocean, with a slow surface bob. A roughly 11-second current alternates between pulling fighters toward the shore and pushing them back out.
- Projectiles crossing underwater travel at 58% speed; the reduced step is applied to actual hit and obstacle checks.

## Canyon sandstorm

- Canyon climate selection offers clear, overcast, or sandstorm. Rain and thunderstorm selections are mapped to the dry sandstorm, preventing overlapping rain.
- Sand gusts replace rain streaks, tan exponential fog clouds the view, and radar contact range drops to 46%. Lock acquisition range and weak aim assistance fall to 55%.

## Validation

The coastal branch of `--climate-smoke --coastal` checks land/sea heights, slope, cover placement, 50% underwater gravity, underwater fog, the water surface, alternating currents, underwater projectile travel, and dry-air restoration. It also captures shoreline and underwater screenshots. The Canyon branch checks sandstorm selection, no rain/snow overlap, weather fog, radar range, and lock-on reduction.

- Unity `6000.6.7f1` Windows build: **passed**, 237,501,666 bytes; process exit 0.
- `--climate-smoke --coastal`: **38 checks passed**, process exit 0.
- `--climate-smoke --outdoor`: **29 checks passed**, process exit 0.
- `Assets/Scenes/Arena.unity` and `Data/Balance/PD-Balance.json` were restored from exact pre-build bytes; their SHA-256 values matched after build.
- Captures: `Docs/Screenshots/PD14-Climate/CoastalBay/CoastalBay-Shoreline.png`, `CoastalBay-Underwater.png`, and `Docs/Screenshots/PD14-Climate/OutdoorCanyon/Sandstorm-Morning.png`.
