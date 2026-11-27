p8 = "Assets/Resources/Levels/DeepOcean/Level_08.asset"
with open(p8, "a", encoding="utf-8") as f:
    f.write("  diversions:\n  - gridPos: {x: 3, y: 1}\n    restrictedDirection: 0\n    hazardName: Hydrothermal Vent\n")
print("Added diversion to Level_08")

p10 = "Assets/Resources/Levels/DeepOcean/Level_10.asset"
with open(p10, "a", encoding="utf-8") as f:
    f.write("  diversions:\n  - gridPos: {x: 5, y: 1}\n    restrictedDirection: 0\n    hazardName: Thermal Fissure\n")
print("Added diversion to Level_10")
