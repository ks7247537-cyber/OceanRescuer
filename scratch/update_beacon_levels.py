p6 = "Assets/Resources/Levels/DeepOcean/Level_06.asset"
with open(p6, "a", encoding="utf-8") as f:
    f.write("  beacons:\n  - gridPos: {x: 4, y: 3}\n    beaconName: Trench Beacon\n")
print("Added beacon to Level_06")

p12 = "Assets/Resources/Levels/DeepOcean/Level_12.asset"
with open(p12, "a", encoding="utf-8") as f:
    f.write("  beacons:\n  - gridPos: {x: 7, y: 0}\n    beaconName: Sanctuary Beacon\n")
print("Added beacon to Level_12")
