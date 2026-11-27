p7 = "Assets/Resources/Levels/DeepOcean/Level_07.asset"
with open(p7, "a", encoding="utf-8") as f:
    f.write("  tremors:\n  - gridPos: {x: 4, y: 2}\n    cycleInterval: 12\n    warningDuration: 2.5\n    tremorDuration: 3\n    pressureSurge: 8\n")
print("Added tremor to Level_07")

p11 = "Assets/Resources/Levels/DeepOcean/Level_11.asset"
with open(p11, "a", encoding="utf-8") as f:
    f.write("  tremors:\n  - gridPos: {x: 3, y: 2}\n    cycleInterval: 10\n    warningDuration: 2\n    tremorDuration: 3\n    pressureSurge: 8\n")
print("Added tremor to Level_11")
