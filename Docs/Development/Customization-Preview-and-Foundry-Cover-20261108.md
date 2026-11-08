# Customization preview and foundry cover — 2026-11-08

- Rotate the portrait hangar with the preview yaw so the rear wall stays behind the rotating robot.
- Focus the backpack row or backpack model to turn the preview to 180 degrees; other part rows return it to the front view.
- Spread the Foundry's destructible cover into the middle and outer lanes. Relocated edge props now sit inside the playable perimeter.

## Validation

- Unity 6000.6.7f1 build passed: `VOXSKY_BUILD_PASS 237472110 bytes` (`Logs/Portrait-Obstacle-Spread-Build-20261108.log`).
- `--capture-docs-gallery` passed and captured 36 images, including the four Foundry sectors (`Logs/Foundry-Obstacle-Spread-Gallery-20261108.log`).
- The balance editor study captured the rear backpack view. Its 144 checks include two existing failures unrelated to these changes: quadruped knee armor attachment and configured damage/velocity/magazine affecting a real shot (`Logs/Portrait-Obstacle-Study-20261108.log`).
- `Arena.unity` and `PD-Balance.json` SHA256 hashes were unchanged by the final build: `590965B01E7EC3160F3E30BB856981FF8133FC296023D01A64C3058317247391` and `3B3D4A3B0A175E929B6E9A552E5B8716104A654C814DE33AC144C884B76D77AD`.
- Reviewed `Docs/Screenshots/Arena/arena-north-sector.png` and the generated backpack preview capture.
